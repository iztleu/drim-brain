namespace WebApi.Features.Carts.Models;

public record CartItemModel(string Id, string ProductId, string ProductName, decimal Price, int Quantity);
