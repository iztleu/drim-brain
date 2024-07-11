using System.Text.Json;
using Events;

namespace AsyncEnumerables;

public class CryptoDepositCreatedBatchedEvents(string _path) : IAsyncEnumerable<CryptoDepositCreatedEvent>
{
    public IAsyncEnumerator<CryptoDepositCreatedEvent> GetAsyncEnumerator(CancellationToken cancellationToken = new())
    {
        return new CryptoDepositCreatedBatchedEventsEnumerator(_path);
    }
}

public class CryptoDepositCreatedBatchedEventsEnumerator(string _path) : IAsyncEnumerator<CryptoDepositCreatedEvent>
{
    private const int BatchSize = 10;

    private readonly StreamReader _reader = new(_path);
    private readonly Queue<CryptoDepositCreatedEvent> _queue = new();

    public CryptoDepositCreatedEvent? Current { get; private set; }

    public async ValueTask<bool> MoveNextAsync()
    {
        if (_queue.TryDequeue(out var @event))
        {
            Current = @event;
            return true;
        }

        await Task.Delay(500);

        if (_reader.EndOfStream)
        {
            return false;
        }

        for (var i = 0; i < BatchSize; i++)
        {
            var line = await _reader.ReadLineAsync();

            if (string.IsNullOrWhiteSpace(line))
            {
                break;
            }

            @event = JsonSerializer.Deserialize<CryptoDepositCreatedEvent>(line);
            _queue.Enqueue(@event!);
        }

        Current = _queue.Dequeue();
        return true;
    }

    public void Reset()
    {
        _reader.BaseStream.Position = 0;
        _reader.DiscardBufferedData();
    }

    public ValueTask DisposeAsync()
    {
        _reader.Dispose();
        return ValueTask.CompletedTask;
    }
}
