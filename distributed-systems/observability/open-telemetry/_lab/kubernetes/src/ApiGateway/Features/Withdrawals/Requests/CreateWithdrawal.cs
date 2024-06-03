using ApiGateway.Common.Metrics;
using ApiGateway.Features.Withdrawals.Metrics;
using ApiGateway.Features.Withdrawals.Models;
using BankingService.Client;
using Common.Web.Endpoints;
using Grpc.Core;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Features.Withdrawals.Requests;

public static class CreateWithdrawal
{
    public class Endpoint : IEndpoint
    {
        private const string Path = "/withdrawals";

        public void MapEndpoint(WebApplication app)
        {
            app.MapPost(Path, async Task<Created<WithdrawalModel>> (
                    RequestBody body,
                    [FromServices] BankingService.Client.Withdrawals.WithdrawalsClient withdrawalsClient,
                    [FromServices] RequestMetrics requestMetrics,
                    [FromServices] WithdrawalsMetrics withdrawalsMetrics,
                    [FromServices] ILogger<Endpoint> logger,
                    CancellationToken cancellationToken) =>
                    await requestMetrics.Measure("CreateWithdrawal", async () =>
                    {
                        var request = new CreateWithdrawalRequest
                        {
                            UserId = body.UserId,
                            AccountNumber = body.AccountNumber,
                            Currency = body.Currency,
                            Amount = body.Amount,
                            CryptoAddress = body.CryptoAddress,
                        };

                        var reply = await withdrawalsClient.CreateWithdrawalAsync(request,
                            new CallOptions(cancellationToken: cancellationToken));

                        var withdrawalModel = MapFrom(reply.Withdrawal);

                        withdrawalsMetrics.WithdrawalsCreated(1);

                        logger.LogInformation("Withdrawal created: {Withdrawal}", withdrawalModel);

                        return TypedResults.Created($"{Path}/{withdrawalModel.Id}", withdrawalModel);
                    }))
                .AllowAnonymous();

            static WithdrawalModel MapFrom(WithdrawalDto withdrawal) => new(withdrawal.Id, withdrawal.AccountNumber,
                withdrawal.Currency, withdrawal.Amount, withdrawal.CryptoAddress, withdrawal.TxId,
                withdrawal.CreatedAt.ToDateTime());
        }

        // TODO: get UserId from JWT
        // TODO: use decimal for Amount
        private record RequestBody(int UserId, string AccountNumber, string Currency, double Amount, string CryptoAddress);
    }
}
