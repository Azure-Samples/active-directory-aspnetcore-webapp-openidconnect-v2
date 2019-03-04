---
services: active-directory
platforms: dotnet
author: jmprieur
level: 200
client: ASP.NET Core .Web App
service: Microsoft Graph, Azure Storage, ASP.NET Core Web API
endpoint: AAD v2.0
---
# Tutorial Phase - Enable your Web Apps to sign-in users

## About this phase

### Scope of this tutorial

In this phase of the tutorial, you will learn, how to add sign-in users to your Web App, leveraging the Microsoft identity platform for developers (fomerly Azure AD v2.0). You'll learn how to use  the ASP.NET Core OpenID Connect (OIDC) middleware itself leveraging [Microsoft Identity Model extensions for .NET](https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet/wiki) to protect your Web App.

   <img src="../ReadmeFiles/sign-in-audiences.png" width="50%"/>

   Depending on your business needs, you have the flexibility to decide which audience to sign-in to your application:
   1. If you are a Line of Business (LOB) developer, you'll want to [sign-in users in your organization](./1.1.%20in%20my%20org).
   1. If you are an ISV, you'll want to [sign-in users in any organization](./1.2.%20in%20any%20org).
   1. If you are an ISV targetting both organizations and individuals, you'll want to [sign-in users with their work and school accounts or Microsoft personal accounts](./1.3.%20with%20work%20and%20school%20or%20personal%20accounts
).
   1. If you target organizations, you can also ensure that your application signs-in users in [national and sovereign clouds](./1.%20WebApp%20signs-in%20users%20with%20Microsoft%20Identity%20(OIDC)/1.4.%20in%20national%20and%20sovereign%20clouds
).
   1. You might also want to [sign-in users with their social identities](./1.5.%20with%20social%20identities%20(B2C)
) using Microsoft Azure AD B2C.
   1. Finally, you'll want to let users [sign-out](./1.6.%20and%20lets%20them%20sign-out) of our application
