using System.Collections.Concurrent;
using System.Threading.Channels;
using NotificationService.Client;

namespace ApiGateway.Features.Notifications.Services;

public class NotificationStream
{
    private readonly Channel<NotificationDto> _publishedNotificationChannel = Channel.CreateUnbounded<NotificationDto>();

    private readonly ConcurrentDictionary<int, Channel<NotificationDto>> _userNotificationChannels = new();
    private Task? _task;

    public void Start()
    {
        _task = Task.Run(async () =>
        {
            await foreach (var notification in _publishedNotificationChannel.Reader.ReadAllAsync())
            {
                if (_userNotificationChannels.TryGetValue(notification.UserId, out var channel))
                {
                    await channel.Writer.WriteAsync(notification);
                }
            }
        });
    }

    public void Stop()
    {
        _task?.Dispose();
    }

    public ChannelReader<NotificationDto> GetChannelReader(int userId)
    {
        var channel = _userNotificationChannels.GetOrAdd(userId, _ => Channel.CreateUnbounded<NotificationDto>());
        return channel.Reader;
    }

    public async Task Publish(NotificationDto notification, CancellationToken cancellationToken)
    {
        await _publishedNotificationChannel.Writer.WriteAsync(notification, cancellationToken);
    }
}
