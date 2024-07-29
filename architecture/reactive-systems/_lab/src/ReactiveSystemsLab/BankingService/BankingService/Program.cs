using BankingService.Database;
using BankingService.Features.CryptoDeposits.HostedServices;
using BankingService.Setup;
using BlockchainService.Api.Kafka.Events;
using Common.Kafka;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    // We have to wait for the database migration to execute. This is a temporary solution since .NET Aspire
    // does not support service restarts or startup dependencies yet
    await Task.Delay(5_000);
}

builder.AddServiceDefaults();

builder.AddNpgsqlDbContext<BankingDbContext>("BankingServiceDb");

builder.AddKafkaConsumer<int, CryptoDepositCreatedEvent>("kafka",
    s =>
    {
        s.Config.GroupId = "banking-service";
    },
    cb =>
    {
        cb.SetValueDeserializer(new KafkaJsonDeserializer<CryptoDepositCreatedEvent>());
    });

builder.AddRabbitMQClient("rabbitmq");

builder.Services.AddHostedService<RabbitMqSetupHostedService>();
builder.Services.AddHostedService<CryptoDepositCreatedConsumerHostedService>();

var app = builder.Build();

app.MapDefaultEndpoints();

app.Run();
