using AsyncEnumerables;

var events = new CryptoDepositCreatedEvents(Path.Combine("Files", "events.txt"));

await using var enumerator = events.GetAsyncEnumerator();
while (await enumerator.MoveNextAsync())
{
    var @event = enumerator.Current;
    Console.WriteLine($"User {@event.UserId} deposited {@event.Amount} {@event.Currency} into Account {@event.AccountId}");
}
