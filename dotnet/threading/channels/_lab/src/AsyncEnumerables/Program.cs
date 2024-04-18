using AsyncEnumerables;

foreach (var @event in await EagerAsyncStream.GetEvents())
{
    Console.WriteLine($"User {@event.UserId} deposited {@event.Amount} {@event.Currency} into Account {@event.AccountId}");
}
