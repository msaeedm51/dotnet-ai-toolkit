# Frontend Rules (React / TypeScript)

Applies only when `config.yaml` `frontend.framework`/`frontend.language` indicate a frontend
is in scope. See `skills/frontend/*` for reasoning and examples.

- Enable TypeScript strict mode; do not introduce an untyped `any` without a comment stating
  why it's necessary.
- Handle both the error and loading state for every asynchronous data fetch, not only the
  success state.
- No unhandled promise rejections — every `async` call site either awaits with error
  handling or explicitly documents why it's fire-and-forget.
- Never store a secret, API key, or credential in frontend code or a bundled JS asset —
  anything shipped to the browser is public.
- Co-locate a component's state with the component unless more than one component needs it;
  don't lift state to a global store by default.
- Follow the project's existing state-management approach; do not introduce a second one
  alongside an established one without explicit direction.
- Validate form input on the client for UX, but never treat client-side validation as a
  substitute for server-side validation.
