using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MediatR;

namespace BlockchainService.Features.Deposits.Subscriptions;

public static class SubscribeForNewDeposits
{
    public record Request(IServerStreamWriter<DepositDto> ResponseStream) : IRequest<Unit>;

    internal class RequestHandler : IRequestHandler<Request, Unit>
    {
        private readonly string[] Currencies = [ "BTC", "ETH", "XMR", "BCH" ];

        public async Task<Unit> Handle(Request request, CancellationToken cancellationToken)
        {
            var id = 0;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await request.ResponseStream.WriteAsync(new DepositDto
                    {
                        Id = id++,
                        UserId = Random.Shared.Next(1, 11),
                        AccountNumber = Random.Shared.Next(100000, 999999).ToString(),
                        Currency = Currencies[id % Currencies.Length],
                        Amount = Random.Shared.Next(1, 15),
                        TxId = Guid.NewGuid().ToString(),
                        SourceCryptoAddress = "def",
                        CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow),
                    }, cancellationToken);

                    await Task.Delay(Random.Shared.Next(300, 1000), cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    // Ignore
                }
            }

            return Unit.Value;
        }
    }
}
