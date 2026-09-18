<!--
Template: Runbook
A runbook is executed under pressure (an incident, an on-call page) -- every step must be
a concrete, verified action. No vague guidance. See
skills/documentation/technical-writing.md and workflows/production-incident.md.
-->

# Runbook: {{PROCEDURE_NAME}}

**When to use this:** {{the specific trigger -- an alert name, a reported symptom, a
scheduled task}}

**Severity/urgency:** {{how urgent this typically is}}

**Owner:** {{team/role responsible for this procedure}}

## Prerequisites

- {{Access required}}
- {{Tools required}}

## Steps

1. {{Concrete action}} -- run:
   ```bash
   {{VERIFIED command}}
   ```
2. {{Concrete action, with the expected output/result stated so the operator knows if it
   worked}}
3. {{Continue...}}

## Verification

{{How to confirm the procedure actually resolved the situation -- a specific check, not
"confirm it's working."}}

```bash
{{VERIFIED verification command}}
```

## If This Doesn't Resolve It

{{Escalation path -- who to page next, and what information to hand them.}}

## Related

{{Links to the alert/monitoring dashboard that triggers this runbook, and any
troubleshooting guide (templates/docs/troubleshooting-guide.template.md) with broader
context.}}
