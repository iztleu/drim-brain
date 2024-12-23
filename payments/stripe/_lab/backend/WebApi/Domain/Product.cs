namespace WebApi.Domain;

public class Product
{
    public const int NameMinLength = 2;
    public const int NameMaxLength = 100;
    public const int DescriptionMinLength = 10;
    public const int DescriptionMaxLength = 1000;

    public long Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required decimal Price { get; init; }
    public required int StockQuantity { get; init; }
}
