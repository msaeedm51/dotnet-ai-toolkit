# Workflow: Authentication Feature

Adding authentication to a project, or changing how it works, is structurally significant
and security-sensitive — this workflow gates it more heavily than
[`workflows/new-feature.md`](new-feature.md).

1. **Clarify the requirement precisely** — which identity provider (Azure AD, Auth0,
   IdentityServer, custom), which flow (JWT bearer for API/SPA clients, cookie for
   server-rendered, OAuth2 Authorization Code + PKCE for third-party client access), and
   what's already in place (`config.yaml` `backend.authentication`) vs. what's new. This is
   exactly the kind of requirement where ambiguity changes the architecture — ask rather
   than guess.
2. **Architect involvement** — [`architect`](../agents/architect.md) evaluates whether this
   requires a new identity provider integration, a new trust boundary, or fits within the
   existing setup; produces an ADR if it's a new provider/flow (see
   [`templates/docs/adr.template.md`](../templates/docs/adr.template.md)).
3. **Design the token/session flow** — see
   [`skills/security/authentication.md`](../skills/security/authentication.md) and
   [`skills/security/jwt-oauth-oidc.md`](../skills/security/jwt-oauth-oidc.md) for flow
   selection.
4. **Implement** — [`dotnet-developer`](../agents/dotnet-developer.md)/
   [`api-engineer`](../agents/api-engineer.md), following
   [`skills/security/authentication.md`](../skills/security/authentication.md) and
   [`skills/security/authorization.md`](../skills/security/authorization.md).
5. **Security review is mandatory, not optional**, regardless of `config.yaml`
   `rules.require_security_review` — [`security-reviewer`](../agents/security-reviewer.md)
   applies [`checklists/security-review-checklist.md`](../checklists/security-review-checklist.md)
   in full before this is considered done.
6. **Test the boundaries explicitly**: valid token succeeds; missing token → `401`; expired/
   tampered token → `401`; valid token but insufficient permission → `403`; resource-level
   ownership enforced, not just route-level policy (IDOR check) — see
   [`skills/testing/integration-testing.md`](../skills/testing/integration-testing.md).
7. **Document** the flow (which provider, which tokens, expiry/rotation policy) so a future
   developer doesn't have to reverse-engineer it from code.

## Exit criteria
[`checklists/security-review-checklist.md`](../checklists/security-review-checklist.md) in
full, plus [`checklists/definition-of-done.md`](../checklists/definition-of-done.md).
