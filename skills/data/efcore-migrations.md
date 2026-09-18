# Skill: EF Core Migrations

## Purpose
Produce migrations that are reviewed for their actual generated SQL, safe to apply against a
live database, and have a real rollback path.

## When to Use
Any entity/configuration change that requires a schema change.

## Prerequisites
[`efcore-fundamentals`](../data/efcore-fundamentals.md),
[`workflows/database-change.md`](../../workflows/database-change.md) for the broader
deployment-safety process this skill feeds into.

## Inputs Required
The entity/configuration change, and the current migration history.

## Engineering Principles
- `dotnet ef migrations add <Name>` generates `Up()`/`Down()` from the model diff — this is
  a draft to review, not a final artifact to trust blindly.
- Read the generated SQL: EF Core sometimes chooses drop-and-recreate for what looks like a
  simple rename (it can't always infer rename vs. drop+add) — if a rename was intended, use
  `.RenameColumn()`/`.RenameTable()` explicitly or the migration will silently lose data.
  data.
- A migration that adds a `NOT NULL` column to an existing table with data needs either a
  `DEFAULT` value or a data migration step first — otherwise it fails against existing rows.
- For a breaking change against a live system, split into multiple migrations following the
  expand/contract sequence in
  [`workflows/database-change.md`](../../workflows/database-change.md) rather than one
  migration that both adds and removes structure atomically.
- Keep migrations small and focused — one conceptual schema change per migration, so a
  problematic one can be identified and rolled back precisely.

## Step-by-Step Workflow
1. Change the entity/`IEntityTypeConfiguration<T>`.
2. `dotnet ef migrations add <DescriptiveName>`.
3. Open the generated migration file; read `Up()` line by line against what was actually
   intended.
4. If EF Core chose drop+recreate for an intended rename, fix it explicitly with
   `migrationBuilder.RenameColumn(...)`.
5. For a new required column on a populated table: add it nullable or with a `DEFAULT`,
   backfill if needed, then (in a later migration) tighten to `NOT NULL` if required.
6. Verify `Down()` actually reverses `Up()` without data loss where avoidable, or document
   why it can't (e.g. a dropped column's data is unrecoverable on rollback — call that out).
7. Apply to a local/test database and verify.

## Code Standards
```csharp
// Multi-step: add nullable, backfill, then tighten in a LATER migration/deploy
// Migration 1
migrationBuilder.AddColumn<string>(
    name: "Region",
    table: "Customers",
    type: "varchar(50)",
    nullable: true);

// (Application code deployed that writes Region on new/updated rows;
//  a data migration or background job backfills existing rows)

// Migration 2 (later deploy, after backfill is confirmed complete)
migrationBuilder.AlterColumn<string>(
    name: "Region",
    table: "Customers",
    type: "varchar(50)",
    nullable: false,
    defaultValue: "unknown");
```

## Architecture Constraints
Migrations live in Infrastructure, generated from `IEntityTypeConfiguration<T>` classes that
also live there — Domain entities remain unaware of the migration history.

## Security Considerations
Never include real production data (sample rows, seeded secrets) in a migration file meant
to run across environments.

## Testing Requirements
Apply the migration against a real/containerized instance of the target engine in CI or
locally before merging — a migration that only "looks right" in the generated C# can still
fail against real data/constraints.

## Common Mistakes
- Trusting the generated migration without reading its SQL, missing an unintended
  drop-and-recreate.
- Adding a `NOT NULL` column with no default against a populated table, failing at
  apply-time.
- Migrations that depend on application code having already run (assuming data is in a state
  only the new application version would produce).

## Anti-Patterns
- Squashing many unrelated schema changes into one enormous migration, making it hard to
  isolate what caused a problem if the migration fails partway or needs rollback.

## Validation Checklist
See [`checklists/database-change-checklist.md`](../../checklists/database-change-checklist.md).

## Definition of Done
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Example
See [Code Standards](#code-standards).

## Related Skills
[`efcore-fundamentals`](../data/efcore-fundamentals.md), [`database-design`](../data/database-design.md).
