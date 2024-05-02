using NotificationService.Features.Deposits.HostedServices;
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

builder.Services.AddHostedService<ConsumerHostedService>();

var app = builder.Build();

app.Run();
