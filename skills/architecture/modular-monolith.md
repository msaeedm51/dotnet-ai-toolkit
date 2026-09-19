# Skill: Modular Monolith

## Purpose
Structure a single deployable application into independent modules with enforced boundaries
— giving most of microservices' organizational benefit (team autonomy, bounded contexts)
without the operational cost (distributed systems, network calls between every module).

## When to Use
A system with multiple distinct business capabilities that benefit from clear separation,
but where independent deployment/scaling per capability isn't yet a real requirement — often
the right default over microservices for a team that hasn't yet proven it needs the
distributed-systems cost (see [`microservices`](../architecture/microservices.md) for when
that changes).

## Prerequisites
Identify the actual bounded contexts/modules from the domain, not from convenience — this
should come out of [`architect`](../../agents/architect.md) analysis or an existing
established structure.

## Inputs Required
The business capabilities involved and their natural boundaries (what changes together, what
different teams own, what has genuinely different data).

## Engineering Principles
- Each module owns its own data access — no module directly queries another module's
  tables; cross-module interaction goes through the other module's public interface (an
  in-process service call), not a shared database join.
- Modules can share a physical database (even the same schema) while still enforcing logical
  separation — the discipline is in the code boundary (project references, internal
  visibility), not necessarily physical database separation, unless there's a reason to
  split it.
- Public APIs between modules are deliberate and narrow — a module exposes an interface
  (`IOrdersModule`/specific application services), not its internal entities or `DbContext`.
- Enforce boundaries with project references (a module's internals are `internal`, not
  `public`) and, ideally, automated architecture tests (see
  [`architecture-testing`](../testing/architecture-testing.md)) checking no module reaches
  into another's internals.
- This structure is a stepping stone: modules with clean boundaries can be extracted into
  real microservices later if independent deployment/scaling genuinely becomes necessary —
  modules with leaky boundaries can't be extracted without a rewrite.

## Step-by-Step Workflow
1. Identify modules from actual business capabilities/bounded contexts.
2. Structure each module as its own project (or clearly-scoped folder with enforced
   internal visibility) with its own Domain/Application/Infrastructure slice if using
   [`clean-architecture`](../architecture/clean-architecture.md) within each module.
3. Define each module's public interface explicitly — what other modules are allowed to
   call.
4. Route cross-module interaction through that public interface, never direct data access
   into another module's tables/entities.
5. Add architecture tests enforcing the boundary.

## Code Standards
```text
src/
  Modules/
    Orders/
      Orders.Domain/          (internal to the module except what's re-exported)
      Orders.Application/
      Orders.Infrastructure/
      Orders.Api/              -- exposes IOrdersModule as the only public surface
    Inventory/
      Inventory.Domain/
      Inventory.Application/
      Inventory.Infrastructure/
      Inventory.Api/
  Host/                        -- composition root wiring all modules together
```
```csharp
// Orders module's public surface -- the only thing Inventory (or anything else) can call
public interface IOrdersModule
{
    Task<OrderSummaryDto?> GetOrderAsync(Guid orderId, CancellationToken ct);
}

// Cross-module interaction via the public interface, never Inventory reaching into
// Orders' database tables directly
public sealed class ReserveStockHandler(IOrdersModule orders, IInventoryRepository inventory)
{
    public async Task HandleAsync(ReserveStockCommand command, CancellationToken ct)
    {
        var order = await orders.GetOrderAsync(command.OrderId, ct)
            ?? throw new NotFoundException($"Order {command.OrderId} not found.");
        // ...
    }
}
```

## Architecture Constraints
No module's `DbContext`/repository is referenced from another module's project. Enforce via
project references and, ideally, `NetArchTest`-based architecture tests.

## Security Considerations
A module boundary is also useful for scoping authorization — a module can enforce its own
access rules for operations only it understands, rather than centralizing every rule in one
place that doesn't have the context.

## Testing Requirements
Architecture tests asserting no forbidden cross-module reference exists
(see [`architecture-testing`](../testing/architecture-testing.md)); integration tests for
cross-module interaction through the public interface.

## Common Mistakes
- A module querying another module's tables directly "just this once for a report" —
  becomes a hidden coupling that blocks future extraction and breaks on schema changes.
- No enforced boundary at all — folders that look modular but have no actual access
  restriction, drifting into a big ball of mud over time.

## Anti-Patterns
- Splitting into microservices prematurely when a modular monolith would have solved the
  actual current problem (team boundaries, code organization) without the distributed-systems
  cost — see [`microservices`](../architecture/microservices.md)'s constraints on when that
  jump is warranted.

## Validation Checklist
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Definition of Done
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Example
See [Code Standards](#code-standards).

## Related Skills
[`clean-architecture`](../architecture/clean-architecture.md),
[`microservices`](../architecture/microservices.md),
[`architecture-testing`](../testing/architecture-testing.md).
