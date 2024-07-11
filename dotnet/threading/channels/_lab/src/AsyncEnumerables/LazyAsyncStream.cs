using System.Text.Json;
using Events;

namespace AsyncEnumerables;

public static class LazyAsyncStream
{
    public static async IAsyncEnumerable<CryptoDepositCreatedEvent> GetEventsLazy()
    {
        using var reader = new StreamReader(Path.Combine("Files", "events.txt"));

        string? line;
        while (!string.IsNullOrWhiteSpace(line = await reader.ReadLineAsync()))
        {
            var @event = JsonSerializer.Deserialize<CryptoDepositCreatedEvent>(line);
            yield return @event!;
            await Task.Delay(500);
        }
    }
}
