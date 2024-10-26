using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using WebApi.Common.Ids;
using WebApi.Database;
using WebApi.Domain;
using WebApi.Features.Carts.Models;
using WebApi.Web.Endpoints;

namespace WebApi.Features.Carts.Requests;

public static class AddItemToCart
{
    private const string Path = "/cart";

    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(WebApplication app)
        {
            app.MapPost(Path, async Task<Ok<CartItemModel>> (
                Body body,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var cartItem = await sender.Send(
                    new Request(User.CurrentUserId, body.ProductId, body.Quantity),
                    cancellationToken);
                return TypedResults.Ok(cartItem);
            });
        }
    }

    private record Body(string ProductId, int Quantity);

    public record Request(long UserId, string ProductId, int Quantity) : IRequest<CartItemModel>;

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty();
            RuleFor(x => x.Quantity)
                .InclusiveBetween(CartItem.QuantityMinValue, CartItem.QuantityMaxValue);
        }
    }

    public record RequestHandler : IRequestHandler<Request, CartItemModel>
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

        public async Task<CartItemModel> Handle(Request request, CancellationToken cancellationToken)
        {
            var productId = IdEncoding.Decode(request.ProductId);

            var product = await _dbContext.Products
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == productId, cancellationToken);

            if (product is null)
            {
                throw new ValidationException("Product not found.");
            }

            var existingCartItem = await _dbContext.CartItems
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.UserId == request.UserId && x.ProductId == productId, cancellationToken);

            if (existingCartItem is not null)
            {
                // This could be handled differently, e.g. by updating the quantity.
                throw new ValidationException("Product already in cart.");
            }

            var cartItem = new CartItem
            {
                Id = _idFactory.Create(),
                UserId = request.UserId,
                ProductId = product.Id,
                Quantity = request.Quantity,
                CreatedAt = DateTime.UtcNow,
            };

            _dbContext.CartItems.Add(cartItem);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new CartItemModel(
                IdEncoding.Encode(cartItem.Id),
                request.ProductId,
                product.Name,
                product.Price,
                cartItem.Quantity);
        }
    }
}
