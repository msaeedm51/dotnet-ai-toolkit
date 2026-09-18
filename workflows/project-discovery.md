# Workflow: Project Discovery

Run before modifying a project the AI assistant hasn't already built up an understanding of
in this session. Referenced from [`AGENTS.md §5`](../AGENTS.md#5-project-discovery). Do not
implement anything until this produces a Project Understanding.

## When to run

- First task in a project this session.
- The project has changed significantly since the assistant last looked (new dependencies,
  restructured folders) and current understanding may be stale.
- `.ai-dotnet/config.yaml` is missing, empty, or its `project.architecture` is empty.

## Inspect

If filesystem access is available, inspect (per [`AGENTS.md §4`](../AGENTS.md#4-tool-agnostic-design)
on optional capabilities — if it isn't, ask the user to paste the relevant files instead):

- **Solution/project files** — `.sln`, `.csproj` files: what projects exist, their
  references (reveals the dependency direction actually in use), target framework.
- **Directory structure** — does it match a named pattern (Clean Architecture layers,
  vertical-slice feature folders, a simple layered structure)? Don't assume; read it.
- **Dependency graph** — project-to-project references, and key NuGet packages (EF Core?
  Dapper? MediatR? a specific auth library?).
- **Configuration** — `appsettings*.json`, `.env.example` if present, options classes —
  what's configurable, what's the deployment target implied by it.
- **Database configuration** — connection strings (engine), `DbContext` classes, migrations
  folder, or raw SQL/Dapper usage.
- **Authentication** — `AddAuthentication()` calls, identity provider configuration,
  `[Authorize]` usage patterns.
- **API endpoints** — Minimal API route registrations or MVC controllers; existing
  conventions for DTOs, error shapes, versioning.
- **Existing patterns** — repository interfaces, CQRS handlers, domain events, result types
  already in use — these are what new code should match, not a personal default.
- **Tests** — test project(s), framework in use, existing conventions, what's actually
  covered vs. not.
- **CI/CD** — `.github/workflows/`, `azure-pipelines.yml`, or equivalent — build/test/deploy
  stages already defined.
- **Docker configuration** — `Dockerfile`, `docker-compose.yml` — deployment shape.
- **Documentation** — `README.md`, any `docs/` folder, existing ADRs.

## Produce

### Project Understanding
- Architecture (as actually implemented, which may differ from what `config.yaml` claims —
  flag the discrepancy if so).
- Major modules/projects and their responsibilities.
- Technologies actually in use (framework version, database engine, ORM, auth scheme,
  frontend if any).
- Important dependencies and why they matter to the task at hand.
- Data access approach (EF Core, Dapper, raw ADO.NET, mixed).
- Authentication/authorization approach.
- Testing strategy (frameworks, what's covered, what conventions exist).
- Deployment model (containerized? cloud target? CI/CD maturity?).

### Risks
Anything that makes the upcoming change riskier than it would otherwise be — thin test
coverage in the affected area, an already-fragile module, a legacy/read-only database
involved, missing CI gates.

### Existing Patterns To Follow
The concrete conventions (naming, DTO shape, error handling, repository pattern shape) the
new code must match.

### Proposed Change
Only after the above — a concrete plan for what will change, grounded in what was actually
found, not assumed.

## Output this feeds

This becomes the shared context for whichever agent implements the task next (see
[`workflows/multi-agent-orchestration.md`](multi-agent-orchestration.md)) — don't repeat
discovery for every subsequent agent in the same session; hand the Project Understanding
forward.
