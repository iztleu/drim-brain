namespace WebApi.Domain;

public class Order
{
    public const int CartItemsMinLength = 1;
    public const int CartItemsMaxLength = 100;

    public long Id { get; init; }
    public long UserId { get; init; }
    public User? User { get; set; }
    public DateTime CreatedAt { get; init; }
    public List<OrderItem> OrderItems { get; set; } = new();
}
