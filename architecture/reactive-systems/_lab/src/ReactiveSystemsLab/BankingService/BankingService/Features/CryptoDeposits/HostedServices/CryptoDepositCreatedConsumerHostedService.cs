using System.Text;
using System.Text.Json;
using BankingService.Api.RabbitMq;
using BankingService.Api.RabbitMQ.Events;
using BankingService.Database;
using BankingService.Domain;
using BlockchainService.Api.Kafka;
using BlockchainService.Api.Kafka.Events;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using RabbitMQ.Client;

namespace BankingService.Features.CryptoDeposits.HostedServices;

public class CryptoDepositCreatedConsumerHostedService(
    IServiceProvider _serviceProvider,
    IConsumer<int, CryptoDepositCreatedEvent> _kafkaConsumer,
    IConnection _rabbitMqConnection,
    ILogger<CryptoDepositCreatedConsumerHostedService> _logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _kafkaConsumer.Subscribe(TopicNames.CryptoDepositCreated);

        using var model = _rabbitMqConnection.CreateModel();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                cts.CancelAfter(TimeSpan.FromSeconds(30));

                var consumeResult = _kafkaConsumer.Consume(stoppingToken);

                if (consumeResult.IsPartitionEOF)
                {
                    continue;
                }

                var ev = consumeResult.Message.Value;

                await using var scope = _serviceProvider.CreateAsyncScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<BankingDbContext>();

                var deposit = new Deposit
                {
                    SourceId = ev.Id,
                    UserId = ev.UserId,
                    Asset = ev.Asset,
                    Amount = ev.Amount,
                    CreatedAt = ev.CreatedAt,
                };

                try
                {
                    dbContext.Deposits.Add(deposit);

                    // TODO: update user balance

                    await dbContext.SaveChangesAsync(cts.Token);

                    _kafkaConsumer.Commit(consumeResult);

                    _logger.LogInformation("Deposit with UserId {UserId} SourceId {SourceId} created", ev.UserId, ev.Id);
                }
                catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: "23505" })
                {
                    _logger.LogInformation("Deposit with SourceId {SourceId} already exists", ev.Id);
                    _kafkaConsumer.Commit(consumeResult);
                    continue;
                }

                var depositCreatedEvent = new DepositCreatedEvent(deposit.Id, deposit.UserId, deposit.Asset, deposit.Amount, deposit.CreatedAt);
                var body = JsonSerializer.Serialize(depositCreatedEvent);
                var bodyBytes = Encoding.UTF8.GetBytes(body);

                model.BasicPublish(ExchangeNames.BankingService, RoutingKeys.DepositCreated, null, bodyBytes);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Consumer timed out");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to consume message");
            }
        }
    }
}
