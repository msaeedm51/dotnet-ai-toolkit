# .NET API Template

A minimal, buildable ASP.NET Core 8 Minimal API — the starting point for a simple API
service with no layering imposed yet (see the
[`clean-architecture`](../clean-architecture/) template if you need Domain/Application/
Infrastructure separation from day one).

## Structure

```
DotnetApi.sln
src/DotnetApi.Api/
  Program.cs              -- composition root: health checks, ProblemDetails, Swagger
  Items/ItemsEndpoints.cs  -- a working example feature (grouped Minimal API endpoints,
                              validation, an in-memory store) -- replace with your own
tests/DotnetApi.Api.Tests/
  ItemsEndpointsTests.cs   -- WebApplicationFactory-based integration tests
```

The `Items` feature demonstrates several toolkit skills at once:
[`aspnetcore-fundamentals`](../../../skills/dotnet/aspnetcore-fundamentals.md),
[`rest-api-design`](../../../skills/api/rest-api-design.md),
[`model-validation-problemdetails`](../../../skills/dotnet/model-validation-problemdetails.md),
[`health-checks`](../../../skills/dotnet/health-checks.md), and
[`integration-testing`](../../../skills/testing/integration-testing.md). Replace it with your
actual first feature; keep the same shape (grouped endpoints, DTOs, `ProblemDetails`
validation, a `WebApplicationFactory` test) if it still fits.

## Using this template

1. Copy this folder, rename `DotnetApi`/`DotnetApi.Api` throughout to your service's name.
2. Vendor the toolkit at `.ai-dotnet/` per the root [README.md](../../../README.md#installing-in-a-net-project).
3. Copy `.ai-dotnet.config.example.yaml` to `.ai-dotnet/config.yaml` and fill in your actual
   database/auth/deployment choices as you add them — it starts with `database.engine: none`
   and `authentication.scheme: none` since this template has neither yet.
4. Add a database via [`efcore-fundamentals`](../../../skills/data/efcore-fundamentals.md)
   and authentication via [`authentication`](../../../skills/security/authentication.md) when
   your service needs them — don't add either speculatively.

## Verify it builds and passes

```bash
dotnet test
```

Applicable toolkit rules: [`rules/api.md`](../../../rules/api.md),
[`rules/dotnet.md`](../../../rules/dotnet.md), [`rules/csharp.md`](../../../rules/csharp.md),
[`rules/testing.md`](../../../rules/testing.md).
