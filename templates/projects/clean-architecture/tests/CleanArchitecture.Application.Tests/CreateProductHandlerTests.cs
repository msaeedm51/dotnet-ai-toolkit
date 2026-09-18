using CleanArchitecture.Application.Products;
using CleanArchitecture.Domain;
using Xunit;

namespace CleanArchitecture.Application.Tests;

public class CreateProductHandlerTests
{
    [Fact]
    public async Task HandleAsync_ValidCommand_PersistsAndReturnsId()
    {
        var repository = new FakeProductRepository();
        var handler = new CreateProductHandler(repository);

        var result = await handler.HandleAsync(new CreateProductCommand("Widget", 9.99m), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(repository.Added);
    }

    [Fact]
    public async Task HandleAsync_EmptyName_ReturnsValidationFailure()
    {
        var repository = new FakeProductRepository();
        var handler = new CreateProductHandler(repository);

        var result = await handler.HandleAsync(new CreateProductCommand("", 9.99m), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.ErrorType);
        Assert.Empty(repository.Added);
    }

    [Fact]
    public async Task HandleAsync_NegativePrice_ReturnsValidationFailure()
    {
        var repository = new FakeProductRepository();
        var handler = new CreateProductHandler(repository);

        var result = await handler.HandleAsync(new CreateProductCommand("Widget", -1m), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.ErrorType);
    }

    private sealed class FakeProductRepository : IProductRepository
    {
        public List<Product> Added { get; } = [];

        public Task<Product?> GetByIdAsync(Guid id, CancellationToken ct) =>
            Task.FromResult(Added.FirstOrDefault(p => p.Id == id));

        public Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct) =>
            Task.FromResult((IReadOnlyList<Product>)Added);

        public Task AddAsync(Product product, CancellationToken ct)
        {
            Added.Add(product);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken ct) => Task.CompletedTask;
    }
}
