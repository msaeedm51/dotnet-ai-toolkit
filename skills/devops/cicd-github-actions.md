# Skill: CI/CD with GitHub Actions

## Purpose
Build a pipeline that builds, tests, and deploys a .NET service correctly — tests gate
deployment, secrets come from GitHub's secret store, and the pipeline matches the project's
actual deployment target.

## When to Use
Setting up or changing a GitHub Actions workflow for a .NET project.

## Prerequisites
[`docker`](../devops/docker.md) if containerized; know the deployment target
(`config.yaml` `deployment.platform`).

## Inputs Required
The build/test/deploy stages needed, and the target environment(s) (e.g. staging then
production, or direct to production with manual approval).

## Engineering Principles
- **Tests gate deployment** — the deploy stage only runs if build and test stages succeed;
  never deploy on a red pipeline.
- **Secrets** come from GitHub Actions secrets (repository or environment-scoped), never
  hard-coded in the workflow file.
- **Environment-scoped secrets and required reviewers** for production deployments — use
  GitHub Environments with protection rules (required approval) for anything deploying to
  production, so a deploy doesn't happen without a human confirming it, unless the project
  has explicitly adopted full continuous deployment.
- Cache NuGet packages (`actions/cache` keyed on `packages.lock.json`/`.csproj` hashes) to
  keep build times reasonable.
- Run the same test suite tiers as local development (unit, then integration with
  Testcontainers-style ephemeral dependencies) — a CI-only test gap defeats the purpose of
  CI.

## Step-by-Step Workflow
1. Define build stage: restore, build, run unit tests.
2. Define integration test stage (may need service containers for a real database — GitHub
   Actions supports `services:` for this).
3. Define a Docker build/push stage if containerized (see
   [`docker`](../devops/docker.md)), tagging with the commit SHA or a semantic version.
4. Define deploy stage(s), gated on the previous stages succeeding, using GitHub
   Environments for anything deploying to production.
5. Verify secrets are referenced via `${{ secrets.X }}`, never inlined.

## Code Standards
```yaml
name: build-test-deploy

on:
  push:
    branches: [main]
  pull_request:

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    services:
      postgres:
        image: postgres:16-alpine
        env:
          POSTGRES_PASSWORD: postgres
        ports: ["5432:5432"]
        options: >-
          --health-cmd pg_isready --health-interval 10s --health-timeout 5s --health-retries 5
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: "8.0.x"
      - name: Cache NuGet packages
        uses: actions/cache@v4
        with:
          path: ~/.nuget/packages
          key: ${{ runner.os }}-nuget-${{ hashFiles('**/packages.lock.json') }}
      - run: dotnet restore
      - run: dotnet build --no-restore -c Release
      - run: dotnet test --no-build -c Release
        env:
          ConnectionStrings__Default: "Host=localhost;Database=test;Username=postgres;Password=postgres"

  deploy:
    needs: build-and-test
    if: github.ref == 'refs/heads/main'
    runs-on: ubuntu-latest
    environment: production # requires configured protection rules for manual approval
    steps:
      - uses: actions/checkout@v4
      - name: Deploy
        run: echo "Deploy using ${{ secrets.DEPLOY_TOKEN }}" # never inline the actual secret value
```

## Architecture Constraints
The workflow's stages mirror the project's actual test tiers
(see [`unit-testing`](../testing/unit-testing.md)/[`integration-testing`](../testing/integration-testing.md))
— don't skip integration tests in CI because they're "slower."

## Security Considerations
Never echo a secret's actual value into logs. Use least-privilege tokens (fine-grained PATs
or GitHub's `GITHUB_TOKEN` with minimal `permissions:`) rather than broad admin credentials.
Pin third-party actions to a commit SHA (not just a tag) for supply-chain safety where the
project's risk tolerance warrants it.

## Testing Requirements
The pipeline itself should be verified to actually fail on a failing test (a common
misconfiguration is a test step that doesn't propagate its exit code) — confirm this when
setting one up.

## Common Mistakes
- A deploy stage with no dependency on the test stage succeeding first.
- Secrets inlined directly in the YAML instead of referenced from GitHub Secrets.
- No caching, causing every run to re-download all NuGet packages from scratch.

## Anti-Patterns
- A single monolithic job doing build+test+deploy with no stage separation, making it hard to
  see which part failed or to gate deployment on test success cleanly.

## Validation Checklist
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Definition of Done
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Example
See [Code Standards](#code-standards).

## Related Skills
[`docker`](../devops/docker.md), [`azure-deployment`](../devops/azure-deployment.md),
[`integration-testing`](../testing/integration-testing.md).
