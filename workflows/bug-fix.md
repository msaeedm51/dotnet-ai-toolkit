# Workflow: Bug Fix (Debugging)

A systematic path from symptom to a verified fix. The critical discipline: never present an
unverified hypothesis as a confirmed root cause.

```
Symptom
  -> Reproduce
  -> Collect evidence
  -> Form hypotheses
  -> Check logs
  -> Inspect code
  -> Inspect database/network if required
  -> Identify root cause
  -> Fix
  -> Regression test
  -> Validate
```

## Fact vs. hypothesis vs. confirmed root cause

Track these as three distinct categories throughout, and label which is which when
reporting:

- **Observed fact** — something directly seen: a log line, an exception stack trace, a
  reproduced failure, a query result.
- **Hypothesis** — a plausible explanation not yet confirmed. State it as one:
  "hypothesis: the null check is missing on the `Customer` navigation property."
- **Confirmed root cause** — a hypothesis verified against evidence (reproduced, traced
  through the actual code path, or proven via a targeted test/log).

Never skip straight from symptom to "fix" without at least one confirmed root cause —
patching a symptom without understanding the cause tends to reappear elsewhere.

## 1. Symptom
What's actually observed — the exact error, the exact incorrect output, the exact
conditions it happens under. Not a restatement of what should happen.

## 2. Reproduce
Get a reliable repro if at all possible. If it can't be reproduced, say so explicitly and
work from available evidence (logs, error reports) rather than guessing.

## 3. Collect evidence
Logs, stack traces, request/response payloads, database state, relevant metrics — whatever's
available (see [`AGENTS.md §4`](../AGENTS.md#4-tool-agnostic-design) for tool-availability
handling).

## 4. Form hypotheses
List plausible causes from the evidence — more than one if the evidence doesn't clearly
point to a single cause.

## 5. Check logs / 6. Inspect code / 7. Inspect database/network if required
Narrow the hypotheses using whatever's needed — don't stop at the first plausible-looking
line of code without tracing the actual execution path.

## 8. Identify root cause
State it as confirmed, with the evidence that confirms it.

## 9. Fix
The smallest change that addresses the confirmed root cause — not a broader refactor riding
along with the fix.

## 10. Regression test
A test that reproduces the original failure and fails without the fix — proves both that the
bug is fixed and that it stays fixed.

## 11. Validate
Confirm the original symptom no longer occurs, and that the fix hasn't broken anything else
covered by the existing test suite.

## Exit criteria
[`checklists/definition-of-done.md`](../checklists/definition-of-done.md), plus: root cause
was confirmed (not just hypothesized), and a regression test exists.

## Related
[`workflows/production-incident.md`](production-incident.md) for a bug found in production,
where mitigation may need to precede full root-cause investigation.
