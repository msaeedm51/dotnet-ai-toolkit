# Code Review Checklist

Used by the [`code-reviewer`](../agents/code-reviewer.md) agent and as a self-review pass by
any agent before calling a change done. See [`workflows/code-review.md`](../workflows/code-review.md)
for the process this checklist fits into.

## Severity levels

- **BLOCKER** — must not merge as-is: breaks behavior, introduces a security hole, breaks
  the build or existing tests.
- **HIGH** — should be fixed before merge: significant correctness, security, or
  architecture risk that isn't immediately breaking.
- **MEDIUM** — should be fixed soon: maintainability or design issue with real but bounded
  cost.
- **LOW** — worth fixing, not blocking: minor inconsistency or missed convention.
- **INFO** — observation, no action required: a note for context or future consideration.

No arbitrary numeric score is generated — severity + count is the signal.

## Categories

**Correctness**
- [ ] Does the code do what it's supposed to, including stated edge cases?
- [ ] Are error conditions handled, not swallowed or silently ignored?

**Architecture**
- [ ] Respects the project's dependency direction and layering
      ([`rules/architecture.md`](../rules/architecture.md))?
- [ ] No new abstraction introduced without a second concrete use case?

**Security**
- [ ] No hard-coded secret, credential, or connection string?
- [ ] Authorization checked at the actual point of execution for sensitive operations?
- [ ] Input validated at the boundary? (Full pass: route to `security-reviewer` if the diff
      touches auth, secrets, or user input non-trivially —
      [`checklists/security-review-checklist.md`](security-review-checklist.md).)

**Performance**
- [ ] No obvious N+1 query pattern?
- [ ] No sync-over-async (`.Result`/`.Wait()`)?
- [ ] No unbounded collection/query without pagination? (Full pass: route to
      `performance-engineer` for anything needing profiling to confirm.)

**Maintainability**
- [ ] Naming, structure, and complexity consistent with the surrounding codebase?
- [ ] No duplicated logic that should be extracted (or over-extracted into unneeded
      abstraction)?

**Testing**
- [ ] New/changed behavior has test coverage, including failure paths?
- [ ] Tests actually assert behavior, not just that mocks were called?

**Observability**
- [ ] Errors and significant state changes are logged at an appropriate level?
- [ ] No sensitive data logged?

**Compatibility**
- [ ] Any breaking change to an existing contract, schema, or config key explicitly called
      out?

**Deployment**
- [ ] Migration, configuration, or infrastructure impact identified and has a rollback path?

## Finding format

Each finding: **category | severity | location | problem | impact | recommendation** — every
finding must point at a specific location and describe a concrete failure scenario or cost,
not a vague preference.
