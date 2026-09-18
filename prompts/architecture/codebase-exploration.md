# Prompt: Codebase Exploration

## Purpose
Get a fast, accurate Project Understanding of an unfamiliar (or partially forgotten)
codebase before making any change — runs
[`workflows/project-discovery.md`](../../workflows/project-discovery.md) explicitly.

## When to Use
First task in a project this session, or when the project has changed significantly since
last explored.

## Loads
Workflow: [`project-discovery`](../../workflows/project-discovery.md).

## Parameters
- `{{FOCUS_AREA}}` — optional: a specific area to focus discovery on (e.g. "the order
  processing module") rather than the whole codebase. Omit for a full survey.

## Prompt Template
```
Load the .NET engineering toolkit and run workflows/project-discovery.md against this
project{{ if FOCUS_AREA }}, focused on {{FOCUS_AREA}}{{ endif }}.

Inspect: solution/project files, directory structure, dependency graph, configuration,
database configuration, authentication setup, API endpoints, existing patterns, tests,
CI/CD, Docker configuration, and existing documentation.

Produce:
1. Project Understanding -- architecture, major modules, technologies, data access
   approach, authentication, testing strategy, deployment model.
2. Risks -- anything that makes future changes here riskier than average (thin test
   coverage, fragile modules, legacy/read-only databases involved).
3. Existing Patterns to Follow -- the concrete conventions (naming, DTO shape, error
   handling, repository shape) future code here should match.

Do not propose any change yet -- this is understanding only.
```

## Expected Output
A structured Project Understanding write-up per
[`workflows/project-discovery.md`](../../workflows/project-discovery.md), usable as shared
context for whatever task comes next in the session.

## Related
[`prompts/architecture/architecture-analysis.md`](architecture-analysis.md),
[`workflows/new-feature.md`](../../workflows/new-feature.md).
