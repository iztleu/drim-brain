using System.Diagnostics;
using BlockchainService.Database;
using Common.Database;
using Confluent.Kafka;
using Confluent.Kafka.Admin;

namespace BlockchainService.Setup;

public class SetupWorker(
    IServiceProvider _serviceProvider,
    IHostApplicationLifetime _hostApplicationLifetime)
    : BackgroundService
{
    public const string ActivitySourceName = "blockchain-service-maintenance";

    private static readonly ActivitySource ActivitySource = new(ActivitySourceName);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await MigrateDatabase(stoppingToken);
        //await CreateKafkaTopics(stoppingToken);

        _hostApplicationLifetime.StopApplication();
    }

    private async Task MigrateDatabase(CancellationToken stoppingToken)
    {
        using var activity = ActivitySource.StartActivity(ActivityKind.Client);



        await DatabaseMigrator.Migrate<BlockchainDbContext>(_serviceProvider, stoppingToken);
    }

    private async Task CreateKafkaTopics(CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity(ActivityKind.Client);

        await using var scope = _serviceProvider.CreateAsyncScope();
        var connectionString = scope.ServiceProvider.GetRequiredService<IConfiguration>().GetConnectionString("kafka");

        var adminClientConfig = new AdminClientConfig
        {
            BootstrapServers = connectionString,
        };

        using var adminClient = new AdminClientBuilder(adminClientConfig).Build();

        var topics = new List<TopicSpecification>
        {
            new()
            {
                Name = "crypto-deposit-created",
                NumPartitions = 5,
                ReplicationFactor = 1,
            },
        };

        await adminClient.CreateTopicsAsync(topics);
    }
}
