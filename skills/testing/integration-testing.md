# Skill: Integration Testing

## Purpose
Verify behavior across real boundaries — HTTP pipeline, database, serialization,
authentication — that unit tests with fakes/mocks can't validate.

## When to Use
Any change to an API endpoint's full request/response behavior, a database query whose
correctness depends on real SQL translation, or an authentication/authorization boundary.
Required (per `config.yaml rules.require_integration_tests_for`) for database and API
changes at minimum.

## Prerequisites
`Microsoft.AspNetCore.Mvc.Testing` for API-level tests. A real or containerized database
(Testcontainers is the default recommendation — avoids relying on a shared dev database
and matches production engine behavior, unlike EF Core's in-memory provider).

## Inputs Required
The API/feature under test, and whether the project already has a `CustomWebApplicationFactory`
or equivalent test harness to build on rather than duplicate.

## Engineering Principles
- Use `WebApplicationFactory<TEntryPoint>` to boot the real DI container and HTTP pipeline
  in-process — this exercises the actual composition root, catching registration bugs unit
  tests can't see.
- Override only what must be overridden for test isolation (database connection string,
  external service clients) — don't mock so much that the test stops exercising the real
  pipeline.
- Use Testcontainers (or an equivalent ephemeral real database) for anything whose
  correctness depends on actual SQL translation — EF Core's in-memory provider does not
  validate real query translation, constraints, or provider-specific behavior, and passing
  against it is not evidence the real query works.
- Each test gets a clean, isolated data state — via a fresh container per test class/run, a
  transaction rolled back per test, or explicit cleanup — never relying on test execution
  order for correctness.

## Step-by-Step Workflow
1. Identify the boundary being tested (API endpoint, repository against a real database,
   authentication flow).
2. Build or reuse a `WebApplicationFactory`-based fixture that overrides only the database
   connection (pointing at a test container) and any external service clients (pointing at
   fakes/stubs for genuinely external systems).
3. Seed the minimal data needed for the test.
4. Exercise the real HTTP endpoint (or repository) via the test client.
5. Assert on the actual HTTP response (status code, body shape) or persisted state.
6. Clean up/isolate state so the next test isn't affected.

## Code Standards
```csharp
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _db = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    public async Task InitializeAsync()
    {
        await _db.StartAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddDbContext<AppDbContext>(o => o.UseNpgsql(_db.GetConnectionString()));
        });
    }

    public new async Task DisposeAsync() => await _db.DisposeAsync();
}

public class CreateOrderEndpointTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Post_ValidRequest_Returns201WithLocation()
    {
        var client = factory.CreateClient();
        var request = new CreateOrderRequest(Guid.NewGuid(), [new OrderLineRequest("SKU-1", 1)]);

        var response = await client.PostAsJsonAsync("/orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task Post_MissingAuth_Returns401()
    {
        var client = factory.CreateClient();
        var request = new CreateOrderRequest(Guid.NewGuid(), []);

        var response = await client.PostAsJsonAsync("/orders", request); // no auth header attached

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
```

## Architecture Constraints
Integration tests live in a separate test project from unit tests (different execution
speed/CI stage) unless the project's existing convention already colocates them with a clear
naming/trait split (`[Trait("Category", "Integration")]`).

## Security Considerations
Never point integration tests at a real production or shared staging database. Test
authentication should use a test-only token issuer or a fake `AuthenticationHandler`, never
real production credentials.

## Testing Requirements
This skill *is* the testing requirement for boundary-crossing behavior; combine with
[`unit-testing`](../testing/unit-testing.md) for the business-rule coverage underneath.

## Common Mistakes
- Using EF Core's in-memory provider as if it proves a query is correct against the real
  database engine — it doesn't validate SQL translation, and some LINQ that works against it
  fails against the real provider.
- Sharing one container/database instance across parallel test runs without isolation,
  causing flaky failures from cross-test data interference.
- Testing only the success path at the integration level and leaving auth/validation
  failures to (nonexistent) unit tests that can't actually exercise the pipeline.

## Anti-Patterns
- An "integration test" that mocks the database, defeating the entire purpose of the test
  tier.
- Extremely slow full end-to-end tests (real browser, real deployed environment) used in
  place of a fast in-process `WebApplicationFactory` test for coverage that doesn't need it.

## Validation Checklist
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md). Skill-specific:
- [ ] Test exercises the real DI container/HTTP pipeline via `WebApplicationFactory`.
- [ ] Database-dependent tests run against a real/containerized engine, not only an
      in-memory provider.
- [ ] Test data is isolated per test/run.

## Definition of Done
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Example
See [Code Standards](#code-standards).

## Related Skills
[`unit-testing`](../testing/unit-testing.md), [`rest-api-design`](../api/rest-api-design.md),
[`efcore-fundamentals`](../data/efcore-fundamentals.md),
[`test-data-management`](../testing/test-data-management.md).
