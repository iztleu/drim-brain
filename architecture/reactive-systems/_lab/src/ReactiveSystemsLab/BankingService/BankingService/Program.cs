using BankingService.Database;
using BankingService.Features.CryptoDeposits.HostedServices;
using BankingService.Setup;
using BlockchainService.Api.Kafka.Events;
using Common.Kafka;

var builder = WebApplication.CreateBuilder(args);

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
