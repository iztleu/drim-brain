using System.Diagnostics;
using BlockchainService.Database;
using Common.Database;

namespace BlockchainService.Maintenance;

public class Worker(
    IServiceProvider _serviceProvider,
    IHostApplicationLifetime _hostApplicationLifetime)
    : BackgroundService
{
    public const string ActivitySourceName = "blockchain-service-maintenance";

    private static readonly ActivitySource ActivitySource = new(ActivitySourceName);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var activity = ActivitySource.StartActivity("Migrating database", ActivityKind.Client);

        await DatabaseMigrator.Migrate<BlockchainDbContext>(_serviceProvider, stoppingToken);

        _hostApplicationLifetime.StopApplication();
    }
}
