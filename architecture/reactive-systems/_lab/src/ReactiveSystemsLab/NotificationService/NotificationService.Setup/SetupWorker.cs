using System.Diagnostics;
using Common.Database;
using NotificationService.Database;

namespace NotificationService.Setup;

public class SetupWorker(
    IServiceProvider _serviceProvider,
    IHostApplicationLifetime _hostApplicationLifetime)
    : BackgroundService
{
    public const string ActivitySourceName = "notification-service-setup";

    private static readonly ActivitySource ActivitySource = new(ActivitySourceName);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await MigrateDatabase(stoppingToken);

        _hostApplicationLifetime.StopApplication();
    }

    private async Task MigrateDatabase(CancellationToken stoppingToken)
    {
        using var activity = ActivitySource.StartActivity(ActivityKind.Client);

        await DatabaseMigrator.Migrate<NotificationDbContext>(_serviceProvider, stoppingToken);
    }
}
