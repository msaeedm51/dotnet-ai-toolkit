# Skill: Observability and Monitoring

## Purpose
Set up the operational signal (logs, metrics, traces, alerts) that lets a team detect and
diagnose a production problem before/as it affects users, rather than only after a user
reports it.

## When to Use
Deploying a new service, or reviewing whether an existing one has adequate observability
before it's trusted with real traffic.

## Prerequisites
[`logging-observability`](../dotnet/logging-observability.md) for the application-level
instrumentation this skill builds on; [`health-checks`](../dotnet/health-checks.md).

## Inputs Required
The service's critical operations and failure modes — what "broken" looks like from a
metrics/alerting perspective.

## Engineering Principles
- Three pillars: **logs** (discrete events, for diagnosis after something's flagged),
  **metrics** (aggregated numbers over time, for detecting a problem and dashboards),
  **traces** (a request's path across a distributed system, for understanding latency/where
  time went) — OpenTelemetry provides all three through one instrumentation approach.
- **Alert on symptoms users would notice** (error rate, latency percentiles, availability),
  not just on every possible internal metric — alert fatigue from over-alerting causes real
  alerts to get ignored.
- Dashboards answer specific operational questions ("is the service healthy right now,"
  "what's driving this latency spike") — a dashboard with fifty unrelated graphs and no
  clear question it answers isn't useful under incident pressure.
- Correlate logs/traces/metrics via a shared trace/correlation id so an alert on a metric can
  be traced back to the specific requests/logs that caused it.
- Export to whatever backend the project/organization already uses (Azure Monitor,
  Prometheus/Grafana, Datadog, etc.) via OpenTelemetry exporters, rather than instrumenting
  directly against a vendor SDK throughout the codebase.

## Step-by-Step Workflow
1. Confirm the observability backend already in use (or being adopted).
2. Wire OpenTelemetry tracing and metrics (see
   [`logging-observability`](../dotnet/logging-observability.md)) exporting to that backend.
3. Define alerts on user-facing symptoms: error rate above a threshold, p95/p99 latency
   above a threshold, health check failing.
4. Build/update a dashboard answering "is this service healthy" at a glance.
5. Verify an intentionally-triggered failure (in staging) actually produces the expected
   alert and traceable signal.

## Code Standards
```csharp
builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService(serviceName: "orders-api", serviceVersion: "1.0.0"))
    .WithTracing(t => t
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddOtlpExporter())
    .WithMetrics(m => m
        .AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation()
        .AddMeter("Orders.Api")
        .AddOtlpExporter());

// Custom business metric, in addition to auto-instrumented HTTP/runtime metrics
public sealed class OrderMetrics
{
    private readonly Counter<int> _ordersCreated;
    public OrderMetrics(IMeterFactory meterFactory)
    {
        _ordersCreated = meterFactory.Create("Orders.Api").CreateCounter<int>("orders.created");
    }
    public void RecordOrderCreated() => _ordersCreated.Add(1);
}
```

## Architecture Constraints
Instrumentation is cross-cutting Infrastructure; Domain logic remains unaware of it (Domain
raises events, Application/Infrastructure records metrics/traces around handling them).

## Security Considerations
Trace/span attributes must not include secrets or full PII — the same discipline as
[`logging-observability`](../dotnet/logging-observability.md) applies to everything exported
to an observability backend.

## Testing Requirements
Verify (in staging or a controlled test) that a deliberately failing dependency or elevated
error rate actually surfaces in the dashboard/alerting — untested alerting is not trustworthy
alerting.

## Common Mistakes
- No alerting at all — relying on users to report outages first.
- Alerting on every possible metric, causing alert fatigue that leads to real alerts being
  ignored or muted.
- Missing correlation between a metric spike and the actual requests/logs that caused it.

## Anti-Patterns
- Instrumenting directly against a vendor SDK throughout business logic instead of the
  OpenTelemetry abstraction, locking the codebase to one observability backend.

## Validation Checklist
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Definition of Done
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Example
See [Code Standards](#code-standards).

## Related Skills
[`logging-observability`](../dotnet/logging-observability.md),
[`health-checks`](../dotnet/health-checks.md).
