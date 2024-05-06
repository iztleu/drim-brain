using BlockchainService.Features.Deposits.HostedServices;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHostedService<PublisherHostedService>();

var app = builder.Build();

app.Run();
