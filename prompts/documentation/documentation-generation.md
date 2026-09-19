# Prompt: Documentation Generation

## Purpose
Produce accurate, verified documentation using the matching template from
[`templates/docs/`](../../templates/docs/), per
[`skills/documentation/technical-writing.md`](../../skills/documentation/technical-writing.md).

## When to Use
After a feature/change is complete and its documentation needs creating or updating (README,
API docs, database docs, feature docs, architecture docs).

## Loads
Agent: [`documentation-engineer`](../../agents/documentation-engineer.md). Skill:
[`technical-writing`](../../skills/documentation/technical-writing.md).

## Parameters
- `{{SUBJECT}}` — what's being documented.
- `{{DOC_TYPE}}` — which template applies (README / API doc / database doc / feature doc /
  architecture doc / troubleshooting guide) — say "not sure, recommend one" if unclear.

## Prompt Template
```text
Load the .NET engineering toolkit's documentation-engineer agent and document:

{{SUBJECT}}

Document type: {{DOC_TYPE}}

1. Identify the correct template from templates/docs/ (recommend one if I said "not
   sure").
2. Read the actual code/config/pipeline being documented -- verify every checkable claim
   (a command, an endpoint, a config key, a file path) rather than assuming.
3. Draft following the template's structure.
4. Check for existing documentation covering the same area -- update it rather than
   creating a duplicate or conflicting document.
5. Flag anything you find that's already out of date elsewhere while you're in there.

Document the why and the non-obvious operational knowledge -- don't restate what
well-named code already makes clear.
```

## Expected Output
A document following the correct template, with every checkable claim verified against the
actual project.

## Related
[`prompts/documentation/deployment-readiness.md`](deployment-readiness.md).
