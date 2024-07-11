using System.Text.Json;
using Events;

namespace AsyncEnumerables;

public static class EagerAsyncStream
{
    public static async Task<IEnumerable<CryptoDepositCreatedEvent>> GetEvents()
    {
        using var reader = new StreamReader(Path.Combine("Files", "events.txt"));

        var events = new List<CryptoDepositCreatedEvent>();

        string? line;
        while (!string.IsNullOrWhiteSpace(line = await reader.ReadLineAsync()))
        {
            var @event = JsonSerializer.Deserialize<CryptoDepositCreatedEvent>(line);
            events.Add(@event!);
            await Task.Delay(500);
        }

        return events;
    }
}
