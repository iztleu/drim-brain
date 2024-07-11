using System.Text.Json;
using Events;

namespace Enumerables;

public static class LazyStream
{
    public static IEnumerable<CryptoDepositCreatedEvent> GetEvents()
    {
        using var reader = new StreamReader(Path.Combine("Files", "events.txt"));

        string? line;
        while (!string.IsNullOrWhiteSpace(line = reader.ReadLine()))
        {
            var @event = JsonSerializer.Deserialize<CryptoDepositCreatedEvent>(line);
            yield return @event!;
            Thread.Sleep(500);
        }
    }
}
