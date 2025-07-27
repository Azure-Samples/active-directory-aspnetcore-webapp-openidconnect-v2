---
services: active-directory
platforms: dotnet
author: negoe
level: 100
client: ASP.NET Core Web App
endpoint: Microsoft identity platform
---
# Build an ASP.NET Core Web app signing-in users in sovereign clouds with the Microsoft identity platform

[![Build status](https://identitydivision.visualstudio.com/IDDP/_apis/build/status/aad%20Samples/.NET%20client%20samples/ASP.NET%20Core%20Web%20App%20tutorial)](https://identitydivision.visualstudio.com/IDDP/_build/latest?definitionId=819)

## Scenario

This sample shows how to build a .NET Core MVC Web app that uses OpenID Connect to sign in users. Users can only sign in with their 'work and school' accounts in their organization **belonging to national or sovereign clouds**. This sample use US Government cloud scenario. It leverages the ASP.NET Core OpenID Connect middleware.


![Sign in with Microsoft Entra ID](ReadmeFiles/sign-in.png)

National clouds (aka Sovereign clouds) are physically isolated instances of Azure. These regions of Azure are designed to make sure that data residency, sovereignty, and compliance requirements are honored within geographical boundaries.

Note that enabling your application for sovereign clouds requires you to:

- register your application in a specific portal, depending on the cloud
- use a specific authority, depending on the cloud in the config file for your application
- in case you want to call the graph, this requires a specific Graph endpoint URL, depending on the cloud.

More details in [Authentication in National Clouds](https://docs.microsoft.com/en-us/azure/active-directory/develop/authentication-national-cloud)

### Changes to the application registration

There are no changes to the application registration. But please note that there is a different Azure Portal address for each cloud.

### Changes to the application code

Update the appsettings.config with the Instance of the new cloud

```diff
{
  "AzureAd": {
+   "Instance": "https://login.microsoftonline.us/",
    "Domain": "[Enter the domain of your tenant, e.g. contoso.onmicrosoft.us]",
    "TenantId": "[Enter 'organizations' or the Tenant Id (Obtained from the Azure portal. Select 'Endpoints' from the 'App registrations' blade and use the GUID in any of the URLs), e.g. da41245a5-11b3-996c-00a8-4d99re19f292]",
    "ClientId": "[Enter the Client Id (Application ID obtained from the Azure portal), e.g. ba74781c2-53c2-442a-97c2-3d60re42f403]",
    "CallbackPath": "/signin-oidc",
    "SignedOutCallbackPath": "/signout-callback-oidc",
    "ClientCredentials": [ 
      {
        "SourceType": "ClientSecret",  // Secrets are weak credentials. Use certificates or federated credentials instead. See https://aka.ms/idweb/client-credentials
        "ClientSecret": "[Enter you secret here]"
      }
    ]
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*"
}

```
