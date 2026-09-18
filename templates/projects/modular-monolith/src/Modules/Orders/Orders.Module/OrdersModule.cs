using System.Collections.Concurrent;
using Orders.Contracts;

namespace Orders.Module;

/// <summary>
/// Internal implementation -- nothing outside this project should reference this class
/// directly; other modules depend on IOrdersModule (in Orders.Contracts) instead.
/// An in-memory store stands in for a real repository/database in this template.
/// </summary>
public sealed class OrdersModule : IOrdersModule
{
    private readonly ConcurrentDictionary<Guid, OrderSummary> _orders = new();

    public Task<Guid> CreateOrderAsync(Guid customerId, CancellationToken ct)
    {
        var id = Guid.NewGuid();
        _orders[id] = new OrderSummary(id, customerId, "Confirmed");
        return Task.FromResult(id);
    }

    public Task<OrderSummary?> GetOrderAsync(Guid orderId, CancellationToken ct) =>
        Task.FromResult(_orders.GetValueOrDefault(orderId));
}
