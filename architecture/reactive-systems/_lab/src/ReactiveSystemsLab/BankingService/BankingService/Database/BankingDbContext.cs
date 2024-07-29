using BankingService.Domain;
using Microsoft.EntityFrameworkCore;

namespace BankingService.Database;

public class BankingDbContext(DbContextOptions<BankingDbContext> options) : DbContext(options)
{
    public DbSet<Deposit> Deposits { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        MapDeposit(modelBuilder);
    }

    private static void MapDeposit(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Deposit>(deposit =>
        {
            deposit.HasKey(e => e.Id);

            deposit.Property(e => e.SourceId)
                .IsRequired();

            deposit.Property(e => e.UserId)
                .IsRequired();

            deposit.Property(e => e.Asset)
                .IsRequired();

            deposit.Property(e => e.Amount)
                .IsRequired();

            deposit.Property(e => e.CreatedAt)
                .IsRequired();

            deposit.HasIndex(e => e.SourceId)
                .IsUnique();
        });
    }
}
