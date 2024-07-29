using Grpc.Core;
using MediatR;
using NotificationService.Features.Notifications.Services;

namespace NotificationService.Features.Notifications.Subscriptions;

public static class SubscribeForNotifications
{
    public record Request(IServerStreamWriter<NotificationDto> ResponseStream) : IRequest<Unit>;

    internal class RequestHandler(
        NotificationStream _notificationStream,
        ILogger<RequestHandler> _logger) : IRequestHandler<Request, Unit>
    {
        public async Task<Unit> Handle(Request request, CancellationToken cancellationToken)
        {
            await foreach (var notification in _notificationStream.Subscribe().ReadAllAsync(cancellationToken))
            {
                await request.ResponseStream.WriteAsync(notification, cancellationToken);
            }

            return Unit.Value;
        }
    }
}
