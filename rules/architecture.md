# Architecture Rules

See [`skills/architecture/clean-architecture.md`](../skills/architecture/clean-architecture.md)
and the other `skills/architecture/*` files for reasoning and examples.

- Dependencies point inward: Domain depends on nothing project-specific; Application depends
  on Domain; Infrastructure and Presentation depend on Application/Domain, never the reverse.
- Domain must not depend on Infrastructure, a specific ORM, or a specific web framework.
- Do not introduce a new architectural pattern (CQRS, event sourcing, a new service
  boundary, a new abstraction layer) without a concrete requirement driving it.
- Prefer the simplest architecture that satisfies the current, actual requirements — not the
  anticipated future ones.
- Respect module/bounded-context boundaries declared in the project; no direct cross-module
  database access where the boundary prohibits it.
- A shared/common library holds genuinely shared concepts, not a dumping ground to avoid
  duplicating a few lines across unrelated modules.
- Record structurally significant architecture decisions as an ADR
  (see [`templates/docs/adr.template.md`](../templates/docs/adr.template.md)).
- A project reference that violates the dependency rule is a defect to fix, not a precedent
  to extend.
