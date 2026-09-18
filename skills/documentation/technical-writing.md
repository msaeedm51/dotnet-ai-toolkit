# Skill: Technical Writing

## Purpose
Write documentation (READMEs, API docs, runbooks, feature docs) that someone unfamiliar with
the current context can act on, using the templates in `templates/docs/`.

## When to Use
Any documentation task — used by
[`documentation-engineer`](../../agents/documentation-engineer.md).

## Prerequisites
The template matching the document type (`templates/docs/*.template.md`); the actual code/
config/behavior being documented, verified rather than assumed.

## Inputs Required
The change/system being documented, and its existing documentation (to update rather than
duplicate).

## Engineering Principles
- Document the **why** and the **non-obvious operational knowledge**, not what well-named
  code already makes clear — restating the code in prose adds a maintenance burden without
  adding information.
- Verify every checkable claim (a command, an endpoint, a config key, a file path) against
  the actual project before writing it — a wrong command in a runbook is worse than no
  runbook, because it wastes time during an incident.
- Write for someone with the general skill but no session-specific context — avoid
  unexplained internal jargon, unresolved pronouns ("it," "that" with no clear referent), or
  assuming the reader was following along with the work that produced the document.
- Prefer concrete, executable steps (an actual command, an actual dashboard link) over vague
  guidance ("check if the service is healthy" → "run `curl -f https://.../health/ready`").
- Keep a document's scope matched to its template — don't let a README grow into an
  architecture document; link to the dedicated one instead.

## Step-by-Step Workflow
1. Identify the document type and load its template from `templates/docs/`.
2. Gather facts by reading the actual code/config/pipeline — don't rely solely on a
   requester's description for anything checkable.
3. Draft following the template's structure.
4. Cross-check against existing documentation for the same area; update it rather than
   creating a duplicate, conflicting document.
5. Flag anything the draft reveals is stale elsewhere.

## Code Standards
Not applicable in the usual sense — the "standard" is structural: follow the relevant
template in `templates/docs/` exactly, and keep prose specific and verified.

## Architecture Constraints
Documentation lives where the project's existing convention puts it (a `docs/` folder,
alongside the code it describes, or a wiki) — match the existing location rather than
introducing a second one.

## Security Considerations
Never document an actual secret value, connection string, or credential — reference where
it's stored (the secret store, the config key name), never the value itself.

## Testing Requirements
Not applicable directly; the "test" is verification — every command/path/endpoint mentioned
should be confirmed to actually work as documented before publishing.

## Common Mistakes
- Documenting intended/aspirational behavior as current behavior.
- A runbook with vague steps that don't actually resolve anything under incident pressure.
- Letting documentation drift out of sync with the code after the code changes.

## Anti-Patterns
- A document that restates the code line-by-line in prose — provides no more information
  than reading the code, at a higher maintenance cost.

## Validation Checklist
- [ ] Every command/endpoint/config key verified against the actual project.
- [ ] Follows the matching template's structure.
- [ ] Doesn't duplicate/conflict with existing documentation elsewhere.

## Definition of Done
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Example
See the templates in `templates/docs/` for structure; this skill governs the prose quality
within them.

## Related Skills
[`adr-authoring`](../documentation/adr-authoring.md).
