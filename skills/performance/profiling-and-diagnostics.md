# Skill: Profiling and Diagnostics

## Purpose
Get real evidence of where time/memory actually goes in a .NET application, instead of
guessing which line is slow.

## When to Use
Any performance investigation, before proposing a fix — see
[`workflows/performance-investigation.md`](../../workflows/performance-investigation.md).

## Prerequisites
Access to run the application under investigation, and, ideally, a load pattern close to
what triggers the symptom.

## Inputs Required
The symptom (high latency, high CPU, high memory, GC pressure) and an environment where it
can be reproduced or observed.

## Engineering Principles
- **CPU-bound issues**: use `dotnet-trace` (collect a trace, view in
  `dotnet-trace`/Visual Studio/PerfView/`speedscope`) to see where CPU time is actually
  spent — don't assume based on code complexity alone.
- **Memory issues**: use `dotnet-gcdump`/`dotnet-counters` to see GC pressure (Gen0/1/2
  collection frequency, allocation rate) and heap snapshots to find what's actually being
  retained.
- **Database-related latency**: capture the actual generated SQL and execution plan (see
  [`efcore-performance`](../data/efcore-performance.md) and the engine-specific skill) — the
  bottleneck is very often the query, not the application code around it.
- **Async/thread-pool issues**: `dotnet-counters` shows thread-pool queue length and
  active thread count — a growing queue under load suggests thread-pool starvation, often
  from sync-over-async blocking calls (see [`async-patterns`](../csharp/async-patterns.md)).
- Reproduce under conditions close to the reported symptom — a profiler run against a single
  request in isolation may not surface contention/pooling issues that only appear under
  concurrent load.

## Step-by-Step Workflow
1. Get the precise symptom: which operation, what latency/resource usage, under what load.
2. Choose the right tool for the suspected category (CPU: `dotnet-trace`; memory:
   `dotnet-gcdump`/`dotnet-counters`; database: execution plan; concurrency:
   `dotnet-counters` thread-pool metrics).
3. Capture evidence under a representative load.
4. Identify the specific hot path/allocation site/query from the evidence.
5. Hand off to the fix (see [`caching-strategy`](../performance/caching-strategy.md),
   [`efcore-performance`](../data/efcore-performance.md),
   [`async-patterns`](../csharp/async-patterns.md) depending on what the evidence points to).
6. Re-measure after the fix with the same tool/load to confirm improvement.

## Code Standards
```bash
# CPU trace for a running process
dotnet-trace collect --process-id <pid> --providers Microsoft-DotNETCore-SampleProfiler

# Live counters: GC, thread pool, exceptions
dotnet-counters monitor --process-id <pid> System.Runtime Microsoft.AspNetCore.Hosting

# Memory snapshot for retained-object analysis
dotnet-gcdump collect --process-id <pid>
```
```csharp
// Instrumentation to correlate a specific operation with the trace, when needed
using var activity = ActivitySource.StartActivity("CreateOrder");
activity?.SetTag("customer.id", customerId);
```

## Architecture Constraints
None — this is an investigative skill, not a structural one.

## Security Considerations
Profiler/dump output can contain sensitive data (memory dumps may include in-flight request
data) — handle and store capture files with the same care as production data, and don't
share them outside the team investigating.

## Testing Requirements
Not directly applicable — this skill produces evidence for
[`workflows/performance-investigation.md`](../../workflows/performance-investigation.md),
not tests itself.

## Common Mistakes
- Profiling in a debug build (JIT optimizations disabled) and drawing conclusions that don't
  hold in the release build actually deployed.
- Profiling a single, uncontended request when the reported symptom only occurs under
  concurrent load.
- Fixing based on where time "seems like it should" go instead of where the profiler shows
  it actually goes.

## Anti-Patterns
- Adding ad hoc `Stopwatch`/`Console.WriteLine` timing sprinkled through code as a substitute
  for using the actual profiling tools — slower to iterate and easy to leave in
  accidentally.

## Validation Checklist
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Definition of Done
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Example
See [Code Standards](#code-standards).

## Related Skills
[`caching-strategy`](../performance/caching-strategy.md),
[`efcore-performance`](../data/efcore-performance.md),
[`async-patterns`](../csharp/async-patterns.md).
