namespace Inventory.Contracts;

public interface IInventoryModule
{
    Task<ReservationResult> ReserveStockForOrderAsync(Guid orderId, CancellationToken ct);
}

public sealed record ReservationResult(bool Success, string? Error);
