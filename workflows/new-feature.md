# Workflow: New Feature

The default path from a requirement to a shipped, tested feature. Do not skip from
requirement to code — each stage gates the next.

```text
Requirement
  -> Clarify assumptions
  -> Inspect existing code
  -> Identify architecture boundary
  -> Design
  -> Implementation plan
  -> Implementation
  -> Unit tests
  -> Integration tests
  -> Security review
  -> Code review
  -> Build
  -> Validation
  -> Documentation
```

## 1. Requirement
Restate what's being asked in concrete terms: what changes, for whom, under what
constraints.

## 2. Clarify assumptions
State anything ambiguous. Ask (per [`AGENTS.md §1.9`](../AGENTS.md#1-global-behavior-rules))
only when the ambiguity would materially change architecture, correctness, or security —
otherwise state the assumption and proceed.

## 3. Inspect existing code
Run [`workflows/project-discovery.md`](project-discovery.md) if not already done this
session; otherwise, inspect the specific area this feature touches — existing patterns,
tests, related code.

## 4. Identify architecture boundary
Which layer(s)/module(s) does this touch? Does it fit the existing structure, or does it
require an [`architect`](../agents/architect.md) decision? See
[`rules/architecture.md`](../rules/architecture.md).

## 5. Design
For anything beyond a trivial change: the shape of the solution — new types, new
interfaces, what's reused vs. added.

## 6. Implementation plan
A short ordered list of what will change, so the change stays scoped to what's planned.

## 7. Implementation
[`dotnet-developer`](../agents/dotnet-developer.md) (plus [`api-engineer`](../agents/api-engineer.md)/
[`database-engineer`](../agents/database-engineer.md) if the feature touches those areas).
Smallest change that satisfies the requirement, matching existing conventions.

## 8. Unit tests
[`test-engineer`](../agents/test-engineer.md) or the implementer — business logic and
branching, per [`skills/testing/unit-testing.md`](../skills/testing/unit-testing.md).

## 9. Integration tests
For anything crossing a real boundary — per
[`skills/testing/integration-testing.md`](../skills/testing/integration-testing.md).

## 10. Security review
Required when the feature touches auth, secrets, or user input non-trivially — see
[`config.yaml`](../config/config.schema.json) `rules.require_security_review`.
[`security-reviewer`](../agents/security-reviewer.md).

## 11. Code review
[`code-reviewer`](../agents/code-reviewer.md) — see
[`workflows/code-review.md`](code-review.md).

## 12. Build
Confirm it compiles cleanly; no new warnings.

## 13. Validation
Run the test suite; manually verify the golden path if UI/frontend is involved (see
`skills/frontend/*`).

## 14. Documentation
Update README/API docs/ADR per [`documentation-engineer`](../agents/documentation-engineer.md)
if the change affects what they describe.

## Exit criteria
[`checklists/definition-of-done.md`](../checklists/definition-of-done.md).
