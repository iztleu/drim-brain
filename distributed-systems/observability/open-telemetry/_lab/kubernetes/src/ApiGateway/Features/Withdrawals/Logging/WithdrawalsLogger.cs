using ApiGateway.Features.Withdrawals.Models;
using Common.Telemetry;

namespace ApiGateway.Features.Withdrawals.Logging;

public static partial class WithdrawalsLogger
{
    [LoggerMessage(
        EventId = LogEventIds.WithdrawalCreatedInApiGateway,
        Level = LogLevel.Information,
        Message = "Withdrawal created: {Withdrawal}")]
    public static partial void WithdrawalCreated(ILogger logger, WithdrawalModel? withdrawal);
}
