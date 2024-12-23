using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using OpenTelemetry.Trace;
using WebApi.Common.Ids;
using WebApi.Database;
using WebApi.Domain;

namespace WebApi.Setup;

public class SetupWorker : BackgroundService
{
    public const string ActivitySourceName = "web-api-setup";

    private static readonly ActivitySource ActivitySource = new(ActivitySourceName);

    private readonly IServiceProvider _serviceProvider;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly ILogger<SetupWorker> _logger;

    public SetupWorker(
        IServiceProvider serviceProvider,
        IHostApplicationLifetime hostApplicationLifetime,
        ILogger<SetupWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _hostApplicationLifetime = hostApplicationLifetime;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await MigrateDatabase(stoppingToken);

        _hostApplicationLifetime.StopApplication();
    }

    private async Task MigrateDatabase(CancellationToken stoppingToken)
    {
        using var activity = ActivitySource.StartActivity(ActivityKind.Client);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        cts.CancelAfter(TimeSpan.FromMinutes(1));

        await using var scope = _serviceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var idFactory = scope.ServiceProvider.GetRequiredService<IdFactory>();

        try
        {
            await EnsureDatabaseAsync(dbContext, cts.Token);
            await RunMigrationAsync(dbContext, cts.Token);

            _logger.LogInformation("Database migration completed successfully.");
            Activity.Current?.AddEvent(new("Database migration completed successfully."));

            await SeedDatabase(dbContext, idFactory, cts.Token);

            _logger.LogInformation("Database seeding completed successfully.");
            Activity.Current?.AddEvent(new("Database seeding completed successfully."));
        }
        catch (Exception ex)
        {
            Activity.Current?.RecordException(ex);

            _logger.LogError(ex, "An error occurred while setting up.");

            throw;
        }
    }

    private static async Task EnsureDatabaseAsync(AppDbContext dbContext, CancellationToken cancellationToken)
    {
        var dbCreator = dbContext.GetService<IRelationalDatabaseCreator>();

        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            if (!await dbCreator.ExistsAsync(cancellationToken))
            {
                await dbCreator.CreateAsync(cancellationToken);
            }
        });
    }

    private static async Task RunMigrationAsync(AppDbContext dbContext, CancellationToken cancellationToken)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            await dbContext.Database.MigrateAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        });
    }

    private static async Task SeedDatabase(AppDbContext dbContext, IdFactory idFactory, CancellationToken cancellationToken)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            await SeedUsers(dbContext, idFactory, cancellationToken);
            await SeedProducts(dbContext, idFactory, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        });
    }

    private static async Task SeedUsers(AppDbContext dbContext, IdFactory idFactory, CancellationToken cancellationToken)
    {
        var user = new User
        {
            Id = User.CurrentUserId,
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedProducts(AppDbContext dbContext, IdFactory idFactory, CancellationToken cancellationToken)
    {
        var products = new[]
        {
            new Product
            {
                Id = idFactory.Create(),
                Name = "MacBook Pro 16 M4 Max",
                Description = "The most powerful MacBook Pro ever.",
                Price = 3000,
                StockQuantity = 30,
            },
            new Product
            {
                Id = idFactory.Create(),
                Name = "iPhone 16 Pro Max",
                Description = "The most advanced iPhone ever.",
                Price = 1200,
                StockQuantity = 50,
            },
            new Product
            {
                Id = idFactory.Create(),
                Name = "iPad Pro 12.9 M4",
                Description = "The most powerful iPad ever.",
                Price = 1000,
                StockQuantity = 40,
            },
            new Product
            {
                Id = idFactory.Create(),
                Name = "Samsung Galaxy S24 Ultra",
                Description = "The most advanced Samsung Galaxy ever.",
                Price = 1300,
                StockQuantity = 60,
            },
            new Product
            {
                Id = idFactory.Create(),
                Name = "Samsung Galaxy Tab S8",
                Description = "The most powerful Samsung Galaxy Tab ever.",
                Price = 800,
                StockQuantity = 20,
            },
            new Product
            {
                Id = idFactory.Create(),
                Name = "Google Pixel 9 Pro",
                Description = "The most advanced Google Pixel ever.",
                Price = 1000,
                StockQuantity = 30,
            }
        };

        dbContext.Products.AddRange(products);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
