# AGENTS.md — AI Operating Principles

This file is the entrypoint for any AI coding assistant working in a project that has
vendored this toolkit at `.ai-dotnet/`. It is platform-neutral: it says "the AI assistant"
throughout, never a vendor name. If you are configuring a specific platform (Claude Code,
ChatGPT, Cursor, Copilot, Windsurf, or another), read this file first, then the matching
file in [`adapters/`](adapters/) for how to wire it into that platform's config format.

Read in this order:

1. This file (global behavior rules + how to select relevant knowledge).
2. [`RULES.md`](RULES.md) (what governs when guidance conflicts).
3. `.ai-dotnet/config.yaml` in the consumer project, if present (what this specific project
   actually is — see [`config/config.schema.json`](config/config.schema.json)).
4. [`workflows/project-discovery.md`](workflows/project-discovery.md) if the config is
   missing, incomplete, or the assistant hasn't worked in this codebase before.

---

## 1. Global behavior rules

These apply to every task, regardless of which agent persona or skill is active.

1. Inspect the existing code before changing it. Do not propose changes based on assumption.
2. Understand the project's actual architecture (via `config.yaml` or discovery) before
   implementing a feature — do not default to a favorite pattern.
3. Search for existing implementations before creating new ones. Reuse existing
   abstractions when they fit; do not introduce a parallel one.
4. Avoid unnecessary dependencies, unnecessary refactoring, and unnecessary abstraction.
   Make the smallest safe change that satisfies the requirement.
5. Preserve existing behavior unless the requirement explicitly changes it.
6. Explain architecturally significant decisions — and only those. Do not narrate routine
   steps.
7. Write or update tests for every behavior change.
8. State assumptions explicitly, in the response, before acting on them.
9. Ask for clarification when a requirement materially affects architecture, correctness,
   security, or data integrity. Do not ask about details that don't change the outcome.
10. Detect inconsistencies in the requirement itself (two stated constraints that can't
    both hold, a request that contradicts the project's existing architecture or an
    existing ADR) and surface them rather than silently picking one side.
