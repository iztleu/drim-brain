using System.Threading.Channels;
using ApiGateway.Features.Notifications.Subscriptions;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Client;

namespace ApiGateway.Features.Notifications;

public class NotificationHub(IMediator _mediator) : Hub
{
    public Task<ChannelReader<NotificationDto>> SubscribeForNotifications(int userId, CancellationToken cancellationToken) =>
        _mediator.Send(new SubscribeForNotifications.Request(userId, cancellationToken), cancellationToken);
}
