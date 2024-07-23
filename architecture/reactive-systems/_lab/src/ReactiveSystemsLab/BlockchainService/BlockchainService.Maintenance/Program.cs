using BlockchainService.Database;
using BlockchainService.Maintenance;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource(Worker.ActivitySourceName));

builder.AddNpgsqlDbContext<BlockchainDbContext>("BlockchainServiceDb");

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

host.Run();
