using FluentValidation;
using Grpc.Core;
using MediatR;

namespace BankingService.Features.Withdrawals.Requests;

public static class SubscribeForWithdrawals
{
    public record Request(int UserId, IServerStreamWriter<WithdrawalDto> ResponseStream) : IRequest<Unit>;

    internal class RequestValidator : AbstractValidator<SubscribeForWithdrawalsRequest>
    {
        public RequestValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
        }
    }

    internal class RequestHandler : IRequestHandler<Request, Unit>
    {
        public async Task<Unit> Handle(Request request, CancellationToken cancellationToken)
        {
            for (var i = 0; i < 20; i++)
            {
                await request.ResponseStream.WriteAsync(new WithdrawalDto(), cancellationToken);
                await Task.Delay(1000, cancellationToken);
            }

            return Unit.Value;
        }
    }
}
