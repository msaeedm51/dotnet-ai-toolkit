<!--
Template: Database Documentation
Verify every table/column/constraint against the actual schema (migrations or live
inspection) -- see skills/documentation/technical-writing.md and
skills/data/database-design.md.
-->

# {{DATABASE_NAME}} Database

**Engine:** {{sql-server | postgresql | other}}
**Role:** {{primary | legacy | read-only | reporting | external -- see config.schema.json}}
**ORM:** {{efcore | dapper | none}}

{{If role is legacy/read-only/external: state that explicitly and what that means for
anyone touching this doc -- e.g. "schema changes here require explicit sign-off from
{{owning team}}."}}

## Schema Overview

{{A brief description of the domain this database models, and the major table groups.}}

## Tables

### {{table_name}}

{{What this table represents.}}

| Column | Type | Constraints | Notes |
|---|---|---|---|
| {{column}} | {{type}} | {{PK / FK / NOT NULL / UNIQUE / CHECK}} | {{non-obvious notes}} |

**Indexes:**
| Index | Columns | Type | Purpose |
|---|---|---|---|
| {{name}} | {{columns}} | {{type}} | {{what query it serves}} |

**Relationships:** {{FK relationships to other tables, and cardinality}}

<!-- Repeat the table block above for each significant table. Don't document every
table exhaustively if there are many similar ones -- document the pattern once and list
exceptions. -->

## Migration Strategy

{{How schema changes are made in this project -- EF Core migrations? Manual scripts?
Link to skills/data/efcore-migrations.md or the project's actual process.}}

## Backup and Restore

{{Actual backup schedule/mechanism and restore procedure, or "not applicable -- managed
by {{platform}}."}}

## Known Data Quality Issues

{{Honest list of known inconsistencies, especially for a legacy database -- this saves
the next person from re-discovering them the hard way.}}
