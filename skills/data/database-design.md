# Skill: Database Design

## Purpose
Design schemas that are correct, appropriately normalized, and matched to how the data is
actually accessed — and know when to reach for something other than the default ORM.

## When to Use
Designing a new table/schema, or evaluating whether an existing schema fits new
requirements.

## Prerequisites
The database engine in use (`config.yaml` `backend.database.engine`) — see
[`sql-server`](../data/sql-server.md)/[`postgresql`](../data/postgresql.md) for engine
specifics this skill doesn't duplicate.

## Inputs Required
The data being modeled, its access patterns (read-heavy? write-heavy? reporting?), and the
database's role in this project (`primary`/`legacy`/`read-only`/`reporting`/`external` per
`config.yaml`).

## Engineering Principles
- Normalize to eliminate update anomalies (3NF as a default target); denormalize
  deliberately, with a stated reason (read performance for a specific hot query, a reporting
  table), not by default.
- Model constraints in the database, not only in application code — a `NOT NULL`, a foreign
  key, a `CHECK` constraint, or a uniqueness constraint catches bugs no application-layer
  validation can fully guarantee under concurrent writes.
- Choose a primary key type deliberately: a surrogate key (auto-increment/sequential id) for
  most tables; a natural key only when it's genuinely immutable and unique; be aware of the
  insert-pattern implications of random vs. sequential keys (see the engine-specific skill).
- Distinguish the database's role: a `primary` database is designed and evolved by this
  project; a `legacy` database is inherited and its schema is typically off-limits for
  redesign; a `read-only`/`reporting` database is queried but never written to by this
  service; an `external` database belongs to another system entirely — never assume write
  access or schema-change rights outside `primary`.

## Step-by-Step Workflow
1. Identify the entities and their relationships from the actual requirement.
2. Normalize; identify any deliberate denormalization with a stated performance/reporting
   reason.
3. Define constraints (NOT NULL, foreign keys, unique, check) matching the actual business
   rules.
4. Choose indexes matching the access patterns that are actually needed (see the
   engine-specific skill for index type/plan guidance).
5. Consider the database's role — don't design a schema change into a `legacy`/`read-only`
   database without explicit confirmation this is actually in scope.

## Code Standards
```sql
CREATE TABLE orders (
    id UUID PRIMARY KEY,
    customer_id UUID NOT NULL REFERENCES customers(id),
    status VARCHAR(20) NOT NULL CHECK (status IN ('draft', 'confirmed', 'cancelled')),
    total NUMERIC(12,2) NOT NULL DEFAULT 0 CHECK (total >= 0),
    created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE order_lines (
    id UUID PRIMARY KEY,
    order_id UUID NOT NULL REFERENCES orders(id) ON DELETE CASCADE,
    sku VARCHAR(64) NOT NULL,
    quantity INT NOT NULL CHECK (quantity > 0),
    unit_price NUMERIC(12,2) NOT NULL CHECK (unit_price >= 0)
);
```

## Architecture Constraints
Cross-module database access is prohibited where module boundaries prohibit it (see
[`modular-monolith`](../architecture/modular-monolith.md)) — a schema design shouldn't create
a table another module directly joins into.

## Security Considerations
Column-level sensitivity matters — flag columns holding PII/secrets so they get appropriate
encryption-at-rest, masking, and access-control treatment, and are excluded from broad
`SELECT *`-style reporting exports.

## Testing Requirements
Schema constraints are best verified with an integration test that attempts to violate them
(insert a duplicate where unique is expected, a negative quantity) and confirms the database
rejects it — application-layer validation alone isn't proof the constraint exists.

## Common Mistakes
- Relying solely on application-layer validation for a rule that should also be a database
  constraint, allowing a bug or a second write path to corrupt data.
- Denormalizing prematurely, before there's a measured reason, creating update-anomaly risk
  for no proven benefit.
- Treating a `legacy`/`external` database's schema as freely changeable.

## Anti-Patterns
- An "entity-attribute-value" (EAV) generic schema used to avoid schema changes — trades
  real constraints and query performance for flexibility that's rarely worth the cost;
  prefer proper columns/tables, with JSON/JSONB columns for genuinely semi-structured data
  only.

## Validation Checklist
See [`checklists/database-change-checklist.md`](../../checklists/database-change-checklist.md).

## Definition of Done
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Example
See [Code Standards](#code-standards).

## Related Skills
[`sql-server`](../data/sql-server.md), [`postgresql`](../data/postgresql.md),
[`efcore-fundamentals`](../data/efcore-fundamentals.md).
