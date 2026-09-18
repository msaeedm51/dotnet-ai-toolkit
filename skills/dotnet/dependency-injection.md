# Skill: Dependency Injection

## Purpose
Use ASP.NET Core's built-in DI container correctly — lifetime selection, avoiding captured
dependencies, and composition patterns that keep `Program.cs` manageable.

## When to Use
Registering a new service, diagnosing a DI-related runtime error, or deciding how a
component should receive its dependencies.

## Prerequisites
[`aspnetcore-fundamentals`](../dotnet/aspnetcore-fundamentals.md).

## Inputs Required
The service being registered and what it depends on (in particular, whether any dependency
is `Scoped`, e.g. `DbContext`).

## Engineering Principles
- Three lifetimes: `Singleton` (one instance, app lifetime — only for genuinely stateless or
  thread-safe shared state), `Scoped` (one per request/unit of work — anything touching
  `DbContext`), `Transient` (new instance every resolution — lightweight, stateless).
- A `Singleton` must never depend on a `Scoped` service — this either throws
  (`InvalidOperationException: Cannot consume scoped service`) with validation enabled, or
  silently captures the first scope's instance forever, a subtle and dangerous bug.
- Prefer constructor injection; avoid the service-locator pattern
  (`IServiceProvider.GetService<T>()` called deep in application code) outside the
  composition root or a few well-understood exceptions (e.g. a factory that must resolve a
  type at runtime).
- Group related registrations into `IServiceCollection` extension methods
  (`AddApplicationServices()`, `AddInfrastructureServices()`) instead of one long
  `Program.cs`.

## Step-by-Step Workflow
1. Determine the correct lifetime by what the service depends on and whether it holds
   per-request state.
2. Register via constructor-injected interface, not a concrete-type-only registration,
   unless there's genuinely only one implementation and no test double is ever needed.
3. If registering a decorator/wrapper, register the inner type with a distinguishing key or
   use `Scrutor`-style decoration only if the project already uses it — otherwise wire it
   explicitly via a factory registration.
4. Enable `ServiceProviderOptions.ValidateScopes = true` (default in `Development`) so
   captured-dependency bugs surface immediately, not in production.

## Code Standards
```csharp
// Correct: Scoped service depending on Scoped DbContext
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Correct: Singleton with no Scoped dependencies
builder.Services.AddSingleton<IClock>(TimeProvider.System);

// Wrong: Singleton capturing a Scoped dependency
builder.Services.AddSingleton<IOrderCache>(sp =>
    new OrderCache(sp.GetRequiredService<AppDbContext>())); // throws/captures scope

// Factory registration when the concrete type must be chosen at runtime
builder.Services.AddScoped<IPaymentProvider>(sp =>
{
    var config = sp.GetRequiredService<IOptions<PaymentOptions>>().Value;
    return config.Provider switch
    {
        "stripe" => sp.GetRequiredService<StripePaymentProvider>(),
        "paypal" => sp.GetRequiredService<PayPalPaymentProvider>(),
        _ => throw new InvalidOperationException($"Unknown provider '{config.Provider}'"),
    };
});
```

## Architecture Constraints
Registration of an interface's implementation happens in the layer that owns the
implementation (typically Infrastructure/Presentation composition root), not in
Application/Domain.

## Security Considerations
Don't register a service that reads secrets eagerly at startup into a `Singleton` field that
then gets logged/serialized inadvertently.

## Testing Requirements
DI misconfiguration is best caught by an integration test that boots the real
`WebApplicationFactory` (proves the whole graph resolves) — see
[`integration-testing`](../testing/integration-testing.md).

## Common Mistakes
- Registering `DbContext`-dependent services as `Singleton`.
- Resolving services via `IServiceProvider` deep in business logic instead of constructor
  injection.
- Registering the same interface multiple times without realizing only the last (or first,
  depending on resolution method) registration wins.

## Anti-Patterns
- A generic `IServiceLocator` wrapper around `IServiceProvider` used throughout the codebase
  — reintroduces the service locator anti-pattern DI is meant to avoid.

## Validation Checklist
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md). Skill-specific:
- [ ] Lifetime matches the service's actual statefulness and dependencies.
- [ ] No Singleton depends on a Scoped service.

## Definition of Done
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Example
See [Code Standards](#code-standards).

## Related Skills
[`aspnetcore-fundamentals`](../dotnet/aspnetcore-fundamentals.md),
[`configuration-options`](../dotnet/configuration-options.md),
[`dependency-injection-design`](../architecture/dependency-injection-design.md).
