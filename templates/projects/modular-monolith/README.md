# Modular Monolith Template

A buildable, tested .NET 8 solution demonstrating
[`modular-monolith`](../../../skills/architecture/modular-monolith.md): two independent
modules (Orders, Inventory) in one deployable, with the module boundary enforced by an
automated architecture test — not just a naming convention.

## Structure

```
ModularMonolith.sln
src/
  Modules/Orders/
    Orders.Contracts/   -- IOrdersModule + OrderSummary. The ONLY thing other modules see.
    Orders.Module/       -- OrdersModule (internal implementation), its endpoints.
  Modules/Inventory/
    Inventory.Contracts/ -- IInventoryModule + ReservationResult.
    Inventory.Module/    -- InventoryModule, depends on Orders.CONTRACTS only (never
                             Orders.Module) to check an order exists before reserving stock.
  Host/                  -- composition root: wires both modules' concrete
                             implementations, the only project that references both
                             Orders.Module and Inventory.Module.
tests/ModularMonolith.Tests/
  ArchitectureTests.cs         -- NetArchTest: fails the build if Inventory.Module ever
                                   references Orders.Module directly.
  InventoryModuleTests.cs      -- unit tests against a fake IOrdersModule.
  CrossModuleEndToEndTests.cs  -- boots the real Host, proves the two modules interact
                                   correctly through their public interfaces.
```

## The rule this enforces

Inventory needs to know whether an order exists before reserving stock for it. The naive
approach — Inventory querying Orders' database table directly — is exactly what
[`rules/architecture.md`](../../../rules/architecture.md) prohibits ("no direct cross-module
database access where module boundaries prohibit it"). Instead, Inventory depends on
`IOrdersModule` (in `Orders.Contracts`), and `ArchitectureTests.cs` fails the build the
moment anyone adds a project reference from `Inventory.Module` to `Orders.Module`.

## Using this template

1. Copy this folder, rename `Orders`/`Inventory`/`ModularMonolith` throughout to your
   project's actual modules.
2. Vendor the toolkit at `.ai-dotnet/` per the root [README.md](../../../README.md#installing-in-a-net-project).
3. Copy `.ai-dotnet.config.example.yaml` to `.ai-dotnet/config.yaml`.
4. Add a third module by copying the `Orders.Contracts`/`Orders.Module` pair's shape; wire
   it in `Host/Program.cs`; add its architecture-boundary test.
5. When you add a real database, give each module its own schema/tables rather than a
   shared one — see [`database-design`](../../../skills/data/database-design.md).
6. If a module later needs to scale or deploy independently, its already-enforced boundary
   is what makes extracting it into a real microservice cheap — see
   [`microservices`](../../../skills/architecture/microservices.md) for when that's
   actually warranted.

## Verify it builds and passes

```bash
dotnet test
```

Applicable toolkit rules: [`rules/architecture.md`](../../../rules/architecture.md),
[`rules/api.md`](../../../rules/api.md), [`rules/testing.md`](../../../rules/testing.md).
