# Security Review Checklist

Used by the [`security-reviewer`](../agents/security-reviewer.md) agent. Mechanisms and
code examples for each item: [`skills/security/security-owasp.md`](../skills/security/security-owasp.md).
Severity levels match [`checklists/code-review-checklist.md`](code-review-checklist.md).

- [ ] **Authentication** — uses the project's established scheme; no hand-rolled token/
      session logic; signing keys resolved dynamically, not hard-coded.
- [ ] **Authorization** — checked at the actual point of execution for every sensitive
      operation; resource-level ownership verified, not just route-level role/policy
      (IDOR check).
- [ ] **Secrets** — none hard-coded in source, config committed to source, Dockerfiles, or
      CI workflow files; sourced from a secret store per environment.
- [ ] **Injection** — all queries parameterized (SQL, and any other query language in use);
      no string-concatenated user input into a query, command, or LDAP filter.
- [ ] **SSRF** — any server-side outbound call built from user-supplied input validates
      against an allow-list of hosts/schemes.
- [ ] **CSRF** — state-changing endpoints reachable from a browser session protect against
      CSRF (anti-forgery tokens or `SameSite` cookie policy, as appropriate to the auth
      scheme).
- [ ] **XSS** — output encoding not defeated (`Html.Raw`/`[AllowHtml]` not applied to
      user-supplied content); `Content-Security-Policy` set where the API serves HTML.
- [ ] **Insecure deserialization** — no unrestricted polymorphic deserialization of
      untrusted input; no legacy binary deserialization of untrusted data.
- [ ] **File upload** — content type and size validated server-side; stored outside the web
      root or in blob storage; filenames randomized; uploaded content never executed.
- [ ] **Path traversal** — any file path built from user input is validated/sanitized
      against traversal sequences.
- [ ] **Sensitive data exposure** — no password, token, API key, or full PII in logs,
      error messages, or default `ToString()`/serialization output.
- [ ] **Logging/monitoring** — authentication failures, authorization denials, and
      input-validation failures are logged at a monitored level, without the sensitive
      payload itself.
- [ ] **Dependency vulnerabilities** — new/changed packages checked against known CVEs
      (`dotnet list package --vulnerable` or the project's SCA tool).
- [ ] **Security headers** — HTTPS enforced (`UseHsts`, `UseHttpsRedirection`); CORS
      allow-lists specific origins, never `AllowAnyOrigin()` combined with credentials.

## Reporting

Each finding: severity (BLOCKER/HIGH/MEDIUM/LOW/INFO), location, the concrete problem, the
exploit scenario (what input/actor triggers it), and a specific recommendation compatible
with the project's existing auth scheme.
