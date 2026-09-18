# Skill: Caching Strategy

## Purpose
Choose the right caching layer and, critically, the right invalidation strategy — the
architectural decision behind the mechanics covered in
[`output-and-distributed-caching`](../dotnet/output-and-distributed-caching.md).

## When to Use
Deciding *whether* and *where* to cache, after
[`profiling-and-diagnostics`](../performance/profiling-and-diagnostics.md) has confirmed a
real bottleneck a cache would address.

## Prerequisites
Confirmed evidence of the bottleneck — this skill is a design decision, not a default.

## Inputs Required
What's being cached, its read/write ratio, how many callers share it, and how stale it's
acceptable to be.

## Engineering Principles
- **Cache what's expensive and read far more than it's written** — a good caching candidate
  has a high read/write ratio and a real cost to recompute/refetch.
- Three invalidation strategies, in order of preference for simplicity: **TTL-only**
  (simplest — accept up to N seconds/minutes of staleness, no explicit invalidation needed);
  **write-through/cache-aside with explicit invalidation** (delete or update the cache entry
  on the write path that changes the underlying data — needed when staleness isn't
  acceptable); **event-driven invalidation** (a change published as an event invalidates
  caches in other processes/instances — needed for distributed caches shared across multiple
  app instances where only one instance's write path fires).
- **State the invalidation strategy before implementing the cache** — an unmanaged cache
  becomes a correctness bug (see [`rules/performance.md`](../../rules/performance.md)).
- Cache locality: in-process (`IMemoryCache`) is fastest but not shared across instances —
  fine for single-instance deployments or data that's safe to be briefly inconsistent across
  instances; distributed (`IDistributedCache`/Redis) is required when multiple instances must
  see a consistent cached value.
- Cache stampede risk: many concurrent requests missing the cache simultaneously (e.g. right
  after expiry) can all hit the origin at once — mitigate with request coalescing (only one
  request recomputes, others wait) for expensive, high-traffic cache entries.

## Step-by-Step Workflow
1. Confirm the bottleneck with evidence (per
   [`profiling-and-diagnostics`](../performance/profiling-and-diagnostics.md)).
2. Choose locality: in-process vs. distributed, based on whether multiple instances need a
   consistent view.
3. Choose invalidation: TTL-only if staleness is acceptable; explicit invalidation if not;
   event-driven if the write path and read path are in different processes.
4. Implement (see [`output-and-distributed-caching`](../dotnet/output-and-distributed-caching.md)
   for the mechanics).
5. Verify: cache hit avoids recomputation, invalidation actually clears/updates the entry,
   and no cross-user/cross-tenant data leakage through a shared cache key.

## Code Standards
```csharp
// Cache-aside with explicit invalidation on write
public sealed class ProductCatalogService(IDistributedCache cache, IProductRepository repository)
{
    public async Task<ProductDto?> GetAsync(string sku, CancellationToken ct)
    {
        var key = $"product:{sku}";
        var cached = await cache.GetStringAsync(key, ct);
        if (cached is not null)
            return JsonSerializer.Deserialize<ProductDto>(cached);

        var product = await repository.GetBySkuAsync(sku, ct);
        if (product is null) return null;

        var dto = product.ToDto();
        await cache.SetStringAsync(key, JsonSerializer.Serialize(dto),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15) }, ct);
        return dto;
    }

    public async Task UpdatePriceAsync(string sku, decimal newPrice, CancellationToken ct)
    {
        await repository.UpdatePriceAsync(sku, newPrice, ct);
        await cache.RemoveAsync($"product:{sku}", ct); // explicit invalidation on write
    }
}
```

## Architecture Constraints
Caching is a cross-cutting Infrastructure concern — Domain logic has no awareness a cache
exists.

## Security Considerations
Never share a cache key across users/tenants for per-user/per-tenant data — include the
distinguishing dimension in the key.

## Testing Requirements
Test that a write correctly invalidates the corresponding cache entry — a cache-invalidation
bug is a silent correctness bug, not a crash, so it needs explicit test coverage, not just
"the cache works" in isolation.

## Common Mistakes
- Caching without a stated invalidation plan.
- Caching data that's written as often as it's read — provides little benefit while adding
  invalidation complexity.
- A cache key missing a dimension (user, tenant, locale) that actually varies the correct
  value.

## Anti-Patterns
- Caching "just in case it helps" without profiling evidence — adds complexity and
  stale-data risk for an unmeasured, possibly nonexistent, benefit.

## Validation Checklist
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md). Skill-specific:
- [ ] Bottleneck confirmed with evidence before adding the cache.
- [ ] Invalidation strategy stated and implemented.

## Definition of Done
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Example
See [Code Standards](#code-standards).

## Related Skills
[`output-and-distributed-caching`](../dotnet/output-and-distributed-caching.md),
[`profiling-and-diagnostics`](../performance/profiling-and-diagnostics.md).
