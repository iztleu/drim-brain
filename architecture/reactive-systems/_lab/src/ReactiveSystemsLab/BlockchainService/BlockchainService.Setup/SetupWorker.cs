using System.Diagnostics;
using BlockchainService.Api.Kafka;
using BlockchainService.Database;
using Common.Database;
using Confluent.Kafka;
using Confluent.Kafka.Admin;

namespace BlockchainService.Setup;

public class SetupWorker(
    IServiceProvider _serviceProvider,
    IConfiguration _configuration,
    IHostApplicationLifetime _hostApplicationLifetime)
    : BackgroundService
{
    public const string ActivitySourceName = "blockchain-service-setup";

    private static readonly ActivitySource ActivitySource = new(ActivitySourceName);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await MigrateDatabase(stoppingToken);
        await CreateKafkaTopics();

        _hostApplicationLifetime.StopApplication();
    }

    private async Task MigrateDatabase(CancellationToken stoppingToken)
    {
        using var activity = ActivitySource.StartActivity(ActivityKind.Client);

        await DatabaseMigrator.Migrate<BlockchainDbContext>(_serviceProvider, stoppingToken);
    }

    private async Task CreateKafkaTopics()
    {
        using var activity = ActivitySource.StartActivity(ActivityKind.Client);

        var connectionString = _configuration.GetConnectionString("kafka");

        var adminClientConfig = new AdminClientConfig
        {
            BootstrapServers = connectionString,
        };

        using var adminClient = new AdminClientBuilder(adminClientConfig).Build();

        var topics = new List<TopicSpecification>
        {
            new()
            {
                Name = TopicNames.CryptoDepositCreated,
                NumPartitions = 5,
                ReplicationFactor = 1,
            },
        };

        await adminClient.CreateTopicsAsync(topics);
    }
}
