# Database Change Checklist

Used by [`database-engineer`](../agents/database-engineer.md) and
[`workflows/database-change.md`](../workflows/database-change.md). Mechanisms and examples:
[`skills/data/efcore-fundamentals.md`](../skills/data/efcore-fundamentals.md) and the
engine-specific skill.

- [ ] Schema/query verified against the actual current database, not assumed.
- [ ] Migration's generated `Up()` SQL reviewed, not just that the C# compiles.
- [ ] Migration's `Down()`/rollback path is real and has been considered, not left as a
      no-op that would lose data on rollback.
- [ ] Locking behavior at expected production table size assessed — does this migration
      hold a long lock on a hot table?
- [ ] For a breaking schema change on a live system: an expand/contract approach considered
      (add new nullable/default column → backfill → switch reads → drop old column in a
      later deploy) rather than a single breaking change.
- [ ] Backward compatibility: will the previous application version still run correctly
      against the new schema during a rolling deploy?
- [ ] Data migration (backfill) plan stated if existing rows need transformation, including
      how long it's expected to take at production volume.
- [ ] New/changed queries covered by an integration test against a real/containerized
      database engine, not only an in-memory provider.
- [ ] Indexes added match an actual query predicate; no speculative indexing.
- [ ] Database role (`primary`/`legacy`/`read-only`/`reporting`/`external` per
      `config.yaml`) respected — `legacy`/`read-only` targets require explicit confirmation
      before any change.
- [ ] Deployment/downtime impact stated explicitly, including whether this is safe to run
      against a live database without downtime.
