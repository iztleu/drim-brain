using BlockchainService.Api;
using BlockchainService.Database;
using BlockchainService.Features.Deposits.Jobs;
using Common.Kafka;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    // We have to wait for the database migration to execute. This is a temporary solution since .NET Aspire
    // does not support service restarts or startup dependencies yet
    await Task.Delay(5_000);
}

builder.AddServiceDefaults();

builder.AddNpgsqlDbContext<BlockchainDbContext>("BlockchainServiceDb");

builder.AddKafkaProducer<int, CryptoDepositCreatedEvent>("kafka", settings =>
{
    settings.SetValueSerializer(new KafkaJsonSerializer<CryptoDepositCreatedEvent>());
});

builder.Services.AddQuartz(q =>
{
    q.SchedulerName = "BlockchainService";
    q.SchedulerId = "AUTO";

    q.UsePersistentStore(s =>
    {
        s.UsePostgres(o =>
        {
            o.ConnectionString = builder.Configuration.GetConnectionString("BlockchainServiceDb")!;
        });
        s.UseClustering();
        s.UseNewtonsoftJsonSerializer();
        s.UseProperties = true;
    });

    var jobKey = new JobKey("DepositScanningJob");

    q.AddJob<DepositScanningJob>(j => j
        .WithIdentity(jobKey)
        .Build());

    q.AddTrigger(t => t
        .ForJob(jobKey)
        .WithIdentity("DepositScanningJobTrigger")
        .StartAt(DateTimeOffset.UtcNow.AddSeconds(10))
        .WithCronSchedule("0/5 * * * * ?"));
});

builder.Services.AddQuartzHostedService(q =>
{
    q.WaitForJobsToComplete = true;
});

var app = builder.Build();

app.MapDefaultEndpoints();

app.Run();
