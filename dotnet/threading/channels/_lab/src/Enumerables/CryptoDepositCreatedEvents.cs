using System.Collections;
using System.Text.Json;
using Events;

namespace Enumerables;

public class CryptoDepositCreatedEvents(string _path) : IEnumerable<CryptoDepositCreatedEvent>
{
    public IEnumerator<CryptoDepositCreatedEvent> GetEnumerator()
    {
        return new CryptoDepositCreatedEventsEnumerator(_path);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public class CryptoDepositCreatedEventsEnumerator(string _path) : IEnumerator<CryptoDepositCreatedEvent>
{
    private readonly StreamReader _reader = new(_path);

    public CryptoDepositCreatedEvent? Current { get; private set; }

    object? IEnumerator.Current => Current;

    public bool MoveNext()
    {
        Thread.Sleep(500);

        if (_reader.EndOfStream)
        {
            return false;
        }

        var line = _reader.ReadLine();

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

    public void Dispose()
    {
        _reader.Dispose();
    }
}
