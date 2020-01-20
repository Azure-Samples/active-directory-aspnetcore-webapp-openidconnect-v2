[![Build status](https://identitydivision.visualstudio.com/IDDP/_apis/build/status/AAD%20Samples/.NET%20client%20samples/ASP.NET%20Core%20Web%20App%20tutorial)](https://identitydivision.visualstudio.com/IDDP/_build/latest?definitionId=819)

# Tutorial Phase - Enable your Web Apps to sign-in users

## Scope of this phase

In this phase of the tutorial, you will learn, how to add sign-in users to your Web App, leveraging the Microsoft identity platform. You'll learn how to use  the ASP.NET Core OpenID Connect (OIDC) middleware itself leveraging [Microsoft Identity Model extensions for .NET](https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet/wiki) to protect your Web App.

   <img src="../ReadmeFiles/sign-in-audiences.png" width="50%"/>

   Depending on your business needs, you have the flexibility to decide what type of users [(signInAudience)](https://docs.microsoft.com/azure/active-directory/develop/supported-accounts-validation) you wish to sign-in to your application:

   1. If you are a Line of Business (LOB) developer, you'll want to [sign-in users in your organization](./1-1-MyOrg) with their work or school accounts.
   1. If you are an ISV, you'll want to [sign-in users from any organization](./1-2-AnyOrg), still  with their work or school accounts.
   1. If you are an ISV targeting both organizations and individuals, you'll want to [sign-in users with their work and school accounts or Microsoft personal accounts](./1-3-AnyOrgOrPersonal).
   1. If you target organizations (work or school accounts), you can also enable your application to sign-in users in [national and sovereign clouds](./1-4-Sovereign).
   1. If you wish to sign-in your customers, or  business partners yo your app, you might  want to look into [sign-in users with their social identities](./1-5-B2C) using Microsoft Azure AD B2C.
   1. Finally, you'll want to let users [sign-out](./1-6-SignOut) of our application, or from their browser session.

## Next chapters

- If you signed-in users with Work or School accounts, or Microsoft personal accounts, you might want to learn how to call an API, starting with [Microsoft Graph](./2-WebApp-graph-user).
- If you signed-in users with social identities, you might want to learn how to [call your own Web API directly](.\4-WebApp-your-API).
