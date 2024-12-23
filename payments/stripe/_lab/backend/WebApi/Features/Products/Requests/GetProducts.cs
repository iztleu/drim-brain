using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Common.Ids;
using WebApi.Database;
using WebApi.Features.Products.Models;
using WebApi.Web.Endpoints;

namespace WebApi.Features.Products.Requests;

public static class GetProducts
{
    public class Endpoint : IEndpoint
    {
        private const string Path = "/products";

        public void MapEndpoint(WebApplication app)
        {
            app.MapGet(Path, async Task<Ok<ProductModel[]>> (
                [FromQuery] string? pageToken,
                [FromQuery] int? maxPageSize,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var products = await sender.Send(new Request(), cancellationToken);
                return TypedResults.Ok(products);
            });
        }
    }

    public record Request : IRequest<ProductModel[]>;

    public record RequestHandler : IRequestHandler<Request, ProductModel[]>
    {
        private readonly AppDbContext _dbContext;

        public RequestHandler(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ProductModel[]> Handle(Request request, CancellationToken cancellationToken)
        {
            var products = await _dbContext.Products
                .OrderBy(x => x.Name)
                .Select(x => new ProductModel(IdEncoding.Encode(x.Id), x.Name, x.Description, x.Price, x.StockQuantity))
                .ToArrayAsync(cancellationToken);

            return products;
        }
    }
}
