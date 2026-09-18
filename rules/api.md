# API Rules

See [`skills/api/rest-api-design.md`](../skills/api/rest-api-design.md) for reasoning and
examples.

- Validate all input at the API boundary before it reaches business logic.
- Use HTTP methods and status codes consistently with their semantics; do not return `200`
  with an error payload.
- Never expose a domain entity or EF Core entity directly in a request or response — define
  and use DTOs at the boundary.
- Return a consistent, predictable error shape (`ProblemDetails` unless the project has an
  established alternative) across all endpoints.
- Enforce authorization at the boundary appropriate to the specific operation — a route
  group's default authorization policy does not excuse checking resource-level ownership
  where it applies.
- Flag and document any breaking change to an existing contract (renamed/removed field,
  changed status code, changed error shape); do not ship one silently.
- Paginate any collection endpoint that can return an unbounded result set.
- Rate-limit public and unauthenticated endpoints.
- Support an idempotency key on `POST` endpoints that trigger an external side effect
  (payment, message send) where retries are expected.
