# Skill: Scalability

## Purpose
Design for handling increased load correctly — statelessness, horizontal scaling
implications, and where a single-request optimization stops mattering compared to a
system-level bottleneck.

## When to Use
Evaluating whether a design will hold up under production-scale traffic, or diagnosing why a
system that's fast for one request degrades under many.

## Prerequisites
[`profiling-and-diagnostics`](../performance/profiling-and-diagnostics.md) for evidence-based
investigation of an actual scaling problem, as opposed to speculative capacity planning.

## Inputs Required
The expected load profile (requests/second, data volume, growth trajectory) — scaling
decisions without a target load are guesses.

## Engineering Principles
- **Statelessness enables horizontal scaling** — an ASP.NET Core app instance shouldn't hold
  request-affine state in memory (in-process session state, singleton caches assumed
  consistent across instances) if it needs to scale to multiple instances behind a load
  balancer; use a distributed store for anything that must be consistent across instances.
- **The database is usually the first bottleneck**, not the application tier — connection
  pool exhaustion, lock contention, and slow queries under concurrent load surface before
  CPU/memory limits on a typical web API; profile there first (see
  [`efcore-performance`](../data/efcore-performance.md) and the engine-specific skill).
- **Connection pooling** must be sized correctly — too small causes queueing/timeouts under
  load; too large can overwhelm the database server. Size based on measured concurrency, not
  a default guess.
- **Async I/O scales thread usage** far better than blocking calls under concurrent load —
  see [`async-patterns`](../csharp/async-patterns.md); this matters more at scale than at low
  traffic where the difference is invisible.
- **Horizontal vs. vertical scaling**: prefer horizontal (more instances) for stateless web
  tiers — it's more resilient (no single point of failure) and matches cloud-native
  deployment models; vertical scaling (bigger instance) is a simpler short-term lever but
  has a ceiling and doesn't improve availability.

## Step-by-Step Workflow
1. Establish the target load (requests/second, data volume, growth) — don't design against
   an unstated assumption.
2. Identify stateful assumptions that would break horizontal scaling (in-memory session,
   in-process cache assumed globally consistent) and move them to a distributed store if
   multiple instances are expected.
3. Load-test against the target, watching the database first (connection pool metrics, query
   latency under concurrency) before assuming the application tier is the bottleneck.
4. Address the actual measured bottleneck — don't optimize speculatively across the whole
   stack.
5. Re-test at target load to confirm.

## Code Standards
```csharp
// Explicit connection pool sizing, informed by measured concurrency -- not left at defaults
// unexamined for a high-throughput service
builder.Services.AddDbContextPool<AppDbContext>(o =>
    o.UseNpgsql(connectionString, npgsql => npgsql.CommandTimeout(30)),
    poolSize: 128); // sized from load testing, not guessed

// Stateless request handling -- no in-process session state that would break
// when a second instance is added behind a load balancer
app.MapPost("/orders", async (CreateOrderRequest request, IOrderService service, CancellationToken ct) =>
{
    var result = await service.CreateAsync(request, ct); // no reliance on prior request's in-memory state
    return result.ToHttpResult();
});
```

## Architecture Constraints
A service intended to scale horizontally must not assume any two requests land on the same
instance — session affinity, if used, should be a deliberate choice with a stated reason,
not an accidental dependency.

## Security Considerations
Horizontal scaling changes the threat model slightly for anything cached/rate-limited
per-instance (see [`rate-limiting`](../dotnet/rate-limiting.md) — a per-instance rate limit
effectively multiplies the limit by instance count unless it's backed by a shared store).

## Testing Requirements
Load testing (k6, JMeter, or similar) against a representative target, not just functional
correctness tests — scaling problems don't show up in single-request integration tests.

## Common Mistakes
- Assuming the application tier is the bottleneck and optimizing C# code when the database
  connection pool is actually saturated under load.
- In-memory rate limiting/caching in a multi-instance deployment, producing inconsistent
  behavior across instances.
- Load testing against a single instance and extrapolating linearly to a multi-instance
  deployment without accounting for the shared database as a common bottleneck.

## Anti-Patterns
- Designing for a hypothetical future scale far beyond any stated requirement, adding
  operational complexity (premature sharding, premature microservices) before there's
  evidence it's needed — see the Architect agent's constraint against unnecessary complexity.

## Validation Checklist
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Definition of Done
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Example
See [Code Standards](#code-standards).

## Related Skills
[`profiling-and-diagnostics`](../performance/profiling-and-diagnostics.md),
[`efcore-performance`](../data/efcore-performance.md),
[`microservices`](../architecture/microservices.md).