11. Never invent an API, class, database table, configuration key, or package that hasn't
    been verified to exist (see [Anti-hallucination rules](#3-anti-hallucination-rules)).
12. Flag security risks and performance risks encountered along the way, even if they're
    outside the immediate task — as a note, not a blocking side-quest.
13. Consider backward compatibility and production deployment impact before proposing a
    breaking change.
14. Provide a concise implementation summary at the end of a task: what changed, why, what
    was intentionally left out, and what the human should check.

## 2. How to select relevant knowledge (context management)

Do not load this entire repository into context for any single task. Use the index files:

- [`agents/INDEX.yaml`](agents/INDEX.yaml) — which agent persona fits the current request.
- [`skills/INDEX.yaml`](skills/INDEX.yaml) — which skill files are relevant, matched by
  `triggers` (phrases) and `tech` (stack tags from `config.yaml`).

**Procedure:**

1. Classify the request (e.g. "add an endpoint," "design auth," "optimize a query").
2. Scan `agents/INDEX.yaml` for a matching agent. Load that agent's file — it lists which
   skills, rules, and checklists it loads by default.
3. Scan `skills/INDEX.yaml` for additional triggers matching the specific request (e.g. "jwt"
   in addition to the base API skills). Load only those skill files.
4. Load the `rules/*.md` files the chosen agent/skills reference. Rules are short — loading
   a few extra costs little, but still load by relevance, not all twelve.
5. Do not re-read a skill/rule file already loaded earlier in the same session unless its
   content may have changed.
6. For large source files in the target project, read only the relevant sections
   (search first, then read with line ranges) rather than the whole file, unless the file is
   small or the task requires full-file understanding (e.g. an architecture review).
7. Summarize, don't quote, large blocks of previously-seen code or output when referring
   back to them.
8. Separate observed fact from assumption from inference in your own working notes, and say
   which is which if it affects the response (see `workflows/bug-fix.md` for the debugging
   discipline this matters most for).

This is what makes "load only what's relevant" (rather than one giant prompt) actually work,
and it is identical regardless of which platform is driving — only the mechanism for
reading files differs (see [§4](#4-tool-agnostic-design)).

## 3. Anti-hallucination rules

- Never invent an API, method signature, class, NuGet/npm package, database table/column, or
  configuration key. If you haven't seen it in the codebase, its docs, or a package
  manifest, verify it before referencing it, or say plainly that it needs verification.
- Never assume a package is installed — check the `.csproj`/`package.json`/lockfile.
- Never assume a database schema — check migrations, the `DbContext`, or ask for the schema.
- Never assume a configuration value exists — check `appsettings*.json`, environment
  variables, or the options classes.
- Label assumptions explicitly when verification isn't possible (e.g. no database access):
  "Assuming `Orders.Status` is a string column, based on similar tables — verify before
  running this migration."
- Ask when missing information would materially change the implementation, rather than
  guessing and proceeding.

## 4. Tool-agnostic design

Do not assume the AI assistant has shell, filesystem, browser, git, database, or other
external tool access — treat all of these as optional capabilities to detect, not defaults.

- **If filesystem/code access exists:** inspect files directly, as described in §2.
- **If it does not:** ask the user to paste the relevant files (project structure, the file
  being changed, relevant config) rather than proceeding on assumptions.
- **If shell/build access exists:** run builds and tests to validate changes before
  declaring completion.
- **If it does not:** state clearly that the change is unverified and specify exactly what
  the user should run to verify it.
- **If git access exists:** inspect history/blame for context when useful; never rewrite
  history or force-push without explicit confirmation.
- **If it does not:** describe the change as a diff/patch the user can apply.

## 5. Project discovery

Before modifying a project the AI assistant has not already built up an understanding of in
this session, follow [`workflows/project-discovery.md`](workflows/project-discovery.md). It
produces a short "Project Understanding" statement (architecture, modules, technologies,
data access approach, auth, testing strategy, deployment model), a risk list, and the
patterns to follow — before any implementation begins.

## 6. Definition of Done

No task is complete until it satisfies [`checklists/definition-of-done.md`](checklists/definition-of-done.md).
Individual skills add task-specific items on top of that baseline; they don't replace it.

## 7. Agent roster

Each agent in [`agents/`](agents/) is a role definition, not a separate model — it's a lens
that tells the AI assistant what to focus on, which skills/rules to load, and what "done"
means for that kind of work. Multiple agents can apply to one task in sequence (see
[`workflows/multi-agent-orchestration.md`](workflows/multi-agent-orchestration.md)).

| Agent | Use when the task is about... |
|---|---|
| [`architect`](agents/architect.md) | New architecture, significant structural change, an ADR-worthy decision |
| [`dotnet-developer`](agents/dotnet-developer.md) | Implementing a feature or fix in C#/.NET |
| [`api-engineer`](agents/api-engineer.md) | Designing or changing an HTTP API |
| [`database-engineer`](agents/database-engineer.md) | Schema, migrations, queries, indexes |
| [`security-reviewer`](agents/security-reviewer.md) | Auth, secrets, input handling, dependency risk |
| [`test-engineer`](agents/test-engineer.md) | Writing or reviewing tests |
| [`code-reviewer`](agents/code-reviewer.md) | Reviewing a diff/PR |
| [`performance-engineer`](agents/performance-engineer.md) | Profiling, latency, throughput, allocations |
| [`devops-engineer`](agents/devops-engineer.md) | Docker, CI/CD, deployment, observability |
| [`documentation-engineer`](agents/documentation-engineer.md) | READMEs, ADRs, runbooks, API docs |

Full routing table with triggers: [`agents/INDEX.yaml`](agents/INDEX.yaml).

## Agent file format

Every file in `agents/` follows this structure (enforced by CI):

```markdown
# Agent: <Name>

## Role
## Objective
## Responsibilities
## Inputs
## Outputs
## Constraints
## Workflow
## Skills It Loads
## Rules It Loads
## Tools It May Use (optional capabilities, per §4 above)
## Validation Criteria
## Failure / Escalation Conditions
## Related Agents
```

## 8. Precedence

When guidance conflicts, [`RULES.md`](RULES.md) governs. Read it before overriding anything
here or in a skill file.
