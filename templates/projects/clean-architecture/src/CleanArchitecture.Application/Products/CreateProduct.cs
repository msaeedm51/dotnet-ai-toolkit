using CleanArchitecture.Domain;

namespace CleanArchitecture.Application.Products;

public sealed record CreateProductCommand(string Name, decimal Price);

public sealed class CreateProductHandler(IProductRepository repository)
{
    public async Task<Result<Guid>> HandleAsync(CreateProductCommand command, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
            return Result<Guid>.Failure("Name is required.", ErrorType.Validation);
        if (command.Price < 0)
            return Result<Guid>.Failure("Price cannot be negative.", ErrorType.Validation);

        var product = Product.Create(command.Name, command.Price);
        await repository.AddAsync(product, ct);
        await repository.SaveChangesAsync(ct);

        return Result<Guid>.Success(product.Id);
    }
}
