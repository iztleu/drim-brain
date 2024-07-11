using System.Text.Json;
using Confluent.Kafka;
using Events;

namespace BlockchainService.Features.Deposits.HostedServices;

public class PublisherHostedService(ILogger<PublisherHostedService> _logger) : BackgroundService
{
    private const string Topic = "crypto-deposit-created";

    private readonly string[] _currencies = [ "BTC", "ETH", "XMR", "BCH" ];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ProducerConfig { BootstrapServers = "localhost:9092" };

        using var p = new ProducerBuilder<int, string>(config).Build();

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

                var message = new Message<int, string> { Key = ev.UserId, Value = JsonSerializer.Serialize(ev) };

                var deliveryResult = await p.ProduceAsync(Topic, message, stoppingToken);

                _logger.LogInformation("Published deposit event for user {UserId} to partition {Partition} with offset {Offset}",
                    deliveryResult.Key,
                    deliveryResult.TopicPartitionOffset.Partition.Value,
                    deliveryResult.TopicPartitionOffset.Offset.Value);

                await Task.Delay(Random.Shared.Next(300, 1000), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Ignore
            }
            catch (ProduceException<int, string> ex)
            {
                _logger.LogError(ex, "Delivery failed: {Reason}", ex.Error.Reason);
            }
        }
    }
}
