namespace WebApi.Domain;

public class CartItem
{
    public const int QuantityMinValue = 1;
    public const int QuantityMaxValue = 100;

    public long Id { get; init; }
    public long UserId { get; init; }
    public User? User { get; init; }
    public long ProductId { get; init; }
    public Product? Product { get; init; }
    public int Quantity { get; init; }
    public DateTime CreatedAt { get; init; }
}
