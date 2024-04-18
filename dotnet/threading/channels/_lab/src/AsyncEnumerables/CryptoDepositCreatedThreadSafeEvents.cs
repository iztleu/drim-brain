using System.Text.Json;
using Events;

namespace AsyncEnumerables;

public class CryptoDepositCreatedThreadSafeEvents(string _path) : IAsyncEnumerable<CryptoDepositCreatedEvent>
{
    public IAsyncEnumerator<CryptoDepositCreatedEvent> GetAsyncEnumerator(CancellationToken cancellationToken = new())
    {
        return new CryptoDepositCreatedThreadSafeEventsEnumerator(_path);
    }
}

public class CryptoDepositCreatedThreadSafeEventsEnumerator(string _path) : IAsyncEnumerator<CryptoDepositCreatedEvent>
{
    private readonly StreamReader _reader = new(_path);
    private CryptoDepositCreatedEvent? _current;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public CryptoDepositCreatedEvent? Current
    {
        get
        {
            _semaphore.Wait();
            try
            {
                return _current;
            }
            finally
            {
                _semaphore.Release();
            }
        }
        private set => _current = value;
    }

    public async ValueTask<bool> MoveNextAsync()
    {
        await _semaphore.WaitAsync();
        try
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
        finally
        {
            _semaphore.Release();
        }
    }

    public void Reset()
    {
        _semaphore.Wait();
        try
        {
            _reader.BaseStream.Position = 0;
            _reader.DiscardBufferedData();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public ValueTask DisposeAsync()
    {
        _reader.Dispose();
        return ValueTask.CompletedTask;
    }
}
