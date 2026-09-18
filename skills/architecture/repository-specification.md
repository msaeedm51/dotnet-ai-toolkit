# Skill: Repository and Specification Patterns

## Purpose
Abstract persistence behind an interface Application depends on, and express reusable query
logic (the "specification") without leaking `IQueryable`/EF Core details into Application/
Domain.

## When to Use
A project using [`clean-architecture`](../architecture/clean-architecture.md)/DDD where
Application must not depend on EF Core directly. **Not** required for a simpler project
where Application is allowed to depend on EF Core directly (a legitimate, simpler choice for
smaller CRUD-shaped services — don't add this abstraction without the constraint that
motivates it).

## Prerequisites
[`clean-architecture`](../architecture/clean-architecture.md) or equivalent layering already
in place, or being adopted as part of the same change.

## Inputs Required
The aggregate/entity being persisted, and the query patterns Application actually needs
(don't build a generic repository with every conceivable method up front).

## Engineering Principles
- A repository's interface lives in Application; its implementation lives in Infrastructure.
- Repository methods reflect actual Application use cases (`GetByIdAsync`,
  `GetOverdueOrdersAsync`), not a generic `IQueryable<T> Query()` escape hatch that just
  re-exposes EF Core's full surface through the "abstraction" — that defeats the purpose.
- The Specification pattern encapsulates a reusable query predicate (and optional
  includes/ordering) as a named, testable object, useful when the same query shape is needed
  in multiple places or needs to be composed.
- Don't build a generic repository framework speculatively — start with the specific methods
  a use case needs; add a Specification only when a query predicate is genuinely reused or
  needs to be composed/tested independently.

## Step-by-Step Workflow
1. Define the repository interface in Application with the specific methods the use case(s)
   need.
2. Implement it in Infrastructure against EF Core (or Dapper, for read-optimized queries).
3. If a query predicate is reused across multiple call sites or needs independent testing,
   extract it into a Specification.
4. Keep write operations (`AddAsync`, `UpdateAsync` — often implicit via `SaveChangesAsync`
   after loading and mutating a tracked aggregate) and read operations conceptually
   separate, especially if the project also uses [`cqrs`](../architecture/cqrs.md).

## Code Standards
```csharp
// Application layer -- interface only
public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(OrderId id, CancellationToken ct);
    Task AddAsync(Order order, CancellationToken ct);
    Task<IReadOnlyList<Order>> FindAsync(ISpecification<Order> specification, CancellationToken ct);
}

public interface ISpecification<T>
{
    Expression<Func<T, bool>> Criteria { get; }
}

public sealed class OverdueOrdersSpecification(DateTimeOffset asOf) : ISpecification<Order>
{
    public Expression<Func<Order, bool>> Criteria =>
        o => o.Status == OrderStatus.Confirmed && o.DueDate < asOf;
}

// Infrastructure layer -- EF Core implementation
public sealed class OrderRepository(AppDbContext db) : IOrderRepository
{
    public Task<Order?> GetByIdAsync(OrderId id, CancellationToken ct) =>
        db.Orders.FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task AddAsync(Order order, CancellationToken ct)
    {
        db.Orders.Add(order);
        await db.SaveChangesAsync(ct);
    }

    public Task<IReadOnlyList<Order>> FindAsync(ISpecification<Order> specification, CancellationToken ct) =>
        db.Orders.Where(specification.Criteria).ToListAsync(ct)
            .ContinueWith(t => (IReadOnlyList<Order>)t.Result, ct);
}
```

## Architecture Constraints
Application never references `Microsoft.EntityFrameworkCore` or any other concrete
persistence package — only the repository/specification interfaces it defines.

## Security Considerations
None beyond the standard query-parameterization rules
(see [`security-owasp`](../security/security-owasp.md)) — specifications built from
`Expression<Func<T,bool>>` are inherently parameterized when translated by EF Core.

## Testing Requirements
Repository interfaces are trivially fakeable for Application-layer unit tests; the real
implementation needs an integration test against a real/containerized database
(see [`integration-testing`](../testing/integration-testing.md)).

## Common Mistakes
- A generic `IRepository<T>` with a `IQueryable<T> Query()` method — re-exposes the entire
  ORM surface through the interface, providing no real abstraction.
- Building a full Specification framework before there's a second reused query predicate
  that would justify it.

## Anti-Patterns
- Wrapping every single-use query in its own Specification class "for consistency" — adds a
  file and a layer of indirection for something used exactly once.

## Validation Checklist
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Definition of Done
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Example
See [Code Standards](#code-standards).

## Related Skills
[`clean-architecture`](../architecture/clean-architecture.md), [`ddd`](../architecture/ddd.md),
[`efcore-fundamentals`](../data/efcore-fundamentals.md).
