# Skill: EF Core Performance

## Purpose
Diagnose and fix the common EF Core performance problems — N+1 queries, over-fetching,
unnecessary tracking, missing indexes surfaced through slow LINQ — with evidence, not
guesses.

## When to Use
A confirmed slow query/endpoint where EF Core is involved (see
[`profiling-and-diagnostics`](../performance/profiling-and-diagnostics.md) for how to
confirm before optimizing).

## Prerequisites
[`efcore-fundamentals`](../data/efcore-fundamentals.md). Query logging enabled
(`.LogTo(...)`/`EnableSensitiveDataLogging()` in dev only) or a profiler to see actual
generated SQL and query counts.

## Inputs Required
The slow operation, and ideally a query count or execution plan showing what's actually
happening.

## Engineering Principles
- **N+1 detection**: if a loop over a collection triggers a query per iteration, that's N+1
  — fix with eager loading (`.Include()`) or a single projected query, not per-item lazy
  access.
- **Projection over full entity loads**: `.Select()` into a DTO with only the needed columns
  is both faster (less data transferred, no tracking overhead) and often avoids N+1
  entirely.
- **`AsNoTracking()`** for any read that won't be saved — tracking overhead is proportional
  to the number of entities materialized.
- **`AsSplitQuery()`** for `.Include()`s that would otherwise produce a cartesian-product
  join across multiple one-to-many relationships — EF Core's default single-query behavior
  can multiply row counts badly with several includes.
- **Compiled queries** (`EF.CompiledQuery`) only for proven hot paths with measured
  query-compilation overhead — not a default optimization.
- **Bulk operations**: `ExecuteUpdateAsync`/`ExecuteDeleteAsync` (EF Core 7+) for updating/
  deleting many rows by predicate, instead of loading entities into memory to mutate and
  save them one by one.

## Step-by-Step Workflow
1. Confirm the actual query count/shape via logging or a profiler — don't guess which query
   is the problem.
2. For N+1: add `.Include()` for a bounded, needed relation, or restructure as a single
   projected query.
3. For over-fetching: project to a DTO with `.Select()` instead of loading full entities.
4. For read-only paths: add `AsNoTracking()`.
5. For multi-include cartesian blowup: consider `AsSplitQuery()`.
6. Re-measure query count/timing after the change to confirm improvement.

## Code Standards
```csharp
// N+1: one query per order to get its lines
var orders = await db.Orders.ToListAsync(ct);
foreach (var order in orders)
{
    var lines = await db.OrderLines.Where(l => l.OrderId == order.Id).ToListAsync(ct); // N+1
}

// Fixed: single query with projection, no tracking
var orderSummaries = await db.Orders
    .AsNoTracking()
    .Select(o => new OrderSummaryDto(
        o.Id.Value,
        o.CustomerId,
        o.Lines.Select(l => new OrderLineDto(l.Sku, l.Quantity)).ToList()))
    .ToListAsync(ct);

// Bulk update without loading entities into memory
await db.Orders
    .Where(o => o.Status == OrderStatus.Draft && o.CreatedAt < cutoff)
    .ExecuteUpdateAsync(s => s.SetProperty(o => o.Status, OrderStatus.Cancelled), ct);
```

## Architecture Constraints
None beyond [`efcore-fundamentals`](../data/efcore-fundamentals.md)'s layering rules.

## Security Considerations
`EnableSensitiveDataLogging()` (which logs parameter values) must never be enabled outside
local development — it can log PII/secrets into logs.

## Testing Requirements
An integration test asserting a bounded query count for a given operation (many providers/
test helpers can count executed commands) prevents an N+1 regression from silently
reappearing — see [`integration-testing`](../testing/integration-testing.md).

## Common Mistakes
- Adding `.Include()` for every navigation property "to be safe," loading far more data than
  the use case needs.
- Optimizing based on a guess instead of the actual generated SQL/query count.
- Applying `AsSplitQuery()` everywhere by default instead of where a measured cartesian
  blowup actually exists (it has its own tradeoffs — multiple round trips vs. one large
  result set).

## Anti-Patterns
- Adding a compiled query or a raw-SQL escape hatch for every query "for performance"
  without evidence any specific one needs it.

## Validation Checklist
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Definition of Done
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Example
See [Code Standards](#code-standards).

## Related Skills
[`efcore-fundamentals`](../data/efcore-fundamentals.md),
[`profiling-and-diagnostics`](../performance/profiling-and-diagnostics.md),
[`sql-server`](../data/sql-server.md), [`postgresql`](../data/postgresql.md).
