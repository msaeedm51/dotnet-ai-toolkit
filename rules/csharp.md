# C# Rules

See [`skills/csharp/modern-csharp.md`](../skills/csharp/modern-csharp.md) for the reasoning
and examples behind these.

- Enable nullable reference types; do not suppress a real nullability warning with `!`
  without a comment stating why it's safe.
- Prefer explicit domain types over primitive obsession (e.g. `OrderId`, not a bare `Guid`
  passed positionally alongside other `Guid`s).
- Avoid mutable static state shared across requests; no static fields holding per-request or
  per-user data.
- Prefer async APIs for I/O.
- Never block async code with `.Result` or `.Wait()`.
- Use `CancellationToken` for long-running and request-bound operations, and thread it
  through to every awaited call that accepts one.
- Avoid unnecessary allocations in proven hot paths (boxing, LINQ inside tight loops, string
  concatenation in loops — use `StringBuilder` or interpolation only once).
- Do not introduce an abstraction (interface, base class, generic wrapper) without a second
  concrete use case already in the codebase.
- Dispose `IDisposable`/`IAsyncDisposable` resources deterministically with `using`/
  `await using`.
- Use `record`/`record struct` for immutable data; `init` accessors over public setters.
- Use exceptions for exceptional conditions, not for expected/anticipated outcomes — use
  `Result<T>` or a `TryX` pattern for expected failures (see
  [`result-pattern`](../skills/architecture/result-pattern.md)).
- Never use `async void` outside a UI event handler.
