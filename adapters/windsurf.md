# Adapter: Windsurf

Windsurf (Codeium) supports a rules directory (`.windsurf/rules/*.md`, each with an
activation mode: Always On, Glob, Model Decision, or Manual) — conceptually the same model
as Cursor's rules system, so the selective-loading approach is nearly identical. (Older
Windsurf versions used a single root `.windsurfrules` file; see the fallback below if
that's what's available.)

## Entry-point files

One always-on core rule, plus one rule per skill category with either a glob pattern or a
Model Decision activation (Windsurf's equivalent of "let the model decide if this is
relevant," using the rule's description) matching `skills/INDEX.yaml`'s `triggers`.

`.windsurf/rules/toolkit-core.md` (Always On):
```markdown
---
trigger: always_on
---

This project uses the dotnet-ai-toolkit, vendored at `.ai-dotnet/`.

Read `.ai-dotnet/AGENTS.md` and `.ai-dotnet/RULES.md` for operating principles and
precedence. Read `.ai-dotnet/config.yaml` for this project's actual architecture/stack;
if absent, run `.ai-dotnet/workflows/project-discovery.md` first.

Route requests to the matching agent in `.ai-dotnet/agents/` and load the relevant
skills from `.ai-dotnet/skills/` per their INDEX.yaml triggers.
```

`.windsurf/rules/database.md` (Glob or Model Decision):
```markdown
---
trigger: glob
globs: ["**/Migrations/**", "**/*DbContext.cs", "**/*.sql"]
---

Load `.ai-dotnet/skills/data/*.md` and `.ai-dotnet/agents/database-engineer.md`. Also
load `.ai-dotnet/rules/database.md` and, for query/index work,
`.ai-dotnet/rules/performance.md`.
```

Repeat per `skills/<category>/` folder, choosing glob patterns from the project's actual
structure (via `.ai-dotnet/workflows/project-discovery.md`) or Model Decision activation
with a clear description when no reliable glob exists (e.g. architecture-level work isn't
tied to one file pattern).

## Selective loading

Same principle as the Cursor adapter: glob-activated rules only enter context when a
matching file is in play, and Model Decision rules let Windsurf pull in a category based
on the request even without an open matching file — the toolkit's `triggers` field in
`skills/INDEX.yaml` is a good source for each rule's description text.

## Precedence notes

Keep project-specific overrides in their own rule file, loaded Always On, separate from
the vendored `.ai-dotnet/` pointers, so upgrading the `.ai-dotnet/` submodule never risks
overwriting them.

## Fallback: legacy `.windsurfrules`

If only the single-file format is available, use the same minimal pointer content as
[`claude-code.md`](claude-code.md)'s `CLAUDE.md` example — the whole file loads every
session, so keep it short and lose the glob-based selectivity.

## Limitations

Windsurf's rules format and activation modes have changed across releases — verify the
current syntax against Windsurf's own documentation before generating these files, and
update this adapter if it changes.
