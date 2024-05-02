using System.Text.Json;
using Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ApiGateway.Features.Deposits.HostedServices;

public class ConsumerHostedService(IConnection _connection, ILogger<ConsumerHostedService> _logger) : BackgroundService
{
    private const string ExchangeName = "CryptoDepositCreatedEventsFanout";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var channel = _connection.CreateModel();

        channel.ExchangeDeclare(
            exchange: ExchangeName,
            type: ExchangeType.Fanout);

        var queueName = channel.QueueDeclare().QueueName;

        channel.QueueBind(queue: queueName,
            exchange: ExchangeName,
            routingKey: string.Empty);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (_, ea) =>
        {
            var body = ea.Body.ToArray();
            var ev = JsonSerializer.Deserialize<CryptoDepositCreatedEvent>(body);

            _logger.LogInformation("Sending SignalR message for event: {@Event}", ev);
        };

        channel.BasicConsume(queue: queueName,
            autoAck: true,
            consumer: consumer);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
