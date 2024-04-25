using BlockchainService.Features.Deposits.Subscriptions;
using Grpc.Core;
using MediatR;

namespace BlockchainService.Features.Deposits;

public class DepositsApi(IMediator _mediator) : BlockchainService.Deposits.DepositsBase
{
    public override Task SubscribeForNewDeposits(SubscribeForNewDepositsRequest request,
        IServerStreamWriter<DepositDto> responseStream, ServerCallContext context) =>
        _mediator.Send(new SubscribeForNewDeposits.Request(responseStream), context.CancellationToken);
}
