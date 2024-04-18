using System.Text.Json;
using Events;

namespace Enumerables;

public static class EagerStream
{
    public static IEnumerable<CryptoDepositCreatedEvent> GetEvents()
    {
        using var reader = new StreamReader(Path.Combine("Files", "events.txt"));

        var events = new List<CryptoDepositCreatedEvent>();

        string? line;
        while (!string.IsNullOrWhiteSpace(line = reader.ReadLine()))
        {
            var @event = JsonSerializer.Deserialize<CryptoDepositCreatedEvent>(line);
            events.Add(@event!);
            Thread.Sleep(500);
        }

        return events;
    }
}
