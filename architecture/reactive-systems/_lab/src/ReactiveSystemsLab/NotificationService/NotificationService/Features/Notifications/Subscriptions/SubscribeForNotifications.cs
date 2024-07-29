using Grpc.Core;
using MediatR;
using NotificationService.Features.Notifications.Services;

namespace NotificationService.Features.Notifications.Subscriptions;

public static class SubscribeForNotifications
{
    public record Request(IServerStreamWriter<NotificationDto> ResponseStream) : IRequest<Unit>;

    internal class RequestHandler(
        NotificationStream _notificationStream)
        : IRequestHandler<Request, Unit>
    {
        public async Task<Unit> Handle(Request request, CancellationToken cancellationToken)
        {
            var clientId = Guid.NewGuid().ToString();

            var channelReader = _notificationStream.Subscribe(clientId);

            try
            {
                await foreach (var notification in channelReader.ReadAllAsync(cancellationToken))
                {
                    await request.ResponseStream.WriteAsync(notification, cancellationToken);
                }
            }
            finally
            {
                _notificationStream.Unsubscribe(clientId);
            }

            return Unit.Value;
        }
    }
}
