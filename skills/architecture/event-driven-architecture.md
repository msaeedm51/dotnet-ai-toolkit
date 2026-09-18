# Skill: Event-Driven Architecture

## Purpose
Use asynchronous events to decouple producers from consumers, where a synchronous
request/response would create unnecessary coupling or block on work that doesn't need an
immediate response.

## When to Use
Cross-module or cross-service reactions to something that happened ("order confirmed →
reserve stock, send confirmation email") where the producer doesn't need to know who
consumes the event or wait for consumers to finish. **Not** a default for every interaction —
a synchronous call is simpler and appropriate when the caller genuinely needs an immediate
answer.

## Prerequisites
A message broker or in-process event mechanism appropriate to scope: in-process domain
events (see [`domain-events-and-outbox`](../architecture/domain-events-and-outbox.md)) for
within a single deployable; a real broker (Azure Service Bus, RabbitMQ, Kafka) for
cross-service.

## Inputs Required
The event being modeled (what happened, not what should happen next — that's the consumer's
decision) and who needs to react to it.

## Engineering Principles
- Events describe **facts that already happened** (`OrderConfirmed`, not `ConfirmOrder`) —
  past tense, immutable, and the producer doesn't know or care who's listening.
- Producers and consumers are decoupled — a producer publishing an event has no reference to
  any consumer; adding a new consumer never requires changing the producer.
- Design for **at-least-once delivery** — most brokers don't guarantee exactly-once, so
  consumers must be idempotent (processing the same event twice produces the same result as
  processing it once).
- Use the [outbox pattern](../architecture/domain-events-and-outbox.md) to publish events
  reliably in the same transaction as the state change that caused them — otherwise a crash
  between "save state" and "publish event" loses the event or causes inconsistency.
- Event schemas are a contract between services — version them deliberately; don't remove or
  repurpose a field a consumer might depend on without a compatibility plan.

## Step-by-Step Workflow
1. Identify the fact that occurred and its relevant data (keep the payload focused — an
   event carries what changed, not the entire aggregate state).
2. Define the event as an immutable type (past-tense name).
3. Publish it transactionally with the state change that caused it (outbox pattern) or, for
   pure in-process domain events, dispatch after the transaction commits successfully.
4. Consumers process idempotently — check whether they've already handled this event
   (by id) before applying its effect, if replay/duplicate delivery is possible.
5. Version the event schema deliberately if it needs to change in a way that could break an
   existing consumer.

## Code Standards
```csharp
public sealed record OrderConfirmed(Guid OrderId, Guid CustomerId, DateTimeOffset ConfirmedAtUtc);

public sealed class OrderConfirmedConsumer(IInventoryRepository inventory, IProcessedEventStore processed)
{
    public async Task HandleAsync(OrderConfirmed @event, CancellationToken ct)
    {
        if (await processed.HasBeenProcessedAsync(@event.OrderId, nameof(OrderConfirmed), ct))
            return; // idempotent: already handled this delivery

        await inventory.ReserveStockForOrderAsync(@event.OrderId, ct);
        await processed.MarkProcessedAsync(@event.OrderId, nameof(OrderConfirmed), ct);
    }
}
```

## Architecture Constraints
A producer never references a consumer's assembly/module; the coupling is entirely through
the event contract (and, for cross-service, the broker's topic/queue naming).

## Security Considerations
Events crossing a service/trust boundary may need to be authenticated/signed depending on
the broker's security model; never include secrets/full PII in an event payload that
multiple, possibly loosely-trusted, consumers will receive.

## Testing Requirements
Test the producer publishes the expected event on the expected trigger; test the consumer's
idempotency explicitly (processing the same event twice produces the same end state as once).

## Common Mistakes
- Naming an event as a command (`ConfirmOrder` instead of `OrderConfirmed`) — conflates "what
  happened" with "what should happen," reintroducing coupling.
- A consumer that isn't idempotent, causing duplicate side effects (double-charging, double-
  reserving stock) on redelivery.
- Publishing the event before the state change is committed, so a crash leaves consumers
  reacting to something that didn't actually happen.

## Anti-Patterns
- Using events for something that genuinely needs a synchronous answer, then building a
  request/reply pattern on top of the event bus to fake synchronicity — just use a direct
  call.

## Validation Checklist
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Definition of Done
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Example
See [Code Standards](#code-standards).

## Related Skills
[`domain-events-and-outbox`](../architecture/domain-events-and-outbox.md),
[`microservices`](../architecture/microservices.md),
[`modular-monolith`](../architecture/modular-monolith.md).
