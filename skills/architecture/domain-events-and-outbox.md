# Skill: Domain Events and the Outbox Pattern

## Purpose
Raise events from within domain behavior (`order.Confirm()` raises `OrderConfirmed`) and
publish them reliably, without losing an event or publishing one for a change that didn't
actually commit.

## When to Use
An aggregate's behavior should trigger a side effect elsewhere (another aggregate, another
module, another service) without the aggregate itself knowing about that side effect.

## Prerequisites
[`ddd`](../architecture/ddd.md) for domain events raised from within an aggregate;
[`event-driven-architecture`](../architecture/event-driven-architecture.md) for the broader
publish/consume model.

## Inputs Required
The aggregate behavior that should raise an event, and whether the consumer is in-process
(same deployable) or needs to cross a service boundary (needs the outbox + real broker).

## Engineering Principles
- A domain event is raised by the aggregate as a record of something that happened during
  its own operation (`order.Confirm()` internally adds an `OrderConfirmed` domain event to a
  pending list) — the aggregate doesn't publish it itself; it just records that it happened.
- Domain events are dispatched **after** the transaction that persisted the state change
  commits successfully — dispatching before commit risks reacting to a change that then gets
  rolled back.
- For events that must reach another service (crossing a database transaction boundary), use
  the **outbox pattern**: write the event to an "outbox" table in the *same* database
  transaction as the state change, then a separate background process reads the outbox and
  publishes to the real message broker, marking each event as sent. This guarantees the
  event is never lost even if the broker is temporarily unavailable, without needing a
  distributed transaction between the database and the broker.
- In-process domain events (same deployable, no broker) can be simpler: collect them on the
  aggregate, dispatch to in-process handlers via a mediator after `SaveChangesAsync`
  succeeds — no outbox needed since there's one transactional boundary.

## Step-by-Step Workflow
1. Aggregate behavior adds a domain event to its own pending-events list (not published yet).
2. Application's command handler calls `SaveChangesAsync` (or equivalent).
3. **In-process only:** after save succeeds, dispatch pending domain events to in-process
   handlers, then clear them from the aggregate.
4. **Crossing a service boundary:** write pending events into an outbox table in the *same*
   transaction as the state change; a separate outbox processor (background service) reads
   unsent outbox rows, publishes them to the broker, and marks them sent.
5. Consumers of published events handle them idempotently
   (see [`event-driven-architecture`](../architecture/event-driven-architecture.md)).

## Code Standards
```csharp
public abstract class Entity
{
    private readonly List<object> _domainEvents = [];
    public IReadOnlyList<object> DomainEvents => _domainEvents;
    protected void Raise(object domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
}

public sealed class Order : Entity
{
    public void Confirm()
    {
        if (Lines.Count == 0) throw new InvalidOperationException("Cannot confirm an empty order.");
        Status = OrderStatus.Confirmed;
        Raise(new OrderConfirmed(Id.Value, CustomerId, DateTimeOffset.UtcNow));
    }
}

// EF Core SaveChanges interceptor: writes domain events into the outbox in the same transaction
public sealed class OutboxInterceptor : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken ct = default)
    {
        var context = eventData.Context!;
        var entities = context.ChangeTracker.Entries<Entity>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity);

        foreach (var entity in entities)
        {
            foreach (var domainEvent in entity.DomainEvents)
            {
                context.Add(new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    Type = domainEvent.GetType().Name,
                    Payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
                    OccurredAtUtc = DateTimeOffset.UtcNow,
                });
            }
            entity.ClearDomainEvents();
        }
        return await base.SavingChangesAsync(eventData, result, ct);
    }
}
```

## Architecture Constraints
The outbox table lives in the same database/transaction scope as the aggregate it's
recording events for — that's the entire point of the pattern.

## Security Considerations
Outbox payloads are serialized event data that may cross a service boundary — don't include
secrets/full PII in the event payload (see
[`event-driven-architecture`](../architecture/event-driven-architecture.md)).

## Testing Requirements
Test that a domain event is raised by the correct aggregate behavior (unit test, no
infrastructure needed); integration-test that the outbox row is written in the same
transaction as the state change, and that the outbox processor correctly publishes and marks
rows sent.

## Common Mistakes
- Publishing a domain event directly from within application code before the transaction
  commits, then having it react to a change that gets rolled back.
- No outbox for cross-service events — a crash between "save state" and "publish to broker"
  silently loses the event.
- Forgetting to clear an aggregate's domain events after dispatch, causing them to fire again
  on a later, unrelated save.

## Anti-Patterns
- Building a full outbox implementation for purely in-process, single-deployable domain
  events where no distributed transaction risk exists — unnecessary complexity; a simple
  post-commit in-process dispatch suffices there.

## Validation Checklist
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Definition of Done
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Example
See [Code Standards](#code-standards).

## Related Skills
[`ddd`](../architecture/ddd.md), [`event-driven-architecture`](../architecture/event-driven-architecture.md),
[`efcore-fundamentals`](../data/efcore-fundamentals.md).
