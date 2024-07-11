using Common.Telemetry;

namespace BankingService.Features.Withdrawals.Logging;

public static partial class WithdrawalsLogger
{
    [LoggerMessage(
        EventId = LogEventIds.WithdrawalCreatedInBankingService,
        Level = LogLevel.Information,
        Message = "Withdrawal created")]
    public static partial void WithdrawalCreated(ILogger logger);
}
