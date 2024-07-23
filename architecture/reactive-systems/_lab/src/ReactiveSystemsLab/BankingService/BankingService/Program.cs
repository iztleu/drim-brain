using Aspire.Confluent.Kafka;
using BankingService.Database;
using BankingService.Features.CryptoDeposits.HostedServices;
using BlockchainService.Api;
using Common.Kafka;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddNpgsqlDbContext<BankingDbContext>("BankingServiceDb");

builder.AddKafkaConsumer<int, CryptoDepositCreatedEvent>("kafka", (KafkaConsumerSettings settings) =>
{
    settings.Config.GroupId = "banking-service";
}, consumerBuilder =>
{
    consumerBuilder.SetValueDeserializer(new KafkaJsonDeserializer<CryptoDepositCreatedEvent>());
});

builder.Services.AddHostedService<CryptoDepositConsumerHostedService>();

var app = builder.Build();

app.MapDefaultEndpoints();

app.Run();
