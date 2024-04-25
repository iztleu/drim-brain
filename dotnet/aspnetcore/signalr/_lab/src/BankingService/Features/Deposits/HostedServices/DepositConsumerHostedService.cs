using BankingService.Features.Deposits.Services;
using Grpc.Core;

namespace BankingService.Features.Deposits.HostedServices;

public class DepositConsumerHostedService(
    BlockchainService.Client.Deposits.DepositsClient _depositsClient,
    DepositsStream _depositsStream,
    ILogger<DepositConsumerHostedService> _logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var call = _depositsClient.SubscribeForNewDeposits(
                    new BlockchainService.Client.SubscribeForNewDepositsRequest(),
                    cancellationToken: stoppingToken);

                await foreach (var deposit in call.ResponseStream.ReadAllAsync(stoppingToken))
                {
                    _logger.LogInformation("Saved deposit in DB {@Deposit}", deposit);

                    await _depositsStream.PublishDeposit(deposit);
                }
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
            {
                _logger.LogInformation("Subscription was cancelled");
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Error while consuming deposits. Retrying in 5 seconds");
                await Task.Delay(5000, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while consuming deposits");
            }
        }
    }
}
