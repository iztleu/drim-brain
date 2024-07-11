using BankingService.Features.Deposits.Subscriptions;
using Grpc.Core;
using MediatR;

namespace BankingService.Features.Deposits;

public class DepositsApi(IMediator _mediator) : BankingService.Deposits.DepositsBase
{
    public override Task SubscribeForNewDeposits(SubscribeForNewDepositsRequest request,
        IServerStreamWriter<DepositDto> responseStream, ServerCallContext context) =>
        _mediator.Send(new SubscribeForNewDeposits.Request(request.UserId, responseStream), context.CancellationToken);
}
