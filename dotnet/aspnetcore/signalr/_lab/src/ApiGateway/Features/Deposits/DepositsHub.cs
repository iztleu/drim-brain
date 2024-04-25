using System.Threading.Channels;
using ApiGateway.Features.Deposits.Subscriptions;
using BankingService.Client;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace ApiGateway.Features.Deposits;

public class DepositsHub(IMediator _mediator) : Hub
{
    public Task<ChannelReader<DepositDto>> SubscribeForNewDeposits(int userId, CancellationToken cancellationToken) =>
        _mediator.Send(new SubscribeForNewDeposits.Request(userId, cancellationToken), cancellationToken);
}
