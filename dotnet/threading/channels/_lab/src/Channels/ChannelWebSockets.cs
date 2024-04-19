using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Channels;
using Events;

namespace Channels;

public static class ChannelWebSockets
{
    public static async Task Run()
    {
        var userWebSockets = new ConcurrentDictionary<int, Channel<CryptoDepositCreatedEvent>>();

        var eventsChannel = Channel.CreateBounded<CryptoDepositCreatedEvent>(100_000);

        var producerTask = Task.Run(async () =>
        {
            using var reader = new StreamReader(Path.Combine("Files", "events.txt"));

            string? line;
            while (!string.IsNullOrWhiteSpace(line = await reader.ReadLineAsync()))
            {
                var @event = JsonSerializer.Deserialize<CryptoDepositCreatedEvent>(line);
                await eventsChannel.Writer.WriteAsync(@event!);
                Thread.Sleep(500);
            }

            eventsChannel.Writer.Complete();
        });

        var demultiplexerTask = Task.Run(async () =>
        {
            await foreach (var @event in eventsChannel.Reader.ReadAllAsync())
            {
                var userWebSocket = userWebSockets.GetOrAdd(@event.UserId, _ => Channel.CreateUnbounded<CryptoDepositCreatedEvent>());

                await userWebSocket.Writer.WriteAsync(@event);

                Console.WriteLine($"Sent event to the channel of user {@event.UserId}. The channel size is {userWebSocket.Reader.Count}");
            }
        });

        await Task.WhenAll(producerTask, demultiplexerTask);
    }
}
