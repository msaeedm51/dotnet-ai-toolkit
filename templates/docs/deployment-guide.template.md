<!--
Template: Deployment Guide
Every command/path here should be verified to actually work, not assumed -- a wrong
command in a deployment guide costs real time during a deploy. See
skills/documentation/technical-writing.md and the relevant skills/devops/* skill for the
target platform.
-->

# {{PROJECT_NAME}} Deployment Guide

**Target platform:** {{azure | linux-docker | kubernetes | iis | other}}
**Deployment method:** {{CI/CD pipeline name, or manual steps}}

## Environments

| Environment | Purpose | URL | Deploy trigger |
|---|---|---|---|
| {{Development}} | {{...}} | {{...}} | {{e.g. "every push to main"}} |
| {{Staging}} | {{...}} | {{...}} | {{...}} |
| {{Production}} | {{...}} | {{...}} | {{e.g. "manual approval after staging soak"}} |

## Prerequisites

- {{Access required -- e.g. "member of the X Azure AD group"}}
- {{Tooling -- e.g. "Azure CLI 2.x", "kubectl configured for the target cluster"}}

## Configuration and Secrets

{{Where configuration comes from per environment, and where secrets live (Key Vault, GitHub
Secrets, etc.) -- never the secret values themselves.}}

## Deploy Steps

### Automated (normal path)

{{Describe the CI/CD pipeline: what triggers it, what stages it runs, where to watch it.
Link to the actual workflow file.}}

### Manual (fallback, if automation is unavailable)

```bash
{{VERIFIED step-by-step commands}}
```

## Database Migrations

{{How/when migrations run relative to the deploy -- before, during, or via a separate
step. Link to workflows/database-change.md for the expand/contract discipline on breaking
changes.}}

## Health Verification

{{What to check after a deploy to confirm it succeeded -- health endpoint URL, dashboard
link, smoke test.}}

```bash
{{VERIFIED health-check command, e.g. curl -f https://.../health/ready}}
```

## Rollback

{{The actual, verified rollback procedure -- not "redeploy the previous version" in the
abstract, but the specific command/pipeline step.}}

```bash
{{VERIFIED rollback command}}
```

## Troubleshooting

{{Link to templates/docs/troubleshooting-guide.template.md's filled-in version rather than
duplicating it here.}}
