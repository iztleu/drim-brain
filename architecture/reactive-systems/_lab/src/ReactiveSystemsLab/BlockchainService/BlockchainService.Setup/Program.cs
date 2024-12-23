using BlockchainService.Database;
using BlockchainService.Setup;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource(SetupWorker.ActivitySourceName));

builder.AddNpgsqlDbContext<BlockchainDbContext>("BlockchainServiceDb");

builder.Services.AddHostedService<SetupWorker>();

var host = builder.Build();

host.Run();
