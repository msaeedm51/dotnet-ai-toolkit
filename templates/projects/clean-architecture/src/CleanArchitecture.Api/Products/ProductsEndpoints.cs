using CleanArchitecture.Application.Products;
using CleanArchitecture.Domain;

namespace CleanArchitecture.Api.Products;

public static class ProductsEndpoints
{
    public static void MapProductsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/products").WithTags("Products");

        group.MapGet("/", async (GetProductsHandler handler, CancellationToken ct) =>
            Results.Ok(await handler.HandleAsync(ct)))
            .WithName("ListProducts");

        group.MapPost("/", async (CreateProductRequest request, CreateProductHandler handler, CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(new CreateProductCommand(request.Name, request.Price), ct);

            return result.IsSuccess
                ? Results.Created($"/products/{result.Value}", new { id = result.Value })
                : result.ErrorType switch
                {
                    ErrorType.Validation => Results.ValidationProblem(new Dictionary<string, string[]>
                    {
                        ["request"] = [result.Error!],
                    }),
                    _ => Results.Problem(result.Error, statusCode: 400),
                };
        })
        .WithName("CreateProduct");
    }
}

public sealed record CreateProductRequest(string Name, decimal Price);
