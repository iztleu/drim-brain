using System.Threading.Channels;
using BankingService.Client;
using Grpc.Core;
using MediatR;

namespace ApiGateway.Features.Deposits.Subscriptions;

public static class SubscribeForNewDeposits
{
    public record Request(int UserId, CancellationToken UnsubscriptionToken) : IRequest<ChannelReader<DepositDto>>;

    public class RequestHandler(BankingService.Client.Deposits.DepositsClient _depositsClient) : IRequestHandler<Request, ChannelReader<DepositDto>>
    {
        public Task<ChannelReader<DepositDto>> Handle(Request request, CancellationToken cancellationToken)
        {
            var channel = Channel.CreateUnbounded<DepositDto>();

            var subRequest = new SubscribeForNewDepositsRequest { UserId = request.UserId };
            var call = _depositsClient.SubscribeForNewDeposits(subRequest, cancellationToken: cancellationToken);

            _ = Task.Run(async () =>
            {
                try
                {
                    await foreach (var deposit in call.ResponseStream.ReadAllAsync(request.UnsubscriptionToken))
                    {
                        await channel.Writer.WriteAsync(deposit, request.UnsubscriptionToken);
                    }

                    channel.Writer.TryComplete();
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
                {
                    channel.Writer.TryComplete(ex);
                }
                catch (Exception ex)
                {
                    channel.Writer.TryComplete(ex);
                }
            }, request.UnsubscriptionToken);

            request.UnsubscriptionToken.Register(() => call.Dispose());

            return Task.FromResult(channel.Reader);
        }
    }
}
