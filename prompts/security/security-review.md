# Prompt: Security Review

## Purpose
Run a full OWASP-oriented security review against a change, per
[`checklists/security-review-checklist.md`](../../checklists/security-review-checklist.md).

## When to Use
Any change touching authentication, authorization, secrets, user input, file handling, or
external calls; mandatory (not optional) for any authentication feature per
[`workflows/authentication-feature.md`](../../workflows/authentication-feature.md).

## Loads
Agent: [`security-reviewer`](../../agents/security-reviewer.md). Skill:
[`security-owasp`](../../skills/security/security-owasp.md). Checklist:
[`security-review-checklist`](../../checklists/security-review-checklist.md).

## Parameters
- `{{DIFF_OR_FEATURE}}` — the diff/feature under review.
- `{{AUTH_SCHEME}}` — the project's authentication scheme, if relevant (JWT/cookie/OIDC
  provider) — helps ground recommendations in what's actually usable here.

## Prompt Template
```
Load the .NET engineering toolkit's security-reviewer agent and review:

{{DIFF_OR_FEATURE}}

This project's authentication scheme: {{AUTH_SCHEME}}

Apply checklists/security-review-checklist.md in full:
- Authentication and authorization (including resource-level ownership, not just role)
- Secrets handling
- Injection risk (SQL and otherwise)
- SSRF, CSRF, XSS, insecure deserialization
- File upload / path traversal if applicable
- Sensitive data exposure in logs/responses
- Dependency vulnerabilities for any new/changed package
- Security headers / HTTPS enforcement

For every finding: severity (BLOCKER/HIGH/MEDIUM/LOW/INFO), location, the concrete
exploit scenario (what input/actor triggers it), and a recommendation compatible with
this project's existing auth scheme. Do not report a theoretical vulnerability class
with no realistic exploitation path as a BLOCKER.
```

## Expected Output
A severity-ranked findings list per
[`checklists/security-review-checklist.md`](../../checklists/security-review-checklist.md),
each with a concrete exploit scenario and actionable recommendation.

## Related
[`prompts/coding/api-design.md`](../coding/api-design.md),
[`workflows/authentication-feature.md`](../../workflows/authentication-feature.md).
