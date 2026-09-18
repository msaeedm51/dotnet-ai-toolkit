# Skill: PostgreSQL

## Purpose
Apply PostgreSQL-specific knowledge — indexing options, `EXPLAIN ANALYZE`, isolation levels,
JSONB — distinct from generic database advice and from SQL Server's mechanics.

## When to Use
Schema design, query optimization, or concurrency issues on a project where
`config.yaml backend.database.engine: postgresql`.

## Prerequisites
`EXPLAIN ANALYZE` output (or `psql`/pgAdmin access) for any performance work — don't
optimize blind.

## Inputs Required
The schema/query in question, ideally with real `EXPLAIN ANALYZE` output.

## Engineering Principles
- Tables have no clustered index by default (rows aren't physically reordered by the PK) —
  `CLUSTER` exists but is a one-time, manually-triggered reorder, not a maintained property
  like SQL Server's clustered index.
- Index types beyond B-tree matter here: GIN for JSONB/array/full-text containment queries,
  GiST for range/geometric types, partial indexes (`WHERE` clause on the index) for
  frequently filtered subsets (e.g. `WHERE status = 'pending'`), expression indexes for
  queries filtering on a computed value.
- Default isolation is `READ COMMITTED`, generally with better default read concurrency
  behavior than SQL Server's default due to MVCC — writers don't block readers. Use
  `REPEATABLE READ`/`SERIALIZABLE` deliberately when true consistency guarantees are needed,
  and handle serialization failures (`40001`) with retry logic when using `SERIALIZABLE`.
- `JSONB` is appropriate for genuinely semi-structured data, not as a substitute for proper
  relational modeling of data you'll query/filter/join on regularly — index JSONB paths you
  actually query with a GIN index.
- Pagination: prefer keyset pagination (`WHERE id > $lastId ORDER BY id`) over deep
  `OFFSET`/`LIMIT` for the same reason as SQL Server — `OFFSET` still scans and discards.

## Step-by-Step Workflow
1. For a new table: choose the primary key type deliberately (`bigint`/`uuid`; note UUID v4
   PKs fragment B-tree indexes under high insert rate — consider `uuid` v7/ULID-style
   sequential identifiers for high-throughput insert tables if the project needs it).
2. For a slow query: run `EXPLAIN (ANALYZE, BUFFERS)` — look for sequential scans on large
   tables, high row-estimate vs. actual mismatches (stale statistics, `ANALYZE` needed), and
   nested loop joins over large row counts.
3. Add the appropriate index type for the access pattern (B-tree for equality/range, GIN for
   JSONB/array containment, partial index for a frequently filtered subset).
4. Re-run `EXPLAIN ANALYZE` to confirm the plan changed and cost/timing improved.
5. For concurrency issues: check `pg_stat_activity`/`pg_locks` for blocking; for
   `SERIALIZABLE` transactions, ensure the application retries on serialization failure.

## Code Standards
```sql
-- Partial index for a hot, narrow query
CREATE INDEX idx_orders_pending ON orders (created_at)
WHERE status = 'pending';

-- GIN index for JSONB containment queries
CREATE INDEX idx_orders_metadata ON orders USING GIN (metadata);
SELECT * FROM orders WHERE metadata @> '{"priority": "high"}';

-- Keyset pagination
SELECT id, customer_id, total, status
FROM orders
WHERE customer_id = $1 AND id > $2
ORDER BY id
LIMIT $3;
```
```csharp
// Npgsql: enable retry-on-serialization-failure for SERIALIZABLE transactions
var strategy = db.Database.CreateExecutionStrategy();
await strategy.ExecuteAsync(async () =>
{
    await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
    order.MarkConfirmed();
    await db.SaveChangesAsync(ct);
    await tx.CommitAsync(ct);
});
```

## Architecture Constraints
Index/isolation-level choices affect the whole table — coordinate with `database-engineer`/
`architect` for changes on a shared production database.

## Security Considerations
Parameterized queries always (Npgsql handles this via `NpgsqlParameter`/EF Core). Use
role-based least-privilege database users; don't connect the application as the database
superuser.

## Testing Requirements
Query changes verified against real PostgreSQL (Testcontainers' `postgres` image is the
default recommendation) — see [`integration-testing`](../testing/integration-testing.md).

## Common Mistakes
- Assuming SQL Server's clustered-index mental model applies — PostgreSQL doesn't maintain
  physical row order the same way.
- Using `OFFSET`-based pagination on a large, frequently-appended table and hitting
  increasingly slow responses on later pages.
- Storing data that's regularly filtered/joined on as JSONB "for flexibility" instead of
  proper columns, then fighting query performance later.
- Forgetting to retry `SERIALIZABLE` transactions on `40001` serialization failure.

## Anti-Patterns
- Case-insensitive matching via `LOWER(column) = LOWER(@value)` on every query instead of a
  `citext` column or a functional index, silently disabling index usage on the naive form.
- Adding a GIN index on a JSONB column that's rarely queried by containment, paying the
  write-cost for no benefit.

## Validation Checklist
See [`checklists/database-change-checklist.md`](../../checklists/database-change-checklist.md).

## Definition of Done
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Example
See [Code Standards](#code-standards).

## Related Skills
[`database-design`](../data/database-design.md),
[`efcore-fundamentals`](../data/efcore-fundamentals.md),
[`efcore-performance`](../data/efcore-performance.md).
