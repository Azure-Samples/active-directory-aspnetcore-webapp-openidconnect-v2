---
services: active-directory
platforms: dotnet
author: negoe
level: 100
client: ASP.NET Core Web App
endpoint: Microsoft identity platform
---
# National-cloud configuration guide

National (sovereign) clouds use separate identity and API endpoints. Reuse the
[standard sign-in sample](../1-1-MyOrg/README.md) or
[standard Microsoft Graph sample](../../2-WebApp-graph-user/2-1-Call-MSGraph/README.md)
with the appropriate configuration; this repository does not maintain a separate
national-cloud application. This is a sample-maintenance decision, not a change
to SDK support for national clouds.

## App registration and authority

1. Follow the standard sample's registration instructions in the portal for your
   target cloud, using a tenant in that cloud. Register the same redirect URIs
   required by the standard sample.
2. Set `AzureAd:Instance` to that cloud's Microsoft Entra authentication endpoint.
   For example, Azure US Government uses `https://login.microsoftonline.us/`.
3. Use the tenant GUID and client ID from that cloud's registration for
   `AzureAd:TenantId` and `AzureAd:ClientId`; update `AzureAd:Domain` where used.
   Configure credentials using the standard sample's instructions. Do not commit
   credentials or reuse an app registration from a different cloud.

Consult [Authentication in national clouds](https://learn.microsoft.com/entra/identity-platform/authentication-national-cloud)
for the current registration portals, authentication endpoints, and supported
account types. Personal Microsoft accounts are not supported in national clouds.
Keep issuer, audience, and tenant validation enabled; changing clouds does not
require bypassing these checks.

## Microsoft Graph

In the standard Graph sample's configuration:

- Set `DownstreamApi:BaseUrl` to the Microsoft Graph service root for the target
  environment, including the supported API version. For example, US Government
  L4 (GCC High) uses `https://graph.microsoft.us/v1.0`.
- Set `DownstreamApi:Scopes` to the corresponding resource's delegated scopes
  (space-separated), such as `https://graph.microsoft.us/User.Read` for that
  environment. Grant the required permissions and consent in the same cloud.
- Keep the authority, tenant, API resource, and permissions aligned. Tokens for
  one cloud cannot be used against another cloud's Graph endpoint.

Use [Microsoft Graph national cloud deployments](https://learn.microsoft.com/graph/deployments)
to choose the service root and check API availability. GCC, GCC High, and DoD do
not all use the same Graph endpoint; do not apply the L4 example to every US
Government tenant. Not every API or feature is available in every environment.

## Validation

Build and run the reused standard sample in an approved environment, then verify
sign-in and, where applicable, the Graph call against that cloud. Existing
public-cloud CI does not establish national-cloud runtime coverage. No tenant,
registration, consent, or deployment automation is introduced by this guide.
