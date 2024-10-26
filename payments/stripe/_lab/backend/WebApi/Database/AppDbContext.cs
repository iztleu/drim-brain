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
    public DbSet<Order> Orders { get; init; }
    public DbSet<OrderItem> OrderItems { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        MapUser(modelBuilder);
        MapProduct(modelBuilder);
        MapCartItem(modelBuilder);
        MapOrder(modelBuilder);
        MapOrderItem(modelBuilder);
    }

    private static void MapUser(ModelBuilder modelBuilder)
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

    private static void MapCartItem(ModelBuilder modelBuilder)
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

    private static void MapOrder(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(order =>
        {
            order.HasKey(o => o.Id);

            order.HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .IsRequired();

            order.Property(o => o.CreatedAt)
                .IsRequired();

            order.HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .IsRequired();
        });
    }

    private static void MapOrderItem(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderItem>(orderItem =>
        {
            orderItem.HasKey(oi => new {oi.OrderId, oi.ProductId});

            orderItem.HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .IsRequired();

            orderItem.HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .IsRequired();

            orderItem.Property(oi => oi.Quantity)
                .IsRequired();

            orderItem.Property(oi => oi.Price)
                .HasColumnType("decimal(18,2)")
                .IsRequired();
        });
    }
}
