# Skill: Dependency Injection as an Architecture Tool

## Purpose
Use dependency injection deliberately as a design tool — for inverting dependency direction
across architectural boundaries — not just as a wiring mechanism. Complements
[`dependency-injection`](../dotnet/dependency-injection.md), which covers the ASP.NET Core
container mechanics; this skill covers the architectural *why*.

## When to Use
Designing the interfaces between layers/modules (Application depending on an interface
Infrastructure implements, a module depending on another module's public interface).

## Prerequisites
[`clean-architecture`](../architecture/clean-architecture.md) or
[`modular-monolith`](../architecture/modular-monolith.md).

## Inputs Required
The architectural boundary being crossed and which side should own the interface.

## Engineering Principles
- The **Dependency Inversion Principle** applied at the architecture level: the interface is
  owned by the layer that *uses* it (Application defines `IOrderRepository`), and
  implemented by the layer that provides the capability (Infrastructure implements it) — this
  is what lets Infrastructure depend on Application instead of the reverse.
- Not every dependency needs an interface — introduce one specifically where it inverts a
  dependency across an architectural boundary, or where a genuine second implementation
  exists/is planned (a real one, not a hypothetical future one). An interface with exactly
  one implementation and no boundary-crossing purpose is often unnecessary indirection.
- Composition happens at the outermost layer (the host/composition root) — that's the one
  place allowed to know about every concrete implementation.
- Constructor injection makes dependencies explicit and testable; avoid hidden dependencies
  acquired via static state or ambient context.

## Step-by-Step Workflow
1. Identify the architectural boundary (which layer needs a capability the other layer
   provides).
2. Define the interface in the layer that needs the capability (typically Application).
3. Implement it in the layer that provides the capability (typically Infrastructure).
4. Wire the concrete implementation to the interface in the composition root.
5. Resist adding an interface for something with no real boundary-crossing purpose — a
   private helper class used only within Infrastructure doesn't need one.

## Code Standards
```csharp
// Application owns the interface -- this is the inversion
namespace Orders.Application;
public interface IEmailSender
{
    Task SendOrderConfirmationAsync(Guid orderId, string toEmail, CancellationToken ct);
}

// Infrastructure implements it, depending on Application, not the reverse
namespace Orders.Infrastructure;
public sealed class SmtpEmailSender(IOptions<SmtpOptions> options) : IEmailSender
{
    public Task SendOrderConfirmationAsync(Guid orderId, string toEmail, CancellationToken ct) =>
        Task.CompletedTask; // real SMTP call
}

// Composition root wires it
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
```

## Architecture Constraints
This *is* the mechanism that makes [`clean-architecture`](../architecture/clean-architecture.md)'s
dependency rule achievable in practice — without interface ownership living in the
inner/using layer, Application would have to reference Infrastructure directly.

## Security Considerations
None beyond standard DI lifetime rules (see
[`dependency-injection`](../dotnet/dependency-injection.md)).

## Testing Requirements
Every interface introduced for this purpose should have at least one fake/test-double
implementation used in Application-layer unit tests, proving the abstraction is actually
useful for testing, not just ceremony.

## Common Mistakes
- Defining the interface in Infrastructure instead of Application — doesn't invert anything;
  Application still ends up depending on Infrastructure's assembly to reference the
  interface.
- An interface with exactly one implementation, never faked in a test, and no plausible
  second implementation — pure ceremony.

## Anti-Patterns
- "Interface everything" as a blanket rule regardless of whether a boundary is actually being
  crossed — adds a file and a layer of indirection with no benefit for purely internal,
  single-implementation code.

## Validation Checklist
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Definition of Done
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Example
See [Code Standards](#code-standards).

## Related Skills
[`dependency-injection`](../dotnet/dependency-injection.md),
[`clean-architecture`](../architecture/clean-architecture.md),
[`repository-specification`](../architecture/repository-specification.md).
