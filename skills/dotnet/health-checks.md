# Skill: Health Checks

## Purpose
Expose an accurate signal of whether the app can actually serve traffic, for load balancers
and orchestrators to act on.

## When to Use
Any service that's load-balanced, orchestrated (Kubernetes, Azure App Service), or has a
dependency (database, cache, message broker, external API) whose unavailability should be
reflected in the app's reported health.

## Prerequisites
[`aspnetcore-fundamentals`](../dotnet/aspnetcore-fundamentals.md).

## Inputs Required
The app's critical dependencies (the ones that mean "can't serve traffic" if unavailable, as
distinct from ones that degrade gracefully).

## Engineering Principles
- A health check that only confirms "the process is running" (no dependency checks) is
  nearly useless for an orchestrator — it won't catch "the process is up but the database is
  unreachable."
- Distinguish **liveness** (should the orchestrator restart this instance?) from
  **readiness** (should the load balancer send it traffic right now?) — a database outage
  should usually fail readiness, not liveness (restarting the app won't fix a database
  outage).
- Health checks must be fast and side-effect-free — a health check that itself causes load
  or takes seconds defeats its purpose.
- Tag checks so liveness/readiness endpoints can filter which checks apply to each.

## Step-by-Step Workflow
1. Add `AddHealthChecks()` and a check per critical dependency (database, cache, message
   broker) — most have a `AspNetCore.HealthChecks.*` NuGet package.
2. Tag checks as `live`/`ready` as appropriate.
3. Map separate endpoints: `/health/live` (process viability only) and `/health/ready`
   (dependency checks included).
4. Configure the orchestrator to use the correct endpoint for each probe type.

## Code Standards
```csharp
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("Default")!, tags: ["ready"])
    .AddRedis(builder.Configuration.GetConnectionString("Redis")!, tags: ["ready"]);

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false, // process-only, no dependency checks
});
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
});
```

## Architecture Constraints
Health check registration lives in the composition root, alongside the dependency
registrations it's checking.

## Security Considerations
Don't expose detailed dependency error messages (connection strings, internal hostnames) in
the health check response body to unauthenticated callers — return a minimal status;
detailed diagnostics go to logs/an internal-only endpoint.

## Testing Requirements
Integration test that `/health/ready` reflects a failing dependency (e.g. point it at an
intentionally unreachable database in a test) — see
[`integration-testing`](../testing/integration-testing.md).

## Common Mistakes
- A single `/health` endpoint used for both liveness and readiness, causing the orchestrator
  to restart healthy instances during a transient dependency blip.
- No health check for a critical dependency at all — the app reports healthy while unable to
  actually serve requests.

## Anti-Patterns
- A health check that performs a heavy query "to really verify the database works" — adds
  load and latency to something that should be cheap and frequent.

## Validation Checklist
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Definition of Done
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Example
See [Code Standards](#code-standards).

## Related Skills
[`aspnetcore-fundamentals`](../dotnet/aspnetcore-fundamentals.md),
[`observability-and-monitoring`](../devops/observability-and-monitoring.md).
