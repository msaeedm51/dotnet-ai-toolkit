using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Inventory.Contracts;

namespace Inventory.Module;

public static class InventoryEndpoints
{
    public static void MapInventoryEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/inventory/reserve/{orderId:guid}", async (Guid orderId, IInventoryModule inventory, CancellationToken ct) =>
        {
            var result = await inventory.ReserveStockForOrderAsync(orderId, ct);
            return result.Success ? Results.Ok() : Results.Problem(result.Error, statusCode: 404);
        })
        .WithTags("Inventory");
    }
}
