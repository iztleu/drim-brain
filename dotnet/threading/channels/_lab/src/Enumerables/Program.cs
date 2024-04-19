using Enumerables;

var events = new CryptoDepositCreatedEvents(Path.Combine("Files", "events.txt"));

using var enumerator = events.GetEnumerator();
while (enumerator.MoveNext())
{
    var @event = enumerator.Current;
    Console.WriteLine($"User {@event.UserId} deposited {@event.Amount} {@event.Currency} into Account {@event.AccountId}");
}
