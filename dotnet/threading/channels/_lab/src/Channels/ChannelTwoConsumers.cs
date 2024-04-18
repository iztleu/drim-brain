using System.Text.Json;
using System.Threading.Channels;
using Events;

namespace Channels;

public static class ChannelTwoConsumers
{
    public static async Task Run()
    {
        var channel = Channel.CreateUnbounded<CryptoDepositCreatedEvent>();

        var producerTask = Task.Run(async () =>
        {
            using var reader = new StreamReader(Path.Combine("Files", "events.txt"));

            string? line;
            while (!string.IsNullOrWhiteSpace(line = await reader.ReadLineAsync()))
            {
                var @event = JsonSerializer.Deserialize<CryptoDepositCreatedEvent>(line);
                await channel.Writer.WriteAsync(@event!);
                Thread.Sleep(500);
            }

            channel.Writer.Complete();

            Console.WriteLine("Producer task completed");
        });

        var consumer1Task = Task.Run(async () =>
        {
            await foreach (var @event in channel.Reader.ReadAllAsync())
            {
                Console.WriteLine($"Consumer 1: User {@event.UserId} deposited {@event.Amount} {@event.Currency} into Account {@event.AccountId}");
            }

            Console.WriteLine("Consumer 1 task completed");
        });

        var consumer2Task = Task.Run(async () =>
        {
            await foreach (var @event in channel.Reader.ReadAllAsync())
            {
                Console.WriteLine($"Consumer 2: User {@event.UserId} deposited {@event.Amount} {@event.Currency} into Account {@event.AccountId}");
            }

            Console.WriteLine("Consumer 2 task completed");
        });

        await Task.WhenAll(producerTask, consumer1Task, consumer2Task);
    }
}
