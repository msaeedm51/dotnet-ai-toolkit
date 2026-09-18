# Skill: Configuration and the Options Pattern

## Purpose
Bind configuration to strongly-typed, validated classes instead of scattering
`IConfiguration["Key:SubKey"]` string lookups through the codebase.

## When to Use
Adding any new configurable value (connection string, external service setting, feature
toggle) to a service.

## Prerequisites
[`aspnetcore-fundamentals`](../dotnet/aspnetcore-fundamentals.md).

## Inputs Required
The setting(s) being added and their source (`appsettings.json`, environment variables,
secret store).

## Engineering Principles
- Define an options class per cohesive configuration section; bind with
  `services.AddOptions<T>().Bind(configuration.GetSection("Section"))`.
- Validate at startup with `.ValidateDataAnnotations().ValidateOnStart()` for anything
  required for the app to function — fail at boot, not on first use in production.
- Use `IOptions<T>` for values that don't change without a restart, `IOptionsSnapshot<T>`
  for values that should be re-read per scope/request (rare), `IOptionsMonitor<T>` only when
  you genuinely need live-reload change notifications.
- Configuration precedence follows the standard ASP.NET Core provider order
  (`appsettings.json` → `appsettings.{Environment}.json` → environment variables → command
  line, later overrides earlier) — don't fight it with custom logic unless there's a real
  reason.

## Step-by-Step Workflow
1. Add the setting to `appsettings.json` (with a safe/non-secret default or omit if it's a
   secret — secrets come from environment variables or a secret store, never committed).
2. Define/extend an options class with data annotations for required fields.
3. Register and bind it with validation.
4. Inject `IOptions<T>`/`IOptionsSnapshot<T>` where needed; never re-parse `IConfiguration`
   ad hoc alongside it.

## Code Standards
```csharp
public sealed class PaymentOptions
{
    [Required]
    public required string Provider { get; init; }

    [Range(1, 300)]
    public int TimeoutSeconds { get; init; } = 30;
}

builder.Services.AddOptions<PaymentOptions>()
    .Bind(builder.Configuration.GetSection("Payment"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

public sealed class PaymentService(IOptions<PaymentOptions> options)
{
    private readonly PaymentOptions _options = options.Value;
}
```

## Architecture Constraints
Options classes live near the component that consumes them (or in a shared
`Infrastructure`/`Configuration` folder by convention) — not in Domain.

## Security Considerations
Never commit a real secret value as an `appsettings.json` default "to make local dev work" —
use user-secrets (`dotnet user-secrets`) locally and a real secret store per environment.

## Testing Requirements
Options validation logic (custom `IValidateOptions<T>` implementations) gets unit tests;
binding itself is proven by the app booting in an integration test.

## Common Mistakes
- Reading `IConfiguration["Payment:Provider"]` directly in a service instead of binding to
  `PaymentOptions`.
- Forgetting `ValidateOnStart()`, so a missing required setting only surfaces when that code
  path first runs in production.

## Anti-Patterns
- One giant options class bound to the entire configuration root — defeats the point of
  scoping validation and injection to what a component actually needs.

## Validation Checklist
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Definition of Done
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Example
See [Code Standards](#code-standards).

## Related Skills
[`aspnetcore-fundamentals`](../dotnet/aspnetcore-fundamentals.md),
[`dependency-injection`](../dotnet/dependency-injection.md).
