using CleanArchitecture.Domain;

namespace CleanArchitecture.Application.Products;

/// <summary>
/// Interface owned by Application, implemented by Infrastructure -- this inversion is
/// what lets Infrastructure depend on Application instead of the reverse.
/// See skills/architecture/dependency-injection-design.md.
/// </summary>
public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct);
    Task AddAsync(Product product, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
