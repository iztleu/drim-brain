using System.Threading.Channels;
using ApiGateway.Features.Notifications.Services;
using MediatR;
using NotificationService.Client;

namespace ApiGateway.Features.Notifications.Subscriptions;

public static class SubscribeForNotifications
{
    public record Request(int UserId, CancellationToken UnsubscriptionToken) : IRequest<ChannelReader<NotificationDto>>;

    public class RequestHandler(
        NotificationStream _notificationStream)
        : IRequestHandler<Request, ChannelReader<NotificationDto>>
    {
        public Task<ChannelReader<NotificationDto>> Handle(Request request, CancellationToken cancellationToken)
        {
            var channelReader = _notificationStream.GetChannelReader(request.UserId);

            request.UnsubscriptionToken.Register(() => _notificationStream.ReleaseChannelReader(request.UserId));

            return Task.FromResult(channelReader);
        }
    }
}
