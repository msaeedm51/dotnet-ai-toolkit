# Skill: Git Workflow

## Purpose
Use git in a way that keeps history readable and useful — focused commits, clear messages,
safe branch operations — matching [`rules/git.md`](../../rules/git.md).

## When to Use
Any commit, branch, or PR operation.

## Prerequisites
None beyond repository access.

## Inputs Required
The change being committed, and the project's existing branch/commit conventions (branch
naming, commit message style, whether it uses conventional commits).

## Engineering Principles
- One logical change per commit — a commit should be revertable/cherry-pickable
  independently without pulling in unrelated changes.
- A commit message explains *why*, not just *what* — the diff already shows what changed;
  the message should carry context the diff can't (the motivating requirement, a
  non-obvious constraint that shaped the approach).
- Never force-push to a shared/main branch. Force-pushing a feature branch only the author
  is working on (e.g. after an interactive rebase to clean up history before review) is
  more acceptable, but still confirm no one else has already pulled it.
- Never rewrite published history other collaborators may have already pulled, without
  explicit coordination.
- Match the project's existing convention — if it uses Conventional Commits
  (`feat:`/`fix:`/`chore:`), follow that; if it doesn't, don't introduce it unilaterally
  mid-project.

## Step-by-Step Workflow
1. Check `git status`/`git diff` before committing — confirm only the intended files are
   staged, no debug code, no accidental inclusion of secrets/`.env` files.
2. Write a commit message following the project's convention, explaining why.
3. For a PR: keep it scoped to one logical change; if it's grown to cover multiple unrelated
   things, split it.
4. Before a destructive operation (`reset --hard`, `checkout` that discards changes,
   `clean`), check `git status` first and stash/commit anything at risk.

## Code Standards
```bash
git status
git diff --staged

git add src/Orders.Application/Orders/CreateOrderHandler.cs src/Orders.Application/Orders/CreateOrderHandlerTests.cs
git commit -m "$(cat <<'EOF'
Reject orders with zero-quantity lines at the handler level

Previously only the API layer validated this; a direct Application-layer
caller (the batch import job) could create invalid orders. Moving the
check into the handler closes that gap for all callers.
EOF
)"
```

## Architecture Constraints
None — this is a process skill, not a code-structure one.

## Security Considerations
Never commit `.env` files, credentials, or connection strings. If one is discovered already
committed, treat it as compromised: rotate the credential — removing it from history alone
doesn't undo an already-possible exposure (see [`rules/git.md`](../../rules/git.md)).

## Testing Requirements
Not applicable.

## Common Mistakes
- A commit bundling an unrelated refactor with the actual bug fix, making the diff harder to
  review and revert independently.
- A commit message that just repeats the file names changed, with no explanation of why.
- Force-pushing to a branch others have already based work on.

## Anti-Patterns
- Squashing an entire feature's history into one giant, unreviewable commit at the end
  instead of a series of focused commits (unless the project's convention is explicitly
  squash-merge, in which case the PR's commits during review can be more granular, and only
  the final squash message needs to carry the summary).

## Validation Checklist
See [`checklists/pr-checklist.md`](../../checklists/pr-checklist.md).

## Definition of Done
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Example
See [Code Standards](#code-standards).

## Related Skills
None — foundational process skill referenced by every workflow.
