---
services: active-directory
platforms: dotnet
author: jmprieur
level: 200
client: ASP.NET Core Web App
endpoint: Microsoft identity platform
page_type: sample
languages:
  - csharp  
products:
  - azure
  - azure-active-directory  
  - dotnet
  - office-ms-graph
description: "Change your ASP.NET Core Web app to sign-in users in any org with the Microsoft identity platform"
---
# Change your ASP.NET Core Web app to sign-in users in any org with the Microsoft identity platform

> This sample is for Azure AD, not Azure AD B2C. See [active-directory-b2c-dotnetcore-webapp](https://github.com/Azure-Samples/active-directory-b2c-dotnetcore-webapp), until we incorporate the B2C variation in the tutorial.

![Build badge](https://identitydivision.visualstudio.com/_apis/public/build/definitions/a7934fdd-dcde-4492-a406-7fad6ac00e17/514/badge)

## Scenario

![Sign in with Azure AD](ReadmeFiles/sign-in.png)

> This is the second chapter of the first phase of this ASP.NET Core Web App tutorial. You learned previously how to build an ASP.NET Core Web app signing-in users with the Microsoft identity platform in [your organization](../1-1-MyOrg). This chapter describes how to change that application to enable users to sign-in from any organization.
>
> If you are not interested in the differentials, but want to understand all the steps, read the full [Readme.md](./Readme.md)

## Enable users from any organization to sign-in to your Web app

### Changes to the application registration

Your application was registered to sign-in users in [your organization](../1-1-MyOrg) only. To enable users signing-in from any organization, you need to change the app registration in the Azure portal

1. Navigate to the Microsoft identity platform for developers [App registrations](https://go.microsoft.com/fwlink/?linkid=2083908) page.
1. Find your application in the list and select it.
1. In the **Authentication** section for your application, in the **Supported account types** section, select **Accounts in any organizational directory**.
1. Select **Save**

### Changes to the code

You will also need to change the configuration file in the code:

In the **appsettings.json** file, replace the `TenantId` value with `"organizations"`

### Remark: effective sign-in audience

The actual sign-in audience (accounts to sign-in) is the lowest set of what is specified in both the application registration portal and the `appsetttings.json` config file. In other words, you could also achieve the same result by:

- setting in the portal the **Supported account types** to **Accounts in any organizational directory and personal Microsoft accounts (e.g. Skype, Xbox, Outlook.com)** and set the `TenantId` value to `"organizations"` in the **appsettings.json** file
- setting in the portal the **Supported account types** to **Accounts in any organizational directory** and set the `TenantId` value to `"common"` in the **appsettings.json** file

## How to restrict users from specific organizations from signing-in your web app

In order to restrict users from specific organizations from signing-in to your web app, you'll need to customize your code a bit more to restrict issuers. In Azure AD, the token issuers are the Azure AD tenants which issue tokens to applications.

In the `Startup.cs` file, in the `ConfigureServices` method, after `services.AddMicrosoftWebAppAuthentication(Configuration)` add some code to validate specific issuers by overriding the `TokenValidationParameters.IssuerValidator` delegate.

```CSharp
    public void ConfigureServices(IServiceCollection services)
    {
    ...
    // Sign-in users with the Microsoft identity platform
    services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApp(options =>
            {
                Configuration.Bind("AzureAd", options);
                // Restrict users to specific belonging to specific tenants
                options.TokenValidationParameters.IssuerValidator = ValidateSpecificIssuers;
            });
   ...
```

An example of code for `ValidateSpecificIssuers` is the following:

```CSharp
    private string ValidateSpecificIssuers(string issuer, SecurityToken securityToken,
                                          TokenValidationParameters validationParameters)
    {
        var validIssuers = GetAcceptedTenantIds()
                             .Select(tid => $"https://login.microsoftonline.com/{tid}");
        if (validIssuers.Contains(issuer))
        {
            return issuer;
        }
        else
        {
            throw new SecurityTokenInvalidIssuerException("The sign-in user's account does not belong to one of the tenants that this Web App accepts users from.");
        }
    }

    private string[] GetAcceptedTenantIds()
    {
        // If you are an ISV who wants to make the Web app available only to certain customers who
        // are paying for the service, you might want to fetch this list of accepted tenant ids from
        // a database.
        // Here for simplicity we just return a hard-coded list of TenantIds.
        return new[]
        {
            "<GUID1>",
            "<GUID2>"
        };
    }
```

> If you are building a SaaS application that will be used in multiple Azure AD tenants, the please note that there are a number of steps that a SaaS developer should be aware of and is beyond the scope of this article. You are advised to go through the the multi-tenant app developer's guide [Build a multi-tenant SaaS web application that calls Microsoft Graph using Azure AD & OpenID Connect](../../2-WebApp-graph-user/2-3-Multi-Tenant/README.md) as well.

> [Consider taking a moment to share your experience with us.](https://forms.office.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbRz0h_jLR5HNJlvkZAewyoWxUNEFCQ0FSMFlPQTJURkJZMTRZWVJRNkdRMC4u)

## Next steps

- Learn how to enable [any Microsoft accounts](../1-3-AnyOrgOrPersonal) to sign-in to your application
- Learn how to enable users from [National clouds](../1-4-Sovereign) to sign-in to your application
- Learn how to enable your [Web App to call a Web API on behalf of the signed-in user](../../2-WebApp-graph-user/README-incremental-instructions.md)
