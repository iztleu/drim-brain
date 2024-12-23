using System.Reflection;
using NotificationService.Database;
using NotificationService.Features.Deposits.HostedServices;
using NotificationService.Features.Notifications;
using NotificationService.Features.Notifications.Services;
using NotificationService.Setup;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddNpgsqlDbContext<NotificationDbContext>("NotificationServiceDb");

builder.AddRabbitMQClient("rabbitmq",
    settings =>
    {
    },
    factory =>
    {
        factory.DispatchConsumersAsync = true;
    });

builder.Services.AddMediatR(cfg => cfg
    .RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

builder.Services.AddGrpc();

builder.Services.AddSingleton<NotificationStream>();

builder.Services.AddHostedService<RabbitMqSetupHostedService>();
builder.Services.AddHostedService<DepositCreatedConsumerHostedService>();

var app = builder.Build();

app.MapGrpcService<NotificationApi>();

app.MapDefaultEndpoints();

app.Run();
