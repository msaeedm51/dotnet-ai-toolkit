# Skill: Test Data Management

## Purpose
Build realistic, maintainable test data — via builders/object mothers — instead of
duplicated inline object construction that breaks on every entity change.

## When to Use
Writing tests that need non-trivial entity/DTO instances, especially where many tests need
similar-but-slightly-varied data.

## Prerequisites
[`unit-testing`](../testing/unit-testing.md)/[`integration-testing`](../testing/integration-testing.md).

## Inputs Required
The entity/DTO types tests need to construct, and what varies between test cases.

## Engineering Principles
- Use the **Test Data Builder** pattern for entities/DTOs with several fields — a fluent
  builder with sensible defaults lets each test specify only the fields it cares about,
  and shields tests from unrelated changes to the type's constructor/shape.
- Test data must be synthetic — never real production data, real customer PII, or real
  credentials, even in a "throwaway" test fixture (see
  [`rules/security.md`](../../rules/security.md)).
- Use a library like `Bogus` for generating realistic-looking but synthetic data (names,
  addresses, emails) when the specific values don't matter but they need to look real (e.g.
  for testing formatting/display logic).
- Seed data for integration tests should be minimal and specific to what that test needs —
  avoid a single giant shared seed dataset that many tests depend on implicitly, which makes
  tests fragile to each other's changes.

## Step-by-Step Workflow
1. Identify entities/DTOs that are constructed repeatedly across tests with minor variation.
2. Build a fluent builder with sensible defaults for every field.
3. Use it in tests, overriding only the fields relevant to that specific test's scenario.
4. For integration tests, seed only the minimal data that specific test needs, isolated from
   other tests (see [`integration-testing`](../testing/integration-testing.md)'s isolation
   guidance).

## Code Standards
```csharp
public sealed class OrderBuilder
{
    private Guid _customerId = Guid.NewGuid();
    private readonly List<(string Sku, int Quantity)> _lines = [("SKU-DEFAULT", 1)];

    public OrderBuilder WithCustomer(Guid customerId) { _customerId = customerId; return this; }
    public OrderBuilder WithLine(string sku, int quantity)
    {
        _lines.Clear();
        _lines.Add((sku, quantity));
        return this;
    }

    public Order Build()
    {
        var order = Order.Create(_customerId);
        foreach (var (sku, quantity) in _lines)
            order.AddLine(sku, quantity);
        return order;
    }
}

// Usage -- each test states only what it cares about
[Fact]
public void Confirm_WithNoLines_Throws()
{
    var order = new OrderBuilder().Build();
    // ... test uses default builder state, doesn't care about specific customer/line values
}

[Fact]
public void AddLine_ForSpecificSku_AppearsInOrder()
{
    var order = new OrderBuilder().WithLine("SKU-42", 3).Build();
    order.Lines.Should().ContainSingle(l => l.Sku == "SKU-42" && l.Quantity == 3);
}
```

## Architecture Constraints
Test builders live in the test project, not production code.

## Security Considerations
No real PII, credentials, or production data snapshots in test fixtures — use synthetic data
generation (`Bogus` or hand-written builders) exclusively.

## Testing Requirements
This skill supports test-writing directly — it doesn't itself require separate tests beyond
confirming the builder produces valid objects.

## Common Mistakes
- Copy-pasted inline object construction across dozens of tests, all breaking together when
  the entity's constructor changes.
- A shared "global" seed dataset for integration tests that many tests implicitly depend on,
  causing unrelated test failures when one test's seed data changes.

## Anti-Patterns
- An overly generic, deeply configurable builder that's harder to read than just constructing
  the object directly — keep builders focused on the variation tests actually need.

## Validation Checklist
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Definition of Done
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Example
See [Code Standards](#code-standards).

## Related Skills
[`unit-testing`](../testing/unit-testing.md), [`integration-testing`](../testing/integration-testing.md).
