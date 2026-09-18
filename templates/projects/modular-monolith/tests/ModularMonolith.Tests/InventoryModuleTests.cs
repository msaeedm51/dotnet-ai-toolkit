using Inventory.Module;
using Orders.Contracts;
using Xunit;

namespace ModularMonolith.Tests;

public class InventoryModuleTests
{
    [Fact]
    public async Task ReserveStockForOrderAsync_ExistingOrder_Succeeds()
    {
        var orders = new FakeOrdersModule(orderExists: true);
        var inventory = new InventoryModule(orders);

        var result = await inventory.ReserveStockForOrderAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.True(result.Success);
    }

    [Fact]
    public async Task ReserveStockForOrderAsync_UnknownOrder_Fails()
    {
        var orders = new FakeOrdersModule(orderExists: false);
        var inventory = new InventoryModule(orders);

        var result = await inventory.ReserveStockForOrderAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.False(result.Success);
        Assert.NotNull(result.Error);
    }

    // Demonstrates that Inventory only needs IOrdersModule (the contract) to be testable in
    // isolation -- it never needs Orders.Module's real implementation.
    private sealed class FakeOrdersModule(bool orderExists) : IOrdersModule
    {
        public Task<Guid> CreateOrderAsync(Guid customerId, CancellationToken ct) =>
            Task.FromResult(Guid.NewGuid());

        public Task<OrderSummary?> GetOrderAsync(Guid orderId, CancellationToken ct) =>
            Task.FromResult(orderExists ? new OrderSummary(orderId, Guid.NewGuid(), "Confirmed") : null);
    }
}
