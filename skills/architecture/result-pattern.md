# Skill: Result Pattern (Railway-Oriented Programming)

## Purpose
Represent expected, anticipated failures (validation errors, business rule violations, "not
found") as values instead of exceptions, reserving exceptions for genuinely exceptional/
unanticipated conditions.

## When to Use
Any Application-layer operation (command/query handler) that can fail in a way the caller is
expected to handle explicitly — most command handlers.

## Prerequisites
[`modern-csharp`](../csharp/modern-csharp.md).

## Inputs Required
The operation and its anticipated failure modes (validation, not-found, conflict, business
rule violation).

## Engineering Principles
- Exceptions signal the unexpected (a bug, an infrastructure failure); `Result<T>` signals an
  anticipated outcome that isn't success — a caller checking `result.IsSuccess` shouldn't
  need a `try`/`catch` for normal control flow.
- A `Result` carries enough information for the caller to act correctly — at minimum success/
  failure, an error message, and ideally an error *type* (validation/not-found/conflict) so
  the API layer can map it to the correct HTTP status
  (see [`model-validation-problemdetails`](../dotnet/model-validation-problemdetails.md)).
- Don't use `Result<T>` for conditions that genuinely are bugs/infrastructure failures (a
  database connection failure) — let those throw; catching and wrapping every possible
  exception into a `Result` defeats fail-fast behavior for real defects.
- Keep the `Result` type in Domain/Application (no framework dependency) so it can be used
  consistently across layers without coupling Domain to a specific library.

## Step-by-Step Workflow
1. Decide whether a given failure is anticipated (encode as `Result`) or exceptional (throw).
2. Command/query handlers return `Result<T>`/`Result` instead of throwing for anticipated
   failures.
3. The API layer maps `Result` failures to the correct HTTP status/`ProblemDetails`
   (see [`model-validation-problemdetails`](../dotnet/model-validation-problemdetails.md)).
4. Don't let a `Result`-returning method also throw for the same class of anticipated
   failure — pick one per failure type and be consistent.

## Code Standards
```csharp
public enum ErrorType { None, Validation, NotFound, Conflict, Unexpected }

public sealed class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public ErrorType ErrorType { get; }

    protected Result(bool isSuccess, string? error, ErrorType errorType)
    {
        IsSuccess = isSuccess; Error = error; ErrorType = errorType;
    }

    public static Result Success() => new(true, null, ErrorType.None);
    public static Result Failure(string error, ErrorType type) => new(false, error, type);
}

public sealed class Result<T> : Result
{
    public T? Value { get; }
    private Result(T value) : base(true, null, ErrorType.None) => Value = value;
    private Result(string error, ErrorType type) : base(false, error, type) { }

    public static Result<T> Success(T value) => new(value);
    public static new Result<T> Failure(string error, ErrorType type) => new(error, type);
}

public async Task<Result<Guid>> HandleAsync(CreateOrderCommand command, CancellationToken ct)
{
    if (command.Lines.Count == 0)
        return Result<Guid>.Failure("An order must have at least one line.", ErrorType.Validation);

    var customer = await customers.GetByIdAsync(command.CustomerId, ct);
    if (customer is null)
        return Result<Guid>.Failure("Customer not found.", ErrorType.NotFound);

    var order = Order.Create(command.CustomerId);
    // ...
    await repository.AddAsync(order, ct);
    return Result<Guid>.Success(order.Id.Value);
}
```

## Architecture Constraints
`Result<T>` is a Domain/Application-layer type with no framework dependency — it doesn't
know about HTTP status codes; that mapping happens at the API boundary.

## Security Considerations
`Result.Error` messages returned to callers should be safe for external consumption — don't
leak internal details (SQL error text, stack traces) into a `Result` failure message that
flows straight to an API response.

## Testing Requirements
Test both the success and failure paths of every `Result`-returning method — a failure path
with no test is untested business logic.

## Common Mistakes
- Mixing exceptions and `Result` for the same class of anticipated failure inconsistently
  across the codebase.
- A `Result<T>` with no `ErrorType`/error classification, forcing the API layer to
  string-match error messages to decide the HTTP status.
- Wrapping literally every possible exception (including genuine bugs) into a `Result`
  failure — masks real defects as if they were expected outcomes.

## Anti-Patterns
- Using `Result<T>` purely to avoid exceptions philosophically, even for infrastructure
  failures that should legitimately propagate and be handled by top-level error handling
  middleware.

## Validation Checklist
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Definition of Done
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Example
See [Code Standards](#code-standards).

## Related Skills
[`cqrs`](../architecture/cqrs.md), [`clean-architecture`](../architecture/clean-architecture.md),
[`model-validation-problemdetails`](../dotnet/model-validation-problemdetails.md).
