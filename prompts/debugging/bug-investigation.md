# Prompt: Bug Investigation

## Purpose
Investigate and fix a bug systematically, per
[`workflows/bug-fix.md`](../../workflows/bug-fix.md) — with the observed-fact/hypothesis/
confirmed-root-cause discipline applied throughout.

## When to Use
Any reported bug that isn't a live production incident (see
[`prompts/debugging/incident-investigation.md`](incident-investigation.md) for that case).

## Loads
Workflow: [`bug-fix`](../../workflows/bug-fix.md). Agent:
[`dotnet-developer`](../../agents/dotnet-developer.md) (or the domain-specific agent the
symptom points to).

## Parameters
- `{{SYMPTOM}}` — the exact observed error/incorrect behavior and the conditions it occurs
  under.
- `{{REPRO_STEPS}}` — reproduction steps if known; state "not yet reproduced" otherwise.

## Prompt Template
```text
Load the .NET engineering toolkit and follow workflows/bug-fix.md for:

Symptom: {{SYMPTOM}}
Reproduction: {{REPRO_STEPS}}

1. Reproduce if not already reproduced; if it can't be reproduced, say so explicitly and
   work from available evidence rather than guessing.
2. Collect evidence: logs, stack traces, relevant request/response data, database state.
3. Form hypotheses from the evidence.
4. Narrow to a confirmed root cause using code inspection and, if needed, database/
   network inspection -- label it as confirmed only once verified against evidence, not
   just "the most likely explanation."
5. Implement the smallest fix that addresses the confirmed root cause.
6. Write a regression test that reproduces the original failure and fails without the fix.
7. Confirm the fix resolves the symptom and doesn't break existing tests.

Distinguish observed fact, hypothesis, and confirmed root cause explicitly throughout --
never present an unverified hypothesis as the confirmed cause.
```

## Expected Output
A confirmed root cause, a scoped fix, and a regression test — or, if the bug can't be
reproduced, a clear statement of that plus the best available evidence-based hypothesis.

## Related
[`prompts/debugging/incident-investigation.md`](incident-investigation.md),
[`prompts/testing/test-generation.md`](../testing/test-generation.md).
