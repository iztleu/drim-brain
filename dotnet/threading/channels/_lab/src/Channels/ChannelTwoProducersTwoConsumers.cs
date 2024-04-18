using System.Text.Json;
using System.Threading.Channels;
using Events;

namespace Channels;

public static class ChannelTwoProducersTwoConsumers
{
    public static async Task Run()
    {
        var channel = Channel.CreateUnbounded<CryptoDepositCreatedEvent>();

        var producer1 = Task.Run(async () =>
        {
            using var reader = new StreamReader(Path.Combine("Files", "events.txt"));

            string? line;
            while (!string.IsNullOrWhiteSpace(line = await reader.ReadLineAsync()))
            {
                var @event = JsonSerializer.Deserialize<CryptoDepositCreatedEvent>(line);
                await channel.Writer.WriteAsync(@event!);
                Thread.Sleep(500);
            }

            Console.WriteLine("Task completed");
        });

        var consumerTask = Task.Run(async () =>
        {
            await foreach (var @event in channel.Reader.ReadAllAsync())
            {
                Console.WriteLine($"User {@event.UserId} deposited {@event.Amount} {@event.Currency} into Account {@event.AccountId}");
            }
        });

        await Task.Delay(5000);
    }
}
