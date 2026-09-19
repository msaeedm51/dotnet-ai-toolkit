# Skill: Docker

## Purpose
Build correct, small, secure Docker images for a .NET application — multi-stage builds,
non-root execution, pinned base images.

## When to Use
Containerizing a .NET service, or reviewing/changing an existing Dockerfile.

## Prerequisites
`config.yaml` `deployment.containerized: true`.

## Inputs Required
The project's target framework and whether it needs any native dependencies beyond the base
.NET runtime image.

## Engineering Principles
- **Multi-stage builds**: an SDK-based build stage compiles/publishes the app; a separate,
  smaller runtime-only base image (`mcr.microsoft.com/dotnet/aspnet`) runs it — never ship
  the SDK image (much larger, contains build tooling) to production.
- **Pin base image versions** explicitly (`mcr.microsoft.com/dotnet/aspnet:8.0` or a specific
  digest) — never float on `latest` in a production Dockerfile, which makes builds
  non-reproducible and can silently introduce breaking changes.
- **Run as non-root** — the official .NET images provide a non-root `app` user; use it unless
  there's a specific, stated reason not to.
- **Layer caching**: copy and restore project/dependency files before copying full source, so
  dependency restoration is cached across builds when only source (not dependencies)
  changed.
- **`.dockerignore`** excludes `bin/`, `obj/`, `.git`, and anything not needed in the build
  context — keeps builds fast and avoids accidentally baking local artifacts/secrets into the
  image.

## Step-by-Step Workflow
1. Write/verify a multi-stage Dockerfile: SDK stage builds and publishes; runtime stage
   copies only the published output.
2. Pin explicit version tags for both stages.
3. Set `USER app` (or the image's non-root user) in the runtime stage.
4. Verify `.dockerignore` excludes build artifacts, `.git`, and any local secret files.
5. Build and run locally; verify the container starts, serves traffic, and health checks
   respond correctly (see [`health-checks`](../dotnet/health-checks.md)).

## Code Standards
```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Orders.Api/Orders.Api.csproj", "src/Orders.Api/"]
COPY ["src/Orders.Application/Orders.Application.csproj", "src/Orders.Application/"]
RUN dotnet restore "src/Orders.Api/Orders.Api.csproj"

COPY . .
RUN dotnet publish "src/Orders.Api/Orders.Api.csproj" -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

USER app
EXPOSE 8080
ENTRYPOINT ["dotnet", "Orders.Api.dll"]
```
```text
# .dockerignore
**/bin/
**/obj/
.git/
**/*.user
**/appsettings.Development.json
```

## Architecture Constraints
The Dockerfile lives alongside the project it builds, or at the solution root if it builds
multiple projects — following the project's existing convention.

## Security Considerations
No secrets baked into the image (build args containing secrets persist in image layers even
if removed in a later layer) — inject secrets at runtime via environment variables or a
mounted secret store, never `COPY`'d or `ARG`'d into the image. Scan images for known
vulnerabilities (`docker scout`/Trivy) as part of CI.

## Testing Requirements
Verify the built image starts and passes its health check in CI before it's considered
deployable — see [`cicd-github-actions`](../devops/cicd-github-actions.md).

## Common Mistakes
- Shipping the SDK image to production instead of a slim runtime-only image.
- Running as root with no stated reason.
- `COPY . .` before restoring dependencies, invalidating the dependency-restore cache layer
  on every source change.
- Floating on `latest` for the base image.

## Anti-Patterns
- A single-stage Dockerfile that includes build tooling in the final production image.

## Validation Checklist
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Definition of Done
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Example
See [Code Standards](#code-standards).

## Related Skills
[`linux-hosting`](../devops/linux-hosting.md), [`cicd-github-actions`](../devops/cicd-github-actions.md),
[`health-checks`](../dotnet/health-checks.md).
