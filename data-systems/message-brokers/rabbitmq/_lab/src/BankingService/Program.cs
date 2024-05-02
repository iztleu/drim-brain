using BankingService.Features.Deposits.HostedServices;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(_ =>
{
    var factory = new ConnectionFactory
    {
        HostName = "localhost",
        DispatchConsumersAsync = true,
    };
    return factory.CreateConnection();
});

// builder.Services.AddHostedService<PublisherHostedService>();
// builder.Services.AddHostedService<FanoutPublisherHostedService>();
builder.Services.AddHostedService<DispatcherPublisherHostedService>();

var app = builder.Build();

app.Run();
