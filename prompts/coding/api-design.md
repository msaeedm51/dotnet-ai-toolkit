# Prompt: API Endpoint Design and Implementation

## Purpose
Design and implement a new HTTP endpoint (or change an existing one) following
[`workflows/api-development.md`](../../workflows/api-development.md).

## When to Use
Adding or changing an API endpoint.

## Loads
Agent: [`api-engineer`](../../agents/api-engineer.md). Workflow:
[`api-development`](../../workflows/api-development.md). Skills:
[`rest-api-design`](../../skills/api/rest-api-design.md),
[`model-validation-problemdetails`](../../skills/dotnet/model-validation-problemdetails.md).

## Parameters
- `{{OPERATION}}` — the resource and operation (e.g. "create an order," "list a customer's
  invoices").
- `{{AUTH_REQUIREMENT}}` — who's allowed to call this (public, authenticated, specific role/
  ownership) — state "unknown, please ask" if unclear.

## Prompt Template
```
Load the .NET engineering toolkit and follow workflows/api-development.md for:

{{OPERATION}}

Authorization requirement: {{AUTH_REQUIREMENT}}

Steps:
1. Check this project's existing endpoint conventions (route style, DTO naming, error
   shape, versioning scheme) and match them.
2. Define the request/response DTOs and validation rules before writing the handler.
3. Determine the correct authorization check for this specific operation, including
   resource-level ownership if the authorization requirement implies it.
4. Implement the endpoint, keeping the handler thin -- business logic goes in
   Application/Domain if this project uses that layering.
5. Add pagination if the response is a collection.
6. If this changes an existing endpoint, assess backward compatibility and flag any
   breaking change explicitly.
7. Write tests: success, validation failure, authorization failure.
8. Update OpenAPI/Swagger metadata.

Validate against checklists/api-design-checklist.md before reporting done.
```

## Expected Output
A working endpoint matching project conventions, with tests covering success/validation-
failure/authorization-failure, and an explicit compatibility note if applicable.

## Related
[`prompts/security/security-review.md`](../security/security-review.md) for endpoints
touching auth/sensitive data.
