# Skill: Unit Testing

## Purpose
Test business logic in isolation, fast and deterministically, so regressions in domain rules
are caught without a database or network call.

## When to Use
Any new or changed business rule, branching logic, or calculation — the default first line
of test coverage for `dotnet-developer` and `test-engineer`.

## Prerequisites
Know the project's test framework (xUnit is the default assumption in this toolkit; adapt if
the project uses NUnit) and assertion library (FluentAssertions is common; plain `Assert` is
fine if that's the existing convention).

## Inputs Required
The unit of behavior under test (a method, a domain rule, a handler) and its dependencies
(to decide what needs a test double vs. what can be real).

## Engineering Principles
- Arrange-Act-Assert structure, one logical behavior per test.
- Test names describe behavior and expected outcome:
  `MethodOrBehavior_Scenario_ExpectedResult` or a sentence-style name — match the project's
  existing convention.
- Test the public contract (inputs → outputs/exceptions/state changes), not private
  implementation details. If you need reflection to test something, that's a sign the design
  needs a seam, not that the test needs reflection.
- **Mock the boundary, not the world.** Mock/stub interfaces your code depends on
  (`IOrderRepository`, `IEmailSender`) — don't mock value types, DTOs, or the class under
  test itself.
- Mocking becomes harmful when: the mock's behavior has to encode business logic to be
  useful (sign the dependency should be real/in-memory instead), when it hides a real
  integration bug (e.g. mocking EF Core's query translation hides SQL translation errors —
  use a real/in-memory provider or an integration test instead), or when tests become so
  wired to mock setup that they break on any refactor even when behavior is unchanged.
- Each test is independent — no shared mutable state between tests, no ordering dependency.

## Step-by-Step Workflow
1. Identify the behavior to test and its expected outcomes, including edge cases (empty
   input, boundary values, null where allowed, exception-triggering input).
2. Arrange: construct the unit under test with real collaborators where cheap and correct,
   test doubles where a collaborator is slow, external, or non-deterministic.
3. Act: invoke the behavior.
4. Assert: check the outcome — return value, thrown exception, or observable state change.
5. Add a negative/failure-path test for every success-path test where failure is a
   meaningful outcome (validation error, business rule violation).

## Code Standards
```csharp
public class OrderTests
{
    [Fact]
    public void AddLine_WithPositiveQuantity_AddsLineToOrder()
    {
        var order = Order.Create(customerId: Guid.NewGuid());

        order.AddLine(sku: "SKU-1", quantity: 2);

        order.Lines.Should().ContainSingle(l => l.Sku == "SKU-1" && l.Quantity == 2);
    }

    [Fact]
    public void AddLine_WithZeroQuantity_ThrowsArgumentOutOfRangeException()
    {
        var order = Order.Create(customerId: Guid.NewGuid());

        var act = () => order.AddLine(sku: "SKU-1", quantity: 0);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}

public class CreateOrderHandlerTests
{
    [Fact]
    public async Task HandleAsync_ValidCommand_PersistsOrderAndReturnsId()
    {
        var repository = Substitute.For<IOrderRepository>();
        var handler = new CreateOrderHandler(repository);
        var command = new CreateOrderCommand(Guid.NewGuid(), [new OrderLineRequest("SKU-1", 1)]);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await repository.Received(1).AddAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
    }
}
```

## Architecture Constraints
Unit tests for Domain/Application code (see
[`clean-architecture`](../architecture/clean-architecture.md)) should not require a database,
HTTP server, or file system — if they do, they're integration tests
(see [`integration-testing`](../testing/integration-testing.md)) and belong in that project
instead.

## Security Considerations
Test data must not contain real credentials, PII, or production data — use synthetic data
(see [`test-data-management`](../testing/test-data-management.md)).

## Testing Requirements
This skill *is* the testing requirement for business logic; pair with
[`integration-testing`](../testing/integration-testing.md) for anything crossing a real
boundary.

## Common Mistakes
- Asserting on mock call counts as the primary assertion instead of the actual outcome
  (`repository.Received(1).AddAsync(...)` alone, with no assertion on the returned result).
- One test covering multiple unrelated behaviors, making failures hard to diagnose.
- Testing a getter/setter or a framework-generated method with no logic in it.

## Anti-Patterns
- Mocking `DateTime.Now`/`DateTime.UtcNow` calls scattered through code instead of injecting
  `TimeProvider` — untestable and inconsistent under load.
- A "unit test" that spins up a real `WebApplicationFactory` — that's an integration test
  wearing a unit test's name; the distinction matters for speed and what a failure tells you.

## Validation Checklist
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md). Skill-specific:
- [ ] Every new business rule has at least one success and one failure-path test.
- [ ] No test depends on execution order or shared mutable state.
- [ ] Mocks are used only for actual boundaries (external systems, I/O), not for the code
      under test's own collaborators that are cheap to construct for real.

## Definition of Done
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Example
See [Code Standards](#code-standards).

## Related Skills
[`integration-testing`](../testing/integration-testing.md),
[`test-data-management`](../testing/test-data-management.md),
[`architecture-testing`](../testing/architecture-testing.md).
