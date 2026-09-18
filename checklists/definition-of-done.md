# Definition of Done

The universal baseline every task must satisfy before it's reported complete. Individual
skills add task-specific items on top of this; they never replace it. See
[AGENTS.md §6](../AGENTS.md#6-definition-of-done).

- [ ] **Requirements understood** — the actual requirement was confirmed, not assumed; any
      ambiguity that would change the outcome was raised, not guessed at.
- [ ] **Architecture respected** — the change fits the project's actual architecture
      (`config.yaml`/discovery), doesn't violate the dependency rule, and doesn't introduce
      an unrequested pattern.
- [ ] **Implementation complete** — no half-finished branches, no `TODO` standing in for
      required behavior.
- [ ] **Tests added/updated** — new/changed behavior has coverage per
      [`rules/testing.md`](../rules/testing.md); failure and authorization-boundary paths
      included where applicable.
- [ ] **Security considered** — input validated at the boundary, authorization checked at
      the point of execution, no secret/credential exposure introduced (see
      [`checklists/security-review-checklist.md`](security-review-checklist.md) for
      anything touching auth/data handling).
- [ ] **Performance considered** — no obvious N+1, no unbounded query/collection, no
      blocking-on-async introduced.
- [ ] **Logging/observability considered** — errors and significant state changes are
      logged at an appropriate level; nothing sensitive is logged.
- [ ] **Documentation updated where required** — README/API docs/ADR updated if the change
      affects what they describe (see `templates/docs/`).
- [ ] **Build passes.**
- [ ] **Tests pass** (or, without execution access, the exact commands to run are stated).
- [ ] **No unrelated changes** — the diff contains only what the task required.
- [ ] **Deployment impact identified** — migrations, config changes, or breaking changes
      that affect deployment are called out explicitly, not left for someone else to
      discover.
- [ ] **Implementation summary provided** — what changed, what was reused vs. added, what
      was intentionally left out, and what the human should verify.
