using System.Text.Json;
using Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NotificationService.Features.Deposits.HostedServices;

public class ConsumerHostedService(IConnection _connection, ILogger<ConsumerHostedService> _logger) : BackgroundService
{
    private const string QueueName = "CryptoDepositCreatedEvents";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var channel = _connection.CreateModel();

        channel.QueueDeclare(queue: QueueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (_, ea) =>
        {
            var body = ea.Body.ToArray();
            var ev = JsonSerializer.Deserialize<CryptoDepositCreatedEvent>(body);

            _logger.LogInformation("Sending email for event: {@Event}", ev);
        };

        channel.BasicConsume(queue: QueueName,
            autoAck: true,
            consumer: consumer);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
