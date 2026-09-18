# Prompt: Feature Implementation

## Purpose
Implement a feature end to end following
[`workflows/new-feature.md`](../../workflows/new-feature.md) — no jumping straight from
requirement to code.

## When to Use
Any new feature or non-trivial change to existing behavior.

## Loads
Agent: [`dotnet-developer`](../../agents/dotnet-developer.md) (plus
[`api-engineer`](../../agents/api-engineer.md)/[`database-engineer`](../../agents/database-engineer.md)
as needed). Workflow: [`new-feature`](../../workflows/new-feature.md).

## Parameters
- `{{REQUIREMENT}}` — what needs to be built, for whom, under what constraints.
- `{{ACCEPTANCE_CRITERIA}}` — how to know it's done correctly (optional but recommended).

## Prompt Template
```
Load the .NET engineering toolkit and follow workflows/new-feature.md for this
requirement:

{{REQUIREMENT}}

Acceptance criteria: {{ACCEPTANCE_CRITERIA}}

Steps:
1. State any assumptions you're making about ambiguous parts of the requirement.
2. Inspect the existing code relevant to this feature before writing anything.
3. Identify which architecture boundary/layer this belongs to.
4. Implement the smallest change that satisfies the requirement, matching this
   project's existing conventions -- reuse existing abstractions where they fit.
5. Write unit tests for the new/changed behavior, and integration tests for anything
   crossing a real boundary (database, HTTP, external service).
6. Self-check against checklists/definition-of-done.md before reporting done.
7. Summarize what changed, what was reused vs. added, and what I should verify.

Do not invent an API, class, database table, or configuration value -- verify it exists
first. Ask if something in the requirement would materially change the architecture or
correctness of the implementation.
```

## Expected Output
A working, tested implementation with a concise summary, or a set of clarifying questions if
the requirement is genuinely ambiguous in a way that affects correctness/architecture.

## Related
[`prompts/coding/api-design.md`](api-design.md) (endpoint-specific variant),
[`prompts/testing/test-generation.md`](../testing/test-generation.md).
