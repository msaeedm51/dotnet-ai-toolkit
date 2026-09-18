# Skill: Background Services

## Purpose
Run long-running or scheduled work correctly inside an ASP.NET Core host using
`BackgroundService`/`IHostedService`, instead of ad hoc `Task.Run` from a request.

## When to Use
Polling a queue, running scheduled cleanup/maintenance, processing a message-broker
subscription, or any work that should run for the app's lifetime independent of any single
HTTP request.

## Prerequisites
[`aspnetcore-fundamentals`](../dotnet/aspnetcore-fundamentals.md).

## Inputs Required
The work to run, its trigger (interval, queue message, startup-once), and its failure
behavior (should the app crash if this fails repeatedly, or degrade gracefully).

## Engineering Principles
- `BackgroundService` (implementing `ExecuteAsync(CancellationToken)`) is the standard
  building block; register with `services.AddHostedService<T>()`.
- Never fire-and-forget a `Task.Run()` from within a request handler for work that should
  outlive the request — it isn't tracked by the host, isn't gracefully shut down, and
  exceptions in it are silently lost.
- Because `BackgroundService` typically runs for the app's lifetime, it can't depend directly
  on a `Scoped` service (like `DbContext`) — create a scope per unit of work with
  `IServiceScopeFactory`.
- Respect the provided `CancellationToken` — it's signaled on graceful shutdown; a background
  service that ignores it delays/blocks shutdown.
- An unhandled exception in `ExecuteAsync` stops the service (and, depending on host
  configuration, can stop the whole app) — catch and log expected transient failures inside
  the work loop; let genuinely fatal ones propagate.

## Step-by-Step Workflow
1. Implement `BackgroundService`, injecting `IServiceScopeFactory` (not `Scoped` services
   directly) plus any genuinely `Singleton` dependencies.
2. In `ExecuteAsync`, loop respecting the `CancellationToken`; create a new DI scope per unit
   of work.
3. Handle expected transient failures (a queue temporarily unavailable) with backoff/retry
   inside the loop; let unexpected exceptions propagate after logging.
4. Register with `AddHostedService<T>()`.

## Code Standards
```csharp
public sealed class OrderReminderService(
    IServiceScopeFactory scopeFactory,
    ILogger<OrderReminderService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await SendRemindersAsync(db, stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Order reminder run failed; will retry next tick.");
            }
        }
    }

    private static Task SendRemindersAsync(AppDbContext db, CancellationToken ct) => Task.CompletedTask;
}

builder.Services.AddHostedService<OrderReminderService>();
```

## Architecture Constraints
Background service classes live in Infrastructure/host project; they call into
Application/Domain the same way an API endpoint would, not with duplicated logic.

## Security Considerations
Background services often run with elevated/system-level credentials — apply the same
least-privilege principle to whatever they authenticate as.

## Testing Requirements
Unit test the work logic (extracted into a testable method/class) directly; integration-test
the hosted service's registration/startup if it has non-trivial wiring.

## Common Mistakes
- Injecting `DbContext` directly into a `BackgroundService` constructor (it's `Scoped`,
  the service is effectively `Singleton`-lifetime) instead of using
  `IServiceScopeFactory`.
- Not catching exceptions inside the work loop, causing the entire background service to
  stop after the first transient failure.
- Ignoring the `CancellationToken`, delaying graceful shutdown.

## Anti-Patterns
- `Task.Run(() => DoWorkAsync())` fired from an HTTP request handler for work that should
  survive beyond that request — use a proper queue + `BackgroundService`, or at minimum
  `IHostApplicationLifetime`-aware tracking.

## Validation Checklist
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Definition of Done
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Example
See [Code Standards](#code-standards).

## Related Skills
[`aspnetcore-fundamentals`](../dotnet/aspnetcore-fundamentals.md),
[`dependency-injection`](../dotnet/dependency-injection.md),
[`observability-and-monitoring`](../devops/observability-and-monitoring.md).
