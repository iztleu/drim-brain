using BankingService.Api.RabbitMq;
using RabbitMQ.Client;

namespace NotificationService.Setup;

public class RabbitMqSetupHostedService(
    IConnection _connection,
    ILogger<RabbitMqSetupHostedService> _logger)
    : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Setting up RabbitMQ");

                using var model = _connection.CreateModel();
                model.ExchangeDeclare(ExchangeNames.BankingService, ExchangeType.Topic, durable: true, autoDelete: false);
                model.QueueDeclare(RabbitMqQueueNames.DepositCreated, durable: true, exclusive: false, autoDelete: false);
                model.QueueBind(RabbitMqQueueNames.DepositCreated, ExchangeNames.BankingService, RoutingKeys.DepositCreated);

                _logger.LogInformation("RabbitMQ setup completed");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while setting up RabbitMQ. Retrying in 5 seconds...");
            }

            Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }

        return Task.CompletedTask;
    }
}

public static class RabbitMqQueueNames
{
    public const string DepositCreated = "deposit-created";
}
