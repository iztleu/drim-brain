using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using WebApi.Common.Ids;
using WebApi.Database;
using WebApi.Domain;
using WebApi.Features.Carts.Models;
using WebApi.Web.Endpoints;

namespace WebApi.Features.Carts.Requests;

public static class GetCart
{
    private const string Path = "/cart";

    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(WebApplication app)
        {
            app.MapGet(Path, async Task<Ok<CartModel>> (
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var cart = await sender.Send(new Request(User.CurrentUserId), cancellationToken);
                return TypedResults.Ok(cart);
            });
        }
    }

    public record Request(long UserId) : IRequest<CartModel>;

    public class RequestHandler : IRequestHandler<Request, CartModel>
    {
        private readonly AppDbContext _dbContext;

        public RequestHandler(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<CartModel> Handle(Request request, CancellationToken cancellationToken)
        {
            var cartItems = await _dbContext.CartItems
                .AsNoTracking()
                .Include(ci => ci.Product)
                .Where(x => x.UserId == request.UserId)
                .Select(ci => new CartItemModel(
                    IdEncoding.Encode(ci.Id),
                    IdEncoding.Encode(ci.ProductId),
                    ci.Product!.Name,
                    ci.Product.Price,
                    ci.Quantity))
                .ToArrayAsync(cancellationToken);

            return new CartModel(cartItems);
        }
    }
}
