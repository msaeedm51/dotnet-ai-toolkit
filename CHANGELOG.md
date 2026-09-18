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

### Added
- Initial repository scaffold: `AGENTS.md`, `RULES.md`, `CONTRIBUTING.md`, config schema,
  directory structure for agents, skills, rules, workflows, checklists, prompts, templates,
  and adapters.
