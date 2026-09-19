# Workflow: Database Change

Primary agent: [`database-engineer`](../agents/database-engineer.md). Primary skills:
[`skills/data/efcore-fundamentals.md`](../skills/data/efcore-fundamentals.md) and the
engine-specific skill.

```text
Requirement
  -> Schema impact
  -> Migration strategy
  -> Backward compatibility
  -> Data migration
  -> Application changes
  -> Testing
  -> Rollback strategy
  -> Deployment
```

## 1. Requirement
What the schema needs to support, and the actual data volumes/access patterns involved —
this drives every decision below.

## 2. Schema impact
What tables/columns/indexes/constraints change. Verify against the actual current schema
(migrations history or live inspection), never assumed.

## 3. Migration strategy
For a **new, additive** change (new nullable column, new table): usually safe as a single
migration. For a **breaking** change (dropping/renaming a column, adding a `NOT NULL`
column without a default, changing a type): use an **expand/contract** approach —

1. **Expand** — add the new structure alongside the old (new column, new table) without
   removing anything.
2. **Migrate** — backfill data into the new structure; deploy application code that writes
   to both old and new (or reads from new, falls back to old) during the transition.
3. **Contract** — once all application instances are on the new code path and backfill is
   complete, remove the old structure in a later, separate deployment.

This avoids a single deploy that requires the schema and application code to change
atomically, which isn't possible in a rolling/zero-downtime deployment.

## 4. Backward compatibility
Will the *previous* application version still function correctly against the *new* schema
during a rolling deploy window? If not, the migration strategy needs another expand/contract
step.

## 5. Data migration
If existing rows need transformation: state the backfill approach, its expected duration at
production volume, and whether it needs to run in batches to avoid long locks or timeouts.

## 6. Application changes
What application code changes alongside the schema (repository, `DbContext` configuration,
queries) — see [`skills/data/efcore-fundamentals.md`](../skills/data/efcore-fundamentals.md).

## 7. Testing
Integration test against a real/containerized instance of the target engine — see
[`skills/testing/integration-testing.md`](../skills/testing/integration-testing.md). The
in-memory EF Core provider does not validate this.

## 8. Rollback strategy
The migration's `Down()` (or manual rollback script) is reviewed and actually works — not a
no-op that would silently lose data if invoked.

## 9. Deployment
State expected locking behavior and duration on the production-scale table, and whether this
is safe to apply without a maintenance window. Coordinate with
[`devops-engineer`](../agents/devops-engineer.md) for the deployment sequencing (schema
change before/after/alongside the app deploy) implied by the migration strategy chosen above.

## Exit criteria
[`checklists/database-change-checklist.md`](../checklists/database-change-checklist.md).
