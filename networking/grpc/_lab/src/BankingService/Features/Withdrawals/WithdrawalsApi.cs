using BankingService.Features.Withdrawals.Requests;
using Grpc.Core;
using MediatR;

namespace BankingService.Features.Withdrawals;

public class WithdrawalsApi(IMediator _mediator) : BankingService.Withdrawals.WithdrawalsBase
{
    public override Task<CreateWithdrawalReply> CreateWithdrawal(CreateWithdrawalRequest request, ServerCallContext context) =>
        _mediator.Send(request, context.CancellationToken);

    public override Task SubscribeForWithdrawals(SubscribeForWithdrawalsRequest request, IServerStreamWriter<WithdrawalDto> responseStream,
        ServerCallContext context) =>
        _mediator.Send(new SubscribeForWithdrawals.Request(request.UserId, responseStream), context.CancellationToken);
}
