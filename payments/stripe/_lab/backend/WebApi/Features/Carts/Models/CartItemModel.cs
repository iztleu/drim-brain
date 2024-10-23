namespace WebApi.Features.Carts.Models;

public record CartItemModel(string Id, string ProductId, decimal Price, int Quantity);
