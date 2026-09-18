<!--
Template: Troubleshooting Guide
Every entry needs a concrete, executable check and fix -- "check if the service is
healthy" is not a troubleshooting step. Verify each command actually works before writing
it. See skills/documentation/technical-writing.md.
-->

# {{PROJECT_NAME}} Troubleshooting Guide

## {{Symptom, e.g. "API returns 503 on /health/ready"}}

**Likely causes, in order of probability:**

1. {{Cause}} -- **Check:** `{{verified command/dashboard query}}` -- **Fix:** {{specific
   action}}
2. {{Cause}} -- **Check:** `{{...}}` -- **Fix:** {{...}}

**Escalate if:** {{condition under which this needs a human/on-call, not a checklist}}

<!-- Repeat the block above per known symptom. Order symptoms by how often they're
actually seen, most common first. -->

## {{Another symptom}}

...

## Useful Queries and Commands

```bash
# {{What this checks}}
{{VERIFIED command}}
```

## Dashboards and Logs

| What | Where |
|---|---|
| {{Application logs}} | {{link/location}} |
| {{Metrics dashboard}} | {{link}} |
| {{Error tracking}} | {{link}} |

## Related Runbooks

{{Links to templates/docs/runbook.template.md's filled-in versions for specific
operational procedures (e.g. "rotating the database credential") rather than duplicating
them here.}}
