# Prompt: Database Schema Change / Migration

## Purpose
Plan and implement a schema change safely, per
[`workflows/database-change.md`](../../workflows/database-change.md), including the
expand/contract approach for breaking changes on a live system.

## When to Use
Any schema change: new table/column, index change, constraint change, or a breaking change
to existing structure.

## Loads
Agent: [`database-engineer`](../../agents/database-engineer.md). Workflow:
[`database-change`](../../workflows/database-change.md). Skill:
[`efcore-migrations`](../../skills/data/efcore-migrations.md) if using EF Core.

## Parameters
- `{{SCHEMA_CHANGE}}` — what needs to change and why.
- `{{IS_BREAKING}}` — does this remove/rename/retype an existing column, or tighten
  constraints on populated data (yes/no/unsure).

## Prompt Template
```
Load the .NET engineering toolkit's database-engineer agent and follow
workflows/database-change.md for:

Schema change: {{SCHEMA_CHANGE}}
Breaking change: {{IS_BREAKING}}

1. Verify the current schema against actual migrations/the database -- don't assume it.
2. If this is additive only: implement as a single migration.
3. If this is breaking (or IS_BREAKING is "unsure" -- determine it): use an
   expand/contract approach -- add new structure alongside old, backfill, switch reads,
   THEN remove old structure in a later, separate deployment. Do not combine expand and
   contract into one migration for a live system.
4. Review the migration's generated SQL directly -- confirm it does what's intended
   (especially for renames, which EF Core can silently turn into drop+add).
5. State the rollback path explicitly.
6. State expected locking behavior/duration at production table size, and whether this
   needs a maintenance window.
7. Write an integration test against the real database engine.

Validate against checklists/database-change-checklist.md before reporting done.
```

## Expected Output
A reviewed migration with a stated rollback path, deployment-safety assessment, and
integration test coverage.

## Related
[`prompts/database/database-optimization.md`](database-optimization.md).
