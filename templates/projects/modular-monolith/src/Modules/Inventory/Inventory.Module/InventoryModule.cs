using System.Collections.Concurrent;
using Inventory.Contracts;
using Orders.Contracts;

namespace Inventory.Module;

/// <summary>
/// Depends on Orders.Contracts (IOrdersModule) to check an order exists before reserving
/// stock for it -- cross-module interaction goes through the other module's PUBLIC
/// interface only. This project must never reference Orders.Module directly; see
/// tests/ModularMonolith.Tests/ArchitectureTests.cs, which enforces that automatically.
/// </summary>
public sealed class InventoryModule(IOrdersModule orders) : IInventoryModule
{
    private readonly ConcurrentDictionary<Guid, bool> _reservations = new();

    public async Task<ReservationResult> ReserveStockForOrderAsync(Guid orderId, CancellationToken ct)
    {
        var order = await orders.GetOrderAsync(orderId, ct);
        if (order is null)
            return new ReservationResult(false, $"Order {orderId} not found.");

        _reservations[orderId] = true;
        return new ReservationResult(true, null);
    }
}
