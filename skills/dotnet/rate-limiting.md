# Skill: Rate Limiting

## Purpose
Protect endpoints from abuse/overload using ASP.NET Core's built-in rate limiting
middleware, sized to the endpoint's actual risk (public/unauthenticated endpoints need it
most).

## When to Use
Any public-facing endpoint, especially unauthenticated ones (login, registration, password
reset, public search) — and any endpoint proxying to a costly downstream resource.

## Prerequisites
[`aspnetcore-fundamentals`](../dotnet/aspnetcore-fundamentals.md).

## Inputs Required
The endpoint's expected legitimate traffic pattern, to set limits that don't block real
users.

## Engineering Principles
- Use `Microsoft.AspNetCore.RateLimiting` (built into ASP.NET Core) rather than a hand-rolled
  counter — get correct concurrency handling and standard policies (fixed window, sliding
  window, token bucket, concurrency limiter) for free.
- Partition limits by a meaningful key — per-IP for unauthenticated endpoints, per-user/
  per-API-key for authenticated ones — a single global limit either blocks everyone too
  early or lets one abusive caller exhaust the budget for everyone else.
- Unauthenticated, security-sensitive endpoints (login, password reset) need a tighter limit
  than general API traffic — they're the most common target for credential-stuffing/
  enumeration attacks.
- Return `429 Too Many Requests` with a `Retry-After` header, not a generic error.

## Step-by-Step Workflow
1. Identify the endpoint's risk profile (public/unauthenticated = higher priority).
2. Choose a partitioning key (IP, user id, API key) appropriate to the auth state.
3. Configure a policy sized to legitimate traffic plus reasonable headroom, not an
   arbitrarily tight number that blocks real usage.
4. Apply the policy to the specific endpoint/group via `RequireRateLimiting()`.
5. Test that the limit triggers `429` under simulated burst traffic and doesn't trigger
   under normal usage.

## Code Standards
```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("login", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
            }));

    options.OnRejected = async (context, ct) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsync("Too many requests. Try again shortly.", ct);
    };
});

app.MapPost("/auth/login", LoginHandler).RequireRateLimiting("login");
```

## Architecture Constraints
Rate limiting configuration lives in the composition root; the limit values themselves
should be configurable (via options) rather than hard-coded, so they can be tuned without a
code change.

## Security Considerations
Rate limiting is a defense-in-depth control, not a substitute for proper authentication/
authorization — it reduces brute-force feasibility but doesn't replace account lockout,
CAPTCHA, or MFA where those are warranted.

## Testing Requirements
Integration test that exceeding the configured limit returns `429`, and that requests within
the limit succeed normally.

## Common Mistakes
- A single global rate limit shared across all endpoints, under-protecting sensitive ones
  and over-restricting high-traffic legitimate ones.
- Partitioning by IP behind a shared corporate NAT/proxy without accounting for many
  legitimate users sharing one IP.

## Anti-Patterns
- Implementing rate limiting purely at a reverse-proxy/CDN layer with no application-level
  awareness, losing the ability to partition by authenticated user/API key.

## Validation Checklist
See [`checklists/api-design-checklist.md`](../../checklists/api-design-checklist.md).

## Definition of Done
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Example
See [Code Standards](#code-standards).

## Related Skills
[`rest-api-design`](../api/rest-api-design.md), [`security-owasp`](../security/security-owasp.md).
