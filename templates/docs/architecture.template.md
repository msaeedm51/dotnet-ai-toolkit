<!--
Template: Architecture Document
A living description of the system's actual structure -- verify every claim against the
real code before writing it (skills/documentation/technical-writing.md). This describes
CURRENT state; decisions and their reasoning belong in ADRs
(templates/docs/adr.template.md), linked from here, not duplicated.
-->

# {{PROJECT_NAME}} Architecture

## Overview

{{2-4 sentences: the architectural style in use (e.g. Clean Architecture, modular
monolith, microservices) and why, in one line, linking to the ADR that decided it.}}

## System Context

{{What this system talks to: upstream/downstream services, external APIs, message
brokers. A simple diagram (even ASCII) is often clearer than prose here.}}

## Module / Layer Structure

```
{{Actual directory/project structure with one-line responsibility per module, verified
against the real solution, not aspirational.}}
```

## Dependency Rules

{{The enforced dependency direction, and how it's enforced (project references? automated
architecture tests? link to them). See rules/architecture.md and, if in use,
skills/testing/architecture-testing.md.}}

## Data Flow

{{How a typical request/operation moves through the system -- entry point, key
transformations, persistence, response. Focus on what's non-obvious from reading the code
top to bottom.}}

## Key Technical Decisions

{{A short list of the most significant decisions with links to their ADRs -- don't
restate the ADRs' content here.}}

- {{Decision}} -- see [ADR-{{NNNN}}](link)
- {{Decision}} -- see [ADR-{{NNNN}}](link)

## Data Storage

{{Database engine(s), ORM, and the role of each database
(primary/legacy/read-only/reporting/external per config.yaml) if there's more than one.}}

## Cross-Cutting Concerns

{{Authentication/authorization approach, logging/observability approach, caching
approach -- one or two lines each, linking to the relevant skill for depth rather than
re-explaining it.}}

## Known Constraints and Tradeoffs

{{Honest statement of what the current architecture does NOT handle well, and why that's
an accepted tradeoff for now rather than an oversight.}}
