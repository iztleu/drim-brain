using ApiGateway.Features.Notifications.Services;
using Grpc.Core;
using NotificationService.Client;

namespace ApiGateway.Features.Notifications.HostedServices;

public class NotificationStreamHostedService(
    NotificationStream _notificationStream,
    NotificationService.Client.Notifications.NotificationsClient _notificationsClient,
    ILogger<NotificationStreamHostedService> _logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _notificationStream.Start();

        while (true)
        {
            try
            {
                var call = _notificationsClient.SubscribeForNotifications(new SubscribeForNotificationsRequest(), cancellationToken: stoppingToken);

                stoppingToken.Register(() => call.Dispose());

                await foreach (var notification in call.ResponseStream.ReadAllAsync(stoppingToken))
                {
                    _logger.LogInformation("Received notification: {Notification}", notification);
                    await _notificationStream.Publish(notification, stoppingToken);
                }
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing notifications.");
            }

            _logger.LogInformation("Reconnecting in 5 seconds...");
            await Task.Delay(5000, stoppingToken);
        }
    }
}
