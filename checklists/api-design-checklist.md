# API Design Checklist

Used by [`api-engineer`](../agents/api-engineer.md). Mechanisms and examples:
[`skills/api/rest-api-design.md`](../skills/api/rest-api-design.md).

- [ ] Resource modeled as a noun; HTTP method matches the operation's semantics.
- [ ] Status codes match outcome (`200`/`201`/`204` success variants; `400`/`401`/`403`/
      `404`/`409`/`422`/`429` failure variants as applicable) — no `200` with an error body.
- [ ] Request/response use DTOs; no domain or EF Core entity exposed directly.
- [ ] Input validated at the boundary before reaching business logic.
- [ ] Error responses use the project's consistent shape (`ProblemDetails` unless
      established otherwise).
- [ ] Authorization enforced per-operation, including resource-level ownership where
      applicable, not just per-route-group.
- [ ] Collection endpoints are paginated; filter/sort parameters validated against an
      allow-list.
- [ ] Idempotency key supported on `POST` endpoints with external side effects where
      retries are expected.
- [ ] Backward compatibility assessed for any change to an existing endpoint; breaking
      changes explicitly flagged (see [`api-versioning-and-compatibility`](../skills/api/api-versioning-and-compatibility.md)).
- [ ] Rate limiting applied to public/unauthenticated endpoints.
- [ ] OpenAPI/Swagger metadata updated to match the actual contract.
- [ ] Tests cover success, validation failure, and authorization failure.
