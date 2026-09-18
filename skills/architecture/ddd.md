# Skill: Domain-Driven Design (Tactical Patterns)

## Purpose
Model business rules explicitly in code — entities with real invariants, value objects,
aggregates with clear consistency boundaries — instead of anemic data classes with logic
scattered in services.

## When to Use
A domain with real business rules and invariants worth protecting in code. **Not** for a
simple CRUD service with no meaningful business logic — DDD tactical patterns add ceremony
that only pays off when there's actual domain complexity to manage.

## Prerequisites
Confirm this is warranted (see the Architect agent's constraint against pattern-for-pattern's
sake) — usually paired with [`clean-architecture`](../architecture/clean-architecture.md).

## Inputs Required
The business rules/invariants that need protecting, and the natural transactional
consistency boundary around them.

## Engineering Principles
- **Entity**: has identity that persists across changes (`OrderId` identifies "this order"
  regardless of what its properties become); equality is by identity, not value.
- **Value object**: has no identity — defined entirely by its values; immutable; equality is
  by value (`record`/`record struct` fits naturally). Use for concepts like `Money`,
  `Address`, `DateRange` instead of primitive fields scattered across entities.
- **Aggregate**: a cluster of entities/value objects treated as one consistency boundary,
  with a single **aggregate root** as the only entry point external code interacts with —
  invariants spanning the aggregate are enforced by the root, never bypassed by reaching
  into internal entities directly.
- Aggregates should be as small as the actual consistency requirement allows — a large
  aggregate (e.g. "the entire Order including every historical event ever") causes
  unnecessary contention and large loads for small operations.
- Business rules live as methods on the entity/aggregate root (`order.AddLine(...)`,
  `order.Cancel()`), not as external service logic manipulating public setters — this is
  what distinguishes a rich domain model from an anemic one.

## Step-by-Step Workflow
1. Identify the invariant/business rule that needs protecting.
2. Identify the natural consistency boundary — what must change atomically together.
3. Model that boundary as an aggregate with a root entity; expose behavior methods, not
   public setters, for anything that must maintain an invariant.
4. Model concepts with no identity as value objects.
5. Persist the aggregate as a whole (see
   [`repository-specification`](../architecture/repository-specification.md)) — don't allow
   partial updates that could leave it in an invalid state.

## Code Standards
```csharp
public sealed record Money(decimal Amount, string Currency)
{
    public static Money Zero(string currency) => new(0, currency);

    public Money Add(Money other)
    {
        if (other.Currency != Currency)
            throw new InvalidOperationException("Cannot add different currencies.");
        return this with { Amount = Amount + other.Amount };
    }
}

public sealed class Order // aggregate root
{
    public OrderId Id { get; }
    private readonly List<OrderLine> _lines = [];
    public IReadOnlyList<OrderLine> Lines => _lines;
    public OrderStatus Status { get; private set; } = OrderStatus.Draft;

    public static Order Create(Guid customerId) => new(OrderId.New(), customerId);

    private Order(OrderId id, Guid customerId) { Id = id; CustomerId = customerId; }
    public Guid CustomerId { get; }

    public void AddLine(string sku, int quantity, Money unitPrice)
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException("Cannot modify a confirmed order.");
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));

        _lines.Add(new OrderLine(sku, quantity, unitPrice));
    }

    public void Confirm()
    {
        if (_lines.Count == 0)
            throw new InvalidOperationException("Cannot confirm an order with no lines.");
        Status = OrderStatus.Confirmed;
    }
}
```

## Architecture Constraints
Aggregate roots are the only types repositories load/save directly — internal entities are
reached only through the root (see [`clean-architecture`](../architecture/clean-architecture.md)).

## Security Considerations
Authorization decisions (who may call `order.Cancel()`) are made by Application before
invoking the domain method, not encoded as a permission check inside the entity itself,
which shouldn't know about "the current user."

## Testing Requirements
Aggregate/entity behavior is fully unit-testable with no infrastructure — see
[`unit-testing`](../testing/unit-testing.md); test every invariant's enforcement, including
the failure case.

## Common Mistakes
- Public setters on an entity that should only change through a behavior method — bypasses
  the invariant the method exists to enforce.
- An aggregate that's too large (spans unrelated consistency concerns), causing unnecessary
  contention.
- Value objects implemented as mutable classes instead of immutable records.

## Anti-Patterns
- "Anemic domain model": entities that are just property bags, with all logic in a separate
  "service" class that manipulates them externally — defeats the purpose of DDD tactical
  patterns; if the project isn't going to use behavior-rich entities, plain DTOs and
  services are more honest than a half-applied DDD veneer.

## Validation Checklist
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Definition of Done
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Example
See [Code Standards](#code-standards).

## Related Skills
[`clean-architecture`](../architecture/clean-architecture.md),
[`repository-specification`](../architecture/repository-specification.md),
[`domain-events-and-outbox`](../architecture/domain-events-and-outbox.md),
[`result-pattern`](../architecture/result-pattern.md).
