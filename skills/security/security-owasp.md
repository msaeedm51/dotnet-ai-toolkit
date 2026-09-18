# Skill: Security Review (OWASP-Oriented, .NET-Specific)

## Purpose
Apply OWASP Top 10-class checks with concrete .NET mechanisms — not a generic vulnerability
list, but what to look for and how to fix it in ASP.NET Core/EF Core code.

## When to Use
Any change touching authentication, authorization, user input, file handling, external
calls, deserialization, logging, or dependencies. Always loaded by `security-reviewer`; also
relevant to `api-engineer` and `dotnet-developer` for first-pass self-review.

## Prerequisites
Know the project's actual authentication scheme and data sensitivity — don't apply
compliance-grade controls to a project that hasn't stated that requirement, but never skip
baseline controls either.

## Inputs Required
The diff/change under review, and the project's existing security conventions (don't
introduce a second auth pattern to "improve" security — flag if the existing one is
insufficient).

## Engineering Principles

**Injection (SQL, command, LDAP).** Use parameterized queries / EF Core LINQ always. Never
string-concatenate user input into SQL, even for "trusted" internal tools.
```csharp
// Wrong
var sql = $"SELECT * FROM Orders WHERE CustomerId = '{customerId}'";

// Right (EF Core)
var orders = await db.Orders.Where(o => o.CustomerId == customerId).ToListAsync(ct);

// Right (Dapper, when raw SQL is warranted)
var orders = await connection.QueryAsync<Order>(
    "SELECT * FROM Orders WHERE CustomerId = @CustomerId", new { CustomerId = customerId });
```

**Broken authentication / session management.** Use `Microsoft.AspNetCore.Authentication`
building blocks (JWT bearer or cookie auth) rather than a hand-rolled scheme. Tokens/session
cookies: `HttpOnly`, `Secure`, `SameSite=Strict`/`Lax` as appropriate. See
[`authentication`](../security/authentication.md).

**Sensitive data exposure.** Never log passwords, tokens, connection strings, or full PII.
Use `[JsonIgnore]`/DTO shaping to keep secrets out of serialized responses. Encrypt secrets
at rest via a secret store (Azure Key Vault, user-secrets in dev), never in
`appsettings.json` committed to source.
```csharp
// Wrong
logger.LogInformation("User {Email} logged in with password {Password}", email, password);

// Right
logger.LogInformation("User {UserId} logged in", userId);
```

**Broken access control.** Every operation checks authorization at the point of execution,
not just at the route level — an id-based lookup (`GET /orders/{id}`) must verify the caller
is entitled to *that* order, not just any order (IDOR — insecure direct object reference).
```csharp
var order = await repository.GetByIdAsync(orderId, ct);
if (order is null) return Results.NotFound();
if (order.CustomerId != currentUser.CustomerId && !currentUser.IsInRole("Admin"))
    return Results.Forbid();
```

**Security misconfiguration.** No detailed exceptions/stack traces outside `Development`.
HTTPS enforced (`UseHsts`, `UseHttpsRedirection`). CORS allow-lists specific origins, never
`AllowAnyOrigin()` combined with credentials.

**XSS.** ASP.NET Core Razor encodes output by default — don't defeat it with
`Html.Raw()`/`[AllowHtml]` on user-supplied content. For APIs consumed by a SPA, the SPA is
responsible for output encoding, but the API should still reject/sanitize obviously
malicious payloads and set `Content-Security-Policy` where the API serves any HTML.

**Insecure deserialization.** Use `System.Text.Json` with a fixed set of known types; avoid
`TypeNameHandling`-style polymorphic deserialization from untrusted input. Never deserialize
untrusted input into `BinaryFormatter` (removed/obsolete for a reason).

**Vulnerable components.** Run `dotnet list package --vulnerable` (or the project's SCA tool)
before adding/reviewing a dependency change; don't add a package with a known critical CVE
without an explicit mitigation.

**Insufficient logging/monitoring.** Log authentication failures, authorization denials, and
input-validation failures at a level that's actually monitored — without logging the
sensitive payload itself.

**SSRF.** If the server makes outbound HTTP calls based on user-supplied URLs, validate
against an allow-list of hosts/schemes; never fetch an arbitrary user-supplied URL from
server-side code without one.

**File upload.** Validate content type and size server-side (not just client-side), store
outside the web root or in blob storage, never execute uploaded content, and randomize
stored filenames to prevent path traversal (`../../etc/passwd`-style attacks via filename).

## Step-by-Step Workflow
1. Identify which of the above surfaces the change touches.
2. Apply the relevant check(s) above.
3. Trace authorization to its actual enforcement point in code — don't assume.
4. Check for secrets/PII in code, config, and log statements.
5. Check new/changed dependencies for known vulnerabilities.
6. Report findings per `checklists/security-review-checklist.md` with severity and concrete
   exploit scenario.

## Code Standards
See inline examples above.

## Architecture Constraints
Authorization logic belongs in Application/Presentation, not Domain (Domain shouldn't model
"the current user" — see [`clean-architecture`](../architecture/clean-architecture.md)).

## Security Considerations
This entire skill is security considerations — see Engineering Principles above.

## Testing Requirements
Every authorization boundary needs a negative test (forbidden/unauthorized case), not just a
happy-path test. IDOR-class bugs specifically need a test where an authenticated user
attempts to access another user's resource by id.

## Common Mistakes
- Assuming `[Authorize]` on a controller covers per-resource ownership checks — it only
  proves authentication/role, not resource ownership.
- Logging the full request body on error, which can capture passwords/tokens submitted in
  that request.
- Trusting client-supplied user/tenant identifiers instead of deriving identity from the
  validated token/session.

## Anti-Patterns
- Rolling a custom crypto/hashing scheme instead of `Microsoft.AspNetCore.Identity`'s
  `PasswordHasher<T>` or a vetted library.
- "We'll add auth later" on an endpoint that handles real data — there is no safe
  intermediate state for that.

## Validation Checklist
See [`checklists/security-review-checklist.md`](../../checklists/security-review-checklist.md).

## Definition of Done
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Example
See inline examples under Engineering Principles.

## Related Skills
[`authentication`](../security/authentication.md),
[`authorization`](../security/authorization.md),
[`jwt-oauth-oidc`](../security/jwt-oauth-oidc.md), [`rest-api-design`](../api/rest-api-design.md).
