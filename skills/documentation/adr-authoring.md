# Skill: ADR Authoring

## Purpose
Record structurally significant architecture decisions so the reasoning survives past the
session that made them — using [`templates/docs/adr.template.md`](../../templates/docs/adr.template.md).

## When to Use
Any decision [`architect`](../../agents/architect.md) identifies as structurally
significant: a new architectural pattern adopted, a new module/service boundary, a
significant technology choice, a reversal of a previous decision.

## Prerequisites
[`technical-writing`](../documentation/technical-writing.md).

## Inputs Required
The decision, the options actually considered, and the concrete tradeoffs that drove the
choice.

## Engineering Principles
- An ADR records a decision **and the context that made it correct at the time** — a future
  reader (possibly re-evaluating the same decision) needs to understand not just what was
  chosen but what constraints made it the right choice then, some of which may no longer
  hold.
- Record real alternatives that were genuinely considered, with their actual tradeoffs — an
  ADR with no alternatives ("we chose X") provides much less value than one showing what was
  weighed and why it lost.
- ADRs are immutable once accepted — a changed decision gets a **new** ADR that supersedes
  the old one (with a link both ways), not an edit to the original; this preserves the
  historical record of what was actually decided when.
- Not every decision needs an ADR — reserve it for decisions that are expensive to reverse,
  affect multiple teams/modules, or that someone is likely to question later ("why didn't we
  just use X").

## Step-by-Step Workflow
1. Confirm the decision is structurally significant enough to warrant an ADR (see
   [`architect`](../../agents/architect.md)'s judgment).
2. Use [`templates/docs/adr.template.md`](../../templates/docs/adr.template.md).
3. Fill in Context (the situation/constraints that made a decision necessary), Decision
   (what was chosen, stated plainly), Alternatives (what else was considered and why it
   lost), Consequences (what this decision makes easier/harder going forward), and Risks.
4. Set Status (`Proposed`/`Accepted`/`Superseded`).
5. If this supersedes a prior ADR, link both directions and update the old one's status.

## Code Standards
Not applicable — structural document, not code. See the template for exact section
requirements.

## Architecture Constraints
ADRs live in a consistent location (typically `docs/adr/` in the consumer project),
numbered sequentially (`ADR-0001`, `ADR-0002`, ...).

## Security Considerations
None beyond the general documentation rule of never recording actual secret values.

## Testing Requirements
Not applicable.

## Common Mistakes
- An ADR with no real alternatives considered — reads as post-hoc justification rather than
  a genuine decision record.
- Editing an old ADR in place instead of writing a new one that supersedes it, losing the
  historical record of what was actually decided at the time.
- Writing an ADR for a routine, easily-reversible decision that didn't need one, adding
  process overhead without corresponding value.

## Anti-Patterns
- ADRs written long after the decision was made, reconstructed from memory rather than
  reflecting the actual reasoning at decision time.

## Validation Checklist
- [ ] Status is set.
- [ ] At least one real alternative is documented with its tradeoff.
- [ ] Consequences (both positive and negative) are stated, not just the upside.
- [ ] If superseding a prior ADR, both are cross-linked.

## Definition of Done
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Example
See [`templates/docs/adr.template.md`](../../templates/docs/adr.template.md).

## Related Skills
[`technical-writing`](../documentation/technical-writing.md).
