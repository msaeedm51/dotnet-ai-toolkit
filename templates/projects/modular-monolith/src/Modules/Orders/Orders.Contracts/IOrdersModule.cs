namespace Orders.Contracts;

/// <summary>
/// The ONLY thing other modules (or the Host) are allowed to depend on for Orders
/// capabilities. Orders.Module's internals (its store, its concrete class) are never
/// referenced directly outside this project. See skills/architecture/modular-monolith.md.
/// </summary>
public interface IOrdersModule
{
    Task<Guid> CreateOrderAsync(Guid customerId, CancellationToken ct);
    Task<OrderSummary?> GetOrderAsync(Guid orderId, CancellationToken ct);
}

public sealed record OrderSummary(Guid Id, Guid CustomerId, string Status);
