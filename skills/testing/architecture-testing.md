# Skill: Architecture Testing

## Purpose
Enforce architectural rules (dependency direction, layer boundaries, module isolation)
automatically, so a violation is caught in CI instead of discovered during a later review.

## When to Use
A project using [`clean-architecture`](../architecture/clean-architecture.md) or
[`modular-monolith`](../architecture/modular-monolith.md) where the dependency rule/module
boundaries need to stay enforced as the codebase grows and more contributors touch it.

## Prerequisites
The architecture's rules are already defined (see
[`rules/architecture.md`](../../rules/architecture.md)) — this skill automates checking them,
it doesn't define them.

## Inputs Required
The specific boundaries to enforce (e.g. "Domain must not reference Infrastructure," "no
module references another module's Infrastructure project").

## Engineering Principles
- Use a library like `NetArchTest` (or `ArchUnitNET`) to express architecture rules as
  executable tests against the compiled assemblies/types — a rule expressed in code is
  enforced continuously, not just at review time.
- Test the *rule*, not the current state — a test asserting "these are the only two projects
  that exist" breaks on every legitimate new project; test the *relationship* ("Domain
  assemblies never depend on Infrastructure assemblies") so it stays valid as the codebase
  grows.
- Run these tests in the same CI gate as regular tests — an architecture violation should
  block merge the same way a failing unit test does.

## Step-by-Step Workflow
1. Identify the boundary rule to enforce.
2. Express it as a `NetArchTest` assertion against the relevant assemblies/namespaces.
3. Run it against the current codebase — if it fails immediately, that's either an existing
   violation to fix or a rule stated too strictly; resolve before relying on it.
4. Add it to the standard test suite so it runs in CI on every PR.

## Code Standards
```csharp
public class ArchitectureTests
{
    [Fact]
    public void Domain_Should_Not_Depend_On_Infrastructure()
    {
        var result = Types.InAssembly(typeof(Order).Assembly)
            .Should()
            .NotHaveDependencyOn("Orders.Infrastructure")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(FailureMessage(result));
    }

    [Fact]
    public void Application_Should_Not_Depend_On_EFCore()
    {
        var result = Types.InAssembly(typeof(CreateOrderHandler).Assembly)
            .Should()
            .NotHaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(FailureMessage(result));
    }

    [Fact]
    public void Modules_Should_Not_Reference_Each_Others_Infrastructure()
    {
        var result = Types.InAssembly(typeof(OrdersModule).Assembly)
            .That().ResideInNamespace("Orders")
            .Should().NotHaveDependencyOn("Inventory.Infrastructure")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(FailureMessage(result));
    }

    private static string FailureMessage(TestResult result) =>
        string.Join(", ", result.FailingTypeNames ?? []);
}
```

## Architecture Constraints
These tests *are* the architecture-constraint enforcement mechanism — they live in a test
project with visibility into the assemblies being checked.

## Security Considerations
None directly, though enforcing that Application doesn't leak security-relevant framework
dependencies (e.g. a specific auth library) into Domain follows the same principle.

## Testing Requirements
This skill produces tests; it doesn't itself need additional test coverage beyond verifying
the rule fails when deliberately violated (useful once, to confirm the assertion actually
catches what it's meant to).

## Common Mistakes
- Writing an architecture test so loosely that it never fails even for a real violation
  (e.g. checking a namespace string that doesn't match the actual code).
- Not running architecture tests in CI, so violations are only caught in manual review — if
  at all.

## Anti-Patterns
- Architecture tests asserting implementation details (exact file counts, exact class names)
  instead of relationships — brittle and unrelated to the actual architectural concern.

## Validation Checklist
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Definition of Done
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Example
See [Code Standards](#code-standards).

## Related Skills
[`clean-architecture`](../architecture/clean-architecture.md),
[`modular-monolith`](../architecture/modular-monolith.md), [`unit-testing`](../testing/unit-testing.md).
