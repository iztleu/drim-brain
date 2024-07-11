using BankingService.Client;
using Common.Web.Endpoints;
using Grpc.Core;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Features.Withdrawals.Requests;

public static class SubscribeForWithdrawals
{
    public class Endpoint : IEndpoint
    {
        private const string Path = "/withdrawals";

        public void MapEndpoint(WebApplication app)
        {
            app.MapPost(Path, async Task<Ok> (
                    RequestBody body,
                    [FromServices] BankingService.Client.Withdrawals.WithdrawalsClient withdrawalsClient,
                    [FromServices] ILogger<Endpoint> logger,
                    CancellationToken cancellationToken) =>
                {
                    var request = new SubscribeForWithdrawalsRequest
                    {
                        UserId = body.UserId,
                    };

                    var serverStream = withdrawalsClient.SubscribeForWithdrawals(request,
                        new CallOptions(cancellationToken: cancellationToken));

                    await foreach (var withdrawalDto in serverStream.ResponseStream.ReadAllAsync(cancellationToken))
                    {
                        logger.LogInformation("Withdrawal received: {Withdrawal}", withdrawalDto);
                    }

                    return TypedResults.Ok();
                })
                .AllowAnonymous();
        }

        private record RequestBody(int UserId);
    }
}
