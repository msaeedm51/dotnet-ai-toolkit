# Pull Request Checklist

For a PR in a consumer .NET project using this toolkit (not to be confused with
[`.github/PULL_REQUEST_TEMPLATE.md`](../.github/PULL_REQUEST_TEMPLATE.md), which is this
toolkit's own PR template).

- [ ] One logical change; unrelated changes split into separate PRs
      ([`rules/git.md`](../rules/git.md)).
- [ ] Builds cleanly with no new warnings introduced.
- [ ] All tests pass locally; new/changed behavior has test coverage
      ([`rules/testing.md`](../rules/testing.md)).
- [ ] Any new migration has been reviewed for its generated SQL and states a rollback path
      ([`checklists/database-change-checklist.md`](database-change-checklist.md)).
- [ ] No secrets, credentials, or `.env` files in the diff.
- [ ] No unrelated formatting/reformatting noise obscuring the real change.
- [ ] Breaking changes to a public contract (API, config key, schema) are explicitly called
      out in the PR description.
- [ ] Documentation updated where the change affects it (README, API docs, ADR).
- [ ] PR description states what changed, why, and how it was verified — not just a
      restatement of the diff.
- [ ] Satisfies [`checklists/definition-of-done.md`](definition-of-done.md).
