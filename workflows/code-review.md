# Workflow: Code Review

Primary agent: [`code-reviewer`](../agents/code-reviewer.md). Checklist:
[`checklists/code-review-checklist.md`](../checklists/code-review-checklist.md).

1. **Understand intent** — read the PR/diff description or linked issue before the diff
   itself; review against what it's trying to do, not a guessed intent.
2. **Read the diff in context** — against the surrounding code, not in isolation; open
   related files (callers, tests, config) as needed.
3. **Check each category** from
   [`checklists/code-review-checklist.md`](../checklists/code-review-checklist.md):
   correctness, architecture, security (obvious issues), performance (obvious issues),
   maintainability, testing, observability, compatibility, deployment.
4. **Route specialist passes** — if the diff touches auth/secrets/data exposure
   non-trivially, request a full [`security-reviewer`](../agents/security-reviewer.md) pass
   rather than reviewing it shallowly; if there's a suspected performance regression needing
   profiling to confirm, route to [`performance-engineer`](../agents/performance-engineer.md).
5. **Rank findings by severity** (BLOCKER/HIGH/MEDIUM/LOW/INFO) — report even INFO-level
   observations separately from blockers so they don't get lost, but don't let them block
   merge.
6. **Report** using the finding format in
   [`checklists/code-review-checklist.md`](../checklists/code-review-checklist.md): category,
   severity, location, problem, impact, recommendation.

## When this runs
- Before merging any PR in a project with `config.yaml` `rules.require_tests`/architecture
  strictness enabled.
- As a self-review pass by the implementing agent before declaring a task done, even without
  a separate reviewer agent invoked.

## Exit criteria
No open BLOCKER or HIGH findings; MEDIUM/LOW findings either addressed or explicitly
accepted with a stated reason.
