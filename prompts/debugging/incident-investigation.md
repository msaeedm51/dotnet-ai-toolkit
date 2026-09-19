# Prompt: Production Incident Investigation

## Purpose
Handle a live production incident, per
[`workflows/production-incident.md`](../../workflows/production-incident.md) — mitigation
before full root-cause investigation when the incident is live and worsening.

## When to Use
An active production incident, as distinct from a routine bug report (see
[`prompts/debugging/bug-investigation.md`](bug-investigation.md) for that case).

## Loads
Workflow: [`production-incident`](../../workflows/production-incident.md). Agents: whichever
fits the symptom's domain
([`dotnet-developer`](../../agents/dotnet-developer.md)/[`database-engineer`](../../agents/database-engineer.md)/
[`devops-engineer`](../../agents/devops-engineer.md)), then
[`test-engineer`](../../agents/test-engineer.md) and
[`code-reviewer`](../../agents/code-reviewer.md).

## Parameters
- `{{IMPACT}}` — what's broken, for whom, and how severely (user-facing errors, data
  integrity risk, full outage, degraded performance).
- `{{ROLLBACK_AVAILABLE}}` — is a known-good previous version/config available to roll back
  to.

## Prompt Template
```text
Load the .NET engineering toolkit and follow workflows/production-incident.md.

Impact: {{IMPACT}}
Rollback available: {{ROLLBACK_AVAILABLE}}

1. Triage severity and blast radius first.
2. If the incident is live and worsening: prefer rollback if available over a hasty
   forward fix. If no rollback is available and impact is active, propose the narrowest
   safe mitigation (feature flag, config toggle) and label it explicitly as a mitigation,
   not a fix.
3. Confirm the mitigation actually stopped the impact before moving on.
4. THEN investigate root cause following the same fact/hypothesis/confirmed discipline as
   a routine bug fix -- now without incident time pressure.
5. Implement the real fix for the confirmed root cause (not just the mitigated symptom),
   with a regression test.
6. Note what should go into a postmortem: timeline, impact, root cause, what mitigated
   it, what fixed it, and what process/monitoring gap let it reach production.

Do not skip straight to a root-cause fix while impact is still active and a faster
mitigation (rollback/flag) is available.
```

## Expected Output
Impact stopped quickly, a confirmed root cause investigated afterward, a real fix with a
regression test, and postmortem notes.

## Related
[`prompts/debugging/bug-investigation.md`](bug-investigation.md),
[`templates/docs/runbook.template.md`](../../templates/docs/runbook.template.md).
