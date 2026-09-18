# Skill: REST API Design

## Purpose
Design HTTP APIs that are predictable, consistent, and safe to evolve — correct resource
modeling, HTTP semantics, error shapes, and boundary validation.

## When to Use
Adding or changing any HTTP endpoint.

## Prerequisites
Know the project's existing route/DTO/error-response conventions before adding a new
endpoint — consistency with what's there beats textbook REST purity.

## Inputs Required
The resource/operation being exposed, the caller's auth context, and the project's existing
endpoint conventions (route style, versioning scheme if any, error shape).

## Engineering Principles
- Model resources as nouns; use HTTP methods for the verb: `GET` (read, safe, idempotent),
  `POST` (create / non-idempotent action), `PUT` (full replace, idempotent), `PATCH`
  (partial update), `DELETE` (remove, idempotent).
- Status codes are part of the contract: `200`/`201`/`204` for success variants, `400` for
  validation failure, `401` unauthenticated, `403` authenticated-but-forbidden, `404` not
  found, `409` conflict, `422` semantically invalid, `429` rate limited, `5xx` server error.
  Don't return `200` with an error payload.
- Never expose domain entities or EF Core entities directly — every request/response is a
  DTO defined at the API boundary.
- Validate all input at the boundary before it reaches business logic
  (see [`model-validation-problemdetails`](../dotnet/model-validation-problemdetails.md)).
- Errors use a consistent shape — `ProblemDetails` (RFC 9457) unless the project has an
  established alternative.
- Paginate any collection endpoint that can return more than a small bounded set; support
  filtering/sorting via query parameters with documented, validated allow-lists (never pass
  a raw sort/filter string into a query).
- `POST` endpoints that create a resource as a side effect of an external call (payment,
  message send) should support an idempotency key to make retries safe.

## Step-by-Step Workflow
1. Identify the resource and operation; choose the HTTP method and route accordingly
   (`POST /orders`, `GET /orders/{id}`, `GET /orders?status=pending&page=2`).
2. Define the request DTO (if any) and its validation rules.
3. Define the response DTO(s) for success and the error shape for failure.
4. Determine the authorization requirement for this specific operation — don't assume the
   route-group default is correct without checking.
5. Implement the handler, delegating business logic to Application/domain code rather than
   inlining it in the endpoint.
6. Add pagination/filtering if the response is a collection.
7. Write tests: success, validation failure, authorization failure, not-found where
   applicable.
8. Update OpenAPI metadata/documentation.

## Code Standards
```csharp
app.MapPost("/orders", async (
        CreateOrderRequest request,
        ICommandHandler<CreateOrderCommand, Result<Guid>> handler,
        CancellationToken ct) =>
    {
        var result = await handler.HandleAsync(request.ToCommand(), ct);
        return result.IsSuccess
            ? Results.Created($"/orders/{result.Value}", new { id = result.Value })
            : Results.ValidationProblem(result.ToErrorDictionary());
    })
    .RequireAuthorization("orders:write")
    .WithName("CreateOrder")
    .Produces<object>(StatusCodes.Status201Created)
    .ProducesValidationProblem();

app.MapGet("/orders", async (
        [AsParameters] OrderListQuery query,
        IQueryHandler<OrderListQuery, PagedResult<OrderSummaryDto>> handler,
        CancellationToken ct) =>
        Results.Ok(await handler.HandleAsync(query, ct)))
    .RequireAuthorization("orders:read")
    .WithName("ListOrders");

public sealed record OrderListQuery(int Page = 1, int PageSize = 20, string? Status = null);
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
```

## Architecture Constraints
Endpoint handlers stay thin — they translate HTTP ⇄ Application layer calls; business logic
lives in Application/Domain (see [`clean-architecture`](../architecture/clean-architecture.md)
if in use).

## Security Considerations
- Enforce authorization per-operation, not just per-route-group — a `GET` on a collection
  and a `GET` on an item by id can have different authorization requirements (e.g. "list my
  orders" vs. "view any order" needs an admin check).
- Never reflect unvalidated input back into an error message in a way that enables
  reflected XSS if the API is ever consumed by a browser directly.
- Rate-limit public-facing endpoints, especially unauthenticated ones
  (see [`rate-limiting`](../dotnet/rate-limiting.md)).

## Testing Requirements
Per endpoint: at least one success test, one validation-failure test, one
authorization-failure test. See [`integration-testing`](../testing/integration-testing.md).

## Common Mistakes
- Returning `200 OK` for a resource that wasn't found instead of `404`.
- Using `GET` for an operation with side effects (breaks caching/idempotency assumptions).
- Unbounded collection endpoints with no pagination, breaking under real production data
  volume.
- Different error shapes across endpoints in the same API.

## Anti-Patterns
- A single generic `/api/execute` endpoint that takes an operation name and dispatches
  internally — defeats HTTP semantics, caching, and authorization-per-operation.
- Returning different DTO shapes for the same resource from different endpoints without
  reason.

## Validation Checklist
See [`checklists/api-design-checklist.md`](../../checklists/api-design-checklist.md) and
[`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Definition of Done
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Example
See [Code Standards](#code-standards).

## Related Skills
[`api-versioning-and-compatibility`](../api/api-versioning-and-compatibility.md),
[`model-validation-problemdetails`](../dotnet/model-validation-problemdetails.md),
[`authentication`](../security/authentication.md),
[`authorization`](../security/authorization.md),
[`integration-testing`](../testing/integration-testing.md).
