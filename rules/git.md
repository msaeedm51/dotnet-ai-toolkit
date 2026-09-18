# Git Rules

See [`skills/git/git-workflow.md`](../skills/git/git-workflow.md) for reasoning and examples.

- Commit messages explain why a change was made, not just what changed.
- One logical change per commit/PR; do not bundle unrelated changes together.
- Never force-push to a shared branch without explicit confirmation from whoever owns it.
- Never commit secrets, credentials, or `.env` files. If one is committed, treat it as
  compromised — rotate it, don't just remove it in a follow-up commit.
- Branch names and PR titles follow the project's existing convention.
- Do not rewrite published history (`rebase`, `amend`) on a branch others may have already
  pulled, without explicit confirmation.
- A PR description states what changed, why, and how it was verified — not just a restatement
  of the diff.
