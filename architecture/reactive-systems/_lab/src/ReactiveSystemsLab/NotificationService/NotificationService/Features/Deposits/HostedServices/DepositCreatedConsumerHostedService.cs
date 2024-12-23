using System.Text;
using System.Text.Json;
using BankingService.Api.RabbitMQ.Events;
using Google.Protobuf.WellKnownTypes;
using NotificationService.Database;
using NotificationService.Domain;
using NotificationService.Features.Notifications.Services;
using NotificationService.Setup;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NotificationService.Features.Deposits.HostedServices;

public class DepositCreatedConsumerHostedService(
    IServiceProvider _serviceProvider,
    IConnection _rabbitMqConnection,
    NotificationStream _notificationStream,
    ILogger<DepositCreatedConsumerHostedService> _logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _notificationStream.Start();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var model = _rabbitMqConnection.CreateModel();

                var consumer = new AsyncEventingBasicConsumer(model);

                model.BasicConsume(RabbitMqQueueNames.DepositCreated, autoAck: false, consumer);

                consumer.Received += async (_, eventArgs) =>
                {
                    Notification notification;

                    try
                    {
                        using var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                        cts.CancelAfter(TimeSpan.FromSeconds(5));

                        var body = eventArgs.Body.ToArray();
                        var json = Encoding.UTF8.GetString(body);
                        var depositCreatedEvent = JsonSerializer.Deserialize<DepositCreatedEvent>(json);

                        await using var scope = _serviceProvider.CreateAsyncScope();
                        var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();

                        notification = new Notification
                        {
                            UserId = depositCreatedEvent!.UserId,
                            Text = $"Deposit of {depositCreatedEvent.Amount} {depositCreatedEvent.Asset} created.",
                            CreatedAt = DateTime.UtcNow,
                        };

                        model.BasicAck(eventArgs.DeliveryTag, multiple: false);

                        dbContext.Notifications.Add(notification);
                        await dbContext.SaveChangesAsync(cts.Token);

                        _logger.LogInformation("Consumed message with user UserId {UserId} and Id {Id}",
                            depositCreatedEvent.UserId, depositCreatedEvent.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An error occurred while consuming message.");
                        return;
                    }

                    var notificationDto = new NotificationDto
                    {
                        Id = notification.Id,
                        UserId = notification.UserId,
                        Text = notification.Text,
                        CreatedAt = notification.CreatedAt.ToTimestamp(),
                    };

                    _notificationStream.Publish(notificationDto);
                };

                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while consuming message. Retrying in 5 seconds...");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
