using System.Text;
using System.Text.Json;
using BankingService.Api.RabbitMq;
using BankingService.Api.RabbitMQ.Events;
using BlockchainService.Api.Kafka.Events;
using Confluent.Kafka;
using RabbitMQ.Client;

namespace BankingService.Features.CryptoDeposits.HostedServices;

public class CryptoDepositCreatedConsumerHostedService(
    IConsumer<int, CryptoDepositCreatedEvent> _kafkaConsumer,
    IConnection _rabbitMqConnection,
    ILogger<CryptoDepositCreatedConsumerHostedService> _logger)
    : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _kafkaConsumer.Subscribe("crypto-deposit-created");

        using var model = _rabbitMqConnection.CreateModel();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = _kafkaConsumer.Consume(stoppingToken);

                if (consumeResult.IsPartitionEOF)
                {
                    continue;
                }

                var ev = consumeResult.Message.Value;

                var depositCreatedEvent = new DepositCreatedEvent(ev.Id, ev.UserId, ev.Asset, ev.Amount, ev.CreatedAt);
                var body = JsonSerializer.Serialize(depositCreatedEvent);
                var bodyBytes = Encoding.UTF8.GetBytes(body);

                model.BasicPublish(ExchangeNames.BankingService, RoutingKeys.DepositCreated, null, bodyBytes);

                _logger.LogInformation("Consumed message with key {Key} and tx hash {TransactionHash}",
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
