using BlockchainService.Api;
using Confluent.Kafka;

namespace BankingService.Features.CryptoDeposits.HostedServices;

public class CryptoDepositConsumerHostedService(
    IConsumer<int, CryptoDepositCreatedEvent> _kafkaConsumer,
    ILogger<CryptoDepositConsumerHostedService> _logger)
    : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _kafkaConsumer.Subscribe("crypto-deposit-created");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = _kafkaConsumer.Consume(stoppingToken);

                _logger.LogInformation("Consumed message with key {Key} and value {TransactionHash}",
                    consumeResult.Message.Key, consumeResult.Message.Value.TransactionHash);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to consume message");
            }
        }

        return Task.CompletedTask;
    }
}
