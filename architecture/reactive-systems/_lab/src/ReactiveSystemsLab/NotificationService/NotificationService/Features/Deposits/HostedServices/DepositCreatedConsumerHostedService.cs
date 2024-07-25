using System.Text;
using System.Text.Json;
using BankingService.Api.RabbitMQ.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NotificationService.Features.Deposits.HostedServices;

public class DepositCreatedConsumerHostedService(
    IConnection _rabbitMqConnection,
    ILogger<DepositCreatedConsumerHostedService> _logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var model = _rabbitMqConnection.CreateModel();

                var consumer = new EventingBasicConsumer(model);
                consumer.Received += (_, eventArgs) =>
                {
                    var body = eventArgs.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    var depositCreatedEvent = JsonSerializer.Deserialize<DepositCreatedEvent>(json);

                    _logger.LogInformation("Consumed message with user id {UserId} and asset {Asset}",
                        depositCreatedEvent!.UserId, depositCreatedEvent.Asset);

                    model.BasicAck(eventArgs.DeliveryTag, multiple: false);
                };

                model.BasicConsume("deposit-created", autoAck: false, consumer);

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
