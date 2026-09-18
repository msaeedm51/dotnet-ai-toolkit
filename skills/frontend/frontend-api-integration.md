# Skill: Frontend API Integration

## Purpose
Consume the .NET API from the frontend consistently — a single client layer, correct auth
token handling, and consistent error handling matching the API's `ProblemDetails` contract.

## When to Use
Any frontend code that calls the backend API.

## Prerequisites
[`typescript`](../frontend/typescript.md); know the API's authentication scheme
(`config.yaml` `backend.authentication`) and its error response shape
(see [`model-validation-problemdetails`](../dotnet/model-validation-problemdetails.md)).

## Inputs Required
The API endpoint(s) being called and their request/response contract.

## Engineering Principles
- One API client layer (a thin wrapper around `fetch`/`axios`, or a generated client from
  the OpenAPI spec) that every component goes through — not `fetch` calls scattered through
  component bodies.
- Attach the auth token consistently (from wherever the app stores it —
  `HttpOnly` cookie handled automatically by the browser, or an in-memory/secure-storage
  token attached as an `Authorization` header) in one place, not per call site.
- Handle the API's `ProblemDetails` error shape consistently — parse it once in the client
  layer and surface a consistent error type to calling code, rather than each component
  parsing the raw response differently.
- Handle token expiry/refresh centrally (a 401 response triggers a refresh-and-retry or a
  redirect to login) rather than duplicated per call site.
- Never store a long-lived access token in `localStorage` (XSS-exfiltrable) — prefer an
  `HttpOnly` cookie set by the backend, or short-lived in-memory storage with silent refresh.

## Step-by-Step Workflow
1. Check for an existing API client layer; extend it rather than calling `fetch` directly in
   a new component.
2. Define the request/response types matching the API contract
   (see [`typescript`](../frontend/typescript.md)).
3. Add the new call to the client layer, going through the shared auth/error handling.
4. Handle loading/error/success in the consuming component
   (see [`react`](../frontend/react.md)).

## Code Standards
```typescript
class ApiError extends Error {
  constructor(public status: number, public title: string, public errors?: Record<string, string[]>) {
    super(title);
  }
}

async function apiFetch<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...init,
    credentials: "include", // HttpOnly auth cookie sent automatically
    headers: { "Content-Type": "application/json", ...init?.headers },
  });

  if (response.status === 401) {
    // centralized handling -- redirect to login or attempt refresh, not per call site
    redirectToLogin();
    throw new ApiError(401, "Unauthorized");
  }

  if (!response.ok) {
    const problem = await response.json().catch(() => null);
    throw new ApiError(response.status, problem?.title ?? "Request failed", problem?.errors);
  }

  return response.status === 204 ? (undefined as T) : response.json();
}

export const ordersApi = {
  create: (request: CreateOrderRequest) =>
    apiFetch<{ id: string }>("/orders", { method: "POST", body: JSON.stringify(request) }),
  get: (id: string) => apiFetch<OrderSummary>(`/orders/${id}`),
};
```

## Architecture Constraints
Components depend on the client layer's typed functions (`ordersApi.create`), never on raw
`fetch` calls or hard-coded URLs.

## Security Considerations
See [`rules/frontend.md`](../../rules/frontend.md): no secrets in client code; prefer
`HttpOnly` cookies for auth tokens where the architecture allows it; always send
credentials/tokens only to the app's own API origin, never to a third party.

## Testing Requirements
Test the client layer's error parsing (a `ProblemDetails` response correctly becomes an
`ApiError` with the right fields) and the 401 handling path, in addition to component-level
tests for loading/error/success rendering.

## Common Mistakes
- `fetch` calls duplicated across many components, each handling errors/auth slightly
  differently.
- Not handling `401` centrally, leaving users on a broken page instead of being redirected to
  re-authenticate.
- Assuming every error response has the same shape without defensively parsing it.

## Anti-Patterns
- A client layer that just re-exports raw `fetch` with no typing, error handling, or auth
  attachment — provides no real abstraction over calling `fetch` directly.

## Validation Checklist
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Definition of Done
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Example
See [Code Standards](#code-standards).

## Related Skills
[`react`](../frontend/react.md), [`typescript`](../frontend/typescript.md),
[`rest-api-design`](../api/rest-api-design.md).
