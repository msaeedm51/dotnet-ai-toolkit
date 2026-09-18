# Skill: React

## Purpose
Build React components that are correctly typed, correctly manage state and side effects,
and handle loading/error states — for projects where `config.yaml` `frontend.framework:
react`. Not assumed for every .NET project.

## When to Use
Any component/page work in a project with a React frontend.

## Prerequisites
[`typescript`](../frontend/typescript.md); confirm this project actually has a frontend
before applying this skill.

## Inputs Required
The UI requirement, and the project's existing component/state-management conventions.

## Engineering Principles
- Function components with hooks — no new class components in a project already on hooks.
- Co-locate state with the component that owns it; lift state only when more than one
  component genuinely needs it (see [`rules/frontend.md`](../../rules/frontend.md)).
- Every async data fetch has explicit loading, error, and success states rendered — a
  component that only handles the success case will show nothing or crash on a slow network
  or a failed request.
- Follow the project's existing data-fetching approach (React Query/TanStack Query, SWR, or
  plain `fetch` + hooks) — don't introduce a second one.
- Keep components focused; extract a custom hook when logic (not just markup) is reused
  across components.

## Step-by-Step Workflow
1. Check existing components for the project's conventions (state management, styling
   approach, data-fetching library).
2. Define the component's props with explicit TypeScript types (no untyped `any`).
3. Handle loading/error/success states for any async data explicitly.
4. Extract reusable logic into a custom hook if it's used in more than one place.
5. Consider accessibility (semantic HTML, keyboard navigation, ARIA where native semantics
   aren't enough).

## Code Standards
```tsx
interface OrderListProps {
  customerId: string;
}

export function OrderList({ customerId }: OrderListProps) {
  const { data: orders, isLoading, error } = useOrders(customerId);

  if (isLoading) return <Spinner />;
  if (error) return <ErrorMessage message="Could not load orders. Please try again." />;
  if (!orders || orders.length === 0) return <EmptyState message="No orders yet." />;

  return (
    <ul>
      {orders.map((order) => (
        <OrderRow key={order.id} order={order} />
      ))}
    </ul>
  );
}

function useOrders(customerId: string) {
  return useQuery({
    queryKey: ["orders", customerId],
    queryFn: () => fetchOrders(customerId),
  });
}
```

## Architecture Constraints
Components consume data through the project's established API client/hooks layer (see
[`frontend-api-integration`](../frontend/frontend-api-integration.md)), not by calling
`fetch` directly scattered through component bodies.

## Security Considerations
Never render unsanitized user-generated content via `dangerouslySetInnerHTML` without an
explicit sanitization step; never embed a secret/API key in client bundle code (see
[`rules/frontend.md`](../../rules/frontend.md)).

## Testing Requirements
Component tests (React Testing Library) covering loading, error, and success rendering
states for any component with async data; interaction tests for user-triggered behavior.

## Common Mistakes
- Rendering only the success state, leaving a blank screen or unhandled crash on error/slow
  network.
- Prop drilling several levels deep instead of appropriate composition or context where it
  genuinely reduces complexity.
- Missing `key` props (or using array index as `key` for a list that can reorder), causing
  subtle rendering bugs.

## Anti-Patterns
- A second state-management library introduced for one feature alongside an established one
  elsewhere in the app.
- Massive components mixing data fetching, business logic, and presentation with no
  separation.

## Validation Checklist
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Definition of Done
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Example
See [Code Standards](#code-standards).

## Related Skills
[`typescript`](../frontend/typescript.md),
[`frontend-api-integration`](../frontend/frontend-api-integration.md).
