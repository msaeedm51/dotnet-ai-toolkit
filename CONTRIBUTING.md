# Contributing

This repository is a knowledge base that AI coding assistants load selectively (see
[AGENTS.md](AGENTS.md)). Every contribution must keep that property intact: content has to
be discoverable through an index, vendor-neutral, and small enough to load on its own.

## Ground rules

1. **No vendor-specific language in core content.** `agents/`, `skills/`, `rules/`,
   `workflows/`, `checklists/`, `prompts/`, and `templates/` must say "the AI assistant" or
   "the AI coding agent," never "Claude," "ChatGPT," "Copilot," etc. Vendor-specific
   phrasing belongs only in `adapters/`.
2. **No duplication.** If guidance already exists in a rule, checklist, or another skill,
   link to it (`[relative/path.md](relative/path.md)`) instead of restating it. If you find
   yourself copying a paragraph, that paragraph belongs in one place and should be extracted.
3. **Every new skill or agent file must be registered in its `INDEX.yaml`** in the same PR.
   An unregistered file is effectively invisible to the selective-loading system and CI will
   reject the PR (`.github/workflows/validate.yml`).
4. **Follow the fixed formats** below exactly — the predictability is what lets an AI agent
   parse these files reliably across hundreds of them.
5. **Every code example must be realistic.** Production-style naming, real error handling,
   a cancellation token where I/O is involved, and — for anything demonstrating a testable
   behavior — an accompanying test. No `Foo`/`Bar`/`DoStuff()` placeholders.
6. **Keep files short.** A skill file that can't be read and applied in under ~2 minutes is a
   candidate for splitting. This is a context-budget constraint, not a style preference.

## Adding a skill

Create `skills/<category>/<skill-id>.md` using this structure:

```markdown
# Skill: <Title>

## Purpose
## When to Use
## Prerequisites
## Inputs Required
## Engineering Principles
## Step-by-Step Workflow
## Code Standards
## Architecture Constraints
## Security Considerations
## Testing Requirements
## Common Mistakes
## Anti-Patterns
## Validation Checklist
(link to the relevant file(s) in checklists/, plus skill-specific items only)
## Definition of Done
(link to checklists/definition-of-done.md, plus skill-specific items only)
## Example
## Related Skills
```

Then add an entry to `skills/INDEX.yaml`:

```yaml
- id: <skill-id>
  path: skills/<category>/<skill-id>.md
  title: <Title>
  category: <category>
  triggers: [<keyword>, <keyword>, ...]
  tech: [<tech tag>, ...]
  related_skills: [<other-skill-id>, ...]
  related_agents: [<agent-id>, ...]
  related_rules: [rules/<file>.md, ...]
```

`triggers` should be phrases a developer would plausibly type in a request ("optimize
postgresql query," not just "postgresql"). This is the field adapters match against.

## Adding an agent

Create `agents/<agent-id>.md` following the structure defined in
[AGENTS.md](AGENTS.md#agent-file-format), then register it in `agents/INDEX.yaml` with the
skills, rules, and checklists it loads by default.

## Adding a rule

Rules live in the existing `rules/<category>.md` files as short, imperative, individually
enforceable bullets — not new files, unless an entire new category is justified. Each rule
should be checkable by inspection (a reviewer or an AI can look at code and say yes/no),
not aspirational ("write good code").

## Adding a workflow, checklist, prompt, or template

Follow the pattern of the existing files in that directory. Workflows are ordered steps with
explicit stage gates (no skipping from requirement to code). Checklists are flat, scannable
lists grouped by concern. Prompts are parameterized and state which skills/rules/agents they
expect to be loaded alongside them. Templates must be buildable — a `templates/projects/*`
entry must compile and its tests must pass before it's merged.

## Pull requests

- One logical addition per PR (e.g., "add postgresql performance skill," not "add 12 skills").
- Fill in the PR template, including which `INDEX.yaml` entries you added or changed.
- CI (`validate.yml`) checks: every indexed path resolves, every skill/agent file is indexed,
  no vendor names appear outside `adapters/`, and markdown/YAML lint passes.

## Reporting issues

Use the issue templates under `.github/ISSUE_TEMPLATE/` — there's a dedicated template for
proposing a new skill/agent/rule versus reporting incorrect or outdated guidance.
