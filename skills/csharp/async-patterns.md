# Skill: Async Patterns

## Purpose
Use `async`/`await` correctly for I/O-bound work — no blocking, no deadlocks, correct
cancellation propagation.

## When to Use
Any I/O-bound operation: database calls, HTTP calls, file I/O, message broker interaction.

## Prerequisites
[`modern-csharp`](../csharp/modern-csharp.md).

## Inputs Required
The operation being made async, and whether it's on a request-bound path (needs
`CancellationToken` threaded from the request).

## Engineering Principles
- Prefer async APIs for I/O — never block on an async call with `.Result`/`.Wait()`/`.GetAwaiter().GetResult()`
  from otherwise-async-capable code; this risks thread-pool starvation and, in some contexts
  (notably classic ASP.NET, less so ASP.NET Core), deadlocks.
- Thread a `CancellationToken` through every awaited call that accepts one, sourced from the
  request (`HttpContext.RequestAborted`) or an explicit timeout — don't silently drop it.
- `async void` is only for top-level event handlers; everywhere else, return `Task`/`Task<T>`
  so callers can await and observe exceptions.
- Use `Task.WhenAll` for independent concurrent operations instead of sequentially awaiting
  each one when there's no data dependency between them.
- `ConfigureAwait(false)` matters primarily in library code without access to a
  synchronization context (classic .NET Framework UI/ASP.NET contexts); in modern ASP.NET
  Core there's no synchronization context to deadlock on, so it's not required for
  correctness there, but still reasonable in shared library code that might run in other
  hosts.

## Step-by-Step Workflow
1. Identify the I/O boundary (database, HTTP, file).
2. Use the async overload of the API being called.
3. Accept and propagate `CancellationToken` down to it.
4. For multiple independent async calls, batch with `Task.WhenAll` rather than awaiting
   sequentially.
5. Avoid wrapping synchronous, CPU-bound work in `Task.Run` on a request path just to "make
   it async" — that adds overhead without benefit; `Task.Run` is for offloading genuinely
   CPU-bound work off a constrained thread (rare in typical web request handling).

## Code Standards
```csharp
// Correct: propagates cancellation, never blocks
public async Task<OrderDto> GetOrderWithHistoryAsync(Guid orderId, CancellationToken ct)
{
    var orderTask = repository.GetByIdAsync(orderId, ct);
    var historyTask = historyRepository.GetForOrderAsync(orderId, ct);

    await Task.WhenAll(orderTask, historyTask);

    return new OrderDto(await orderTask, await historyTask);
}

// Wrong: blocks a thread waiting on async work, risks starvation
public OrderDto GetOrderWithHistory(Guid orderId)
{
    var order = repository.GetByIdAsync(orderId, CancellationToken.None).Result; // never do this
    return new OrderDto(order, []);
}
```

## Architecture Constraints
None beyond consistency — mixing sync-over-async and proper async in the same call chain
tends to reintroduce the exact problem async was adopted to avoid.

## Security Considerations
A missing `CancellationToken` on a long-running operation means a client that disconnects
doesn't free the server-side work — under load, this can be exploited/contribute to resource
exhaustion.

## Testing Requirements
Test cancellation behavior explicitly for operations where it matters (e.g. a long-running
query correctly stops when the token is cancelled) — see
[`unit-testing`](../testing/unit-testing.md).

## Common Mistakes
- `.Result`/`.Wait()` on an async call "just to make the signature simpler."
- `async void` methods outside event handlers, silently swallowing exceptions.
- Sequential `await`s for independent operations that could run concurrently via
  `Task.WhenAll`.

## Anti-Patterns
- Wrapping every method in `async`/`Task.Run` regardless of whether it does I/O — adds
  overhead and signals a misunderstanding of what async is for.

## Validation Checklist
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Definition of Done
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Example
See [Code Standards](#code-standards).

## Related Skills
[`modern-csharp`](../csharp/modern-csharp.md),
[`httpclient-resilience`](../dotnet/httpclient-resilience.md).
