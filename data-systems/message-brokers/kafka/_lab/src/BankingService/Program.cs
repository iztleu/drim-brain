using BankingService.Features.Deposits.HostedServices;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHostedService<ConsumerHostedService>();

var app = builder.Build();

app.Run();
