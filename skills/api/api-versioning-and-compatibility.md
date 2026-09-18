# Skill: API Versioning and Backward Compatibility

## Purpose
Change an API's contract without breaking existing callers, and version deliberately when a
breaking change is genuinely unavoidable.

## When to Use
Any change to an existing endpoint's request/response shape, status codes, or behavior; any
decision about whether a new capability needs a new API version.

## Prerequisites
[`rest-api-design`](../api/rest-api-design.md).

## Inputs Required
The change being made to an existing contract, and who consumes it (internal-only, easier to
coordinate; external/third-party, much harder).

## Engineering Principles
- **Additive changes are not breaking**: adding a new optional field to a response, adding a
  new endpoint, adding a new optional query parameter — existing callers are unaffected.
- **Breaking changes** include: removing/renaming a field, changing a field's type or
  meaning, changing a status code for an existing scenario, tightening validation that
  previously accepted a value, changing error response shape.
- Prefer evolving a contract additively wherever possible over introducing a new version.
- When a breaking change is unavoidable, version explicitly — URL segment (`/v2/orders`),
  header, or media-type versioning, matching whatever scheme the project already uses (don't
  introduce a second scheme).
- Support the previous version for a defined deprecation window, communicated to consumers,
  rather than removing it the moment the new version ships.
- Internal-only APIs (all consumers deployed together, no external contract) have more
  latitude — coordinate the breaking change with the deployment, but a formal version bump is
  often unnecessary; state this reasoning explicitly rather than assuming.

## Step-by-Step Workflow
1. Classify the change: additive or breaking.
2. If additive: ship it directly.
3. If breaking and consumers are external/uncoordinated: introduce a new version following
   the project's existing versioning scheme; keep the old version functional through a
   stated deprecation window.
4. If breaking and consumers are internal/coordinated (e.g. a monolith's own frontend
   deployed together): coordinate the deploy explicitly; still flag it as a breaking change
   in the PR description.
5. Document the change and, for external APIs, update any published changelog/deprecation
   notice.

## Code Standards
```csharp
// URL-segment versioning with Asp.Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

var v1 = app.NewVersionedApi("Orders").MapGroup("/v{version:apiVersion}/orders").HasApiVersion(1.0);
var v2 = app.NewVersionedApi("Orders").MapGroup("/v{version:apiVersion}/orders").HasApiVersion(2.0);

v1.MapGet("/{id:guid}", GetOrderV1); // old response shape, still supported during deprecation window
v2.MapGet("/{id:guid}", GetOrderV2); // new response shape
```

## Architecture Constraints
Versioned endpoints typically share the same Application-layer handlers where the underlying
operation hasn't changed — only the API-boundary DTO mapping differs between versions;
don't duplicate business logic per version.

## Security Considerations
A deprecated version still enforces the same authorization/security rules as the current one
for as long as it's live — deprecation is not a reason to relax security review on the old
path.

## Testing Requirements
Both the current and any still-supported deprecated version need their own contract tests —
a breaking change caught only in the new version's tests could still silently break the old
one if handler code is shared incorrectly.

## Common Mistakes
- Silently changing a field's type/meaning without a version bump, breaking existing callers
  without warning.
- Removing an old API version immediately instead of honoring a stated deprecation window.
- Treating every schema tweak as requiring a new major version, causing unnecessary version
  churn for genuinely additive changes.

## Anti-Patterns
- A version number in the URL that never actually changes behavior between versions —
  version numbers used as decoration rather than a real compatibility boundary.

## Validation Checklist
See [`checklists/api-design-checklist.md`](../../checklists/api-design-checklist.md).

## Definition of Done
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Example
See [Code Standards](#code-standards).

## Related Skills
[`rest-api-design`](../api/rest-api-design.md).
