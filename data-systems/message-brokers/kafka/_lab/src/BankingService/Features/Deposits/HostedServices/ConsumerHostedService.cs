using System.Text.Json;
using Confluent.Kafka;
using Events;

namespace BankingService.Features.Deposits.HostedServices;

public class ConsumerHostedService(ILogger<ConsumerHostedService> _logger) : BackgroundService
{
    private const string Topic = "crypto-deposit-created";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = "localhost:9092",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            GroupId = "BankingService",
        };

        using var consumer = new ConsumerBuilder<int, string>(config).Build();
        consumer.Subscribe(Topic);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(30, stoppingToken);

                var consumeResult = consumer.Consume(stoppingToken);

                _logger.LogInformation("Consumed deposit event for user {UserId} from partition {Partition} with offset {PartitionOffset}\n{@Event}",
                    consumeResult.Message.Key,
                    consumeResult.TopicPartitionOffset.Partition.Value,
                    consumeResult.TopicPartitionOffset.Offset.Value,
                    JsonSerializer.Deserialize<CryptoDepositCreatedEvent>(consumeResult.Message.Value));
            }
            catch (OperationCanceledException)
            {
                // Ignore
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "Error consuming message: {Reason}", ex.Error.Reason);
            }
        }

        consumer.Unsubscribe();
        consumer.Close();
    }
}
