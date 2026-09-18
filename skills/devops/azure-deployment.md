# Skill: Azure Deployment

## Purpose
Deploy a .NET service to Azure using the appropriate compute option and Azure-native secret/
configuration management, for `config.yaml` `deployment.platform: azure`.

## When to Use
Any deployment/infrastructure task targeting Azure.

## Prerequisites
Know which Azure compute option the project uses or should use (App Service, Container Apps,
AKS) — don't assume one without checking `config.yaml`/existing infrastructure.

## Inputs Required
The service being deployed, its dependencies (database, cache, messaging), and the project's
existing Azure resources/IaC (Bicep/Terraform) if any.

## Engineering Principles
- **Compute choice**: Azure App Service for straightforward web APIs with minimal
  orchestration needs; Azure Container Apps for containerized services wanting managed
  scaling/Dapr integration without full Kubernetes complexity; AKS when the team already
  needs Kubernetes-level control (multi-service orchestration, custom networking) — don't
  default to AKS for a single simple service; it's the highest-complexity option.
  for a single simple service; it's the highest-complexity option.
- **Secrets**: Azure Key Vault, referenced via managed identity — never an Azure resource's
  connection string/secret hard-coded in App Service configuration as plaintext when Key
  Vault reference syntax is available (`@Microsoft.KeyVault(...)`).
- **Managed identity** over connection-string-based service-to-service auth wherever the
  target Azure service supports it (SQL Database, Key Vault, Service Bus, Storage) —
  eliminates a credential to manage/rotate/leak.
- **Infrastructure as code** (Bicep or Terraform) for repeatable, reviewable infrastructure
  changes — avoid manual portal changes for anything beyond initial exploration, since
  they're not tracked or reproducible.
- Application Insights (or OpenTelemetry exported to Azure Monitor) for observability — see
  [`observability-and-monitoring`](../devops/observability-and-monitoring.md).

## Step-by-Step Workflow
1. Confirm the compute target and existing IaC pattern.
2. Define/update the Bicep/Terraform for the service and its dependencies.
3. Configure secrets via Key Vault + managed identity, not plaintext app settings.
4. Wire CI/CD (see [`cicd-github-actions`](../devops/cicd-github-actions.md)) to deploy to
   the target compute option.
5. Verify health checks are correctly wired to the platform's health probe mechanism (App
   Service health check path, Container Apps probes).

## Code Standards
```bicep
resource appService 'Microsoft.Web/sites@2023-01-01' = {
  name: 'orders-api'
  location: location
  identity: {
    type: 'SystemAssigned' // managed identity, no credential to manage
  }
  properties: {
    siteConfig: {
      appSettings: [
        {
          name: 'ConnectionStrings__Default'
          value: '@Microsoft.KeyVault(SecretUri=${keyVaultSecretUri})'
        }
      ]
      healthCheckPath: '/health/ready'
    }
  }
}

resource keyVaultAccessPolicy 'Microsoft.KeyVault/vaults/accessPolicies@2023-07-01' = {
  parent: keyVault
  name: 'add'
  properties: {
    accessPolicies: [
      {
        objectId: appService.identity.principalId
        tenantId: subscription().tenantId
        permissions: { secrets: ['get', 'list'] }
      }
    ]
  }
}
```

## Architecture Constraints
Infrastructure definitions live in a dedicated `infra/` (or similar) folder alongside the
solution, versioned in the same repository or a designated infra repository per the
project's convention.

## Security Considerations
Managed identity eliminates a class of credential-leak risk — prefer it over
connection-string auth wherever supported. Key Vault access policies/RBAC follow least
privilege — grant only `get`/`list` on the specific secrets needed, not broad vault access.

## Testing Requirements
Validate Bicep/Terraform changes with `what-if`/`plan` before applying; smoke-test the
deployed health endpoint after deploy as part of the CI/CD pipeline.

## Common Mistakes
- Plaintext connection strings in App Service configuration instead of Key Vault references.
- Manual portal changes that drift from the IaC definition, causing confusion on the next
  IaC apply.
- Choosing AKS for a single simple service, taking on Kubernetes operational complexity with
  no corresponding need.

## Anti-Patterns
- Mixing manually-created and IaC-managed resources in the same resource group without a
  clear record of which is which.

## Validation Checklist
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Definition of Done
See [`checklists/definition-of-done.md`](../../checklists/definition-of-done.md).

## Example
See [Code Standards](#code-standards).

## Related Skills
[`docker`](../devops/docker.md), [`cicd-github-actions`](../devops/cicd-github-actions.md),
[`observability-and-monitoring`](../devops/observability-and-monitoring.md).
