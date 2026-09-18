# Skill: SQL Server

## Purpose
Apply SQL Server-specific knowledge — indexing, execution plans, isolation levels, locking —
that's distinct from generic database advice.

## When to Use
Schema design, query optimization, or concurrency/locking issues on a project where
`config.yaml backend.database.engine: sql-server`.

## Prerequisites
Access to SQL Server Management Studio, Azure Data Studio, or `SET STATISTICS IO/TIME ON` +
execution plan output for anything performance-related — don't optimize blind.

## Inputs Required
The schema/query in question, and ideally its actual execution plan or `STATISTICS IO`
output for performance work.

## Engineering Principles
- Clustered index defines physical row order — choose it deliberately (usually the primary
  key, but not always; a monotonically increasing key like `IDENTITY`/sequential `GUID`
  avoids page-split fragmentation that a random `NEWID()` clustered key causes).
- Non-clustered indexes should match actual query predicates (`WHERE`, `JOIN`, `ORDER BY`)
  — an index that doesn't match a real query pattern is pure write-cost with no benefit.
- Use `INCLUDE` columns on a non-clustered index to make it covering for a hot query,
  avoiding a key lookup.
- Default isolation level is `READ COMMITTED`, which still blocks readers behind writers.
  For read-heavy workloads with write contention, `READ COMMITTED SNAPSHOT ISOLATION`
  (RCSI) — enabled at the database level — removes most reader/writer blocking at the cost
  of tempdb version-store overhead; it's a database-level, not per-query, decision — confirm
  with the team before enabling on an existing production database.
- Pagination: `OFFSET`/`FETCH NEXT` is fine for shallow pages; for deep pagination on large
  tables, keyset pagination (`WHERE Id > @lastId ORDER BY Id`) avoids the cost of scanning
  and discarding all preceding rows.

## Step-by-Step Workflow
1. For a new table: define the clustered index (usually PK) with fragmentation in mind.
2. For a slow query: capture the actual execution plan (`SET SHOWPLAN_XML ON` or the GUI) —
   look for table scans on large tables, key lookups, and high estimated-vs-actual row count
   mismatches (stale statistics).
3. Add/adjust a non-clustered index matching the query's `WHERE`/`JOIN`/`ORDER BY` columns;
   consider `INCLUDE` for covering.
4. Re-run and compare `STATISTICS IO` (logical reads) before/after.
5. For concurrency/deadlock issues: capture the deadlock graph (`SET DEADLOCK_PRIORITY`
   tracing or Extended Events), identify the two resource-acquisition orders in conflict, and
   fix by ensuring consistent access order or reducing transaction scope.

## Code Standards
```sql
-- Covering index for a hot query filtering by CustomerId, ordered by CreatedAt
CREATE NONCLUSTERED INDEX IX_Orders_CustomerId_CreatedAt
ON Orders (CustomerId, CreatedAt DESC)
INCLUDE (Status, Total);

-- Keyset pagination instead of deep OFFSET
SELECT TOP (@pageSize) Id, CustomerId, Total, Status
FROM Orders
WHERE CustomerId = @customerId AND Id > @lastSeenId
ORDER BY Id;
```
```csharp
// Explicit transaction with a bounded, short scope -- reduces lock duration
await using var transaction = await db.Database.BeginTransactionAsync(ct);
try
{
    order.MarkConfirmed();
    await db.SaveChangesAsync(ct);
    await transaction.CommitAsync(ct);
}
catch
{
    await transaction.RollbackAsync(ct);
    throw;
}
```

## Architecture Constraints
Index and isolation-level decisions affect the whole table/database — coordinate with
`database-engineer`/`architect` before changing isolation level on a shared production
database.

## Security Considerations
Use parameterized queries/EF Core always (see
[`security-owasp`](../security/security-owasp.md)). Apply least-privilege SQL logins — an
application connection string should not use `sa`/`db_owner` where a scoped role suffices.

## Testing Requirements
Query changes verified against a real SQL Server instance (Testcontainers'
`mssql` image or a local/dev instance) — see
[`integration-testing`](../testing/integration-testing.md); the in-memory EF Core provider
does not validate real SQL Server behavior.

## Common Mistakes
- Adding an index without checking whether an equivalent one already exists (duplicate/
  overlapping indexes cost writes for no read benefit).
- Optimizing based on estimated row counts without checking if statistics are stale
  (`UPDATE STATISTICS`).
- Wrapping a large batch operation in one giant transaction, holding locks far longer than
  necessary — batch it instead.

## Anti-Patterns
- `NOLOCK` hints sprinkled to "fix" blocking without understanding they permit dirty reads —
  use RCSI instead if the goal is reduced blocking without unbounded read inconsistency.
- Rebuilding all indexes on a schedule without checking actual fragmentation first.

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
