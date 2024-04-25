using System.Text.Json;
using BankingService.Client;
using Microsoft.AspNetCore.SignalR.Client;

var cts = new CancellationTokenSource();

Console.CancelKeyPress += (sender, args) =>
{
    args.Cancel = true;
    cts.Cancel();
};

var connection = new HubConnectionBuilder()
    .WithUrl("http://localhost:5290/depositsHub")
    .Build();

connection.Closed += async (error) =>
{
    await Task.Delay(new Random().Next(0,5) * 1000);
    await connection.StartAsync(cts.Token);
};

await connection.StartAsync(cts.Token);

var userId = Random.Shared.Next(1, 11);

var channel = await connection.StreamAsChannelAsync<DepositDto>(
    "SubscribeForNewDeposits", userId, cancellationToken: cts.Token);

await foreach (var dto in channel.ReadAllAsync(cts.Token))
{
    Console.WriteLine(JsonSerializer.Serialize(dto));
}

Console.WriteLine("Streaming completed");
