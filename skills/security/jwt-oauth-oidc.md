# Skill: JWT, OAuth2, and OIDC Flow Selection

## Purpose
Choose the correct OAuth2/OIDC flow for the client type involved, and understand what a JWT
access token does and doesn't guarantee.

## When to Use
Designing a new authentication integration, or evaluating whether an existing flow is
appropriate for a new client type (mobile app, SPA, server-to-server, third-party
integration).

## Prerequisites
[`authentication`](../security/authentication.md).

## Inputs Required
The client type (browser SPA, server-rendered app, mobile app, machine/service) and whether
a human is involved in the auth (interactive login) or not (service-to-service).

## Engineering Principles
- **OIDC** (built on OAuth2) is for authentication (who is this) and issues an ID token
  alongside an access token; **OAuth2** alone is for authorization (what can this token do)
  and issues only an access token — know which one the requirement actually needs.
- **Authorization Code + PKCE** is the correct flow for any client that can't securely hold a
  client secret — SPAs and mobile apps — regardless of older guidance suggesting Implicit
  flow for SPAs (deprecated; Implicit flow exposes tokens in the URL fragment and lacks
  refresh capability).
- **Client Credentials** flow is for service-to-service/machine authentication with no human
  involved — the service authenticates with its own client id/secret.
- **Refresh tokens** let a client obtain new access tokens without re-prompting login; store
  them more protectively than access tokens (rotation on use is recommended) since they're
  longer-lived.
- A JWT access token's claims are only as trustworthy as its signature validation — an API
  receiving a JWT must validate signature, issuer, audience, and expiry every time
  (see [`authentication`](../security/authentication.md)); it should not trust claims from a
  token it hasn't independently validated.

## Step-by-Step Workflow
1. Identify the client type and whether a human is authenticating.
2. Select the flow: Authorization Code + PKCE (SPA/mobile/any public client),
   Client Credentials (service-to-service), Authorization Code (confidential
   server-rendered clients that can hold a secret).
3. Configure the identity provider's client registration for that flow (redirect URIs, PKCE
   requirement, allowed grant types).
4. On the API side, validate the resulting access token per
   [`authentication`](../security/authentication.md) regardless of which flow produced it —
   the API's validation logic doesn't change based on how the client obtained the token.
5. For refresh tokens: implement rotation (each use issues a new refresh token and
   invalidates the old one) if the provider supports it, to limit the damage of a leaked
   refresh token.

## Code Standards
```csharp
// SPA/mobile client-side (conceptual, using Authorization Code + PKCE)
// -- the API side validation is identical to skills/security/authentication.md's example
// regardless of which flow issued the token.

// Client Credentials for a background service calling another API
var client = new HttpClient();
var tokenResponse = await client.PostAsync(tokenEndpoint, new FormUrlEncodedContent(new Dictionary<string, string>
{
    ["grant_type"] = "client_credentials",
    ["client_id"] = clientId,
    ["client_secret"] = clientSecret, // from a secret store, never hard-coded
    ["scope"] = "orders.read",
}));
```

## Architecture Constraints
Flow selection is a client-side/identity-provider configuration concern; the API's token
validation logic (see [`authentication`](../security/authentication.md)) stays uniform
regardless of which flow produced the token.

## Security Considerations
- Never use Implicit flow for new integrations.
- Client secrets for confidential clients (server-rendered apps, Client Credentials
  services) live in a secret store, never in client-side code or source control.
- Validate `aud` (audience) strictly — a token issued for one API should not be accepted by
  another API sharing the same issuer, unless deliberately designed as a shared audience.

## Testing Requirements
Test token validation edge cases: wrong audience, wrong issuer, expired token, tampered
signature — each should be rejected with `401` (see
[`authentication`](../security/authentication.md)'s testing guidance).

## Common Mistakes
- Using Implicit flow for a new SPA because of outdated tutorials/guidance.
- Storing a refresh token in `localStorage` on a SPA (XSS-exfiltrable) instead of an
  `HttpOnly` cookie or secure platform storage on mobile.
- Skipping audience validation, allowing a token meant for a different API to be accepted.

## Anti-Patterns
- Rolling a custom OAuth2/OIDC client implementation instead of a maintained library
  (`Microsoft.Identity.Web`, `IdentityModel`, or the identity provider's official SDK).

## Validation Checklist
See [`checklists/security-review-checklist.md`](../../checklists/security-review-checklist.md).

## Definition of Done
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Example
See [Code Standards](#code-standards).

## Related Skills
[`authentication`](../security/authentication.md), [`authorization`](../security/authorization.md).
