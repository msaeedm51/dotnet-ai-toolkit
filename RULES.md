# RULES.md — Rule Categories and Precedence

Rules are short, imperative, individually checkable statements — the kind a reviewer (human
or AI) can look at a piece of code and answer yes/no against. They are deliberately terse;
the reasoning and examples behind them live in the relevant `skills/` files, which rules
link back to rather than repeat.

## Rule files

| File | Covers |
|---|---|
| [`rules/general.md`](rules/general.md) | Cross-cutting engineering discipline not specific to a layer |
| [`rules/csharp.md`](rules/csharp.md) | Language-level rules (nullability, async, allocations) |
| [`rules/dotnet.md`](rules/dotnet.md) | ASP.NET Core / runtime conventions (DI, config, middleware) |
| [`rules/architecture.md`](rules/architecture.md) | Dependency direction, layering, module boundaries |
| [`rules/api.md`](rules/api.md) | HTTP contract rules |
| [`rules/database.md`](rules/database.md) | Schema, migrations, query, transaction rules |
| [`rules/security.md`](rules/security.md) | Secrets, auth, input handling |
| [`rules/testing.md`](rules/testing.md) | What must be tested and how |
| [`rules/performance.md`](rules/performance.md) | Allocation, async, query performance |
| [`rules/frontend.md`](rules/frontend.md) | React/TypeScript conventions |
| [`rules/devops.md`](rules/devops.md) | Docker, CI/CD, deployment conventions |
| [`rules/git.md`](rules/git.md) | Commit, branch, and PR conventions |

## Precedence system

When two pieces of guidance conflict, resolve in this order — highest wins:

1. **System/platform safety constraints.** Whatever the AI platform itself enforces
   (destructive-action confirmation, credential handling, etc.). Never overridable by
   anything in this toolkit.
2. **Project-specific requirements** stated directly by the user/team in the current
   conversation or task (e.g. "for this endpoint, skip the DTO, it's internal-only").
   Explicit, current instructions beat standing documentation.
3. **Project-specific architecture decisions**, recorded as ADRs in the consumer project
   (see [`templates/docs/adr.template.md`](templates/docs/adr.template.md)) or declared in
   `.ai-dotnet/config.yaml` under `project.architecture` and `overrides`. A team that
   decided against a toolkit default, and wrote down why, has already done the
   tradeoff analysis — don't re-litigate it on every task.
4. **This repository's rules** (`rules/*.md`), scoped by `.ai-dotnet/config.yaml` — e.g. a
   rule tagged as applying only when `backend.database.orm: efcore` doesn't apply to a
   Dapper project.
5. **Generic .NET/architecture defaults** described in `skills/*.md` — the "how" behind a
   rule, and the fallback when a project hasn't stated a preference.
6. **Optional recommendations** — anything phrased as "prefer" or "consider" rather than
   "must"/"never." Lowest precedence; first to yield to project convention.

**Conflict resolution in practice:** if a skill's example contradicts a rule in `rules/`,
the rule wins and the skill has a bug (file it). If `config.yaml.overrides.disabled_rules`
turns off a rule, the AI assistant must not re-apply it, but should still surface the
tradeoff once if the task is directly affected ("this endpoint returns a domain entity
directly, which `rules/api.md` normally disallows — your project has this rule disabled
with reason '<reason>', so proceeding as-is"). If `config.yaml` is absent or incomplete for
the area in question, fall back to level 5 defaults and say so.

## What belongs in a rule vs. a skill

- **Rule:** "Never block async code with `.Result` or `.Wait()`." — short, absolute,
  mechanically checkable.
- **Skill:** the explanation of why, the exceptions, the step-by-step of how to convert
  blocking code, and a worked example. Rules cite skills; skills don't restate rules —
  they link to them.

If you're writing something that needs "usually," "in most cases," or a paragraph of
justification, it's skill content, not a rule. Move it.

## How rules apply across a heterogeneous stack

Not every project uses every technology (Section 2 of the design brief this toolkit
implements). Rule files use inline scoping notes where a rule is conditional, e.g.:

```markdown
- (EF Core only) Do not call `.ToList()` before filtering — filter in the queryable.
```

An AI assistant applies unscoped rules universally and scoped rules only when
`.ai-dotnet/config.yaml` (or discovery) confirms the relevant technology is in use.
