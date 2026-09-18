# Testing Rules

See [`skills/testing/unit-testing.md`](../skills/testing/unit-testing.md) and
[`skills/testing/integration-testing.md`](../skills/testing/integration-testing.md) for
reasoning and examples.

- Every business rule has at least one test covering it.
- Test failure paths explicitly, not only the happy path.
- Test authorization boundaries explicitly (unauthorized and forbidden cases), not just
  successful access.
- Do not test implementation details (private method shape, internal call sequencing) when a
  behavioral assertion would do.
- Integration-test any behavior that crosses a real boundary (database, HTTP, file system,
  external service) — a unit test with mocks is not sufficient evidence that boundary works.
- Tests are independent: no shared mutable state between tests, no dependency on execution
  order.
- A bug fix ships with a regression test that reproduces the original failure and fails
  without the fix.
- Do not merge a change with reduced test coverage for the area it touches without a stated
  reason.
