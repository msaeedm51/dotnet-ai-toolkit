# Prompt: Deployment Readiness

## Purpose
Assess whether a change is actually safe to deploy — secrets, health checks, rollback path,
migration sequencing — and produce/update a deployment guide if needed, per
[`checklists/definition-of-done.md`](../../checklists/definition-of-done.md) and
[`skills/devops/*`](../../skills/devops/).

## When to Use
Before deploying a change with infrastructure, configuration, or schema impact.

## Loads
Agent: [`devops-engineer`](../../agents/devops-engineer.md) (plus
[`database-engineer`](../../agents/database-engineer.md) if a migration is involved).

## Parameters
- `{{CHANGE}}` — the change being deployed.
- `{{TARGET_ENV}}` — the deployment target (`config.yaml` `deployment.platform`).

## Prompt Template
```
Load the .NET engineering toolkit's devops-engineer agent and assess deployment
readiness for:

{{CHANGE}}

Target environment: {{TARGET_ENV}}

Check:
1. No secret/credential is in plaintext anywhere in the diff (Dockerfile, config, CI
   workflow).
2. Health checks cover any new/changed dependency.
3. A rollback path exists and is stated explicitly.
4. If a database migration is involved, confirm its deployment sequencing relative to
   the application deploy (expand/contract ordering per workflows/database-change.md).
5. CI runs the full test suite before any deploy stage.

Report readiness as a clear go/no-go with the specific blockers if not ready. If
documentation (a deployment guide) needs creating/updating for this change, do that
using templates/docs/deployment-guide.template.md.
```

## Expected Output
A go/no-go readiness assessment with specific blockers named, and an updated deployment
guide if warranted.

## Related
[`prompts/database/database-migration.md`](../database/database-migration.md).
