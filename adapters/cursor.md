# Adapter: Cursor

Cursor's Rules system (`.cursor/rules/*.mdc` files, each with a small YAML frontmatter
block) supports exactly the selective-loading behavior this toolkit is designed around:
a rule file can be **Always** applied, **Auto Attached** by glob pattern, **Agent
Requested** (the model decides whether it's relevant, using the rule's description), or
**Manual** (only when explicitly invoked). This maps directly onto
`skills/INDEX.yaml`/`agents/INDEX.yaml`'s `triggers`/`tech` fields.

(Cursor previously used a single root `.cursorrules` file; if your Cursor version only
supports that legacy format, see the fallback at the bottom of this file.)

## Entry-point files

One always-on root rule, plus one auto-attached or agent-requested rule per skill category
(not per individual skill — that's too many files; group by the `skills/<category>/`
folders).

`.cursor/rules/000-toolkit-core.mdc` (Always applied):
```markdown
---
description: dotnet-ai-toolkit operating principles -- always active
alwaysApply: true
---

This project uses the dotnet-ai-toolkit, vendored at `.ai-dotnet/`.

Read `.ai-dotnet/AGENTS.md` and `.ai-dotnet/RULES.md` for operating principles and
precedence. Read `.ai-dotnet/config.yaml` for this project's actual architecture/stack;
if absent, run `.ai-dotnet/workflows/project-discovery.md` first.

Route requests to the matching agent in `.ai-dotnet/agents/` and load skills from
`.ai-dotnet/skills/` per their INDEX.yaml triggers -- most of that selection happens via
the category-specific rules below, which Cursor attaches automatically.
```

`.cursor/rules/api.mdc` (Auto Attached, one per `skills/<category>/`):
```markdown
---
description: API design and ASP.NET Core endpoint work
globs: ["**/Controllers/**", "**/Endpoints/**", "**/*.Api/**"]
---

Load `.ai-dotnet/skills/api/*.md` and `.ai-dotnet/agents/api-engineer.md` for this task.
Also load `.ai-dotnet/rules/api.md` and `.ai-dotnet/rules/security.md`.
```

Repeat for `architecture`, `data`, `security`, `testing`, `dotnet`, `csharp`, `performance`,
`frontend`, `devops` — glob patterns based on the project's actual folder conventions
(discovered via `.ai-dotnet/workflows/project-discovery.md` on first use, then fixed).

## Selective loading

This is Cursor's native mechanism, not something the toolkit has to simulate — Auto
Attached rules only enter context when a file matching their glob is open/edited, which
is a stronger guarantee of relevance than keyword matching alone. Agent Requested rules
(with a good `description`) let the model pull in a category proactively even without a
matching file open, similar to how `AGENTS.md`'s trigger-matching works for other
platforms.

## Precedence notes

Project-specific overrides belong in their own rule file (e.g.
`.cursor/rules/999-project-overrides.mdc`, Always applied, loaded after the toolkit-core
rule) so they're easy to find and don't get mixed into the vendored `.ai-dotnet/` content.

## Fallback: legacy `.cursorrules`

If only the single-file format is available, use the same short pointer content as the
Claude Code adapter's `CLAUDE.md` (see [`claude-code.md`](claude-code.md)) — Cursor will
load the whole file every session, so keep it minimal; you lose the glob-based selective
attachment.

## Limitations

Cursor's rule-loading behavior has changed across versions (legacy `.cursorrules` → `.cursor/rules/`
directory with activation modes) — verify the current mechanism against Cursor's own docs
before generating these files, and update this adapter if it changes again.
