# Adapter: GitHub Copilot

GitHub Copilot (in VS Code / Visual Studio / the JetBrains plugin / Copilot Chat) supports
repo-level custom instructions at `.github/copilot-instructions.md`, and, on newer
versions, path-scoped instruction files at `.github/instructions/*.instructions.md` with an
`applyTo` glob — Copilot's equivalent of Cursor/Windsurf's glob-activated rules.

## Entry-point files

`.github/copilot-instructions.md` (always included in Copilot's context for this repo):
```markdown
This project uses the dotnet-ai-toolkit, vendored at `.ai-dotnet/`.

Before non-trivial suggestions or chat responses:
1. Read `.ai-dotnet/AGENTS.md` and `.ai-dotnet/RULES.md`.
2. Read `.ai-dotnet/config.yaml` for this project's actual architecture/stack; if
   absent, follow `.ai-dotnet/workflows/project-discovery.md` first.
3. Load the agent from `.ai-dotnet/agents/` and the skills from `.ai-dotnet/skills/`
   matching the current request, per their INDEX.yaml files.

Project-specific overrides take precedence over the toolkit's generic defaults, per
`.ai-dotnet/RULES.md`.
```

Path-scoped instructions (if your Copilot version supports `.github/instructions/`), one per
skill category, e.g. `.github/instructions/database.instructions.md`:
```markdown
---
applyTo: "**/Migrations/**,**/*DbContext.cs"
---

Load `.ai-dotnet/skills/data/*.md` and `.ai-dotnet/agents/database-engineer.md` for
work in these files. Also apply `.ai-dotnet/rules/database.md`.
```

## Selective loading

Where path-scoped instructions are supported, they behave like Cursor/Windsurf's
glob-activated rules — the toolkit's category structure maps directly onto them. Where only
the single repo-level file is supported, Copilot doesn't have a native mechanism for
conditional loading; the pointer file's step 3 above asks Copilot to do the selection itself
by reading `INDEX.yaml`, which depends on Copilot Chat having filesystem read access in the
current context (it generally does inside an IDE with the repo open, less reliably in other
surfaces).

## Precedence notes

Copilot's own content-filtering and safety behavior sits above anything in
`.ai-dotnet/RULES.md` per that file's precedence level 1. Keep project-specific overrides in
a clearly separated section of `copilot-instructions.md` (or a dedicated
`.github/instructions/project-overrides.instructions.md` with a broad `applyTo`).

## Limitations

- Instruction-file support (single vs. path-scoped) varies by Copilot surface (VS Code vs.
  Visual Studio vs. JetBrains vs. github.com) and version — verify what's available before
  generating path-scoped files, and fall back to the single `copilot-instructions.md` if
  needed.
- Copilot's inline code-completion (as opposed to Copilot Chat) has much shallower context
  awareness than a chat-driven agent — this toolkit's workflows (multi-step processes,
  agent handoffs) are designed for Copilot Chat / Copilot's agent mode, not raw
  autocomplete suggestions.
