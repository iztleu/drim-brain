using Microsoft.EntityFrameworkCore;
using WebApi.Domain;

namespace WebApi.Database;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        MapProduct(modelBuilder);
    }

    private static void MapProduct(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(product =>
        {
            product.HasKey(p => p.Id);

            product.Property(p => p.Name)
                .HasMaxLength(Product.NameMaxLength)
                .IsRequired();

            product.Property(p => p.Description)
                .HasMaxLength(Product.DescriptionMaxLength)
                .IsRequired();

            product.Property(p => p.Price)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            product.Property(p => p.StockQuantity)
                .IsRequired();
        });
    }
}
