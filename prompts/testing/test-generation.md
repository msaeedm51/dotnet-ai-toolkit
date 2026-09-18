# Prompt: Test Generation

## Purpose
Generate real, behavior-verifying tests for existing or new code, following
[`skills/testing/unit-testing.md`](../../skills/testing/unit-testing.md) and
[`skills/testing/integration-testing.md`](../../skills/testing/integration-testing.md).

## When to Use
A change lacks adequate test coverage, or new tests are needed for a feature just
implemented.

## Loads
Agent: [`test-engineer`](../../agents/test-engineer.md). Skills:
[`unit-testing`](../../skills/testing/unit-testing.md),
[`integration-testing`](../../skills/testing/integration-testing.md),
[`test-data-management`](../../skills/testing/test-data-management.md).

## Parameters
- `{{TARGET}}` — the code/behavior needing tests.
- `{{KNOWN_EDGE_CASES}}` — any edge cases already known to matter (optional).

## Prompt Template
```
Load the .NET engineering toolkit's test-engineer agent and write tests for:

{{TARGET}}

Known edge cases to cover: {{KNOWN_EDGE_CASES}}

Requirements:
1. Match this project's existing test framework and conventions -- don't introduce a
   second testing library.
2. Write unit tests for business logic/branching, including failure paths and
   authorization boundaries where applicable -- not just the happy path.
3. Write integration tests for anything crossing a real boundary (database, HTTP,
   external service) -- do not rely on an in-memory provider alone to prove database
   query correctness.
4. Use test data builders for non-trivial entities/DTOs rather than repeated inline
   construction.
5. Decide deliberately where mocking is appropriate (external boundaries) vs. harmful
   (mocking something whose real behavior the test should actually verify).
6. State explicitly what remains untested and why, if anything.

Run the tests if you have execution access; otherwise tell me exactly what to run.
```

## Expected Output
A set of tests covering success, failure, and boundary cases, following project
conventions, with an explicit statement of any known coverage gap.

## Related
[`prompts/coding/refactoring.md`](../coding/refactoring.md) (characterization tests before
refactoring).
