# Prompt: Performance Review

## Purpose
Investigate a suspected performance problem with evidence, per
[`workflows/performance-investigation.md`](../../workflows/performance-investigation.md) —
never present an unverified hypothesis as a diagnosed cause.

## When to Use
A reported slow endpoint/operation, high resource usage, or a suspected regression flagged
during code review.

## Loads
Agent: [`performance-engineer`](../../agents/performance-engineer.md). Skills:
[`profiling-and-diagnostics`](../../skills/performance/profiling-and-diagnostics.md),
[`efcore-performance`](../../skills/data/efcore-performance.md) if database-related.

## Parameters
- `{{SYMPTOM}}` — the precise, measured symptom (which operation, what latency/resource
  usage, under what load).
- `{{EVIDENCE_AVAILABLE}}` — what's already available (profiler access, execution plans,
  logs, metrics) — state "none yet" if starting from scratch.

## Prompt Template
```text
Load the .NET engineering toolkit's performance-engineer agent and investigate:

Symptom: {{SYMPTOM}}
Evidence available: {{EVIDENCE_AVAILABLE}}

Follow workflows/performance-investigation.md:
1. If evidence isn't yet available, propose the minimal instrumentation/profiling needed
   to get it -- do not propose a fix before you have evidence.
2. Form a hypothesis from the evidence, stated explicitly as unconfirmed.
3. Narrow to a confirmed root cause, with the evidence that confirms it.
4. Propose a fix targeting that specific cause, explaining its mechanism of improvement.
5. State how to measure before/after to confirm the fix worked.

Label every claim as "observed fact," "hypothesis," or "confirmed root cause" -- do not
present a guess as a diagnosed cause. Do not trade correctness/consistency for speed
without flagging that tradeoff explicitly for confirmation.
```

## Expected Output
A confirmed root cause (with evidence), a targeted fix, and a measurement plan — or, if
evidence isn't available yet, a precise plan to obtain it.

## Related
[`prompts/database/database-optimization.md`](../database/database-optimization.md) for
database-specific performance work.
