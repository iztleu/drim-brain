using System.Text.Json;
using Events;

namespace AsyncEnumerables;

public class CryptoDepositCreatedEvents(string _path) : IAsyncEnumerable<CryptoDepositCreatedEvent>
{
    public IAsyncEnumerator<CryptoDepositCreatedEvent> GetAsyncEnumerator(CancellationToken cancellationToken = new())
    {
        return new CryptoDepositCreatedEventsEnumerator(_path);
    }
}

public class CryptoDepositCreatedEventsEnumerator(string _path) : IAsyncEnumerator<CryptoDepositCreatedEvent>
{
    private readonly StreamReader _reader = new(_path);

    public CryptoDepositCreatedEvent? Current { get; private set; }

    public async ValueTask<bool> MoveNextAsync()
    {
        await Task.Delay(500);

        if (_reader.EndOfStream)
        {
            return false;
        }

        var line = await _reader.ReadLineAsync();

        if (string.IsNullOrWhiteSpace(line))
        {
            return false;
        }

        Current = JsonSerializer.Deserialize<CryptoDepositCreatedEvent>(line);
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
