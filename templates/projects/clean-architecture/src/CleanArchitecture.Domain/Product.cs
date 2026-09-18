namespace CleanArchitecture.Domain;

/// <summary>
/// Example aggregate root demonstrating a behavior method that enforces an invariant,
/// rather than public setters. See skills/architecture/ddd.md.
/// </summary>
public sealed class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public decimal Price { get; private set; }

    private Product() { } // for EF Core materialization

    public static Product Create(string name, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name is required.", nameof(name));
        if (price < 0)
            throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative.");

        return new Product
        {
            Id = Guid.NewGuid(),
            Name = name,
            Price = price,
        };
    }

    public void ChangePrice(decimal newPrice)
    {
        if (newPrice < 0)
            throw new ArgumentOutOfRangeException(nameof(newPrice), "Price cannot be negative.");

        Price = newPrice;
    }
}
