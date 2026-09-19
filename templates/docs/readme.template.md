<!--
Template: Project README
Fill in every {{PLACEHOLDER}}. Delete a section only if it genuinely doesn't apply --
don't leave a section with no content and no explanation.
See skills/documentation/technical-writing.md for the writing discipline this follows:
verify every command/path before writing it, document the why and the non-obvious, not
what well-named code already makes clear.
-->

# {{PROJECT_NAME}}

{{ONE_TO_TWO_SENTENCE_DESCRIPTION}}

## What this is

{{2-4 sentences: what the service/library does, who uses it, and how it fits into the
broader system if it's one of several services.}}

## Tech stack

- **Runtime:** {{e.g. .NET 8, ASP.NET Core}}
- **Database:** {{engine + ORM, or "none"}}
- **Frontend:** {{framework, or "none -- API only"}}
- **Deployment:** {{platform}}

## Getting started

### Prerequisites

- {{SDK version, e.g. ".NET 8 SDK"}}
- {{database/dependency, e.g. "Docker (for local PostgreSQL)"}}

### Setup

```bash
{{VERIFIED setup commands -- clone, restore, migrate, seed}}
```

### Run locally

```bash
{{VERIFIED run command}}
```

### Run tests

```bash
{{VERIFIED test command}}
```

## Project structure

```text
{{high-level directory tree with one-line descriptions -- only what a newcomer actually
needs to orient themselves, not every file}}
```

## Configuration

{{Key configuration values and where they come from -- appsettings.json sections,
required environment variables, secrets. Never include actual secret values here.}}

## Architecture

{{1-3 sentences + a link to templates/docs/architecture.template.md's filled-in version if
one exists, rather than duplicating it here.}}

## Deployment

{{Link to templates/docs/deployment-guide.template.md's filled-in version rather than
duplicating it here.}}

## Contributing

{{Link to a CONTRIBUTING.md if one exists, or state the PR/review process briefly.}}
