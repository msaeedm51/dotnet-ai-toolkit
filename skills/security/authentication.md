# Skill: Authentication

## Purpose
Set up and correctly consume authentication in ASP.NET Core — verifying who the caller is,
before any authorization decision is made.

## When to Use
Adding authentication to a new API, adding a new authentication scheme, or implementing an
endpoint that needs to read the authenticated caller's identity.

## Prerequisites
Know the project's chosen scheme (`config.yaml backend.authentication.scheme`) — JWT bearer,
cookie, or an external OIDC provider. Don't introduce a second scheme alongside an existing
one without an explicit reason.

## Inputs Required
The identity provider details (issuer, audience, signing key source), and whether this is a
new setup or adding a consumer to an existing one.

## Engineering Principles
- Authentication answers "who is this caller," authorization answers "what can they do" —
  keep them as separate concerns even though they're configured adjacently.
- For API-to-API and SPA-to-API scenarios, JWT bearer tokens validated against the issuing
  authority (Azure AD, IdentityServer, Auth0, etc.) is the default; for server-rendered apps
  with a browser session, cookie authentication is appropriate.
- Never validate a JWT's signature against a key baked into application code — always
  resolve signing keys dynamically from the issuer's JWKS endpoint (or a rotated
  configuration), so key rotation doesn't require a redeploy.
- Token lifetime should be short for access tokens (minutes-to-low-hours); use refresh
  tokens (rotated, revocable) rather than long-lived access tokens.

## Step-by-Step Workflow
1. Confirm the identity provider and scheme.
2. Register the authentication handler with correct validation parameters (issuer, audience,
   signature).
3. Add `UseAuthentication()` before `UseAuthorization()` in the pipeline
   (see [`aspnetcore-fundamentals`](../dotnet/aspnetcore-fundamentals.md)).
4. In handlers/endpoints, read identity via `ClaimsPrincipal` (`HttpContext.User`), never by
   trusting a client-supplied header/body field for "who am I."
5. Test: valid token succeeds, expired token rejected, tampered token rejected, missing
   token rejected with `401`.

## Code Standards
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Auth:Authority"];
        options.Audience = builder.Configuration["Auth:Audience"];
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromMinutes(1),
        };
    });

// Reading identity in an endpoint
app.MapGet("/me", (ClaimsPrincipal user) =>
{
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
    return Results.Ok(new { userId });
}).RequireAuthorization();
```

## Architecture Constraints
Authentication configuration lives in the host/composition-root project; downstream
Application/Domain code depends only on an already-validated identity (e.g. a `UserId` value
passed in) — it doesn't parse tokens itself.

## Security Considerations
- `ValidateIssuerSigningKey`, `ValidateIssuer`, and `ValidateAudience` must all be `true` in
  production — a common misconfiguration disables one of these "to make it work" during
  debugging and it ships that way.
- Store client secrets (if using confidential-client OAuth flows) in a secret store, never
  in `appsettings.json`.
- For SPA/browser clients, prefer the Authorization Code flow with PKCE over implicit flow.
- See [`jwt-oauth-oidc`](../security/jwt-oauth-oidc.md) for token flow selection and
  [`security-owasp`](../security/security-owasp.md) for broader session-security concerns.

## Testing Requirements
Integration tests covering: valid token → `200`, no token → `401`, expired/tampered token →
`401`. See [`integration-testing`](../testing/integration-testing.md) for issuing test tokens
via a test authentication handler rather than a real IdP in CI.

## Common Mistakes
- Trusting a `userId` passed in the request body/header instead of extracting it from the
  validated token's claims.
- Disabling token validation checks "temporarily" to unblock local development, and it
  never gets re-enabled.
- Mixing cookie and bearer authentication on the same endpoint without an explicit policy
  for which one applies.

## Anti-Patterns
- Hand-rolled JWT parsing/validation instead of `Microsoft.AspNetCore.Authentication.JwtBearer`.
- Storing access tokens in `localStorage` on a SPA client (XSS-exfiltrable) instead of an
  `HttpOnly` cookie or in-memory storage with silent refresh.

## Validation Checklist
See [`checklists/security-review-checklist.md`](../../checklists/security-review-checklist.md).

## Definition of Done
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Example
See [Code Standards](#code-standards).

## Related Skills
[`authorization`](../security/authorization.md),
[`jwt-oauth-oidc`](../security/jwt-oauth-oidc.md),
[`security-owasp`](../security/security-owasp.md), [`rest-api-design`](../api/rest-api-design.md).
