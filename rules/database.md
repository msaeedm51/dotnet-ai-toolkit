# Database Rules

See [`skills/data/efcore-fundamentals.md`](../skills/data/efcore-fundamentals.md),
[`skills/data/sql-server.md`](../skills/data/sql-server.md), and
[`skills/data/postgresql.md`](../skills/data/postgresql.md) for reasoning and examples.

- Never assume a schema — verify against migrations, the `DbContext`, or the actual database
  before writing a query or migration against it.
- Every migration states its rollback path before it's considered done.
- Use parameterized queries or EF Core LINQ always; never string-concatenate user input into
  SQL.
- (EF Core) Add an index to match an actual query predicate; do not add one speculatively.
- (EF Core) Use `AsNoTracking()` for read-only queries.
- Wrap multi-statement writes in a transaction sized to the minimum scope needed — long-held
  transactions increase lock contention.
- Treat a database whose `config.yaml` `role` is `legacy` or `read-only` as change-nothing by
  default; schema changes there require explicit confirmation.
- Verify query correctness against a real or containerized database engine — an in-memory
  provider passing is not evidence a query works against the real engine.
- A schema change that requires downtime or long-held locks on a production-scale table is
  flagged and planned per [`workflows/database-change.md`](../workflows/database-change.md)
  before it's implemented, not after.
