# Skill: Model Validation and ProblemDetails

## Purpose
Validate incoming requests at the boundary and return a consistent, standards-based error
shape (RFC 9457 `ProblemDetails`).

## When to Use
Any endpoint accepting input.

## Prerequisites
[`rest-api-design`](../api/rest-api-design.md).

## Inputs Required
The request DTO and its validation rules (required fields, ranges, formats, cross-field
rules).

## Engineering Principles
- Validate at the boundary, before the request reaches Application/Domain logic — Domain
  still enforces its own invariants independently (defense in depth), but the API shouldn't
  rely on a domain exception as its only validation mechanism.
- Use data annotations for simple per-field rules; use `FluentValidation` (if already in the
  project) or a manual validator for cross-field/conditional rules data annotations can't
  express cleanly — don't introduce FluentValidation into a project that doesn't already use
  it without a reason.
- Validation failures return `400 Bad Request` with a `ValidationProblemDetails` body listing
  every failing field, not just the first one.
- `ProblemDetails` for all error responses (validation and otherwise) keeps the error shape
  consistent across the whole API — configure `AddProblemDetails()` once, globally.

## Step-by-Step Workflow
1. Define validation rules on the request DTO (annotations) or a dedicated validator.
2. For Minimal APIs: either rely on automatic annotation validation (built-in as of .NET 8's
   Minimal API validation support) or an `IEndpointFilter` that validates and short-circuits.
3. For MVC: `[ApiController]` automatically returns `400` with `ValidationProblemDetails` for
   annotation failures — don't duplicate that manually.
4. For business-rule failures surfaced from Application (via `Result<T>`), map the failure to
   the correct status code (`400`/`404`/`409`/`422`) and a `ProblemDetails` body, not a bare
   `500`.

## Code Standards
```csharp
public sealed record CreateOrderRequest(
    [property: Required] Guid CustomerId,
    [property: MinLength(1)] IReadOnlyList<OrderLineRequest> Lines);

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
    };
});

app.MapPost("/orders", (CreateOrderRequest request) =>
{
    if (request.Lines.Count == 0)
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["lines"] = ["At least one order line is required."]
        });
    // ...
    return Results.Ok();
});

// Mapping a Result<T> failure to ProblemDetails
public static IResult ToProblem(this Result result) => result.ErrorType switch
{
    ErrorType.Validation => Results.ValidationProblem(result.ToErrorDictionary()),
    ErrorType.NotFound => Results.Problem(statusCode: 404, title: result.Error),
    ErrorType.Conflict => Results.Problem(statusCode: 409, title: result.Error),
    _ => Results.Problem(statusCode: 500),
};
```

## Architecture Constraints
Validation-error-to-HTTP-status mapping lives at the API boundary; Application/Domain code
returns a `Result<T>`/throws a domain exception without knowing about HTTP status codes.

## Security Considerations
Don't reflect raw exception messages into a `ProblemDetails` `detail` field in production —
map known error types to safe messages; log the full exception server-side instead.

## Testing Requirements
Every endpoint needs at least one validation-failure test asserting `400` and the expected
field-level error — see [`integration-testing`](../testing/integration-testing.md).

## Common Mistakes
- Returning a bare `400` with no body/detail, leaving the caller to guess what was wrong.
- Letting an unhandled exception surface as a generic `500` with a raw stack trace instead of
  going through the configured `ProblemDetails` exception handler.

## Anti-Patterns
- A custom error response shape invented per endpoint instead of the project-wide
  `ProblemDetails` convention.

## Validation Checklist
See [`checklists/api-design-checklist.md`](../../checklists/api-design-checklist.md).

## Definition of Done
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Example
See [Code Standards](#code-standards).

## Related Skills
[`rest-api-design`](../api/rest-api-design.md),
[`result-pattern`](../architecture/result-pattern.md).
