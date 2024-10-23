using Microsoft.EntityFrameworkCore;
using WebApi.Domain;

namespace WebApi.Database;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; init; }
    public DbSet<Product> Products { get; init; }
    public DbSet<CartItem> CartItems { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        MapUser(modelBuilder);
        MapProduct(modelBuilder);
        MapCartItem(modelBuilder);
    }

    private void MapCartItem(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CartItem>(cartItem =>
        {
            cartItem.HasKey(ci => ci.Id);

            cartItem.HasOne(ci => ci.User)
                .WithMany()
                .HasForeignKey(ci => ci.UserId)
                .IsRequired();

            cartItem.HasOne(ci => ci.Product)
                .WithMany()
                .HasForeignKey(ci => ci.ProductId)
                .IsRequired();

            cartItem.Property(ci => ci.Quantity)
                .IsRequired();

            cartItem.Property(ci => ci.CreatedAt)
                .IsRequired();

            cartItem.HasIndex(ci => new {ci.UserId, ci.ProductId})
                .IsUnique();
        });
    }

    private void MapUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(user =>
        {
            user.HasKey(u => u.Id);
        });
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
