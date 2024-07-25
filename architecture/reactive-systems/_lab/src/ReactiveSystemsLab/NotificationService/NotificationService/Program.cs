using NotificationService.Database;
using NotificationService.Features.Deposits.HostedServices;
using NotificationService.Setup;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddNpgsqlDbContext<NotificationDbContext>("NotificationServiceDb");

builder.AddRabbitMQClient("rabbitmq");

builder.Services.AddHostedService<RabbitMqSetupHostedService>();
builder.Services.AddHostedService<DepositCreatedConsumerHostedService>();

var app = builder.Build();

app.MapDefaultEndpoints();

app.Run();
