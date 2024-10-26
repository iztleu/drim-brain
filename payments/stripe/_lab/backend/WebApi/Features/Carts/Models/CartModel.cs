namespace WebApi.Features.Carts.Models;

public class CartModel
{
    public CartModel(CartItemModel[] items)
    {
        Items = items;
    }

    public CartItemModel[] Items { get; }

    public decimal TotalPrice => Items.Sum(x => x.Price * x.Quantity);
}
