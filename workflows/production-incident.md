# Workflow: Production Incident

A live incident is different from a routine bug fix: mitigation comes before full root-cause
investigation. Multi-agent sequence: Investigator (using
[`dotnet-developer`](../agents/dotnet-developer.md)/[`database-engineer`](../agents/database-engineer.md)/
[`devops-engineer`](../agents/devops-engineer.md) as the symptom dictates) → Developer → Test
Engineer → Reviewer — see [`workflows/multi-agent-orchestration.md`](multi-agent-orchestration.md).

## 1. Triage
Assess severity and blast radius first: how many users/requests affected, is data integrity
at risk, is it actively getting worse. This determines whether to mitigate immediately or
investigate first.

## 2. Mitigate before full diagnosis, if the incident is live and worsening
- Is there a known-good previous version to roll back to? Prefer rollback over a hasty
  forward fix when available — see [`devops-engineer`](../agents/devops-engineer.md)'s
  rollback-path requirement.
- Is there a safe feature flag / config toggle to disable the affected path?
- Only apply a forward mitigation without full root-cause understanding when rollback isn't
  available and the incident is actively causing harm — and say explicitly that it's a
  mitigation, not a fix.

## 3. Stabilize
Confirm the mitigation actually stopped the impact (check the same signal that revealed the
incident) before moving to investigation.

## 4. Investigate root cause
Follow [`workflows/bug-fix.md`](bug-fix.md)'s fact/hypothesis/confirmed discipline — now
without the time pressure of an active incident.

## 5. Fix
The confirmed root cause, not just the symptom that was mitigated in step 2.

## 6. Regression test
Per [`workflows/bug-fix.md`](bug-fix.md) — a test that reproduces the original failure.

## 7. Review
[`code-reviewer`](../agents/code-reviewer.md) reviews the fix; if the incident involved a
security or data-integrity issue, [`security-reviewer`](../agents/security-reviewer.md)
reviews it too.

## 8. Deploy the real fix
Following the project's normal deployment process — an incident fix still goes through
review and CI unless the mitigation in step 2 already addressed the live impact.

## 9. Postmortem
Document: timeline, impact, root cause, what mitigated it, what fixed it, and what
process/monitoring gap allowed it to reach production — update a runbook
([`templates/docs/runbook.template.md`](../templates/docs/runbook.template.md)) if this
class of incident is likely to recur.

## Exit criteria
Incident impact stopped, root cause confirmed and fixed (not just mitigated), regression
test in place, postmortem documented.
