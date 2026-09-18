# Skill: HttpClient and Resilience

## Purpose
Make outbound HTTP calls correctly — via `IHttpClientFactory`, with timeouts, retries, and
circuit breaking appropriate to the dependency being called.

## When to Use
Any code calling an external HTTP API or another internal service over HTTP.

## Prerequisites
[`aspnetcore-fundamentals`](../dotnet/aspnetcore-fundamentals.md).

## Inputs Required
The external service being called, its expected latency/failure characteristics, and whether
the call is on a user-facing request path (needs a tight timeout) or background (can afford
more retry).

## Engineering Principles
- Always use `IHttpClientFactory` (`AddHttpClient<T>()`), never `new HttpClient()` per call —
  avoids socket exhaustion from improper `HttpClient` lifetime management.
- Apply resilience (retry, circuit breaker, timeout) via `Microsoft.Extensions.Http.Resilience`
  (built on Polly) — configured per named/typed client, not ad hoc in calling code.
- Retries must be bounded and use backoff (exponential, with jitter) — unbounded or
  fixed-interval retry against a struggling downstream service makes the outage worse.
- Only retry idempotent operations automatically; a non-idempotent `POST` needs an
  idempotency key (see [`rest-api-design`](../api/rest-api-design.md)) before it's safe to
  retry.
- Set an explicit timeout shorter than the caller's own deadline — a hung downstream call
  shouldn't hang the entire request indefinitely.

## Step-by-Step Workflow
1. Define a typed client (`IOrdersApiClient`/`OrdersApiClient : IOrdersApiClient`).
2. Register with `AddHttpClient<IOrdersApiClient, OrdersApiClient>()` and configure base
   address/default headers.
3. Attach a resilience pipeline (timeout, retry with backoff, circuit breaker) sized to the
   dependency's actual behavior — don't copy-paste generic defaults without considering
   whether this call is idempotent and how critical it is.
4. Propagate `CancellationToken` from the caller through to the HTTP call.

## Code Standards
```csharp
builder.Services.AddHttpClient<IOrdersApiClient, OrdersApiClient>(client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["OrdersApi:BaseUrl"]!);
        client.Timeout = TimeSpan.FromSeconds(10);
    })
    .AddResilienceHandler("orders-api", builder =>
    {
        builder.AddRetry(new HttpRetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,
        });
        builder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
        {
            FailureRatio = 0.5,
            SamplingDuration = TimeSpan.FromSeconds(30),
        });
        builder.AddTimeout(TimeSpan.FromSeconds(5));
    });

public sealed class OrdersApiClient(HttpClient client) : IOrdersApiClient
{
    public async Task<OrderDto?> GetOrderAsync(Guid id, CancellationToken ct) =>
        await client.GetFromJsonAsync<OrderDto>($"/orders/{id}", ct);
}
```

## Architecture Constraints
The typed client's interface lives in Application (as a dependency Infrastructure
implements); Application code depends on the interface, not `HttpClient` directly.

## Security Considerations
Never log full request/response bodies containing tokens/PII sent to or received from an
external API. Validate TLS certificate behavior isn't disabled for convenience
("temporarily" skipping cert validation is a common path to a real incident).

## Testing Requirements
Unit test calling code against a fake `IOrdersApiClient`; integration-test the typed client
itself against a mock HTTP server (e.g. `WireMock.Net` or a `DelegatingHandler` test double)
to verify request shaping and resilience behavior.

## Common Mistakes
- `new HttpClient()` per request — socket exhaustion under load.
- Retrying a non-idempotent `POST` without an idempotency key, causing duplicate side
  effects on transient failure.
- No timeout configured, letting a hung downstream call hang the caller indefinitely.

## Anti-Patterns
- Hand-rolled retry loops with `Thread.Sleep`/fixed delay scattered through calling code
  instead of a configured resilience pipeline.

## Validation Checklist
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Definition of Done
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Example
See [Code Standards](#code-standards).

## Related Skills
[`aspnetcore-fundamentals`](../dotnet/aspnetcore-fundamentals.md),
[`scalability`](../performance/scalability.md).
