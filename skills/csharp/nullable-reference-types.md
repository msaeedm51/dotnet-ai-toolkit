# Skill: Nullable Reference Types

## Purpose
Use nullable reference types so the compiler catches null-reference bugs at compile time
instead of runtime.

## When to Use
Any C# code in a project with `<Nullable>enable</Nullable>` (the default assumption for new
.NET 8+ projects in this toolkit).

## Prerequisites
Confirm the project has nullable annotations enabled — if it doesn't and enabling it is in
scope, that's a separate, deliberate change (it surfaces many warnings across the codebase),
not something to flip incidentally while doing unrelated work.

## Inputs Required
The type/method signature being written or reviewed.

## Engineering Principles
- A `string` means "never null here"; a `string?` means "caller/reader must handle null."
  The type signature is the contract — don't lie in either direction.
- Handle nulls explicitly: pattern matching (`is null`/`is not null`), null-conditional
  (`?.`), null-coalescing (`??`), or an early return/guard clause.
- The null-forgiving operator (`!`) tells the compiler "trust me," not "make the warning go
  away" — every use should be justified (e.g. "this is set by the framework before this
  method can run") and ideally commented when non-obvious.
- Constructors should leave every non-nullable member definitively assigned; use `required`
  members or constructor parameters, not a nullable field that's "always set right after
  construction."

## Step-by-Step Workflow
1. Write the signature reflecting reality: can this parameter/return value actually be null?
2. Handle every nullable value at its use site — don't propagate a possibly-null value
   several calls deep before finally checking it.
3. If a warning appears that seems wrong, verify it isn't actually catching a real gap
   before suppressing it.
4. Use `required` for members that must be set at construction instead of a nullable
   placeholder with a runtime null-check.

## Code Standards
```csharp
public sealed class OrderService(IOrderRepository repository)
{
    // Explicit: this can genuinely return null (not found)
    public async Task<Order?> FindAsync(Guid id, CancellationToken ct) =>
        await repository.GetByIdAsync(id, ct);

    // Explicit: this always returns a value or throws
    public async Task<Order> GetAsync(Guid id, CancellationToken ct) =>
        await repository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Order {id} not found.");
}

public sealed class CreateOrderRequest
{
    public required Guid CustomerId { get; init; } // must be set, never null
    public string? Notes { get; init; } // genuinely optional
}
```

## Architecture Constraints
None beyond consistent application across all layers — a project that enables nullable
reference types in Domain but not Infrastructure loses most of the benefit at the seam.

## Security Considerations
A missed null check on user-derived identity/claims data can become an authorization bypass
(e.g. treating a null tenant id as "no restriction" instead of "reject the request") —
handle nulls on security-relevant data explicitly and restrictively.

## Testing Requirements
Null-handling branches (the "not found" / "not provided" path) need their own test, not just
the non-null happy path.

## Common Mistakes
- Suppressing a nullability warning with `!` to silence the compiler rather than fixing the
  actual gap.
- A nullable field that's "always set by the time it's used" — if that's true, it shouldn't
  be nullable; use `required` or constructor injection instead.

## Anti-Patterns
- `#nullable disable` at the top of a file to bulk-suppress warnings during a deadline crunch
  — defers the problem rather than solving it, and tends to become permanent.

## Validation Checklist
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Definition of Done
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Example
See [Code Standards](#code-standards).

## Related Skills
[`modern-csharp`](../csharp/modern-csharp.md), [`async-patterns`](../csharp/async-patterns.md).
