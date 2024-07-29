using BlockchainService.Api.Kafka;
using BlockchainService.Api.Kafka.Events;
using Confluent.Kafka;
using Quartz;

namespace BlockchainService.Features.Deposits.Jobs;

public class DepositScanningJob(
    IProducer<int, CryptoDepositCreatedEvent> _kafkaProducer,
    ILogger<DepositScanningJob> _logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            var id = Guid.NewGuid().ToString();
            var userId = Random.Shared.Next(1, 11);
            var amount = Random.Shared.NextDouble();

            await _kafkaProducer.ProduceAsync(TopicNames.CryptoDepositCreated, new Message<int, CryptoDepositCreatedEvent>
            {
                Key = userId,
                Value = new CryptoDepositCreatedEvent(id, userId, "BTC", (decimal)amount, "0x1234567890", DateTime.UtcNow),
            }, cts.Token);

            _logger.LogInformation("DepositScanningJob executed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DepositScanningJob failed");
        }
    }
}
