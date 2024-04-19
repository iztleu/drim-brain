using System.Text.Json;
using System.Threading.Channels;
using Events;

namespace Channels;

public static class ChannelTwoProducersTwoConsumers
{
    public static async Task Run()
    {
        var channel = Channel.CreateUnbounded<CryptoDepositCreatedEvent>();

        var producer1Task = Task.Run(async () =>
        {
            using var reader = new StreamReader(Path.Combine("Files", "events.txt"));

            string? line;
            while (!string.IsNullOrWhiteSpace(line = await reader.ReadLineAsync()))
            {
                var @event = JsonSerializer.Deserialize<CryptoDepositCreatedEvent>(line);
                await channel.Writer.WriteAsync(@event!);
                Thread.Sleep(500);
            }

            Console.WriteLine("Producer 1 task completed");
        });

        var producer2Task = Task.Run(async () =>
        {
            using var reader = new StreamReader(Path.Combine("Files", "other-events.txt"));

            string? line;
            while (!string.IsNullOrWhiteSpace(line = await reader.ReadLineAsync()))
            {
                var @event = JsonSerializer.Deserialize<CryptoDepositCreatedEvent>(line);
                await channel.Writer.WriteAsync(@event!);
                Thread.Sleep(500);
            }

            Console.WriteLine("Producer 2 task completed");
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

        await Task.WhenAll(producer1Task, producer2Task);

        channel.Writer.Complete();

        await Task.WhenAll(consumer1Task, consumer2Task);
    }
}
