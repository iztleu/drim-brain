using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using WebApi.Common.Ids;
using WebApi.Database;
using WebApi.Domain;
using WebApi.Features.Orders.Models;
using WebApi.Web.Endpoints;

namespace WebApi.Features.Orders.Requests;

public static class CreateOrder
{
    private const string Path = "/orders";

    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(WebApplication app)
        {
            app.MapPost(Path, async Task<Ok<OrderModel>> (
                Body body,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var order = await sender.Send(
                    new Request(User.CurrentUserId, body.CartItemIds),
                    cancellationToken);
                return TypedResults.Ok(order);
            });
        }

        private record Body(string[] CartItemIds);
    }

    public record Request(long UserId, string[] CartItemIds) : IRequest<OrderModel>;

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.CartItemIds)
                .NotEmpty()
                .Must(ci => ci.Length is >= Order.CartItemsMinLength and <= Order.CartItemsMaxLength);
        }
    }

    public record RequestHandler : IRequestHandler<Request, OrderModel>
    {
        private readonly AppDbContext _dbContext;
        private readonly IdFactory _idFactory;

        public RequestHandler(
            AppDbContext dbContext,
            IdFactory idFactory)
        {
            _dbContext = dbContext;
            _idFactory = idFactory;
        }

        public async Task<OrderModel> Handle(Request request, CancellationToken cancellationToken)
        {
            var cartItemIds = request.CartItemIds.Select(IdEncoding.Decode);

            var cartItems = await _dbContext.CartItems
                .Include(ci => ci.Product)
                .Where(ci => cartItemIds.Contains(ci.Id))
                .ToListAsync(cancellationToken);

            var order = new Order
            {
                Id = _idFactory.Create(),
                UserId = request.UserId,
                CreatedAt = DateTime.UtcNow,
                OrderItems = cartItems.Select(ci => new OrderItem
                {
                    ProductId = ci.Product!.Id,
                    Quantity = ci.Quantity,
                    Price = ci.Product.Price,
                }).ToList()
            };

            _dbContext.Orders.Add(order);

            _dbContext.RemoveRange(cartItems);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return order.ToModel();
        }
    }
}
