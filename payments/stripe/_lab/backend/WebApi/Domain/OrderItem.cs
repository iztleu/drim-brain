namespace WebApi.Domain;

public class OrderItem
{
    public long OrderId { get; init; }
    public Order? Order { get; set; }
    public long ProductId { get; init; }
    public Product? Product { get; set; }
    public int Quantity { get; init; }
    public decimal Price { get; init; }
}
