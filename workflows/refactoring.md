# Workflow: Refactoring

Change structure without changing behavior. The safety net comes first; the refactor comes
second.

## Before refactoring

1. **Identify current behavior** — what does the code actually do today, including edge
   cases and any surprising-but-intentional behavior? Don't refactor based on what it's
   supposed to do if that's not verified against what it does.
2. **Identify callers** — every caller of what's being changed, in this codebase and, if
   it's a public API, potentially outside it.
3. **Identify tests** — what already covers this code? If coverage is thin, that's the
   safety net gap to close before refactoring, not after.
4. **Identify dependencies** — what this code depends on, and what depends on it.
5. **Identify risk** — how central is this code, how wide is its blast radius, how
   confident is the existing test coverage.
6. **Establish a safety net** — if coverage is insufficient for the risk level, add
   characterization tests (tests that pin down current observable behavior) *before*
   changing structure.

## During refactoring

- Make incremental changes — small enough to verify at each step, not one large rewrite.
- Run tests after each increment.
- Preserve behavior exactly, per [`rules/general.md`](../rules/general.md) — a refactor that
  also changes behavior is two changes bundled into one, which makes both harder to review
  and revert independently.
- Avoid unrelated changes riding along (reformatting untouched code, renaming unrelated
  things) — keep the diff readable as "structure changed, behavior didn't."

## When a refactor reveals a bug

Stop. Decide explicitly: fix it as part of this change (and call it out clearly as a
behavior change, not pure refactoring) or file it separately and continue the refactor
around it. Don't silently fix it inside what's presented as a pure refactor.

## Exit criteria
[`checklists/definition-of-done.md`](../checklists/definition-of-done.md), plus: all
existing tests still pass unmodified (their assertions didn't need to change — if they did,
behavior changed, which needs to be justified separately).
