using System.Collections.Concurrent;
using System.Threading.Channels;
using NotificationService.Client;

namespace ApiGateway.Features.Notifications.Services;

public class NotificationStream
{
    private readonly Channel<NotificationDto> _publishedNotificationChannel = Channel.CreateUnbounded<NotificationDto>();

    private readonly ConcurrentDictionary<int, Channel<NotificationDto>> _userNotificationChannels = new();

    private readonly Dictionary<int, int> _userConnectionCount = new();

    private readonly object _lock = new();

    public void Start()
    {
        _ = Task.Run(async () =>
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

    public ChannelReader<NotificationDto> GetChannelReader(int userId)
    {
        lock (_lock)
        {
            var channel = _userNotificationChannels.GetOrAdd(userId, _ => Channel.CreateUnbounded<NotificationDto>());

            if (_userConnectionCount.TryGetValue(userId, out var count))
            {
                _userConnectionCount[userId] = count + 1;
            }
            else
            {
                _userConnectionCount[userId] = 1;
            }

            return channel.Reader;
        }
    }

    public void ReleaseChannelReader(int userId)
    {
        lock (_lock)
        {
            if (_userConnectionCount.TryGetValue(userId, out var count))
            {
                if (count == 1)
                {
                    _userConnectionCount.Remove(userId);
                    _userNotificationChannels.TryRemove(userId, out _);
                }
                else
                {
                    _userConnectionCount[userId] = count - 1;
                }
            }
            else
            {
                _userNotificationChannels.TryRemove(userId, out _);
            }
        }
    }

    public async Task Publish(NotificationDto notification, CancellationToken cancellationToken)
    {
        await _publishedNotificationChannel.Writer.WriteAsync(notification, cancellationToken);
    }
}
