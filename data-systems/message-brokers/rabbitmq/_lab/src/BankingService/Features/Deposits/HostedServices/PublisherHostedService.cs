using System.Text.Json;
using Events;
using RabbitMQ.Client;

namespace BankingService.Features.Deposits.HostedServices;

public class PublisherHostedService(IConnection _connection) : BackgroundService
{
    private const string QueueName = "CryptoDepositCreatedEvents";

    private readonly string[] _currencies = [ "BTC", "ETH", "XMR", "BCH" ];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var channel = _connection.CreateModel();

        channel.QueueDeclare(
            queue: QueueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var id = 0;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var ev = new CryptoDepositCreatedEvent(
                    id++,
                    Random.Shared.Next(1, 11),
                    Random.Shared.Next(100000, 999999).ToString(),
                    _currencies[id % _currencies.Length],
                    Random.Shared.Next(1, 15),
                    Guid.NewGuid().ToString(),
                    "def",
                    DateTime.UtcNow);

                channel.BasicPublish(
                    exchange: string.Empty,
                    routingKey: QueueName,
                    basicProperties: null,
                    body: JsonSerializer.SerializeToUtf8Bytes(ev));

                await Task.Delay(Random.Shared.Next(300, 1000), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Ignore
            }
        }
    }
}
