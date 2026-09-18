# Skill: Middleware and Filters

## Purpose
Choose correctly between middleware (pipeline-wide, transport-level concerns) and filters
(MVC/endpoint-specific, request-handling concerns), and place middleware at the correct
pipeline position.

## When to Use
Adding cross-cutting behavior: logging, exception handling, request/response transformation,
per-endpoint authorization augmentation.

## Prerequisites
[`aspnetcore-fundamentals`](../dotnet/aspnetcore-fundamentals.md).

## Inputs Required
The cross-cutting behavior needed and whether it applies to the whole pipeline or specific
endpoints/controllers.

## Engineering Principles
- Middleware runs for every request through the pipeline (or the branch it's mapped to) —
  use it for genuinely pipeline-wide concerns (exception handling, HTTPS redirection,
  authentication, response compression).
- Filters (`IEndpointFilter` for Minimal APIs, action/result filters for MVC) run within the
  endpoint execution and have access to route/model-bound data — use them for
  endpoint-specific cross-cutting concerns (input validation shortcuts, response shaping).
- Order matters and is not automatically re-derived — always insert new middleware with the
  existing pipeline's order in mind (see
  [`aspnetcore-fundamentals`](../dotnet/aspnetcore-fundamentals.md) for the canonical order).
- Prefer `IEndpointFilter` over middleware for Minimal API-specific concerns — it composes
  per route group without affecting unrelated routes.

## Step-by-Step Workflow
1. Decide scope: whole pipeline (middleware) or specific endpoints (filter).
2. For middleware: implement `IMiddleware` or a conventional
   `Invoke(HttpContext, RequestDelegate)` class; register at the correct pipeline position.
3. For a filter: implement `IEndpointFilter`; apply via `.AddEndpointFilter<T>()` on the
   relevant route(s)/group.
4. Test that it fires for the intended requests and doesn't fire (or correctly short-circuits)
   for others.

## Code Standards
```csharp
public sealed class RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        await next(context);
        logger.LogInformation("{Method} {Path} took {ElapsedMs}ms",
            context.Request.Method, context.Request.Path, sw.ElapsedMilliseconds);
    }
}
app.UseMiddleware<RequestTimingMiddleware>();

public sealed class ValidateOrderFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var request = context.GetArgument<CreateOrderRequest>(0);
        if (request.Lines.Count == 0)
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["lines"] = ["At least one order line is required."]
            });

        return await next(context);
    }
}
app.MapPost("/orders", CreateOrder).AddEndpointFilter<ValidateOrderFilter>();
```

## Architecture Constraints
Middleware/filters handle transport-level concerns; they don't contain business logic that
belongs in Application/Domain.

## Security Considerations
Exception-handling middleware must not leak stack traces or internal details outside
`Development` — return a `ProblemDetails` with a generic message and a correlation id.

## Testing Requirements
Integration test that the middleware/filter fires and produces the expected effect (header
added, short-circuit response, etc.) — see [`integration-testing`](../testing/integration-testing.md).

## Common Mistakes
- Implementing authentication/authorization logic as a filter when it should be centralized
  middleware/policy — duplicated, inconsistent checks across endpoints.
- Registering middleware after `MapXxx()` calls it was meant to protect.

## Anti-Patterns
- A "kitchen sink" middleware that does five unrelated things — split by concern so each can
  be reasoned about and tested independently.

## Validation Checklist
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Definition of Done
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Example
See [Code Standards](#code-standards).

## Related Skills
[`aspnetcore-fundamentals`](../dotnet/aspnetcore-fundamentals.md),
[`model-validation-problemdetails`](../dotnet/model-validation-problemdetails.md).
