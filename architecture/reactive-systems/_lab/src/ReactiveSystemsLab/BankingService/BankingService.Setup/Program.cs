using BankingService.Database;
using BankingService.Setup;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource(SetupWorker.ActivitySourceName));

builder.AddNpgsqlDbContext<BankingDbContext>("BankingServiceDb");

builder.Services.AddHostedService<SetupWorker>();

var host = builder.Build();

host.Run();
