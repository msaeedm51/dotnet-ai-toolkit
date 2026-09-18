# Skill: Logging and Observability

## Purpose
Produce logs, metrics, and traces that make production behavior diagnosable, using
structured logging and OpenTelemetry conventions.

## When to Use
Adding logging to new code, or investigating why existing observability didn't catch an
issue.

## Prerequisites
[`aspnetcore-fundamentals`](../dotnet/aspnetcore-fundamentals.md).

## Inputs Required
What's being logged/measured and at what level of significance.

## Engineering Principles
- Structured logging always: `logger.LogInformation("Order {OrderId} created", orderId)`,
  never string-interpolated messages (`$"Order {orderId} created"`) — structured fields are
  queryable, interpolated strings aren't.
- Log levels mean something: `Trace`/`Debug` for diagnostic detail (off in production by
  default), `Information` for significant business events, `Warning` for recoverable
  problems, `Error` for failures needing attention, `Critical` for failures threatening the
  app itself.
- Never log secrets, tokens, passwords, or full PII (see
  [`security-owasp`](../security/security-owasp.md)).
- Use OpenTelemetry (`Microsoft.Extensions.Diagnostics`/`OpenTelemetry` packages) for traces
  and metrics so the app is portable across observability backends (App Insights, Jaeger,
  Prometheus, etc.) rather than locked into a vendor SDK throughout the codebase.
- Correlate logs with a request/trace id so a single request's logs can be reconstructed
  across services.

## Step-by-Step Workflow
1. Identify what's worth logging: state changes, decisions, failures — not every method
   entry/exit.
2. Use the appropriate level and structured parameters.
3. For cross-cutting instrumentation (request duration, dependency call duration), prefer
   OpenTelemetry auto-instrumentation over manual `Stopwatch` logging where available.
4. Ensure correlation ids flow through async/background work, not just the initial request.

## Code Standards
```csharp
builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("orders-api"))
    .WithTracing(t => t.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation().AddOtlpExporter())
    .WithMetrics(m => m.AddAspNetCoreInstrumentation().AddRuntimeInstrumentation().AddOtlpExporter());

public sealed class CreateOrderHandler(IOrderRepository repository, ILogger<CreateOrderHandler> logger)
{
    public async Task<Result<Guid>> HandleAsync(CreateOrderCommand command, CancellationToken ct)
    {
        logger.LogInformation("Creating order for customer {CustomerId} with {LineCount} lines",
            command.CustomerId, command.Lines.Count);

        try
        {
            var order = Order.Create(command.CustomerId);
            // ...
            await repository.AddAsync(order, ct);
            logger.LogInformation("Order {OrderId} created", order.Id);
            return Result<Guid>.Success(order.Id.Value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create order for customer {CustomerId}", command.CustomerId);
            throw;
        }
    }
}
```

## Architecture Constraints
`ILogger<T>` is injected like any other dependency; logging code doesn't leak into Domain
entities (Domain raises events/exceptions; Application/Infrastructure logs them).

## Security Considerations
Audit log statements for accidental secret/PII exposure whenever reviewing a diff that logs
a new object — logging a whole request/entity via `{@Request}` structured capture can
inadvertently include sensitive fields.

## Testing Requirements
Logging itself is rarely unit-tested directly; when a log statement's presence is a
requirement (e.g. an audit trail), assert via an `ILogger` test double capturing calls.

## Common Mistakes
- String-interpolating log messages instead of using structured parameters.
- Logging at `Information` for high-frequency, low-significance events, flooding the log
  store and burying real signals.
- Missing correlation id propagation into background/queued work, breaking traceability.

## Anti-Patterns
- `Console.WriteLine` debugging left in committed code.
- A vendor-specific logging SDK used directly throughout business logic instead of the
  `ILogger`/OpenTelemetry abstraction.

## Validation Checklist
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Definition of Done
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Example
See [Code Standards](#code-standards).

## Related Skills
[`health-checks`](../dotnet/health-checks.md),
[`observability-and-monitoring`](../devops/observability-and-monitoring.md).
