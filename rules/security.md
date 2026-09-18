# Security Rules

See [`skills/security/security-owasp.md`](../skills/security/security-owasp.md) for
reasoning and examples.

- Never hard-code secrets, connection strings, or credentials in source code.
- Never log passwords, tokens, API keys, or full PII.
- Validate all untrusted input at the boundary it enters the system.
- Apply least privilege for database roles, service accounts, and API scopes/permissions.
- Explicitly authorize every sensitive operation at its actual point of execution, not just
  at the route/controller level (guards against IDOR-class bugs).
- Use vetted authentication/cryptography libraries (`Microsoft.AspNetCore.Identity`,
  `System.Security.Cryptography`); never hand-roll password hashing, token validation, or
  crypto primitives.
- Check new or changed dependencies for known vulnerabilities before adding them.
- Enforce HTTPS and appropriate security headers in every non-development environment.
- Never deserialize untrusted input with a mechanism that allows arbitrary type
  instantiation (e.g. unrestricted polymorphic deserialization).
- Treat any secret found committed to source history as compromised — rotate it, don't just
  remove it going forward.
