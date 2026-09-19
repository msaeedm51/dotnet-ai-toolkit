# Prompt: Architecture Analysis

## Purpose
Get a grounded architecture recommendation for a new capability or a proposed structural
change, using the [`architect`](../../agents/architect.md) agent's discipline (options
considered, tradeoffs named, no pattern adopted without a concrete reason).

## When to Use
Before implementing a new module/service, adopting a new pattern, or making any decision
that would be expensive to reverse later.

## Loads
Agent: [`architect`](../../agents/architect.md). Workflow:
[`project-discovery`](../../workflows/project-discovery.md) if not already run this session.
Rules: [`rules/architecture.md`](../../rules/architecture.md).

## Parameters
- `{{REQUIREMENT}}` — the capability/change needed, in plain terms.
- `{{CONSTRAINTS}}` — known constraints (team size, deadline, existing architecture, scale
  target) — state "none known, please ask if it matters" if unclear.

## Prompt Template
```text
Load the .NET engineering toolkit's architect agent (agents/architect.md) and its
supporting rules (rules/architecture.md).

Requirement: {{REQUIREMENT}}
Known constraints: {{CONSTRAINTS}}

Before proposing anything:
1. Confirm you understand this project's current architecture (run project discovery
   if you haven't already established it this session).
2. Identify 2-3 viable options scoped to this requirement -- not a full-system redesign
   unless the requirement genuinely calls for one.
3. Evaluate tradeoffs against the actual constraints above, not textbook ideals.
4. Recommend one option with reasoning, and list concrete risks.
5. If this decision is structurally significant, draft an ADR using
   templates/docs/adr.template.md.

Do not adopt a new pattern (CQRS, a new service boundary, a new abstraction layer)
without a concrete reason from the requirement above. If ambiguity in the requirement
would change your recommendation, ask before proceeding.
```

## Expected Output
A short options-and-tradeoffs writeup, a concrete recommendation naming affected files/
modules, a risk list, and (if warranted) an ADR draft.

## Related
[`workflows/multi-agent-orchestration.md`](../../workflows/multi-agent-orchestration.md)
(Architecture Decision sequence).
