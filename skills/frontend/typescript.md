# Skill: TypeScript

## Purpose
Use TypeScript's type system to catch integration errors (especially against the .NET API's
contract) at compile time rather than runtime.

## When to Use
Any frontend code in a project with `frontend.language: typescript`.

## Prerequisites
`strict` mode enabled in `tsconfig.json` (verify — flipping it repo-wide is a separate,
deliberate change if not already on, not something to change incidentally).

## Inputs Required
The data shapes being modeled, ideally derived from or matched against the actual API
contract rather than guessed.

## Engineering Principles
- `strict: true` (or the individual strict flags it enables) — catches null/undefined
  misuse, implicit `any`, and unsound type narrowing.
- Model API response/request shapes as explicit `interface`/`type` declarations matching the
  actual .NET DTOs — ideally generated from the API's OpenAPI spec (`openapi-typescript` or
  similar) to keep them in sync automatically, rather than hand-maintained and prone to
  drift.
- Avoid `any`; when a type genuinely can't be known statically, use `unknown` and narrow it
  explicitly, which forces a runtime check before use.
- Use discriminated unions for state that has genuinely distinct shapes per case (e.g. a
  fetch result that's `{status: 'loading'} | {status: 'error', error} | {status: 'success',
  data}`) instead of a single type with many optional fields.

## Step-by-Step Workflow
1. Check whether the project generates types from the API's OpenAPI spec — use that as the
   source of truth if it exists rather than hand-writing duplicate types.
2. If hand-writing: match the type to the actual API response shape, including which fields
   are genuinely optional (`?`) vs. always present.
3. Avoid `any`; use `unknown` with explicit narrowing where the type truly can't be known
   ahead of time.
4. Model multi-case state as a discriminated union.

## Code Standards
```typescript
// Matches the .NET API's OrderSummaryDto exactly -- ideally generated, not hand-guessed
interface OrderSummary {
  id: string;
  customerId: string;
  total: number;
  status: "Draft" | "Confirmed" | "Cancelled";
}

// Discriminated union for fetch state instead of optional-field soup
type FetchState<T> =
  | { status: "loading" }
  | { status: "error"; error: string }
  | { status: "success"; data: T };

function renderOrders(state: FetchState<OrderSummary[]>) {
  switch (state.status) {
    case "loading": return <Spinner />;
    case "error": return <ErrorMessage message={state.error} />;
    case "success": return <OrderList orders={state.data} />;
  }
}

// unknown, narrowed explicitly, instead of any
function parseApiError(payload: unknown): string {
  if (typeof payload === "object" && payload !== null && "title" in payload) {
    return String((payload as { title: unknown }).title);
  }
  return "An unexpected error occurred.";
}
```

## Architecture Constraints
API contract types live in a shared location the API client layer imports from — not
redefined ad hoc in every component that happens to use that data.

## Security Considerations
TypeScript types are compile-time only — they provide no runtime guarantee against a
malformed or malicious API response; validate/narrow untrusted external data at the boundary
where it enters the app (see [`frontend-api-integration`](../frontend/frontend-api-integration.md)).

## Testing Requirements
Type correctness itself isn't unit-tested (the compiler enforces it); test the runtime
behavior that depends on correctly narrowed types (error parsing, discriminated union
rendering).

## Common Mistakes
- Hand-maintained API types drifting out of sync with the actual .NET DTOs after a backend
  change.
- `any` used as an escape hatch to silence a type error instead of fixing the actual type
  mismatch.
- Marking a field optional (`?`) when the API always returns it, or vice versa, causing
  either unnecessary null-checks or missed ones.

## Anti-Patterns
- `// @ts-ignore` used to suppress a type error instead of fixing the underlying mismatch.

## Validation Checklist
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Definition of Done
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Example
See [Code Standards](#code-standards).

## Related Skills
[`react`](../frontend/react.md), [`frontend-api-integration`](../frontend/frontend-api-integration.md).
