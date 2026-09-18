# Skill: Output Caching and Distributed Caching

## Purpose
Cache correctly at the right layer — response-level (output caching) or data-level
(distributed cache) — with an explicit invalidation strategy.

## When to Use
A read-heavy endpoint/query with measured latency or database-load pressure, where the data
can tolerate being briefly stale.

## Prerequisites
Evidence this is actually a bottleneck (see
[`profiling-and-diagnostics`](../performance/profiling-and-diagnostics.md)) — caching added
without evidence is a common source of unnecessary complexity and stale-data bugs.

## Inputs Required
The data/endpoint being cached, its acceptable staleness window, and what invalidates it
(time-based expiry, or an explicit event like "this order changed").

## Engineering Principles
- **State the invalidation strategy before adding the cache** — a cache with no clear
  invalidation plan is a correctness bug waiting to happen, per
  [`rules/performance.md`](../../rules/performance.md).
- **Output caching** (`Microsoft.AspNetCore.OutputCaching`) caches full HTTP responses —
  appropriate for public, cacheable `GET` endpoints with no per-user variation (or with
  vary-by-user configured explicitly).
- **Distributed caching** (`IDistributedCache`, backed by Redis or similar) caches
  application data shared across instances — appropriate for expensive-to-compute or
  expensive-to-query data reused across requests/users.
- Prefer time-based expiry (short TTL) for data where brief staleness is acceptable and
  explicit invalidation would be complex; use explicit invalidation (cache-aside with
  delete-on-write) when staleness is not acceptable for that data.
- Never cache per-user sensitive data under a key that could be served to a different user
  (a classic output-caching bug when vary-by-user isn't configured).

## Step-by-Step Workflow
1. Confirm this is a measured bottleneck, not a speculative optimization.
2. Choose the layer: output caching for whole cacheable responses, distributed cache for
   reused computed/queried data.
3. Define the cache key precisely, including any dimensions that must vary it (user, tenant,
   query parameters).
4. Set a TTL matched to acceptable staleness, or wire explicit invalidation on the write path
   that changes the cached data.
5. Verify cache hit/miss behavior and that stale data doesn't leak across users/tenants.

## Code Standards
```csharp
// Output caching -- public, non-user-specific data
builder.Services.AddOutputCache(options =>
{
    options.AddPolicy("products", b => b.Expire(TimeSpan.FromMinutes(5)).Tag("products"));
});
app.MapGet("/products", GetProducts).CacheOutput("products");

// Invalidate on write
app.MapPost("/products", async (CreateProductRequest request, IOutputCacheStore cache, CancellationToken ct) =>
{
    // ... create the product ...
    await cache.EvictByTagAsync("products", ct);
    return Results.Created();
});

// Distributed cache-aside for expensive computed data
public sealed class PricingService(IDistributedCache cache, IPricingCalculator calculator)
{
    public async Task<decimal> GetPriceAsync(string sku, CancellationToken ct)
    {
        var key = $"price:{sku}";
        var cached = await cache.GetStringAsync(key, ct);
        if (cached is not null) return decimal.Parse(cached);

        var price = await calculator.CalculateAsync(sku, ct);
        await cache.SetStringAsync(key, price.ToString(), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
        }, ct);
        return price;
    }
}
```

## Architecture Constraints
Caching is an Infrastructure/cross-cutting concern — Domain logic doesn't know a cache
exists; Application code depends on an abstraction if it needs cache-aware behavior beyond
what output caching handles transparently.

## Security Considerations
Never cache authorization decisions or per-user data under a shared/global key — verify
vary-by-user/vary-by-tenant is correctly configured wherever output caching is applied to
anything beyond fully public data.

## Testing Requirements
Test that a cached response is actually served from cache (no re-computation) within the TTL,
and that invalidation actually clears it — not just that the cache "works" in isolation.

## Common Mistakes
- Caching a per-user response under a key that doesn't vary by user, leaking one user's data
  to another.
- Adding a cache with no measured need, then debugging "stale data" reports that are actually
  the cache working as designed but misunderstood.

## Anti-Patterns
- Caching everything "for performance" without evidence, multiplying the surface area for
  stale-data bugs.

## Validation Checklist
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md). Skill-specific:
- [ ] Invalidation strategy stated and implemented (TTL or explicit).
- [ ] Cache key includes every dimension that must vary it (user/tenant/params).

## Definition of Done
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Example
See [Code Standards](#code-standards).

## Related Skills
[`caching-strategy`](../performance/caching-strategy.md),
[`profiling-and-diagnostics`](../performance/profiling-and-diagnostics.md).
