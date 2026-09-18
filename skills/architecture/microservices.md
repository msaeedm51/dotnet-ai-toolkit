# Skill: Microservices

## Purpose
Split a system into independently deployable services only when there's a concrete
requirement (independent scaling, independent deployment cadence, genuine team autonomy at
that granularity) that a [`modular-monolith`](../architecture/modular-monolith.md) can't
satisfy.

## When to Use
A specific, real requirement: a component needs to scale independently at a very different
rate than the rest of the system, a team needs to deploy independently on its own cadence
without coordinating with others, or a component has genuinely different technology/runtime
needs. **Do not** adopt microservices as a default architecture or for "future scalability"
with no current requirement — this is the pattern this toolkit most explicitly warns
against overapplying (see the Architect agent's constraints).

## Prerequisites
A modular monolith (or equivalent clean module boundary) already exists or is the honest
starting point — extracting a service from a well-bounded module is far cheaper than
designing microservices from scratch with unclear boundaries.

## Inputs Required
The specific requirement driving the split, and the module boundary being extracted.

## Engineering Principles
- Each service owns its data exclusively — no shared database between services accessed
  directly; cross-service data needs go through the owning service's API or an event.
- Prefer asynchronous, event-driven communication for anything that doesn't need a
  synchronous response — reduces coupling and cascading-failure risk compared to synchronous
  service-to-service calls for everything.
- Every synchronous inter-service call needs the same resilience discipline as any external
  HTTP call (see [`httpclient-resilience`](../dotnet/httpclient-resilience.md)) — timeouts,
  retries with backoff, circuit breakers — because a downstream service being slow/down is
  now a normal operating condition, not an edge case.
- Distributed transactions across services are avoided; use the
  [Saga pattern](https://en.wikipedia.org/wiki/Long-running_transaction) or eventual
  consistency via events instead of two-phase commit across service boundaries.
- Each service needs its own health checks, observability, and deployment pipeline — the
  operational cost multiplies per service; account for that cost explicitly when deciding to
  split.

## Step-by-Step Workflow
1. Confirm the concrete requirement driving the split (not "it seems more scalable").
2. Extract from an already well-bounded module — its public interface becomes the new
   service's API.
3. Give the new service its own database/schema; migrate its data out of the shared database
   if it was previously part of a modular monolith.
4. Replace in-process calls to the module with resilient HTTP/messaging calls to the new
   service.
5. Add the new service's own CI/CD, observability, and health checks
   (see [`devops`](../devops/) skills).
6. Handle the cross-service consistency implications (eventual consistency, saga/
   compensation) explicitly — don't assume atomicity that no longer exists.

## Code Standards
```csharp
// Resilient inter-service call -- same discipline as any external HTTP dependency
builder.Services.AddHttpClient<IInventoryServiceClient, InventoryServiceClient>(client =>
        client.BaseAddress = new Uri(builder.Configuration["Services:Inventory:BaseUrl"]!))
    .AddResilienceHandler("inventory-service", b =>
    {
        b.AddRetry(new HttpRetryStrategyOptions { MaxRetryAttempts = 3, BackoffType = DelayBackoffType.Exponential });
        b.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions());
        b.AddTimeout(TimeSpan.FromSeconds(3));
    });

// Event-driven alternative for non-blocking cross-service updates
public sealed record OrderConfirmed(Guid OrderId, IReadOnlyList<OrderLineDto> Lines);
// Published to a broker; Inventory service subscribes and reserves stock asynchronously,
// rather than Orders synchronously calling Inventory and blocking on the response.
```

## Architecture Constraints
No service accesses another service's database directly, under any circumstance. Every
cross-service call is treated as an unreliable network call requiring timeout/retry/
circuit-breaking.

## Security Considerations
Service-to-service authentication (mTLS, service tokens) is required — internal network
trust ("it's behind the firewall") is not a substitute for authenticating inter-service
calls, especially in a containerized/cloud environment.

## Testing Requirements
Contract tests between services (verifying the API shape both sides agree on) in addition to
each service's own unit/integration tests — see
[`skills/testing/integration-testing.md`](../testing/integration-testing.md).

## Common Mistakes
- Splitting services along technical layers (a "database service," a "business-logic
  service") instead of business capability boundaries — creates chatty, tightly-coupled
  services that are effectively a distributed monolith with worse latency.
- Sharing a database between two "microservices" — this is not microservices, it's a
  distributed monolith with none of the benefits and most of the cost.
- No resilience handling on inter-service calls, so one slow service cascades failures
  through the whole system.

## Anti-Patterns
- Adopting microservices at project inception "to scale later" with no current requirement
  — pay this cost when a real requirement demands it, not preemptively (see
  [`modular-monolith`](../architecture/modular-monolith.md) as the default starting point).

## Validation Checklist
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Definition of Done
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Example
See [Code Standards](#code-standards).

## Related Skills
[`modular-monolith`](../architecture/modular-monolith.md),
[`event-driven-architecture`](../architecture/event-driven-architecture.md),
[`httpclient-resilience`](../dotnet/httpclient-resilience.md).
