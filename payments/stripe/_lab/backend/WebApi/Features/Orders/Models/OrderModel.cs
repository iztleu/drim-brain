using WebApi.Common.Ids;
using WebApi.Domain;

namespace WebApi.Features.Orders.Models;

public record OrderModel(string Id, DateTime CreatedAt, OrderItemModel[] OrderItems);

public record OrderItemModel(string ProductId, string ProductName, int Quantity, decimal Price);

public static class OrderMappingExtensions
{
    public static OrderModel ToModel(this Order order)
    {
        return new OrderModel(
            IdEncoding.Encode(order.Id),
            order.CreatedAt,
            order.OrderItems.Select(oi => oi.ToModel()).ToArray()
        );
    }

    private static OrderItemModel ToModel(this OrderItem orderItem)
    {
        return new OrderItemModel(
            IdEncoding.Encode(orderItem.ProductId),
            orderItem.Product!.Name,
            orderItem.Quantity,
            orderItem.Price
        );
    }
}
