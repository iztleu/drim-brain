using Enumerables;

foreach (var @event in EagerStream.GetEvents())
{
    Console.WriteLine($"User {@event.UserId} deposited {@event.Amount} {@event.Currency} into Account {@event.AccountId}");
}

// using var enumerator = events.GetEnumerator();
// while (enumerator.MoveNext())
// {
//     var @event = enumerator.Current;
//     Console.WriteLine($"User {@event.UserId} deposited {@event.Amount} {@event.Currency} into Account {@event.AccountId}");
// }
