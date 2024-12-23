using Microsoft.EntityFrameworkCore;
using NotificationService.Domain;

namespace NotificationService.Database;

public class NotificationDbContext(DbContextOptions<NotificationDbContext> options) : DbContext(options)
{
    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        MapNotification(modelBuilder);
    }

    private static void MapNotification(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notification>(notification =>
        {
            notification.HasKey(e => e.Id);

            notification.Property(e => e.UserId)
                .IsRequired();

            notification.Property(e => e.Text)
                .IsRequired();

            notification.Property(e => e.CreatedAt)
                .IsRequired();
        });
    }
}
