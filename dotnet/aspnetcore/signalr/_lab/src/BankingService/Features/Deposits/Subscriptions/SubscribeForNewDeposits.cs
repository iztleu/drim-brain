using BankingService.Features.Deposits.Services;
using FluentValidation;
using Grpc.Core;
using MediatR;

namespace BankingService.Features.Deposits.Subscriptions;

public static class SubscribeForNewDeposits
{
    public record Request(int UserId, IServerStreamWriter<DepositDto> ResponseStream) : IRequest<Unit>;

    internal class RequestValidator : AbstractValidator<SubscribeForNewDepositsRequest>
    {
        public RequestValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
        }
    }

    internal class RequestHandler(DepositsStream _depositsStream) : IRequestHandler<Request, Unit>
    {
        public async Task<Unit> Handle(Request request, CancellationToken cancellationToken)
        {
            await foreach (var deposit in _depositsStream.Subscribe(request.UserId).ReadAllAsync(cancellationToken))
            {
                await request.ResponseStream.WriteAsync(Map(deposit), cancellationToken);
            }

            return Unit.Value;

            static DepositDto Map(BlockchainService.Client.DepositDto dto) =>
                new()
                {
                    Id = dto.Id,
                    UserId = dto.UserId,
                    AccountNumber = dto.AccountNumber,
                    Currency = dto.Currency,
                    Amount = dto.Amount,
                    TxId = dto.TxId,
                    SourceCryptoAddress = dto.SourceCryptoAddress,
                    CreatedAt = dto.CreatedAt,
                };
        }
    }
}
