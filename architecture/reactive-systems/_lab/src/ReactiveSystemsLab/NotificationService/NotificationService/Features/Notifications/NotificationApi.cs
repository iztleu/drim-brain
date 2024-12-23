using Grpc.Core;
using MediatR;
using NotificationService.Features.Notifications.Subscriptions;

namespace NotificationService.Features.Notifications;

public class NotificationApi(IMediator _mediator) : NotificationService.Notifications.NotificationsBase
{
    public override Task SubscribeForNotifications(SubscribeForNotificationsRequest request,
        IServerStreamWriter<NotificationDto> responseStream, ServerCallContext context) =>
        _mediator.Send(new SubscribeForNotifications.Request(responseStream), context.CancellationToken);
}
