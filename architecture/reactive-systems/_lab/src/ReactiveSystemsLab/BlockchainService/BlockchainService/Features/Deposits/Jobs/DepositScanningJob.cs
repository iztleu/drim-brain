using Quartz;

namespace BlockchainService.Features.Deposits.Jobs;

public class DepositScanningJob(ILogger<DepositScanningJob> _logger) : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("DepositScanningJob executed");

        return Task.CompletedTask;
    }
}
