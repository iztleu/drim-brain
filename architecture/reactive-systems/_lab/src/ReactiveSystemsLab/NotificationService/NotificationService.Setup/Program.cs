using NotificationService.Database;
using NotificationService.Setup;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource(SetupWorker.ActivitySourceName));

builder.AddNpgsqlDbContext<NotificationDbContext>("NotificationServiceDb");

builder.Services.AddHostedService<SetupWorker>();

var host = builder.Build();

host.Run();
