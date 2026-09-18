# Skill: Modern C#

## Purpose
Apply current, idiomatic C# (targeting C# 12 / .NET 8+) so new code looks like it belongs
next to well-maintained modern code, not like it was written against C# 6.

## When to Use
Any time you're writing or reviewing C# — this is the baseline every other skill assumes.

## Prerequisites
.NET 8+ SDK, `<Nullable>enable</Nullable>` in the project (verify — see
[`nullable-reference-types`](../csharp/nullable-reference-types.md); if the project hasn't
enabled it, don't unilaterally flip it repo-wide, raise it as a separate change).

## Inputs Required
The file(s) being written/changed, and the project's existing style (file-scoped namespaces?
global usings? primary constructors already in use elsewhere?) — match it.

## Engineering Principles
- Prefer explicit domain types over primitive obsession (`OrderId` over a bare `int`/`Guid`
  passed around positionally — see [`ddd`](../architecture/ddd.md) for the value-object
  pattern).
- Immutability by default for data-carrying types: `record`/`record struct`, `init` setters.
- Nullable reference types on, and taken seriously — a `string?` return means callers must
  handle null, not get suppressed with `!`.
- Pattern matching over chained `if`/`else` when branching on a type or shape.
- Prefer async APIs for I/O; never block on them (see [`async-patterns`](../csharp/async-patterns.md)).

## Step-by-Step Workflow
1. Check the project's existing conventions (file-scoped namespace? global usings file?
   primary constructors?) and match them — don't introduce a second style.
2. For a new DTO/value object: use a `record` (or `record struct` for small, frequently
   allocated value types) unless the project has an established class-based DTO convention.
3. For a new entity or service: use a primary constructor when it removes real boilerplate,
   not reflexively.
4. Enable nullable annotations to mean something — don't suppress warnings with `!` to make
   the compiler quiet; fix the actual nullability.
5. Use pattern matching (`switch` expressions, `is` patterns) for type/shape-based branching.

## Code Standards
```csharp
public sealed record OrderId(Guid Value)
{
    public static OrderId New() => new(Guid.NewGuid());
}

public sealed record CreateOrderRequest(
    Guid CustomerId,
    IReadOnlyList<OrderLineRequest> Lines);

public sealed record OrderLineRequest(string Sku, int Quantity);

public sealed class Order
{
    public OrderId Id { get; }
    public Guid CustomerId { get; }
    private readonly List<OrderLine> _lines = [];
    public IReadOnlyList<OrderLine> Lines => _lines;

    public Order(OrderId id, Guid customerId)
    {
        Id = id;
        CustomerId = customerId;
    }

    public void AddLine(string sku, int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive.");

        _lines.Add(new OrderLine(sku, quantity));
    }
}

public sealed record OrderLine(string Sku, int Quantity);

// Pattern matching over a discriminated result
public string Describe(OrderStatus status) => status switch
{
    OrderStatus.Pending => "Awaiting confirmation",
    OrderStatus.Confirmed => "Confirmed",
    OrderStatus.Cancelled => "Cancelled",
    _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
};
```

## Architecture Constraints
Language-feature choices don't override architecture rules — a `record` used as an EF Core
entity still has to follow the project's persistence conventions
(see [`efcore-fundamentals`](../data/efcore-fundamentals.md)).

## Security Considerations
Records' auto-generated `ToString()` prints all properties — don't put secrets, tokens, or
PII in a record that gets logged via default `ToString()`/serialization without reviewing
what that exposes.

## Testing Requirements
No dedicated tests for language-feature choice itself; the behavior it implements is what
gets tested (see [`unit-testing`](../testing/unit-testing.md)).

## Common Mistakes
- Using `!` (null-forgiving operator) to silence a real nullability warning instead of
  handling the null case.
- Mutable `record` types via `set` instead of `init`, defeating the point of using a record.
- Primitive obsession: passing `Guid customerId, Guid orderId` positionally instead of
  distinct types, inviting argument-swap bugs.
- Using `async void` outside event handlers.

## Anti-Patterns
- Mixing file-scoped and block-scoped namespaces in the same project without reason.
- Overusing primary constructors on classes with complex construction logic or validation —
  a normal constructor body reads better once there's real work to do.

## Validation Checklist
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md). Skill-specific:
- [ ] No nullability warnings suppressed with `!` without a documented reason.
- [ ] New DTOs/value objects are immutable (`record`/`init`).
- [ ] Matches the file's/project's existing style conventions.

## Definition of Done
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Example
See [Code Standards](#code-standards).

## Related Skills
[`nullable-reference-types`](../csharp/nullable-reference-types.md),
[`async-patterns`](../csharp/async-patterns.md), [`ddd`](../architecture/ddd.md),
[`result-pattern`](../architecture/result-pattern.md).
