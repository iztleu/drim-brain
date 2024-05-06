using System.Text.Json;
using Confluent.Kafka;
using Events;

namespace FraudService.Features.Deposits.HostedServices;

public class ConsumerHostedService(
    IConsumer<int, string> _consumer,
    ILogger<ConsumerHostedService> _logger)
    : BackgroundService
{
    private const string Topic = "crypto-deposit-created";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe(Topic);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(30, stoppingToken);

                var consumeResult = _consumer.Consume(stoppingToken);

                _logger.LogInformation("Consumed deposit event for user {UserId} from partition {Partition} with offset {PartitionOffset}\n{@Event}",
                    consumeResult.Message.Key,
                    consumeResult.TopicPartitionOffset.Partition.Value,
                    consumeResult.TopicPartitionOffset.Offset.Value,
                    JsonSerializer.Deserialize<CryptoDepositCreatedEvent>(consumeResult.Message.Value));

                _consumer.Commit(consumeResult);
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

        _consumer.Unsubscribe();
        _consumer.Close();
    }
}
