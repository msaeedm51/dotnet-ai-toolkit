# Skill: ASP.NET Core Fundamentals

## Purpose
Establish the baseline conventions for an ASP.NET Core host — project bootstrap, DI
container usage, middleware pipeline ordering, and configuration — that every other ASP.NET
Core skill in this toolkit assumes.

## When to Use
Any task that touches `Program.cs`, adds/changes a service registration, adds middleware, or
needs to understand how a request flows through the app before making a change.

## Prerequisites
.NET 8+ SDK. Know whether the project uses Minimal APIs, MVC controllers, or both (check
`Program.cs` and whether `MapControllers()` is called) — see
[`minimal-apis-vs-controllers`](../dotnet/minimal-apis-vs-controllers.md) before choosing a
style for new code; match what's already there.

## Inputs Required
- `Program.cs` (or `Startup.cs` on older layouts — flag as legacy if found; single-file
  bootstrap has been standard since .NET 6).
- `appsettings.json` / `appsettings.{Environment}.json`.
- The project's existing service registrations, to avoid duplicating or conflicting with them.

## Engineering Principles
- One composition root (`Program.cs`). Service registration lives there or in small
  `IServiceCollection` extension methods it calls — not scattered across the codebase.
- Middleware order is semantics, not style: exception handling first, then HTTPS
  redirection, routing, authentication, authorization, then endpoints. Getting this wrong
  causes silent auth bypasses or unhandled exceptions leaking stack traces.
- Configuration flows from `IConfiguration` into strongly-typed `IOptions<T>` classes at the
  boundary — application code should never read `IConfiguration["Some:Key"]` directly deep
  in a service (see [`configuration-options`](../dotnet/configuration-options.md)).
- Services are registered with the narrowest lifetime that's correct: `Singleton` only for
  genuinely stateless or thread-safe shared state, `Scoped` for anything touching
  `DbContext` or a per-request unit of work, `Transient` for lightweight stateless services.

## Step-by-Step Workflow
1. Identify what's being added: a service, a middleware, an endpoint group, or a
   configuration section.
2. For a service: register it with the correct lifetime in `Program.cs` or an extension
   method grouped with related registrations (e.g. `AddApplicationServices()`,
   `AddInfrastructureServices()` if the project already has that convention).
3. For middleware: insert it at the correct pipeline position relative to existing
   middleware — read the existing pipeline top to bottom before inserting.
4. For configuration: add the section to `appsettings.json`, bind it to an options class with
   `services.AddOptions<T>().Bind(configuration.GetSection("Section")).ValidateDataAnnotations()`,
   inject `IOptions<T>`/`IOptionsSnapshot<T>` where needed.
5. Verify the app still starts and the change doesn't reorder unrelated middleware.

## Code Standards
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<OrdersOptions>()
    .Bind(builder.Configuration.GetSection("Orders"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();
builder.Services.AddAuthorization();

builder.Services.AddProblemDetails();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapOrdersEndpoints(); // extension method grouping this feature's routes

app.Run();
```

## Architecture Constraints
- `Program.cs` composes; it does not contain business logic.
- Respect the project's declared architecture (`config.yaml` `project.architecture`) for
  where service interfaces vs. implementations live — see
  [`clean-architecture`](../architecture/clean-architecture.md) if in use.

## Security Considerations
- `UseAuthentication()` must precede `UseAuthorization()`, and both must precede endpoint
  mapping.
- Don't call `UseDeveloperExceptionPage()`/detailed error output outside `Development`.
- `ValidateOnStart()` on options that include secrets/connection strings fails fast on
  misconfiguration instead of failing on first use in production.

## Testing Requirements
Service registration changes are usually covered indirectly via
[`integration-testing`](../testing/integration-testing.md) (`WebApplicationFactory` boot
proves the composition root is valid). Add a direct test only when a service has
non-trivial registration logic (conditional registration, decorators).

## Common Mistakes
- Registering a service as `Singleton` that depends on a `Scoped` service (captured
  dependency — throws at runtime, or worse, silently captures the first request's scope).
- Reading configuration directly via `IConfiguration` deep in a service instead of binding
  to options.
- Placing `UseAuthorization()` before `UseAuthentication()`.

## Anti-Patterns
- A `Program.cs` that grows past ~50-80 lines of raw registration — extract feature-grouped
  extension methods instead of one giant file.
- Service locator usage (`app.Services.GetService<T>()`) outside the composition root.

## Validation Checklist
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md). Skill-specific:
- [ ] New service registered with the correct lifetime.
- [ ] Middleware inserted at the correct pipeline position.
- [ ] Configuration bound to an options class, not read ad hoc.

## Definition of Done
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Example
See the [Code Standards](#code-standards) block above for a representative `Program.cs`.

## Related Skills
[`dependency-injection`](../dotnet/dependency-injection.md),
[`configuration-options`](../dotnet/configuration-options.md),
[`middleware-filters`](../dotnet/middleware-filters.md),
[`minimal-apis-vs-controllers`](../dotnet/minimal-apis-vs-controllers.md),
[`health-checks`](../dotnet/health-checks.md).
