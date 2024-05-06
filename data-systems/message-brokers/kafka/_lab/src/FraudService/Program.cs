using Confluent.Kafka;
using FraudService.Features.Deposits.HostedServices;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddKafkaClient();

builder.Services.AddHostedService<ConsumerHostedService>();

var app = builder.Build();

app.Run();
