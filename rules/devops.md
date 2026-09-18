# DevOps Rules

See `skills/devops/*` (docker, linux-hosting, cicd-github-actions, azure-deployment,
observability-and-monitoring, health-checks) for reasoning and examples.

- Never place a secret, connection string, or credential in a Dockerfile, a committed config
  file, or a CI workflow file in plaintext — source it from the platform's secret store.
- Every service that's load-balanced or orchestrated exposes a health check endpoint that
  reflects its actual ability to serve traffic (not just "process is running").
- Every deployment has a defined, tested rollback path before it ships.
- Use multi-stage Docker builds; do not ship SDK/build tooling in the runtime image.
- Pin base image versions explicitly; do not float on `latest` in a production Dockerfile.
- Run containers as a non-root user where the base image supports it.
- CI must run the test suite before a deploy stage; a failing test blocks deployment, it
  doesn't warn and continue.
- Log structured, machine-parseable output in production; avoid unstructured `Console.WriteLine`-style
  logging outside local development.
