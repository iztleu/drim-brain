using System.Collections.Concurrent;
using System.Threading.Channels;

namespace NotificationService.Features.Notifications.Services;

public class NotificationStream
{
    private readonly Channel<NotificationDto> _publishedNotificationChannel = Channel.CreateUnbounded<NotificationDto>();

    private readonly ConcurrentDictionary<string, Channel<NotificationDto>> _clientChannels = new();

    public void Start()
    {
        _ = Task.Run(async () =>
        {
            await foreach (var notification in _publishedNotificationChannel.Reader.ReadAllAsync())
            {
                foreach (var clientChannel in _clientChannels.Values)
                {
                    await clientChannel.Writer.WriteAsync(notification);
                }
            }
        });
    }

    public void Publish(NotificationDto notification)
    {
        _publishedNotificationChannel.Writer.TryWrite(notification);
    }

    public ChannelReader<NotificationDto> Subscribe(string clientId)
    {
        var channel = Channel.CreateUnbounded<NotificationDto>();

        _clientChannels.TryAdd(clientId, channel);

        return channel.Reader;
    }

    public void Unsubscribe(string clientId)
    {
        _clientChannels.TryRemove(clientId, out _);
    }
}
