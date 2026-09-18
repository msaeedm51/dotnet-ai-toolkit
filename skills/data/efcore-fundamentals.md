# Skill: EF Core Fundamentals

## Purpose
Use EF Core correctly for the common cases — `DbContext` lifetime, entity configuration,
migrations, tracking behavior — and recognize when EF Core is the wrong tool.

## When to Use
Any change touching a `DbContext`, entity configuration, or a LINQ query against EF Core.

## Prerequisites
Know the target engine (`config.yaml backend.database.engine`) — SQL Server or PostgreSQL —
since provider-specific behavior differs (see [`sql-server`](../data/sql-server.md) /
[`postgresql`](../data/postgresql.md)).

## Inputs Required
The entity/relationship being added or changed, and the existing `DbContext`/configuration
conventions already in the project.

## Engineering Principles
- `DbContext` is registered `Scoped` (one per request/unit of work) — never `Singleton`, and
  never shared across concurrent operations.
- Entity configuration lives in `IEntityTypeConfiguration<T>` classes, not data annotations
  scattered on the entity, and not inline in `OnModelCreating` for anything beyond a couple
  of lines — keeps Domain entities free of persistence attributes
  (see [`clean-architecture`](../architecture/clean-architecture.md)).
- Default to `AsNoTracking()` for read-only queries — tracking has a real cost and is only
  needed when you intend to call `SaveChangesAsync()` on the same context afterward.
- Every migration is reviewed for its generated SQL before it's trusted, not just for
  whether the C# builds — the generated `Up()`/`Down()` is what actually runs.
- Concurrency: use a concurrency token (`[Timestamp]`/`rowversion` on SQL Server, `xmin` on
  PostgreSQL) for entities that can be concurrently edited, and handle
  `DbUpdateConcurrencyException` explicitly rather than letting it surface as a raw 500.

## Step-by-Step Workflow
1. Add/change the entity and its `IEntityTypeConfiguration<T>`.
2. Add a migration: `dotnet ef migrations add <Name>`.
3. Read the generated migration's `Up()` — confirm it does what's intended, especially for
   column type changes, renames (EF Core defaults to drop+add unless told otherwise), and
   index changes.
4. For queries: write against `IQueryable`, filter before materializing, project to a DTO
   with `Select()` when you don't need the full entity, use `AsNoTracking()` for reads.
5. Apply the migration to a local/test database and verify.
6. For production deployment implications (locking, duration), see
   [`database-change` workflow](../../workflows/database-change.md).

## Code Standards
```csharp
public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id)
            .HasConversion(id => id.Value, value => new OrderId(value));

        builder.Property(o => o.CustomerId).IsRequired();
        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasMany(o => o.Lines)
            .WithOne()
            .HasForeignKey("OrderId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property<uint>("Version").IsRowVersion(); // PostgreSQL xmin-backed token
    }
}

// No-tracking read, projected -- avoids loading full entity graph
public Task<OrderSummaryDto?> GetSummaryAsync(Guid orderId, CancellationToken ct) =>
    db.Orders
        .AsNoTracking()
        .Where(o => o.Id == new OrderId(orderId))
        .Select(o => new OrderSummaryDto(o.Id.Value, o.CustomerId, o.Total, o.Status.ToString()))
        .FirstOrDefaultAsync(ct);

// Tracked write, goes through the aggregate
public async Task AddLineAsync(Guid orderId, string sku, int quantity, CancellationToken ct)
{
    var order = await db.Orders.FirstAsync(o => o.Id == new OrderId(orderId), ct);
    order.AddLine(sku, quantity);
    await db.SaveChangesAsync(ct);
}
```

## Architecture Constraints
`IEntityTypeConfiguration<T>` classes live in Infrastructure, not Domain. Domain entities
have no `[Column]`/`[Table]`/navigation-tracking attributes.

## Security Considerations
Never build a LINQ query with a raw string interpolated for anything that becomes part of
generated SQL (`FromSqlRaw` with concatenated input) — use `FromSqlInterpolated`/parameters
always (see [`security-owasp`](../security/security-owasp.md)).

## Testing Requirements
Query correctness (real SQL translation) needs an integration test against a real/
containerized database, not just the in-memory provider — see
[`integration-testing`](../testing/integration-testing.md).

## Common Mistakes
- N+1 queries from lazy-loading or looping over a collection and querying inside the loop —
  use `.Include()`/projection to fetch what's needed in one query.
- Loading a full tracked entity graph to read one field.
- Registering `DbContext` (or a `DbContextFactory`-produced context) as `Singleton`.
- Ignoring migration `Down()` correctness, leaving no real rollback path.

## Anti-Patterns
- Using EF Core for bulk operations (updating/deleting large row sets) — prefer
  `ExecuteUpdateAsync`/`ExecuteDeleteAsync` (EF Core 7+) or raw batched SQL over loading
  entities into memory to mutate and save them one by one.
- Using EF Core to build a complex multi-join reporting query where hand-written SQL or a
  dedicated reporting view would be clearer and faster — know when to drop to Dapper/raw SQL
  (see [`database-design`](../data/database-design.md)).
- Compiled queries (`EF.CompiledQuery`) added preemptively without profiling evidence they're
  needed — added complexity for an unmeasured gain.

## Validation Checklist
See [`checklists/database-change-checklist.md`](../../checklists/database-change-checklist.md)
and [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Definition of Done
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Example
See [Code Standards](#code-standards).

## Related Skills
[`efcore-performance`](../data/efcore-performance.md),
[`efcore-migrations`](../data/efcore-migrations.md),
[`database-design`](../data/database-design.md), [`sql-server`](../data/sql-server.md),
[`postgresql`](../data/postgresql.md).
