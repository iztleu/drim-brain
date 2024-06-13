using Common.Telemetry;

namespace BlockchainService.Features.Withdrawals.Logging;

public static partial class WithdrawalsLogger
{
    [LoggerMessage(
        EventId = LogEventIds.FeeEstimated,
        Level = LogLevel.Information,
        Message = "Fee estimated")]
    public static partial void FeeEstimated(ILogger logger);

    [LoggerMessage(
        EventId = LogEventIds.TransactionSent,
        Level = LogLevel.Information,
        Message = "Transaction sent")]
    public static partial void TransactionSent(ILogger logger);
}
