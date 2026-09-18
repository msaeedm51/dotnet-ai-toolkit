# Skill: Minimal APIs vs. Controllers

## Purpose
Choose consistently between Minimal API endpoints and MVC controllers, and avoid mixing
styles without reason within the same project.

## When to Use
Starting a new API project, or adding an endpoint to a project that already has an
established style.

## Prerequisites
[`aspnetcore-fundamentals`](../dotnet/aspnetcore-fundamentals.md).

## Inputs Required
The project's existing convention (check for `MapControllers()`/`[ApiController]` classes
vs. `MapGet`/`MapPost` route registrations) — match it. This is a new-project decision only
when there's no existing convention.

## Engineering Principles
- **Match what's already there.** Introducing a second style into an established project
  fragments conventions, tooling (filters, model binding, OpenAPI generation) and onboarding
  — this rule outranks personal preference.
- For a **new** project with no existing convention: Minimal APIs suit small-to-medium
  surface areas, microservices, and teams that want less ceremony; MVC controllers suit
  larger surface areas benefiting from `[ApiController]` conventions, view engines
  (unlikely for a pure API), and teams with existing MVC-based tooling/muscle memory.
- Both support the same underlying features (model binding, validation, filters,
  authorization, OpenAPI) in modern .NET — this is a style/ergonomics choice, not a
  capability tradeoff for a pure API.
- Group related Minimal API endpoints with `MapGroup()` and an extension method
  (`app.MapOrdersEndpoints()`) to avoid one enormous `Program.cs`.

## Step-by-Step Workflow
1. Check the project's existing style before adding a new endpoint.
2. If Minimal APIs: add to (or create) a feature-grouped extension method using `MapGroup()`.
3. If controllers: add an action to the existing/appropriate controller, following its
   existing attribute and DI conventions.
4. Keep the decision itself out of scope for a routine feature task — don't propose
   switching styles as part of an unrelated change.

## Code Standards
```csharp
// Minimal API, grouped
public static class OrdersEndpoints
{
    public static void MapOrdersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/orders").WithTags("Orders").RequireAuthorization();

        group.MapPost("/", CreateOrder).WithName("CreateOrder");
        group.MapGet("/{id:guid}", GetOrder).WithName("GetOrder");
    }

    private static async Task<IResult> CreateOrder(CreateOrderRequest request, /* ... */) => Results.Ok();
    private static async Task<IResult> GetOrder(Guid id, /* ... */) => Results.Ok();
}

// MVC controller equivalent
[ApiController]
[Route("orders")]
[Authorize]
public sealed class OrdersController(ICommandHandler<CreateOrderCommand, Result<Guid>> handler) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateOrderRequest request, CancellationToken ct)
    {
        var result = await handler.HandleAsync(request.ToCommand(), ct);
        return result.IsSuccess ? CreatedAtAction(nameof(GetById), new { id = result.Value }, null) : ValidationProblem();
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id) => Ok();
}
```

## Architecture Constraints
Neither style should contain business logic — both delegate to Application/Domain
(see [`clean-architecture`](../architecture/clean-architecture.md)).

## Security Considerations
Authorization attributes/`RequireAuthorization()` calls must be applied consistently
regardless of style — a mixed-style project is more prone to an endpoint accidentally
missing the check because it doesn't inherit a base controller's `[Authorize]`.

## Testing Requirements
Both styles are tested identically via `WebApplicationFactory` — see
[`integration-testing`](../testing/integration-testing.md).

## Common Mistakes
- Introducing controllers into a Minimal API project (or vice versa) for a single endpoint
  "because it was easier," fragmenting the project's conventions.
- Forgetting `RequireAuthorization()`/`[Authorize]` on a new endpoint because it wasn't
  inherited from a base class/group the way the rest of the project's endpoints are.

## Anti-Patterns
- A `Program.cs` with 40+ inline `MapGet`/`MapPost` calls and no grouping — extract to
  feature-grouped extension methods regardless of which style is chosen.

## Validation Checklist
See [`checklists/api-design-checklist.md`](../../checklists/api-design-checklist.md).

## Definition of Done
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Example
See [Code Standards](#code-standards).

## Related Skills
[`rest-api-design`](../api/rest-api-design.md),
[`aspnetcore-fundamentals`](../dotnet/aspnetcore-fundamentals.md).
