namespace CleanArchitecture.Application.Products;

public sealed record ProductDto(Guid Id, string Name, decimal Price);

public sealed class GetProductsHandler(IProductRepository repository)
{
    public async Task<IReadOnlyList<ProductDto>> HandleAsync(CancellationToken ct)
    {
        var products = await repository.GetAllAsync(ct);
        return products.Select(p => new ProductDto(p.Id, p.Name, p.Price)).ToList();
    }
}
