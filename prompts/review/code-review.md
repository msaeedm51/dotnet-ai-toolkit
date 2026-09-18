# Prompt: Code Review

## Purpose
Review a diff/PR structurally, per
[`workflows/code-review.md`](../../workflows/code-review.md) and
[`checklists/code-review-checklist.md`](../../checklists/code-review-checklist.md).

## When to Use
Before merging a PR, or as a self-review pass before declaring a task done.

## Loads
Agent: [`code-reviewer`](../../agents/code-reviewer.md). Checklist:
[`code-review-checklist`](../../checklists/code-review-checklist.md).

## Parameters
- `{{DIFF_OR_PR}}` — the diff, PR number/link, or branch to review.
- `{{CONTEXT}}` — what the change is trying to do (optional if it's self-evident from a
  linked issue/description).

## Prompt Template
```
Load the .NET engineering toolkit's code-reviewer agent and review:

{{DIFF_OR_PR}}

Context: {{CONTEXT}}

Check each category from checklists/code-review-checklist.md: correctness, architecture,
security (obvious issues), performance (obvious issues), maintainability, testing,
observability, compatibility, deployment.

If the diff touches authentication, authorization, secrets, or user input non-trivially,
recommend a full security-reviewer pass (prompts/security/security-review.md) rather than
reviewing it shallowly yourself. If there's a suspected performance regression that would
need profiling to confirm, recommend prompts/review/performance-review.md instead of
guessing at severity.

Report findings ranked by severity (BLOCKER/HIGH/MEDIUM/LOW/INFO), each with: category,
location, the concrete problem, its impact, and a specific recommendation. Do not
generate an overall numeric score.
```

## Expected Output
A severity-ranked findings list per
[`checklists/code-review-checklist.md`](../../checklists/code-review-checklist.md), with
specialist reviews recommended (not performed shallowly) where warranted.

## Related
[`prompts/security/security-review.md`](../security/security-review.md),
[`prompts/review/performance-review.md`](performance-review.md).
