# .NET / ASP.NET Core Rules

See [`skills/dotnet/aspnetcore-fundamentals.md`](../skills/dotnet/aspnetcore-fundamentals.md)
for reasoning and examples.

- Register services with the narrowest correct lifetime; never inject a `Scoped` service
  (including anything depending on `DbContext`) into a `Singleton`.
- `UseAuthentication()` must precede `UseAuthorization()`, and both must precede endpoint
  mapping.
- Bind configuration to strongly-typed options classes; do not read `IConfiguration` values
  directly outside the composition root/options binding.
- Validate required options on start (`ValidateOnStart()`) so misconfiguration fails at boot,
  not on first use in production.
- No secrets in `appsettings.json` committed to source control — use a secret store
  (environment variables, Key Vault, user-secrets in local dev) per environment.
- Add a health check for every service the app depends on to function (database, cache,
  message broker, critical downstream API).
- Use `IHttpClientFactory` for outbound HTTP calls; never `new HttpClient()` per call site.
- (Background work) Use `BackgroundService`/hosted services for long-running or scheduled
  work; do not fire-and-forget a `Task.Run` from within a request handler.
- No detailed exception/stack-trace output outside the `Development` environment.
