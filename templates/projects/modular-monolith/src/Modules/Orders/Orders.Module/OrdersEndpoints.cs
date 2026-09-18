using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Orders.Contracts;

namespace Orders.Module;

public static class OrdersEndpoints
{
    public static void MapOrdersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/orders").WithTags("Orders");

        group.MapPost("/", async (CreateOrderRequest request, IOrdersModule orders, CancellationToken ct) =>
        {
            var id = await orders.CreateOrderAsync(request.CustomerId, ct);
            return Results.Created($"/orders/{id}", new { id });
        });

        group.MapGet("/{id:guid}", async (Guid id, IOrdersModule orders, CancellationToken ct) =>
        {
            var order = await orders.GetOrderAsync(id, ct);
            return order is null ? Results.NotFound() : Results.Ok(order);
        });
    }
}

public sealed record CreateOrderRequest(Guid CustomerId);
