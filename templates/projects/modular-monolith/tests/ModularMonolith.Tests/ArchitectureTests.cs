using Inventory.Module;
using NetArchTest.Rules;
using Xunit;

namespace ModularMonolith.Tests;

/// <summary>
/// Enforces the module boundary from skills/architecture/modular-monolith.md automatically:
/// Inventory may depend on Orders.Contracts (the public interface) but never on
/// Orders.Module (Orders' internal implementation). Keep this test in CI on every PR.
/// </summary>
public class ArchitectureTests
{
    [Fact]
    public void Inventory_Should_Not_Depend_On_Orders_Internal_Implementation()
    {
        var result = Types.InAssembly(typeof(InventoryModule).Assembly)
            .Should()
            .NotHaveDependencyOn("Orders.Module")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join(", ", result.FailingTypeNames ?? []));
    }
}
