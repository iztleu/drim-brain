using BlockchainService.Api;
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

            await _kafkaProducer.ProduceAsync("crypto-deposit-created", new Message<int, CryptoDepositCreatedEvent>
            {
                Key = 1,
                Value = new CryptoDepositCreatedEvent
                {
                    Id = 1,
                    UserId = 1,
                    Asset = "BTC",
                    Amount = 0.1m,
                    TransactionHash = "0x1234567890",
                    CreatedAt = DateTime.UtcNow
                },
            }, cts.Token);

            _logger.LogInformation("DepositScanningJob executed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DepositScanningJob failed");
        }
    }
}
