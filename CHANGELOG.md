# Changelog

All notable changes to this toolkit are documented in this file.

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project uses [Semantic Versioning](https://semver.org/).

## Versioning strategy

- **MAJOR** — a rule, agent contract, or config schema key changes in a way that breaks
  a consumer project already pinned to this toolkit (e.g. a renamed `config.yaml` key,
  a removed skill/agent id still referenced by `INDEX.yaml`, a rule reversal).
- **MINOR** — new skills, agents, rules, workflows, prompts, or templates are added;
  existing ones gain content without changing their meaning or file paths.
- **PATCH** — wording fixes, typo corrections, broken-link fixes, example corrections
  that do not change guidance.

Consumer projects vendor this repo via a git submodule pinned to a tag (see
[README.md](README.md#installing-in-a-net-project)) and should bump that pin deliberately,
reading the changelog entries between their old and new tag first.

## [Unreleased]

## [1.0.0] - 2026-09-19

### Added
- Core operating system: `AGENTS.md` (AI behavior rules, selective-loading algorithm,
  anti-hallucination rules, agent roster), `RULES.md` (precedence system), `CONTRIBUTING.md`,
  `config/config.schema.json` + example, `.github/` issue/PR templates and a CI workflow
  (`tools/validate_index.py`) enforcing index consistency and vendor neutrality.
- 10 agents (`agents/*.md` + `agents/INDEX.yaml`): architect, dotnet-developer, api-engineer,
  database-engineer, security-reviewer, test-engineer, code-reviewer, performance-engineer,
  devops-engineer, documentation-engineer.
- 55 skills (`skills/*/*.md` + `skills/INDEX.yaml`) across dotnet, csharp, architecture,
  data, api, security, testing, performance, frontend, devops, documentation, and git.
- 12 rule files (`rules/*.md`).
- 11 workflows and 6 checklists (`workflows/*.md`, `checklists/*.md`).
- 15 prompt templates (`prompts/**/*.md`) across all 8 categories.
- 4 buildable, tested project templates (`templates/projects/*`: library, dotnet-api,
  clean-architecture, modular-monolith) and 9 document templates (`templates/docs/*`).
- 6 platform adapters (`adapters/*.md`: claude-code, chatgpt, cursor, copilot, windsurf,
  generic) and `tools/init-project.sh`, a bootstrap script for consumer projects.
- `tools/check_links.py`, a repo-wide markdown link checker used in the consistency review.

### Fixed
- A relative-path bug in `skills/dotnet/output-and-distributed-caching.md`'s link to
  `rules/performance.md`, found by `tools/check_links.py`.
- `tools/init-project.sh` losing its executable bit on a Windows checkout; pinned via
  `.gitattributes` (LF line endings for scripts/docs/config/C# files) and an explicit
  chmod.
