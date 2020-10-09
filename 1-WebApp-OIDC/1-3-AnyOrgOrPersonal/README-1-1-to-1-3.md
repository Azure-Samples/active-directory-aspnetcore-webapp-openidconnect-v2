---
services: active-directory
platforms: dotnet
author: jmprieur
level: 100
client: ASP.NET Core Web App
endpoint: Microsoft identity platform
---
# Change your ASP.NET Core Web app to sign-in users in any org with the Microsoft identity platform

> This sample is for Azure AD, not Azure AD B2C. See [active-directory-b2c-dotnetcore-webapp](https://github.com/Azure-Samples/active-directory-b2c-dotnetcore-webapp), until we incorporate the B2C variation in the tutorial.

![Build badge](https://identitydivision.visualstudio.com/_apis/public/build/definitions/a7934fdd-dcde-4492-a406-7fad6ac00e17/514/badge)

## Scenario

![Sign in with Azure AD](ReadmeFiles/sign-in.png)

> This is the third chapter of the first phase of this ASP.NET Core Web App tutorials. You learned previously how to build an ASP.NET Core Web app that signs-in users with the Microsoft identity platform in [your organization](../1-1-MyOrg) or [any organization](../1-2-AnyOrg). This chapter describes how to change that application to enable users to sign-in from any work or school account or Microsoft personal account.
>
> If you are not interested in the differentials, but want to understand all the steps, read the full [Readme.md](./Readme.md)

## Enable users from any organization or Microsoft personal accounts to sign-in to your Web app

### Changes to the application registration

Your application was registered to sign-in users in [your organization](../1-1-MyOrg) only or from [any organization](../1-2-AnyOrg). To enable users signing-in from any organization, you need to change the app registration in the Azure portal

1. Navigate to the Microsoft identity platform for developers [App registrations](https://go.microsoft.com/fwlink/?linkid=2083908) page.
1. Find your application in the list and select it.
1. In the **Authentication** section for your application, in the **Supported account types** section, select **Accounts in any organizational directory and personal Microsoft accounts (e.g. Skype, Xbox, Outlook.com)**.
1. Select **Save**

### Changes to the code

You will also need to change the configuration file in the code:

In the **appsettings.json** file, replace the `TenantId` value with `"common"`

> [Consider taking a moment to share your experience with us.](https://forms.office.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbRz0h_jLR5HNJlvkZAewyoWxUNEFCQ0FSMFlPQTJURkJZMTRZWVJRNkdRMC4u)

## Next steps

- A recording of a Microsoft Identity Platform developer session that covered this topic of developing a multi-tenant app with Azure Active Directory is available at [Develop multi-tenant applications with Microsoft identity platform](https://www.youtube.com/watch?v=B416AxHoMJ4).

- Learn how to enable users from [National clouds](../1-4-Sovereign) to sign-in to your application
- Learn how to enable your [Web App to call a Web API on behalf of the signed-in user](../../2-WebApp-graph-user)
