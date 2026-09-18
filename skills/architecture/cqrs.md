# Skill: CQRS (Command Query Responsibility Segregation)

## Purpose
Separate operations that change state (commands) from operations that read state (queries),
so each can be modeled, optimized, and evolved independently.

## When to Use
The domain has meaningfully different read and write shapes (a write model with rich
invariants vs. reads that need denormalized, joined, or aggregated views), or read and write
load need to scale independently. **Not** a default for every project — a service with
simple CRUD and no divergent read/write needs gets nothing from CQRS but extra files.

## Prerequisites
Confirm this is warranted by an actual requirement (complex domain rules, reporting-style
reads very different from the write model, independent scaling need) — see the Architect
agent's constraint against pattern-for-pattern's-sake.

## Inputs Required
The use case being implemented, and whether it's mutating state (command) or retrieving it
(query).

## Engineering Principles
- Commands express intent and return only what the caller needs to proceed (an id, a
  `Result`, nothing else) — never a full read model.
- Queries never mutate state and can bypass the write model entirely — reading directly via
  a lightweight projection (Dapper, raw SQL, an EF Core no-tracking projection) is
  legitimate and often preferable to reusing the write-side aggregate.
- CQRS does not require a separate database or event sourcing — most .NET projects use
  "CQRS-lite": same database, separate command/query code paths. Don't add
  eventual-consistency complexity (separate read store, projections) without a requirement
  that needs it.
- This toolkit teaches the pattern generically; a mediator library (e.g. MediatR) is a common
  implementation choice but not required — the plain-interface version below works without
  one.

## Step-by-Step Workflow
1. Classify the use case: command (changes state) or query (reads state).
2. Commands: define a command record, a handler that loads the aggregate via a repository,
   invokes domain behavior, persists, and returns a minimal result.
3. Queries: define a query record and a handler that returns a purpose-built DTO, typically
   via a no-tracking/projected read, not through the write-model aggregate.
4. Keep command and query handlers in separate classes/files even if colocated in the same
   folder — don't let a "handler" do both.
5. Validate commands at the boundary (see [`model-validation-problemdetails`](../dotnet/model-validation-problemdetails.md))
   before the handler runs domain logic.

## Code Standards
```csharp
public interface ICommandHandler<TCommand, TResult>
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken ct);
}

public interface IQueryHandler<TQuery, TResult>
{
    Task<TResult> HandleAsync(TQuery query, CancellationToken ct);
}

public sealed record CreateOrderCommand(Guid CustomerId, IReadOnlyList<OrderLineRequest> Lines);

public sealed class CreateOrderHandler(IOrderRepository repository)
    : ICommandHandler<CreateOrderCommand, Result<Guid>>
{
    public async Task<Result<Guid>> HandleAsync(CreateOrderCommand command, CancellationToken ct)
    {
        var order = Order.Create(command.CustomerId);
        foreach (var line in command.Lines)
            order.AddLine(line.Sku, line.Quantity);

        await repository.AddAsync(order, ct);
        return Result<Guid>.Success(order.Id.Value);
    }
}

public sealed record OrderSummaryQuery(Guid OrderId);
public sealed record OrderSummaryDto(Guid Id, string CustomerName, decimal Total, string Status);

public sealed class OrderSummaryHandler(AppDbContext db)
    : IQueryHandler<OrderSummaryQuery, OrderSummaryDto?>
{
    public Task<OrderSummaryDto?> HandleAsync(OrderSummaryQuery query, CancellationToken ct) =>
        db.Orders
            .Where(o => o.Id == query.OrderId)
            .Select(o => new OrderSummaryDto(o.Id, o.Customer.Name, o.Total, o.Status.ToString()))
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);
}
```

## Architecture Constraints
Query handlers may read directly from the persistence layer with a projection; command
handlers must go through the domain model/aggregate to preserve invariants — a query never
mutates, and a command never returns a full denormalized read model.

## Security Considerations
Authorization is still required on both sides — a query handler exposing more data than the
caller is entitled to is as much a vulnerability as an unauthorized command. Apply
authorization at the same boundary regardless of command/query
(see [`authorization`](../security/authorization.md)).

## Testing Requirements
Command handlers: unit test against the domain model (and a fake/in-memory repository) for
business-rule correctness; at least one integration test through the real repository per
command. Query handlers: integration test against a real/containerized database, since their
correctness depends on the actual query translation
(see [`integration-testing`](../testing/integration-testing.md)).

## Common Mistakes
- A "query" that also updates a `LastAccessedAt` timestamp — that's a hidden command; make
  it explicit or don't do it inline with the read.
- Reusing the exact write-side aggregate shape as an API response DTO for reads, forcing
  N+1 loads to populate fields the read side doesn't need.
- Introducing a separate read database/event sourcing for a project with no requirement that
  justifies the operational cost.

## Anti-Patterns
- CQRS applied uniformly across an entire simple CRUD service "for consistency" — apply it
  where the read/write divergence is real, plain CRUD elsewhere.
- A command handler that both mutates and directly returns a fully hydrated read DTO by
  reusing query logic inline — keeps two concerns entangled that the pattern exists to
  separate.

## Validation Checklist
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md). Skill-specific:
- [ ] Command handlers only return minimal results (id/status), not full read models.
- [ ] Query handlers don't mutate state.
- [ ] Authorization applied on both command and query paths.

## Definition of Done
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Example
See [Code Standards](#code-standards).

## Related Skills
[`clean-architecture`](../architecture/clean-architecture.md),
[`ddd`](../architecture/ddd.md), [`result-pattern`](../architecture/result-pattern.md),
[`efcore-fundamentals`](../data/efcore-fundamentals.md).
