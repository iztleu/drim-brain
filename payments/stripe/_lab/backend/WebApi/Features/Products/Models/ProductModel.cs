namespace WebApi.Features.Products.Models;

public record ProductModel(string Id, string Name, string Description, decimal Price, int StockQuantity);
