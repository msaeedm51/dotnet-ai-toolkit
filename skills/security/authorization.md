# Skill: Authorization

## Purpose
Enforce what an authenticated caller is allowed to do — at the correct boundary, including
resource-level ownership, not just role/policy at the route level.

## When to Use
Any endpoint or operation that isn't universally accessible to every authenticated user.

## Prerequisites
[`authentication`](../security/authentication.md) — authorization presumes identity has
already been established.

## Inputs Required
The operation, who's allowed to perform it, and whether that depends on the specific
resource being accessed (not just the caller's role).

## Engineering Principles
- **Role-based** (`[Authorize(Roles = "Admin")]`) is coarse-grained — appropriate when the
  rule is genuinely about role membership.
- **Policy-based** (`[Authorize(Policy = "CanManageOrders")]`, defined via
  `AddAuthorization(options => options.AddPolicy(...))`) expresses more nuanced rules
  (claims-based, multi-condition) in a reusable, named, testable way.
- **Resource-based** authorization (`IAuthorizationService.AuthorizeAsync(user, resource,
  requirement)`) is required whenever "can this user do X" depends on the specific resource
  instance, not just their role — e.g. "can edit *their own* order" vs. "can edit *any*
  order." A route-level `[Authorize]` alone cannot express this; it must be checked in the
  handler against the loaded resource.
- Authorization is enforced at the point of execution — checking it only in a shared
  middleware that doesn't know about the specific resource being loaded further down the
  pipeline leaves resource-level checks undone (the IDOR-class bug class from
  [`security-owasp`](../security/security-owasp.md)).
- Fail closed: an authorization check that can't determine the answer (missing claim, unknown
  resource) denies, it doesn't default to allow.

## Step-by-Step Workflow
1. Identify whether the rule is role-level, policy-level, or resource-level.
2. Role/policy: apply `[Authorize(Roles = ...)]`/`[Authorize(Policy = ...)]` or
   `.RequireAuthorization(...)` at the route.
3. Resource-level: after loading the resource in the handler, explicitly check ownership/
   permission against the authenticated user before returning/mutating it.
4. Test both: authorized access succeeds, unauthorized access (wrong role, or right role but
   wrong resource owner) is denied with the correct status (`403`, not a silent `200` with
   filtered data unless that's the deliberate, documented behavior).

## Code Standards
```csharp
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("CanManageOrders", p => p.RequireRole("Admin", "OrderManager"));

app.MapGet("/orders/{id:guid}", async (
        Guid id, ClaimsPrincipal user, IOrderRepository repository,
        IAuthorizationService authz, CancellationToken ct) =>
    {
        var order = await repository.GetByIdAsync(new OrderId(id), ct);
        if (order is null) return Results.NotFound();

        // Resource-level check: is this order actually this caller's?
        var result = await authz.AuthorizeAsync(user, order, "OwnsOrder");
        if (!result.Succeeded) return Results.Forbid();

        return Results.Ok(order.ToDto());
    })
    .RequireAuthorization(); // authentication + baseline policy

public sealed class OwnsOrderRequirement : IAuthorizationRequirement { }

public sealed class OwnsOrderHandler : AuthorizationHandler<OwnsOrderRequirement, Order>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, OwnsOrderRequirement requirement, Order resource)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (resource.CustomerId.ToString() == userId || context.User.IsInRole("Admin"))
            context.Succeed(requirement);
        return Task.CompletedTask;
    }
}
```

## Architecture Constraints
Resource-level authorization checks happen in Application/Presentation, after loading the
resource — never inside Domain, which shouldn't model "the current user."

## Security Considerations
This entire skill is a security control — see
[`security-owasp`](../security/security-owasp.md)'s "Broken access control" section for the
IDOR-class failure this exists to prevent.

## Testing Requirements
Every authorization boundary needs both a positive test (authorized caller succeeds) and a
negative test (unauthorized caller — including "authenticated but wrong owner" — is denied).

## Common Mistakes
- Checking only role-based authorization at the route and assuming that's sufficient for an
  id-based resource lookup.
- Returning `404` instead of `403` (or vice versa) inconsistently — pick one policy
  deliberately (returning `404` for unauthorized access to avoid confirming a resource
  exists is a legitimate, deliberate choice in some contexts; document if used).

## Anti-Patterns
- Authorization logic duplicated ad hoc across multiple endpoints instead of expressed as a
  named, reusable policy/requirement.

## Validation Checklist
See [`checklists/security-review-checklist.md`](../../checklists/security-review-checklist.md).

## Definition of Done
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Example
See [Code Standards](#code-standards).

## Related Skills
[`authentication`](../security/authentication.md), [`security-owasp`](../security/security-owasp.md).
