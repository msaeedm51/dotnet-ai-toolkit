# Skill: Linux Hosting

## Purpose
Run a .NET service correctly on Linux — reverse proxy, HTTPS termination, process
management — when not deployed via a fully managed platform.

## When to Use
`config.yaml` `deployment.platform: linux-docker` (or a self-managed Linux VM, not a fully
managed PaaS).

## Prerequisites
[`docker`](../devops/docker.md) if containerized; systemd familiarity if running the .NET
process directly on the host.

## Inputs Required
The deployment target's actual setup — reverse proxy in use (Nginx is the default
assumption), domain/TLS certificate source, process manager.

## Engineering Principles
- **Nginx as reverse proxy** in front of Kestrel — Kestrel is a capable web server but
  running behind a reverse proxy is the standard, recommended pattern: it handles TLS
  termination, static content, and buffering more efficiently at the edge, and lets you run
  multiple apps on one host cleanly.
- **`X-Forwarded-*` headers**: configure ASP.NET Core's `ForwardedHeadersMiddleware`
  (`UseForwardedHeaders`) so the app sees the real client IP/scheme through the proxy —
  without it, request logging, rate limiting by IP, and HTTPS-redirect logic all see the
  proxy's address instead of the real client's.
- **systemd** manages the process directly-hosted (non-containerized) case: automatic
  restart on failure, log integration via journald, and controlled startup ordering.
- **HTTPS termination** typically happens at Nginx (with a cert from Let's Encrypt/a managed
  CA), with Kestrel listening on plain HTTP internally — or Kestrel terminates TLS itself if
  there's no reverse proxy; don't do both redundantly without reason.

## Step-by-Step Workflow
1. Configure `UseForwardedHeaders` in the app if it sits behind a reverse proxy.
2. Configure Nginx as a reverse proxy: forward to Kestrel's internal port, set
   `X-Forwarded-For`/`X-Forwarded-Proto` headers, terminate TLS.
3. If not containerized: create a systemd unit that runs the published app, restarts on
   failure, and starts on boot.
4. Verify the app correctly sees the real client IP and scheme through the proxy (test
   against `HttpContext.Connection.RemoteIpAddress` and `Request.Scheme`).

## Code Standards
```csharp
// Program.cs -- trust the reverse proxy's forwarded headers
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // In production, restrict KnownProxies/KnownNetworks to the actual proxy's address
    // rather than trusting any forwarded header from any source.
});
app.UseForwardedHeaders();
```
```nginx
server {
    listen 443 ssl;
    server_name api.example.com;

    ssl_certificate     /etc/letsencrypt/live/api.example.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/api.example.com/privkey.pem;

    location / {
        proxy_pass http://127.0.0.1:5000;
        proxy_set_header Host $host;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```
```ini
# /etc/systemd/system/orders-api.service
[Unit]
Description=Orders API
After=network.target

[Service]
WorkingDirectory=/var/www/orders-api
ExecStart=/usr/bin/dotnet /var/www/orders-api/Orders.Api.dll
Restart=always
RestartSec=5
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production

[Install]
WantedBy=multi-user.target
```

## Architecture Constraints
None specific — this is deployment-layer configuration, separate from the application's own
architecture.

## Security Considerations
Restrict `ForwardedHeadersOptions.KnownProxies`/`KnownNetworks` to the actual reverse proxy's
address in production — trusting forwarded headers from any source lets a client spoof its
apparent IP, defeating IP-based rate limiting/logging. Run the systemd service as a
non-root user.

## Testing Requirements
Verify (in a staging environment matching the real topology) that `RemoteIpAddress`/
`Request.Scheme` reflect the real client through the full Nginx → Kestrel path, not just in
local development without the proxy in front.

## Common Mistakes
- Missing `UseForwardedHeaders`, so all requests appear to come from the reverse proxy's
  local address, breaking IP-based rate limiting/logging.
- Trusting forwarded headers from any source instead of restricting to the known proxy.
- Running the systemd service as root with no stated reason.

## Anti-Patterns
- Terminating TLS at both Nginx and Kestrel redundantly without a reason, adding operational
  complexity (two certificates to manage) for no benefit.

## Validation Checklist
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Definition of Done
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Example
See [Code Standards](#code-standards).

## Related Skills
[`docker`](../devops/docker.md), [`observability-and-monitoring`](../devops/observability-and-monitoring.md).
