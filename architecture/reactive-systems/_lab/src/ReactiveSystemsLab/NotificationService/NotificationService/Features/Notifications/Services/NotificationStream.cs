using System.Threading.Channels;

namespace NotificationService.Features.Notifications.Services;

public class NotificationStream
{
    private readonly Channel<NotificationDto> _publishedNotificationChannel = Channel.CreateUnbounded<NotificationDto>();

    public void Publish(NotificationDto notification)
    {
        _publishedNotificationChannel.Writer.TryWrite(notification);
    }

    public ChannelReader<NotificationDto> Subscribe()
    {
        return _publishedNotificationChannel.Reader;
    }
}
