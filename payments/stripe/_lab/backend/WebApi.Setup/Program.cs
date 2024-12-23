using ServiceDefaults;
using WebApi.Common.Ids;
using WebApi.Database;
using WebApi.Setup;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource(SetupWorker.ActivitySourceName));

builder.AddNpgsqlDbContext<AppDbContext>(ResourceNames.WebApiDb);

builder.Services.AddIdFactory(1);

builder.Services.AddHostedService<SetupWorker>();

var host = builder.Build();
host.Run();
