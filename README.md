---
languages:
- csharp
- powershell
- html
page_type: sample
description: "Learn how to add sign-in users to your web app, and how to call web APIs, either from Microsoft or your own."
products:
- azure
- azure-active-directory
- dotnet
- azure-storage
- aspnet
- office-ms-graph
urlFragment: enable-webapp-signin
---

# Tutorial - Enable your Web Apps to sign-in users and call APIs with the Microsoft identity platform for developers

[![Build status](https://identitydivision.visualstudio.com/IDDP/_apis/build/status/AAD%20Samples/.NET%20client%20samples/ASP.NET%20Core%20Web%20App%20tutorial)](https://identitydivision.visualstudio.com/IDDP/_build/latest?definitionId=819)

## About this tutorial

### Scope of this tutorial

In this tutorial, you will learn, incrementally, how to add sign-in users to your Web App, and how to call Web APIs, either from Microsoft or your own. Finally, you'll learn best practices and how to deploy your app to Azure

[![Tutorial Overview](./ReadmeFiles/aspnetcore-webapp-tutorial.svg)](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/raw/master/ReadmeFiles/aspnetcore-webapp-tutorial-alt.svg?sanitize=true)

> Note
>
> We recommend that you right click on the picture above and open it in a new tab, or a new window. You'll see a clickable image:
>
> - clicking on a metro/railway station will get you directly to the README.md for the corresponding part of the tutorial (some are still in progress)
> - clicking on some of the connectors between stations will get you to an incremental README.md showing how to get from one part of the tutorial to the next (that's for instance the case for the Sign-in ... stations)

### Details of the phases

1. The first phase is to [add sign-in to your Web App](1-WebApp-OIDC) leveraging the Microsoft identity platform for developers (fomerly Azure AD v2.0). You'll learn how to use  the ASP.NET Core OpenID Connect (OIDC) middleware itself leveraging [Microsoft Identity Model extensions for .NET](https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet/wiki) to protect your Web App.

   ![Web apps signs-in users](ReadmeFiles/Web-app-signs-in-users.svg)

   Depending on your business needs, you have the flexibility to decide which audience to sign-in to your application:
   1. If you are a Line of Business (LOB) developer, you'll want to [sign-in users in your organization](./1-WebApp-OIDC/1-1-MyOrg) with their work or school accounts.
   1. If you are an ISV, you'll want to [sign-in users in any organization](./1-WebApp-OIDC/1-2-AnyOrg), still  with their work or school accounts.
   1. If you are an ISV targetting both organizations and individuals, you'll want to [sign-in users with their work and school accounts or Microsoft personal accounts](./1-WebApp-OIDC/1-3-AnyOrgOrPersonal).
   1. LOB developer or ISV, if you target organizations (work or school accounts), you can also enable your application to sign-in users in [national and sovereign clouds](./1-WebApp-OIDC/1-4-Sovereign).
   1. If you are a business wanting to connect with your customers, or with small business partners, you might also want to [sign-in users with their social identities](./1-WebApp-OIDC/1-5-B2C) using Microsoft Azure AD B2C.
   1. Finally, you'll want to let users [sign-out](./1-WebApp-OIDC/1-6-SignOut) of our application, or globally of the browser.

2. Your Web App might maintain its own resources (in that case you have all you need so far), but it could also be that it calls Microsoft APIs.

   ![Web apps calls Microsoft Graph](ReadmeFiles/Web-app-calls-Microsoft-Graph.svg)

   Learn how to update your Web App to [call Microsoft Graph](2-WebApp-graph-user):

   1. Using the [authorization code flow](2-WebApp-graph-user/2-1-Call-MSGraph), initiated by ASP.NET Core, but completed by Microsoft Authentication Library for .NET (MSAL.NET)
   2. Learn how to [customize the token cache serialization](2-WebApp-graph-user/2-2-TokenCache)
) with different technologies depending on your needs (in memory cache, Session token cache, SQL Cache, Redis Cache)
   3. Learn the [**coming soon**]  [best practices and practices to avoid](./2-WebApp-graph-user/2-3-Best-Practices) when calling an API.

3. Your Web App might also want to call other Web APIs than Microsoft Graph.

   ![Web apps calls Microsoft APIs](ReadmeFiles/web-app-calls-microsoft-apis.svg)

   Learn how to [call several Microsoft APIS](./3-WebApp-multi-APIs), feature conditional access and claims challenge:

   1. the Azure Storage API. This is the opportunity to learn about incremental consent, and conditional access, and how to process them.
   2. the Azure ARM API. This is the opportunity to learn about admin consent.

4. Then you might yourself have written a Web API, and want to call it from your Web App.

   ![Web apps calls Microsoft APIs](ReadmeFiles/web-app-calls-your-api.svg)

5. Once you know how to sign-in users and call Web APIs from your Web App, you might want to restrict part of the application depending on the user having a role in the application or belonging to a group. So far you've learnt how to add and process authentication. Now learn how to [add authorization to your Web application](./5-WebApp-AuthZ):

   1. [with application roles](./5-WebApp-AuthZ/5-1-Roles)
   2. [with Azure AD groups](./5-WebApp-AuthZ/5-2-Groups)

6. [Planned] Chances are that you want to [deploy your complete app to Azure](./6-Deploy-to-Azure). Learn how to do that, applying best practices:

   1. Changing the app registration to add more ReplyUris
   2. Using certificates instead of client secrets
   3. Possibly leveraging Managed identities to get these certificates from KeyVault

### Reusable code for your Web Apps and Web APIs

In this tutorial, the complexities of ASP.NET Core OpenID connect middleware and MSAL.NET are encapsulated into a library project that you can reuse in your own code, to make it easier to build your Web Apps on top of Microsoft identity platform for developers: [Microsoft.Identity.Web](Microsoft.Identity.Web)

### Daemon apps  - Out of scope

This tutorial only covers the case the Web App calls a Web API on behalf of a user. If you are interested in Web Apps calling Web APIs with their own identity (daemon Web Apps), please see [Build a daemon Web App with Microsoft Identity platform for developers](https://github.com/Azure-Samples/active-directory-dotnet-daemon-v2)

## How to run this sample

### Pre-requisites

- Install .NET Core for Windows by following the instructions at [dot.net/core](https://dot.net/core), which will include [Visual Studio 2017](https://aka.ms/vsdownload).
- An Internet connection
- An Azure Active Directory (Azure AD) tenant. For more information on how to get an Azure AD tenant, see [How to get an Azure AD tenant](https://docs.microsoft.com/azure/active-directory/develop/quickstart-create-new-tenant)
- A user account in your Azure AD tenant, or a Microsoft personal account

### Step 1:  Clone or download this repository

From your shell or command line:

```Shell
git clone https://github.com/Azure-Samples/microsoft-identity-platform-aspnetcore-webapp-tutorial webapp
cd webapp
```

> Given that the name of the sample is pretty long, that it has sub-folders and so are the name of the referenced NuGet pacakges, you might want to clone it in a folder close to the root of your hard drive, to avoid file size limitations on Windows.

- We recommend that you start by the first part [1. WebApp signs-in users with Microsoft identity (OIDC)](1-WebApp-OIDC) where you will learn how to sign-in users within your own organization
- It's however possible to start at any phase of the tutorial as the full code is provided in each folder.

## Community Help and Support

Use [Stack Overflow](http://stackoverflow.com/questions/tagged/msal) to get support from the community.
Ask your questions on Stack Overflow first and browse existing issues to see if someone has asked your question before.
Make sure that your questions or comments are tagged with [`msal` `dotnet`].

If you find a bug in the sample, please raise the issue on [GitHub Issues](../../issues).

To provide a recommendation, visit the following [User Voice page](https://feedback.azure.com/forums/169401-azure-active-directory).

## Contributing

If you'd like to contribute to this sample, see [CONTRIBUTING.MD](/CONTRIBUTING.md).

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information, see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Other samples and documentation

- the documentation for the Microsoft identity platform is available from [https://aka.ms/aadv2](https://aka.ms/aadv2)
- Other samples for the Microsoft identity platform are available from [https://aka.ms/aaddevsamplesv2](https://aka.ms/aaddevsamplesv2)
- The conceptual documentation for MSAL.NET is available from [https://aka.ms/msalnet](https://aka.ms/msalnet)
