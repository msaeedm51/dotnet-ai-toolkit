# Adapter: Claude Code

Claude Code has full filesystem, shell, and git tool access by default, and a native
mechanism (`CLAUDE.md`) for project-level instructions that's loaded automatically at
session start. This makes it the most direct fit for the toolkit's design: Claude Code can
read `AGENTS.md`, `RULES.md`, and the index files at will, so the entry-point file only
needs to point it there.

## Entry-point file

`CLAUDE.md` at the project root (not inside `.ai-dotnet/` — Claude Code looks for it at the
root of the working directory).

```markdown
# CLAUDE.md

This project uses the dotnet-ai-toolkit, vendored at `.ai-dotnet/`.

Before any non-trivial task:
1. Read `.ai-dotnet/AGENTS.md` for operating principles and how to select relevant
   knowledge.
2. Read `.ai-dotnet/RULES.md` for precedence.
3. Read `.ai-dotnet/config.yaml` if present, for this project's actual architecture and
   stack -- if absent, run `.ai-dotnet/workflows/project-discovery.md` first.
4. Scan `.ai-dotnet/agents/INDEX.yaml` and `.ai-dotnet/skills/INDEX.yaml` for entries
   matching the current request; load only those files.

Project-specific overrides (if any) go below this line -- they take precedence over the
toolkit's generic defaults, per `.ai-dotnet/RULES.md`'s precedence system.
```

Keep this file exactly that short. It's a pointer, not a copy — the moment someone starts
pasting rule content into `CLAUDE.md` directly, it drifts out of sync with `.ai-dotnet/`.

## Selective loading

Claude Code's `Read`/`Grep`/`Glob` tools let it implement `AGENTS.md`'s selection algorithm
literally: grep `agents/INDEX.yaml` and `skills/INDEX.yaml` for trigger matches, then `Read`
only the matched files. No special configuration is needed for this — it's just how Claude
Code already works once `CLAUDE.md` tells it to.

## Optional: toolkit agents as Claude Code subagents

Claude Code supports subagent definitions at `.claude/agents/*.md` with YAML frontmatter
(`name`, `description`, `tools`). You can generate one per toolkit agent as a thin wrapper:

```markdown
---
name: dotnet-api-engineer
description: Use for designing or changing an HTTP API endpoint in this .NET project.
tools: Read, Edit, Write, Bash, Grep, Glob
---

Follow `.ai-dotnet/agents/api-engineer.md` in full -- its role, workflow, constraints,
and validation criteria. Load skills via `.ai-dotnet/skills/INDEX.yaml` as that file
directs.
```

This is optional — Claude Code can apply the toolkit's agents without dedicated subagent
files, just by reading `agents/*.md` directly when `AGENTS.md` routes it there. Generating
subagents is worthwhile mainly if you want them addressable individually (e.g. via an
explicit subagent invocation) rather than selected implicitly.

## Precedence notes

Claude Code's own safety/permission prompts (file edits, shell commands, destructive git
operations) sit above everything in `.ai-dotnet/RULES.md`'s precedence list, per
`RULES.md` level 1 — the toolkit never asks Claude Code to bypass those.

## Limitations

None specific — this is the toolkit's reference implementation. If a future Claude Code
feature changes how project instructions are loaded, update this file, not `AGENTS.md`.
