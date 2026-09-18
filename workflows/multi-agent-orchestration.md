# Workflow: Multi-Agent Orchestration

Formalizes how information flows between agents when a task needs more than one. Read
[`AGENTS.md §7`](../AGENTS.md#7-agent-roster) first — an agent here is a lens/role, not a
separate model; a single agent can apply multiple lenses in sequence within one session.

## When to use more than one agent

Use a sequence when the task genuinely spans concerns a single lens would do poorly (e.g. an
implementer self-grading their own security review). **Do not** default to a long agent
chain for a small, well-understood change — a one-line bug fix doesn't need
Architect → Developer → Security → Reviewer; it needs
[`workflows/bug-fix.md`](bug-fix.md) run by `dotnet-developer` with a self-check against the
relevant checklists. Unnecessary multi-agent complexity is itself a cost (more handoffs,
more chances to lose context) — match the sequence to the task's actual risk and scope.

## Standard sequences

### Feature development
```
architect -> dotnet-developer -> test-engineer -> security-reviewer -> code-reviewer
```
- `architect` hands off: boundaries/constraints, chosen pattern, risks, ADR if produced.
- `dotnet-developer` hands off: the implementation, what was reused vs. added, open
  questions.
- `test-engineer` hands off: coverage added, what's intentionally untested and why.
- `security-reviewer` hands off: findings (if any) with severity; blockers must be resolved
  before proceeding.
- `code-reviewer` hands off: final findings list; this is typically the last gate before
  merge.

Skip `security-reviewer` only when the feature genuinely doesn't touch auth, secrets, or
user input (per `config.yaml` `rules.require_security_review` and the implementer's own
judgment) — see [`workflows/new-feature.md`](new-feature.md).

### Production bug
```
investigator -> dotnet-developer -> test-engineer -> code-reviewer
```
"Investigator" is whichever agent fits the symptom's domain
(`dotnet-developer`/`database-engineer`/`devops-engineer`/`performance-engineer`) running
[`workflows/bug-fix.md`](bug-fix.md) or [`workflows/production-incident.md`](production-incident.md).
- Investigator hands off: confirmed root cause (not hypothesis), evidence.
- `dotnet-developer` hands off: the fix, scoped to the confirmed cause only.
- `test-engineer` hands off: the regression test.
- `code-reviewer` hands off: final findings.

### Architecture decision
```
architect -> security-reviewer -> performance-engineer -> database-engineer -> architect (final)
```
- Initial `architect` pass: frames the decision, options, and which specialist input is
  actually needed — not every architecture decision needs all three specialists; invoke only
  the ones whose domain the decision materially affects.
- Each specialist hands off: risks/constraints from their domain that affect the decision,
  not a full implementation.
- Final `architect` pass: synthesizes input into the recommendation and ADR.

## Information flow discipline

- Each agent's handoff is written down (in the response, or in an ADR/PR description for
  anything persisted) — not held implicitly across an assumed shared context.
- A downstream agent trusts an upstream agent's confirmed findings but still applies its own
  lens — `code-reviewer` doesn't skip its security category just because
  `security-reviewer` already ran, though it can reference "security-reviewer already
  covered this" rather than duplicating a full pass.
- If a downstream agent finds something that invalidates an upstream decision (e.g.
  `database-engineer` finds the proposed schema can't support a requirement `architect`
  assumed it could), escalate back rather than silently working around it.

## Related
[`AGENTS.md §7`](../AGENTS.md#7-agent-roster) for the full agent roster and routing;
[`agents/INDEX.yaml`](../agents/INDEX.yaml) for trigger-based agent selection.
