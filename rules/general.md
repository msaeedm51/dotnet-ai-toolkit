# General Rules

Cross-cutting engineering discipline, independent of layer or technology. See
[RULES.md](../RULES.md) for precedence and how rules relate to skills.

- Make the smallest change that satisfies the requirement — no speculative generalization.
- Do not introduce a dependency for functionality the standard library or an existing
  dependency already provides.
- Delete dead code; do not comment it out "in case it's needed later."
- Do not leave TODOs without an issue/owner reference, or debug/trace output, in committed
  code.
- Match the existing naming, formatting, and file-organization conventions of the file/area
  being edited, even where a different convention would be preferred on a greenfield project.
- A public API (method signature, class, HTTP endpoint, config key) is a commitment to
  callers — do not expose one that isn't ready to be depended on.
- A comment explains *why*, not *what* — if removing it wouldn't confuse a future reader,
  remove it.
- Fail fast and loudly on programmer errors (invalid internal state, violated invariants);
  fail gracefully and informatively on expected failures (bad user input, an external system
  being unavailable).
- Do not refactor unrelated code as a side effect of an unrelated change.
- State assumptions explicitly when a requirement is ambiguous, rather than silently picking
  one interpretation.
