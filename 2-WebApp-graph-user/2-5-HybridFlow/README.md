---
page_type: sample
name: Creating a Hybrid Graph API Application
services: active-directory
platforms: dotnet
urlFragment: active-directory-aspnetcore-webapp-openidconnect-v2
description: This sample demonstrates an ASP.NET Core hybrid calling Microsoft Graph API.
languages:
 - csharp
 - javascript
products:
 - aspnet-core
 - azure-active-directory
---

# How to secure an ASP.NET Core Web API with the Microsoft identity platform

[![Build status](https://identitydivision.visualstudio.com/IDDP/_apis/build/status/AAD%20Samples/.NET%20client%20samples/ASP.NET%20Core%20Web%20App%20tutorial)](https://identitydivision.visualstudio.com/IDDP/_build/latest?definitionId=819)

Table Of Contents

* [Scenario](#Scenario)
* [Prerequisites](#Prerequisites)
* [Setup the sample](#Setup-the-sample)
* [Troubleshooting](#Troubleshooting)
* [Using the sample](#Using-the-sample)
* [About the code](#About-the-code)
* [How the code was created](#How-the-code-was-created)
* [How to deploy this sample to Azure](#How-to-deploy-this-sample-to-Azure)
* [Next Steps](#Next-Steps)
* [Contributing](#Contributing)
* [Learn More](#Learn-More)

## Scenario

 This is a ASP.NET Core single page application/web application hybrid sample that calls the Microsoft Graph API using Razor and MSAL.js.

 1. The client ASP.NET Core Web App uses the [Microsoft.Identity.Web](https://aka.ms/microsoft-identity-web) to sign-in and obtain a JWT [Access Tokens](https://docs.microsoft.com/azure/active-directory/develop/access-tokens) from **Azure AD** as well as an additional [Spa Authorization Code](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/SPA-Authorization-Code) to be passed to a client-side single page application.
 1. The [Access Tokens](https://docs.microsoft.com/azure/active-directory/develop/access-tokens) is used as a bearer token to call the **Microsoft Graph API**.
 1. The [Spa Authorization Code](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/SPA-Authorization-Code) is passed to the razor single page application to be exchanged for an access token client side.

## Prerequisites

* Either [Visual Studio](https://visualstudio.microsoft.com/downloads/) or [Visual Studio Code](https://code.visualstudio.com/download) and [.NET Core SDK](https://www.microsoft.com/net/learn/get-started)
* An **Azure AD** tenant. For more information, see: [How to get an Azure AD tenant](https://docs.microsoft.com/azure/active-directory/develop/test-setup-environment#get-a-test-tenant)
* A user account in your **Azure AD** tenant. This sample will not work with a **personal Microsoft account**.  If you're signed in to the [Azure portal](https://portal.azure.com) with a personal Microsoft account and have not created a user account in your directory before, you will need to create one before proceeding.

## Setup the sample

### Step 1: Clone or download this repository

From your shell or command line:

```console
    git clone https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2.git
```

or download and extract the repository .zip file.

>:warning: To avoid path length limitations on Windows, we recommend cloning into a directory near the root of your drive.

### Step 2: Navigate to project folder

```console
    cd 2-WebApp-graph-user\2-5-HybridFlow
```

### Step 3: Application Registration

There are two projects in this sample. Each needs to be separately registered in your Azure AD tenant. To register these projects:

You can follow the [manual steps](#Manual-steps)

**OR**

### Run automation scripts

* use PowerShell scripts that:
  * **automatically** creates the Azure AD applications and related objects (passwords, permissions, dependencies) for you.
  * modify the projects' configuration files.

  <details>
   <summary>Expand this section if you want to use this automation:</summary>

    > **WARNING**: If you have never used **Azure AD Powershell** before, we recommend you go through the [App Creation Scripts guide](./AppCreationScripts/AppCreationScripts.md) once to ensure that your environment is prepared correctly for this step.
  
    1. On Windows, run PowerShell as **Administrator** and navigate to the root of the cloned directory
    1. In PowerShell run:

       ```PowerShell
       Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process -Force
       ```

    1. Run the script to create your Azure AD application and configure the code of the sample application accordingly.
    1. For interactive process - in PowerShell run:

       ```PowerShell
       cd .\AppCreationScripts\
       .\Configure.ps1 -TenantId "[Optional] - your tenant id" -Environment "[Optional] - Azure environment, defaults to 'Global'"
       ```

       > Other ways of running the scripts are described in [App Creation Scripts guide](./AppCreationScripts/AppCreationScripts.md)
       > The scripts also provide a guide to automated application registration, configuration and removal which can help in your CI/CD scenarios.

  </details>

### Manual Steps

 > Note: skip this part if you've just used Automation steps

Follow the steps below for manually register and configure your apps

<details>
   <summary>Expand this section if you want to use this automation:</summary>
  1. Sign in to the [Azure portal](https://portal.azure.com).
  1. If your account is present in more than one Azure AD tenant, select your profile at the top right corner in the menu on top of the page, and then **switch directory** to change your portal session to the desired Azure AD tenant.

#### Register the service app (TodoListService-aspnetcore-webapi)

  1. Navigate to the [Azure portal](https://portal.azure.com) and select the **Azure Active Directory** service.
  1. Select the **App Registrations** blade on the left, then select **New registration**.
  1. In the **Register an application page** that appears, enter your application's registration information:
     * In the **Name** section, enter a meaningful application name that will be displayed to users of the app, for example `HybridFlow-aspnetcore`.
  1. Under **Supported account types**, select **Accounts in this organizational directory only**
  1. Click **Register** to create the application.
  1. In the app's registration screen, find and note the **Application (client) ID**. You'll need to use this value in your app's configuration files.
  1. In the app's registration screen, select **Authentication** in the menu.
     * If you don't have a platform added, select **Add a platform** and select the **Web** option.
     * In the **Redirect URIs** section, enter the following redirect URIs.
        * `https://localhost:44321/signin/`
     * Click on the **Add a platform** button in the **Platform configurations** section of the page
       * Select the **Single-page application** button and enter `https://localhost:7089/` as the **Redirect URI** and click the **Configure** button
     * Under the **Implicit grant and hybrid flows** section enable both the **Access tokens** and **ID tokens** options.
  1. Select **Save** to save your changes.
  1. In the app's registration screen, select the **Certificates & secrets** blade in the left to open the page where we can generate secrets and upload certificates.
  1. In the **Client secrets** section, select **New client secret**:
     * Type a key description (for instance `app secret`),
     * Select one of the available key durations (**6 months**, **12 months** or **Custom**) as per your security posture.
     * The generated key value will be displayed when you select the **Add** button. Copy and save the generated value for use in later steps.
     * You'll need this key later in your code's configuration files. This key value will not be displayed again, and is not retrievable by any other means, so make sure to note it from the Azure portal before navigating to any other screen or blade.
  1. In the app's registration screen, select the **API permissions** blade in the left to open the page where we add access to the APIs that your application needs.
     * Select the **Add a permission** button and then,
     * Ensure that the **Microsoft APIs** tab is selected.
     * In the *Commonly used Microsoft APIs* section, select **Microsoft Graph**
     * In the **Delegated permissions** section, select the **User.Read** in the list followed by **Mail.Read**. Use the search box if necessary.
     * Select the **Add permissions** button at the bottom.
     * Click the **Grant admin consent** button in the page to allow your application access to these scopes.
  1. Select the `Manifest` blade on the left.
     * Set `accessTokenAcceptedVersion` property to **2**.
     * Click on **Save**.

##### Configure the application to use your registration

  Open the project in your IDE (like Visual Studio or Visual Studio Code) to configure the code.

   > In the steps below, "ClientID" is the same as "Application ID" or "AppId".

  1. Open the `appsettings.json` file.
  1. Find the key `Domain` and replace the existing value with your Azure AD tenant name.
  1. Find the key `TenantId` and replace the existing value with your Azure AD tenant ID.
  1. Find the key `ClientId` and replace the existing value with the application ID (clientId) of `HybridFlow-aspnetcore` app copied from the Azure portal.
  1. Find the key `ClientSecret` and replace the existing value with the **client secret** of `HybridFlow-aspnetcore` app copied from the Azure portal.

### Step 4: Running the sample

 For command line run the next commands:

```console
    cd 2-WebApp-graph-user\2-5-HybridFlow
    dotnet run
```

## Troubleshooting

<details>
 <summary>Expand for troubleshooting info</summary>

Use [Stack Overflow](http://stackoverflow.com/questions/tagged/msal) to get support from the community.
Ask your questions on Stack Overflow first and browse existing issues to see if someone has asked your question before.
Make sure that your questions or comments are tagged with [`azure-active-directory` `adal` `msal` `dotnet`].

If you find a bug in the sample, please raise the issue on [GitHub Issues](../../issues).

To provide a recommendation, visit the following [User Voice page](https://feedback.azure.com/forums/169401-azure-active-directory).
</details>

## Next Steps

Learn how to:

* [Change your app to sign-in users from any organization or any Microsoft accounts](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/tree/master/1-WebApp-OIDC/1-3-AnyOrgOrPersonal)
* [Enable users from National clouds to sign-in to your application](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/tree/master/1-WebApp-OIDC/1-4-Sovereign)
* [Enable your Web App to call a Web API on behalf of the signed-in user](https://github.com/Azure-Samples/ms-identity-dotnetcore-ca-auth-context-app)

## Contributing

If you'd like to contribute to this sample, see [CONTRIBUTING.MD](/CONTRIBUTING.md).

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information, see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Learn More

* Microsoft identity platform (Azure Active Directory for developers)](https://docs.microsoft.com/azure/active-directory/develop/)
* Overview of Microsoft Authentication Library (MSAL)](https://docs.microsoft.com/azure/active-directory/develop/msal-overview)
* Authentication Scenarios for Azure AD](https://docs.microsoft.com/azure/active-directory/develop/authentication-flows-app-scenarios)
* Azure AD code samples](https://docs.microsoft.com/azure/active-directory/develop/sample-v2-code)
* Register an application with the Microsoft identity platform](https://docs.microsoft.com/azure/active-directory/develop/quickstart-register-app)
* Building Zero Trust ready apps](https://aka.ms/ztdevsession)

For more information, visit the following links:

 *To lean more about the application registration, visit:
  *[Quickstart: Register an application with the Microsoft identity platform](https://docs.microsoft.com/azure/active-directory/develop/quickstart-register-app)
  *[Quickstart: Configure a client application to access web APIs](https://docs.microsoft.com/azure/active-directory/develop/quickstart-configure-app-access-web-apis)
  *[Quickstart: Configure an application to expose web APIs](https://docs.microsoft.com/azure/active-directory/develop/quickstart-configure-app-expose-web-apis)

  *To learn more about the code, visit:
  *[Conceptual documentation for MSAL.NET](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki#conceptual-documentation) and in particular:
  *[Acquiring tokens with authorization codes on web apps](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/Acquiring-tokens-with-authorization-codes-on-web-apps)
  *[Customizing Token cache serialization](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/token-cache-serialization)

  *To learn more about security in aspnetcore,
  *[Introduction to Identity on ASP.NET Core](https://docs.microsoft.com/aspnet/core/security/authentication/identity)
  *[AuthenticationBuilder](https://docs.microsoft.com/dotnet/api/microsoft.aspnetcore.authentication.authenticationbuilder)
  *[Azure Active Directory with ASP.NET Core](https://docs.microsoft.com/aspnet/core/security/authentication/azure-active-directory)
