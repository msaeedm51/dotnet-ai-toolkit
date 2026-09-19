# Workflow: Performance Investigation

Primary agent: [`performance-engineer`](../agents/performance-engineer.md). Same
fact/hypothesis/confirmed discipline as [`workflows/bug-fix.md`](bug-fix.md) — never present
an unverified hypothesis as a diagnosed cause.

```text
Symptom
  -> Reproduce
  -> Collect evidence (profiler, execution plan, metrics, logs)
  -> Form hypothesis
  -> Narrow to confirmed root cause
  -> Propose fix
  -> Measure before/after
  -> Report result and residual risk
```

## 1. Symptom
The precise, measured symptom: which endpoint/operation, what latency/throughput/resource
usage, under what load — not "it feels slow."

## 2. Reproduce
Get a reliable repro, ideally with a load profile close to what triggers the symptom in
production.

## 3. Collect evidence
Whatever's available: a .NET profiler (dotnet-trace, a commercial profiler), a database
execution plan, APM/metrics dashboards, structured logs with timing. If nothing is
available, the first deliverable is a minimal instrumentation plan to get evidence — not a
guessed fix.

## 4. Form hypothesis
State it explicitly as unconfirmed: "hypothesis: the `/orders` endpoint is slow due to an
N+1 query loading `OrderLines` per order."

## 5. Narrow to confirmed root cause
Confirm via the evidence — an execution plan showing the repeated query, a profiler trace
showing time spent, a query count assertion in a test.

## 6. Propose fix
Target the specific confirmed cause. State the mechanism of improvement
("`.Include()` reduces N+1 queries to 1 join query") — not just "this should help."

## 7. Measure before/after
Re-run the same reproduction and compare. If measurement tooling isn't available, state
exactly what to run and what to look for.

## 8. Report
Root cause (confirmed, with evidence), fix, measured or expected improvement, and any
residual risk (e.g. the fix trades memory for CPU, or only addresses one of several
contributing factors).

## Constraints
- Do not trade correctness/consistency for speed without explicit confirmation
  ([`rules/performance.md`](../rules/performance.md)).
- Any caching addition states its invalidation strategy before being considered done.

## Exit criteria
[`checklists/definition-of-done.md`](../checklists/definition-of-done.md), plus: root cause
labeled confirmed (not hypothesis), and improvement measured or a measurement plan stated.
