# Clean Architecture Template

A buildable, tested .NET 8 solution demonstrating
[`clean-architecture`](../../../skills/architecture/clean-architecture.md): Domain →
Application → Infrastructure/Api, with the dependency rule enforced by an automated
architecture test, not just convention.

## Structure

```
CleanArchitecture.sln
src/
  CleanArchitecture.Domain/          -- Product aggregate, Result<T>. No package references.
  CleanArchitecture.Application/     -- CreateProduct/GetProducts commands+handlers,
                                         IProductRepository (interface only).
  CleanArchitecture.Infrastructure/  -- AppDbContext, ProductConfiguration, ProductRepository
                                         (EF Core + SQLite).
  CleanArchitecture.Api/             -- Minimal API endpoints, Program.cs composition root.
tests/
  CleanArchitecture.Application.Tests/  -- unit tests (fake repository) + ArchitectureTests.cs
                                            (NetArchTest, enforces the dependency rule)
  CleanArchitecture.Api.Tests/          -- WebApplicationFactory integration tests against
                                            an isolated in-memory SQLite database
```

**Why SQLite, not SQL Server/PostgreSQL:** this template needs to build and pass its tests
with zero external services, in this repository's own CI. SQLite is a real relational engine
(unlike EF Core's in-memory provider, it validates real SQL translation), so the pattern
demonstrated is legitimate — but swap to your actual production engine
([`sql-server`](../../../skills/data/sql-server.md) or
[`postgresql`](../../../skills/data/postgresql.md)) before this goes beyond a demo. Only
`Program.cs` and the `.csproj` package reference need to change; `AppDbContext`,
`ProductConfiguration`, and `ProductRepository` are engine-agnostic EF Core code.

## Using this template

1. Copy this folder, rename `CleanArchitecture`/`Product` throughout to your domain's actual
   name.
2. Vendor the toolkit at `.ai-dotnet/` per the root [README.md](../../../README.md#installing-in-a-net-project).
3. Copy `.ai-dotnet.config.example.yaml` to `.ai-dotnet/config.yaml`.
4. Replace `Product`/`Products` with your actual first aggregate, following the same
   four-project shape.
5. Switch the database engine when you're past prototyping (see above).
6. Replace `Database.EnsureCreated()` in `Program.cs` with real migrations
   ([`efcore-migrations`](../../../skills/data/efcore-migrations.md)) before this handles
   real data.

## Verify it builds and passes

```bash
dotnet test
```

This runs 8 tests: 3 unit tests for `CreateProductHandler` (success + 2 validation
failures), 2 architecture tests (Domain has no Application/Infrastructure dependency;
Application has no Infrastructure/EF Core dependency), and 3 API integration tests through
a real `WebApplicationFactory`.

Applicable toolkit rules: [`rules/architecture.md`](../../../rules/architecture.md),
[`rules/api.md`](../../../rules/api.md), [`rules/database.md`](../../../rules/database.md),
[`rules/csharp.md`](../../../rules/csharp.md), [`rules/testing.md`](../../../rules/testing.md).
