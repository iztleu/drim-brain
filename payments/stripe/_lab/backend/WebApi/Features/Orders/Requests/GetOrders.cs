using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using WebApi.Common.Ids;
using WebApi.Database;
using WebApi.Features.Orders.Models;
using WebApi.Web.Endpoints;

namespace WebApi.Features.Orders.Requests;

public static class GetOrders
{
    private const string Path = "/orders";

    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(WebApplication app)
        {
            app.MapGet(Path, async Task<Ok<OrderModel[]>> (
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var orders = await sender.Send(new Request(), cancellationToken);
                return TypedResults.Ok(orders);
            });
        }
    }

    public record Request : IRequest<OrderModel[]>;

    public record RequestHandler : IRequestHandler<Request, OrderModel[]>
    {
        private readonly AppDbContext _dbContext;

        public RequestHandler(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<OrderModel[]> Handle(Request request, CancellationToken cancellationToken)
        {
            var orders = await _dbContext.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.CreatedAt)
                .ToArrayAsync(cancellationToken);

            return orders.Select(o => o.ToModel()).ToArray();
        }
    }
}
