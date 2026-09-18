# Skill: Clean Architecture

## Purpose
Structure a .NET solution so business logic doesn't depend on frameworks, databases, or UI
— those depend on it. Enables testing business rules without a database, and swapping
infrastructure without touching domain logic.

## When to Use
Non-trivial business logic that needs to outlive any particular framework/database choice,
or a codebase expected to grow past a handful of endpoints. **Not** for a small CRUD service
with no real business rules — that's over-engineering (see Anti-Patterns).

## Prerequisites
Confirm this is actually the project's declared architecture
(`config.yaml project.architecture`) or that adopting it is the explicit task — don't impose
it on a project that's deliberately simpler.

## Inputs Required
The feature/requirement, and the existing project's layer structure if one already exists.

## Engineering Principles
- **Dependency rule:** dependencies point inward only. Domain depends on nothing.
  Application depends on Domain. Infrastructure depends on Application/Domain (implements
  their interfaces). Presentation (API) depends on Application.
- Domain holds entities, value objects, domain events, and business rules — no EF Core
  attributes, no HTTP concerns, no `ILogger` from a specific framework.
- Application holds use cases (commands/queries/handlers) and the interfaces Infrastructure
  implements (`IOrderRepository`, `IEmailSender`) — Application does not reference EF Core,
  ASP.NET Core, or any concrete infrastructure package.
- Infrastructure implements Application's interfaces using real technology (EF Core, HTTP
  clients, message brokers).
- Presentation (API project) wires everything together via DI and translates HTTP
  ⇄ Application layer (DTOs in, DTOs/results out).

## Step-by-Step Workflow
1. Identify which layer the change belongs to by asking: "does this express a business
   rule?" (Domain), "does this orchestrate a use case?" (Application), "does this talk to an
   external system?" (Infrastructure), "does this handle HTTP?" (Presentation).
2. Check existing project references — a reference from Domain to Infrastructure, or
   Application to a specific ORM, is a violation to flag, not a precedent to extend.
3. Add the new interface to Application if the use case needs a new external capability;
   implement it in Infrastructure.
4. Wire the implementation via DI in the Presentation/host project's composition root.
5. Keep DTOs at the Presentation boundary distinct from Domain entities — never return a
   Domain entity directly from an endpoint.

## Code Standards
```
src/
  Orders.Domain/          -- Order, OrderLine, OrderStatus, domain events. No package refs
                              beyond the BCL.
  Orders.Application/     -- CreateOrderCommand, CreateOrderHandler, IOrderRepository
                              (interface only), validation.
  Orders.Infrastructure/  -- OrderRepository : IOrderRepository (EF Core), AppDbContext,
                              migrations.
  Orders.Api/             -- Minimal API endpoints, DTOs, Program.cs composition root.
```

```csharp
// Orders.Application/Orders/CreateOrderHandler.cs -- no EF Core, no ASP.NET Core reference
public sealed class CreateOrderHandler(IOrderRepository repository, TimeProvider clock)
{
    public async Task<Result<OrderId>> HandleAsync(CreateOrderCommand command, CancellationToken ct)
    {
        var order = Order.Create(command.CustomerId, clock.GetUtcNow());
        foreach (var line in command.Lines)
            order.AddLine(line.Sku, line.Quantity);

        await repository.AddAsync(order, ct);
        return Result<OrderId>.Success(order.Id);
    }
}
```

## Architecture Constraints
- Enforce the dependency rule with project references: Domain project references nothing
  else in the solution; Application references only Domain; Infrastructure and Presentation
  may reference Application and Domain.
- Enforce it in CI where possible — see [`architecture-testing`](../testing/architecture-testing.md)
  for automated dependency-direction checks (e.g. with `NetArchTest`).
- Domain entities never carry EF Core attributes/navigation-tracking concerns that leak
  persistence details into business logic — use `IEntityTypeConfiguration<T>` in
  Infrastructure instead (see [`efcore-fundamentals`](../data/efcore-fundamentals.md)).

## Security Considerations
Authorization decisions belong in Application (a use case knows what a caller is allowed to
do) or Presentation (transport-level gate), not scattered into Domain — Domain shouldn't
know what "the current user" is.

## Testing Requirements
Domain and Application layers should be fully unit-testable with no database, no HTTP
server, and no mocks of framework types — if a Domain/Application test needs
`WebApplicationFactory` or a real `DbContext`, that's a sign a boundary has been crossed
incorrectly.

## Common Mistakes
- `Order` entity with `[Key]`/`[Column]` EF Core data annotations — couples Domain to EF Core.
- Application layer directly calling `DbContext.SaveChangesAsync()` instead of going through
  a repository interface.
- API layer containing business logic ("if status is X, apply 10% discount") that belongs in
  Domain/Application.

## Anti-Patterns
- Applying full four-project Clean Architecture to a service with two endpoints and no real
  business rules — that's needless indirection; a simpler layered or vertical-slice
  structure serves it better (see [`modular-monolith`](../architecture/modular-monolith.md)
  for a middle ground).
- "Anemic" Application handlers that just forward to Infrastructure with no orchestration —
  if there's no use case logic, question whether Application needs a handler at all.

## Validation Checklist
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md). Skill-specific:
- [ ] No project reference violates the dependency rule.
- [ ] Domain has no framework/infrastructure package references.
- [ ] New endpoint returns a DTO, not a Domain entity.

## Definition of Done
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Example
See [Code Standards](#code-standards).

## Related Skills
[`ddd`](../architecture/ddd.md), [`cqrs`](../architecture/cqrs.md),
[`repository-specification`](../architecture/repository-specification.md),
[`result-pattern`](../architecture/result-pattern.md),
[`architecture-testing`](../testing/architecture-testing.md).
