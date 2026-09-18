<!--
Template: API Documentation
For a resource/endpoint group. Prefer generating this from the OpenAPI spec where
possible; use this template for the narrative context OpenAPI metadata alone doesn't
carry (why an endpoint exists, non-obvious constraints). Verify every field/status code
against the actual implementation -- see skills/documentation/technical-writing.md.
-->

# {{RESOURCE_NAME}} API

{{One or two sentences: what this resource represents and the operations available.}}

Base path: `{{/resource-path}}`

Authentication: {{scheme, e.g. "Bearer JWT, see skills/security/authentication.md"}} |
{{none, if public}}

## Endpoints

### {{HTTP_METHOD}} {{path}}

{{What this operation does.}}

**Authorization:** {{who can call this -- role, ownership requirement, or "any authenticated user"}}

**Request:**
```json
{{example request body, if any}}
```

| Field | Type | Required | Notes |
|---|---|---|---|
| {{field}} | {{type}} | {{yes/no}} | {{validation rule}} |

**Response `{{status code}}`:**
```json
{{example response body}}
```

**Error responses:**

| Status | Condition |
|---|---|
| `400` | {{validation failure condition}} |
| `401` | {{unauthenticated}} |
| `403` | {{authenticated but not authorized -- be specific about the rule}} |
| `404` | {{resource not found}} |

<!-- Repeat the endpoint block above for each operation. -->

## Versioning

{{Current version, and the deprecation policy for prior versions if applicable -- see
skills/api/api-versioning-and-compatibility.md.}}

## Rate Limits

{{If applicable -- limit, window, and what happens when exceeded.}}
