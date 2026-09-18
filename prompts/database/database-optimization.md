# Prompt: Database Query Optimization

## Purpose
Optimize a slow query with evidence (execution plan, query count), per
[`skills/data/efcore-performance.md`](../../skills/data/efcore-performance.md) and the
engine-specific skill.

## When to Use
A confirmed slow query/database operation.

## Loads
Agent: [`database-engineer`](../../agents/database-engineer.md). Skills:
[`efcore-performance`](../../skills/data/efcore-performance.md),
[`sql-server`](../../skills/data/sql-server.md)/[`postgresql`](../../skills/data/postgresql.md).

## Parameters
- `{{QUERY_OR_OPERATION}}` — the slow query/operation, ideally with actual code/SQL.
- `{{EVIDENCE}}` — execution plan, query count, or timing data if available; state "none
  yet" otherwise.

## Prompt Template
```
Load the .NET engineering toolkit's database-engineer agent and optimize:

{{QUERY_OR_OPERATION}}

Evidence available: {{EVIDENCE}}

1. If no execution plan/evidence is available yet, tell me exactly how to obtain it
   (EXPLAIN ANALYZE / execution plan capture) before proposing a fix.
2. Identify the actual bottleneck from the evidence -- missing index, N+1 pattern,
   over-fetching, poor isolation/locking behavior -- don't guess.
3. Propose the specific fix (index, query restructure, projection, AsNoTracking, etc.)
   and explain its mechanism of improvement.
4. Verify query correctness against this project's actual database engine, not only an
   in-memory provider.
5. Re-measure (query count, execution plan, or timing) to confirm improvement.

Do not trade correctness/consistency for speed without flagging that tradeoff explicitly.
```

## Expected Output
A confirmed bottleneck (from evidence), a targeted fix, and before/after measurement.

## Related
[`prompts/review/performance-review.md`](../review/performance-review.md).
