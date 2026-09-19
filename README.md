# dotnet-ai-toolkit

A reusable, **LLM-agnostic** AI Engineering Operating System for professional .NET
development — a library of agents, skills, rules, workflows, checklists, and prompts that
any AI coding assistant (Claude Code, ChatGPT, Gemini, GitHub Copilot, Cursor, Windsurf, or
whatever comes next) can load selectively while working in a .NET codebase.

It is not a giant system prompt. It's an indexed knowledge base: an AI assistant loads only
the handful of files relevant to the task in front of it, the same way a senior engineer
only pulls up the docs pages that matter for the change they're making.

## Why this exists

Every AI coding platform reinvents "here's how we do things" for .NET projects, and that
knowledge doesn't travel between platforms or between projects. This toolkit separates the
**engineering knowledge** (architecture patterns, coding rules, review standards, workflows —
all vendor-neutral) from the **platform wiring** (how Claude Code, Cursor, etc. each want
their config files structured). You vendor the knowledge once per project; you get a thin
adapter file per platform for free.

## Repository structure

```text
AGENTS.md          AI operating principles + agent roster (start here)
RULES.md            Rule categories + precedence system
config/             JSON Schema + example for a consumer project's config.yaml
agents/             Specialized AI role definitions (architect, dotnet-developer, ...)
skills/             Practical, focused how-to knowledge, indexed and selectively loaded
rules/              Short, deterministic, enforceable rules by category
workflows/          Ordered processes (new feature, bug fix, database change, ...)
checklists/         Shared review/DoD checklists referenced by skills and agents
prompts/            Reusable, parameterized prompt templates by task type
templates/projects/ Buildable solution scaffolds (Clean Architecture, modular monolith, ...)
templates/docs/     Document templates (README, ADR, runbook, ...)
adapters/           Thin per-platform translation layers (Claude Code, ChatGPT, Cursor, ...)
```

See [AGENTS.md](AGENTS.md) for how an AI assistant is expected to navigate this structure,
and the design rationale in [CONTRIBUTING.md](CONTRIBUTING.md) for how to extend it.

## Installing in a .NET project

Recommended: vendor this repo as a git submodule pinned to a release tag, so updates are
deliberate.

```bash
git submodule add https://github.com/msaeedm51/dotnet-ai-toolkit.git .ai-dotnet
cd .ai-dotnet
git checkout v1.0.0
cd ..
git add .ai-dotnet .gitmodules
git commit -m "Add dotnet-ai-toolkit"
```

Then:

1. Run `./.ai-dotnet/tools/init-project.sh <adapter>` (one of `claude-code`, `chatgpt`,
   `cursor`, `copilot`, `windsurf`, `generic`) from your project root. It creates
   `.ai-dotnet/config.yaml` from the example (fill in your project's actual architecture and
   stack — schema: `config/config.schema.json`) and generates that platform's entry-point
   pointer file, without overwriting anything that already exists. Prefer doing this by hand?
   Copy `.ai-dotnet/config/config.example.yaml` to `.ai-dotnet/config.yaml` yourself and follow
   the matching file under [`adapters/`](adapters/) directly. Skipping `config.yaml` entirely
   is fine too — the AI assistant will run
   [`workflows/project-discovery.md`](workflows/project-discovery.md) instead of guessing.
2. Read your platform's adapter under [`adapters/`](adapters/) for anything the generated
   file doesn't cover (e.g. Cursor/Windsurf category-specific rules). The adapter file is a
   pointer into `.ai-dotnet/`, not
   a copy of it — the knowledge stays in one place.
3. Tell your AI assistant something like: *"Load the .NET engineering toolkit, inspect this
   project, and help me implement <feature>."* From there it selects the relevant agents,
   skills, and rules on its own — see [AGENTS.md](AGENTS.md#2-how-to-select-relevant-knowledge-context-management).

Updating later:

```bash
cd .ai-dotnet
git fetch --tags
git checkout v1.1.0
cd ..
git add .ai-dotnet
git commit -m "Bump dotnet-ai-toolkit to v1.1.0"
```

Check [CHANGELOG.md](CHANGELOG.md) between your old and new tag before bumping — a MAJOR
bump can change rule defaults or config schema keys.

Don't want a submodule? [`adapters/generic.md`](adapters/generic.md) documents a subtree and
a manual-sync alternative.

## Supported AI platforms

| Platform | Adapter |
|---|---|
| Claude Code | [`adapters/claude-code.md`](adapters/claude-code.md) |
| ChatGPT | [`adapters/chatgpt.md`](adapters/chatgpt.md) |
| Cursor | [`adapters/cursor.md`](adapters/cursor.md) |
| GitHub Copilot | [`adapters/copilot.md`](adapters/copilot.md) |
| Windsurf | [`adapters/windsurf.md`](adapters/windsurf.md) |
| Anything else | [`adapters/generic.md`](adapters/generic.md) |

Nothing in `agents/`, `skills/`, `rules/`, `workflows/`, `checklists/`, `prompts/`, or
`templates/` is vendor-specific — only `adapters/` is. That's a hard rule enforced by CI
(see `CONTRIBUTING.md`).

## Who this is for

An experienced .NET developer working across ASP.NET Core, EF Core, SQL Server/PostgreSQL,
REST/Minimal APIs, React/TypeScript frontends, Azure, Docker, and Linux — using Clean
Architecture, DDD, CQRS, modular monoliths, or microservices as appropriate per project. The
toolkit does not assume every project uses every technology; `config.yaml` and project
discovery determine what actually applies (see [AGENTS.md](AGENTS.md)).

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for the file formats, indexing requirements, and PR
process.

## License

[MIT](LICENSE)
