# Performance Rules

See `skills/performance/*` (profiling-and-diagnostics, caching-strategy, scalability) for
reasoning and examples.

- Profile or measure before optimizing; never guess which line is slow.
- State a cache's invalidation strategy before adding it — an unmanaged cache is a
  correctness bug waiting to happen, not a free performance win.
- Avoid N+1 query patterns; verify query count or execution plan before/after a change that
  touches a loop over data access.
- Avoid unnecessary allocations only in code paths with evidence they're hot; do not trade
  readability for an unmeasured micro-optimization on a cold path.
- Use cancellation tokens and sane timeouts on any I/O call that can hang.
- Do not trade correctness or consistency for speed (relaxed isolation levels, skipped
  validation) without explicit, documented confirmation from whoever owns that tradeoff.
- Report a root cause as confirmed (from evidence) or hypothesis (not yet verified) — never
  present a guess as a diagnosed cause.
