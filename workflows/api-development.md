# Workflow: API Development

A specialization of [`workflows/new-feature.md`](new-feature.md) for adding or changing an
HTTP endpoint. Primary agent: [`api-engineer`](../agents/api-engineer.md). Primary skill:
[`skills/api/rest-api-design.md`](../skills/api/rest-api-design.md).

1. **Confirm the resource and operation** — what resource, what HTTP method, what boundary
   (public/internal, authenticated/not).
2. **Check existing endpoint conventions** — route style, DTO naming, error shape,
   versioning scheme already in the project; new endpoints match, they don't introduce a
   second convention.
3. **Define the contract first** — request/response DTOs, status codes, validation rules —
   before writing the handler body.
4. **Determine authorization requirement** for this specific operation, including
   resource-level ownership if applicable — don't assume the route group's default is
   sufficient.
5. **Implement**, delegating business logic to Application/Domain
   (see [`clean-architecture`](../skills/architecture/clean-architecture.md) if in use) —
   the endpoint handler stays thin.
6. **Add pagination/filtering** if the response is a collection.
7. **Assess backward compatibility** if modifying an existing endpoint — flag any breaking
   change per [`rules/api.md`](../rules/api.md).
8. **Write tests**: success, validation failure, authorization failure, not-found where
   applicable — see [`skills/testing/integration-testing.md`](../skills/testing/integration-testing.md).
9. **Security pass** — apply [`skills/security/security-owasp.md`](../skills/security/security-owasp.md)'s
   relevant checks; route to [`security-reviewer`](../agents/security-reviewer.md) for
   anything touching auth or sensitive data.
10. **Update OpenAPI/Swagger metadata** and any API documentation.

## Exit criteria
[`checklists/api-design-checklist.md`](../checklists/api-design-checklist.md) and
[`checklists/definition-of-done.md`](../checklists/definition-of-done.md).
