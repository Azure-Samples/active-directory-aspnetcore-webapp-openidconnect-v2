---
page_type: sample
name: Integrate a web app and Web Api that authenticates users and calls a web API and Microsoft Graph using the multi-tenant integration pattern (SaaS)
description: Integrate a web app and Web Api that authenticates users and calls the protected Web API and Microsoft Graph using the multi-tenant integration pattern (SaaS)
languages:
 - csharp
products:
 - azure
 - dotnet
 - azure-active-directory
 - office-graph-api
urlFragment: microsoft-identity-platform-aspnetcore-webapp-tutorial
extensions:
- services: ms-identity
- platform: AspNetCore
- endpoint: AAD v2.0
- level: 400
- client: ASP.NET Core Web App
- service: ASP.NET Core Web API
---

# Integrate a web app and Web Api that authenticates users and calls a web API and Microsoft Graph using the multi-tenant integration pattern (SaaS)

> This sample is for Azure AD, not Azure AD B2C.

[![Build status](https://identitydivision.visualstudio.com/IDDP/_apis/build/status/AAD%20Samples/.NET%20client%20samples/ASP.NET%20Core%20Web%20App%20tutorial)](https://identitydivision.visualstudio.com/IDDP/_build/latest?definitionId=819)

- [About this sample](#about-this-sample)
- [Scenario](#scenario)
- [Overview](#overview)
- [Prerequisites](#prerequisites)
- [Setup the sample](#setup-the-sample)
  - [Step 1:  Clone or download this repository](#step-1--clone-or-download-this-repository)
  - [Step 2: Navigate to project folder](#step-2-navigate-to-project-folder)
  - [Step 2:  Register the sample application(s) in your tenant](#step-2--register-the-sample-applications-in-your-tenant)
  - [Step 3: Running the sample](#step-3-running-the-sample)
  - [Testing the Application](#testing-the-application)
  - [The different ways of obtaining admin consent](#the-different-ways-of-obtaining-admin-consent)
  - [Explore the sample](#explore-the-sample)
- [About the code](#about-the-code)
  - [Provisioning your Multi-tenant Apps in another Azure AD Tenant programmatically](#provisioning-your-multi-tenant-apps-in-another-azure-ad-tenant-programmatically)
  - [Code for the Web App (TodoListClient)](#code-for-the-web-app-todolistclient)
  - [Admin Consent Endpoint](#admin-consent-endpoint)
  - [Handle the **MsalUiRequiredException** from Web API](#handle-the-msaluirequiredexception-from-web-api)
  - [Code for the Web API (ToDoListService)](#code-for-the-web-api-todolistservice)
- [Community Help and Support](#community-help-and-support)
- [Contributing](#contributing)
- [More information](#more-information)

## About this sample

This sample demonstrates how to secure a [multi-tenant](https://docs.microsoft.com/azure/active-directory/develop/howto-convert-app-to-be-multi-tenant) ASP.NET Core MVC web application (TodoListClient) which calls another protected **multi-tenant** ASP.NET Core Web API (ToDoListService) with the Microsoft Identity Platform. This sample builds on the concepts introduced in the [Integrate an app that authenticates users and calls Microsoft Graph using the multi-tenant integration pattern (SaaS)](../../2-WebApp-graph-user/2-3-Multi-Tenant) sample. We advise you go through that sample once before trying this sample.  
  
## Scenario

In this sample, we would protect an ASP.Net Core Web API using the Microsoft Identity Platform. The Web API will be protected using Azure Active Directory [OAuth 2.0 Bearer Authorization](https://docs.microsoft.com/azure/active-directory/develop/v2-oauth2-auth-code-flow). The API will support authenticated users with Work and School accounts. Further on the API will also call a downstream API ([Microsoft Graph](https://aka.ms/graph)) **on behalf of the signed-in user** using the [OAuth 2.0 on-behalf-of flow](https://docs.microsoft.com/azure/active-directory/develop/v2-oauth2-on-behalf-of-flow) to provide additional value to its client apps.

The Web API is marked as a [multi-tenant](https://docs.microsoft.com/azure/active-directory/develop/single-and-multi-tenant-apps) app, so that it can be [provisioned](#provisioning-your-multi-tenant-apps-in-another-azure-ad-tenant-programmatically) into Azure AD tenants where the registered client applications in that tenant can then obtain [Access Tokens](https://docs.microsoft.com/azure/active-directory/develop/access-tokens) for this web API and make calls to it.

> Note that the client applications that want to call this web API do not need to be multi-tenant themselves to be able to do so.

## Overview

This sample presents a client Web application that signs-in users and obtains an Access Token for this protected Web API.

Both applications use the [Microsoft.Identity.Web](https://aka.ms/microsoft-identity-web) and Microsoft Authentication Library [MSAL.NET](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet) to sign-in user and obtain a JWT access token through the [OAuth 2.0](https://docs.microsoft.com/azure/active-directory/develop/active-directory-protocols-oauth-code) protocol.

The client Web App:

1. Signs-in users using the [MSAL.NET](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet) and [Microsoft.Identity.Web](https://github.com/AzureAD/microsoft-identity-web) libraries.
1. Acquires an [Access Token](https://docs.microsoft.com/azure/active-directory/develop/access-tokens) for the protected Web API.
1. Calls the ASP.NET Core Web API by using the access token as a bearer token in the authentication header of the Http request.

The Web API:

1. Authorizes the caller (user) using the [Microsoft.Identity.Web](https://github.com/AzureAD/microsoft-identity-web).
1. Acquires another access token on-behalf-of the signed-in user using the [on-behalf of flow](https://docs.microsoft.com/azure/active-directory/develop/v2-oauth2-on-behalf-of-flow).
1. The Web API then uses this new Access token to call Microsoft Graph.

> A recording of a Microsoft Identity Platform developer session that covered this topic of developing a multi-tenant app with Azure Active Directory is available at [Develop multi-tenant applications with Microsoft identity platform](https://www.youtube.com/watch?v=B416AxHoMJ4).

![Topology](./ReadmeFiles/topology.png)

## Prerequisites

* Either [Visual Studio](https://visualstudio.microsoft.com/downloads/) or [Visual Studio Code](https://code.visualstudio.com/download) and [.NET Core SDK](https://www.microsoft.com/net/learn/get-started)
* At least two Azure Active Directory (Azure AD) tenants. For more information on how to get an Azure AD tenant, see [How to get an Azure AD tenant](https://docs.microsoft.com/azure/active-directory/develop/test-setup-environment/)
* A user account in each of your **Azure AD** tenants.

>This sample will not work with a **personal Microsoft account**. If you're signed in to the [Azure portal](https://portal.azure.com) with a personal Microsoft account and have not created a user account in your directory before, you will need to create one before proceeding.

## Setup the sample

### Step 1:  Clone or download this repository

From your shell or command line:

```console
git clone https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2.git
cd "4-WebApp-your-API\4-3-AnyOrg"
```

or download and extract the repository *.zip* file.

> :warning: To avoid path length limitations on Windows, we recommend cloning into a directory near the root of your drive.

### Step 2: Navigate to project folder

```console
cd 4-WebApp-your-API\4-3-AnyOrg\TodoListService
```

### Step 2:  Register the sample application(s) in your tenant

There are two projects in this sample. Each needs to be separately registered in your Azure AD tenant. To register these projects, you can:

- follow the steps below for manually register your apps
- or use PowerShell scripts that:
  - **automatically** creates the Azure AD applications and related objects (passwords, permissions, dependencies) for you.
  - modify the projects' configuration files.

<details>
  <summary>Expand this section if you want to use this automation:</summary>
> :warning: If you have never used **Microsoft Graph PowerShell** before, we recommend you go through the [App Creation Scripts Guide](./AppCreationScripts/AppCreationScripts.md) once to ensure that your environment is prepared correctly for this step.

1. On Windows, run PowerShell as **Administrator** and navigate to the root of the cloned directory
1. In PowerShell run:

   ```PowerShell
   Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process -Force
   ```

1. Run the script to create your Azure AD application and configure the code of the sample application accordingly.
1. For interactive process -in PowerShell, run:

   ```PowerShell
   cd .\AppCreationScripts\
    .\Configure.ps1 -TenantId "[Optional] - your tenant id" -AzureEnvironmentName "[Optional] - Azure environment, defaults to 'Global'"
   ```

> Other ways of running the scripts are described in [App Creation Scripts guide](./AppCreationScripts/AppCreationScripts.md). The scripts also provide a guide to automated application registration, configuration and removal which can help in your CI/CD scenarios.

> ***Inside TodoListService/appsettings.json, find the app key 'AllowedTenants' and add your tenant (for which you've registered the application). Add the Tenant/Directory Id that you've noted earlier during registering the app. You can also add more tenants into this array if you want apps from those tenant being able to access this Web API***.

</details>

#### Choose the Azure AD tenant where you want to create your applications

To manually register the apps, as a first step you'll need to:

1. Sign in to the [Azure portal](https://portal.azure.com).
1. If your account is present in more than one Azure AD tenant, select your profile at the top right corner in the menu on top of the page, and then **switch directory** to change your portal session to the desired Azure AD tenant.

#### Register the service app (WebApi-MultiTenant-v2)

1. Navigate to the [Azure portal](https://portal.azure.com) and select the **Azure Active Directory** service.
1. Select the **App Registrations** blade on the left, then select **New registration**.
1. In the **Register an application page** that appears, enter your application's registration information:
    1. In the **Name** section, enter a meaningful application name that will be displayed to users of the app, for example `WebApi-MultiTenant-v2`.
    1. Under **Supported account types**, select **Accounts in any organizational directory**
    1. Select **Register** to create the application.
1. In the **Overview** blade, find and note the **Application (client) ID**. You use this value in your app's configuration file(s) later in your code.
1. In the app's registration screen, select the **Authentication** blade to the left.
1. In the app's registration screen, select the **Certificates & secrets** blade in the left to open the page where you can generate secrets and upload certificates.
1. In the **Client secrets** section, select **New client secret**:
    1. Type a key description (for instance `app secret`).
    1. Select one of the available key durations (**6 months**, **12 months** or **Custom**) as per your security posture.
    1. The generated key value will be displayed when you select the **Add** button. Copy and save the generated value for use in later steps.
    1. You'll need this key later in your code's configuration files. This key value will not be displayed again, and is not retrievable by any other means, so make sure to note it from the Azure portal before navigating to any other screen or blade.
    > :bulb: For enhanced security, instead of using client secrets, consider [using certificates](./README-use-certificate.md) and [Azure KeyVault](https://azure.microsoft.com/services/key-vault/#product-overview).
    1. Since this app signs-in users, we will now proceed to select **delegated permissions**, which is is required by apps signing-in users.
    1. In the app's registration screen, select the **API permissions** blade in the left to open the page where we add access to the APIs that your application needs:
    1. Select the **Add a permission** button and then:
    1. Ensure that the **Microsoft APIs** tab is selected.
    1. In the *Commonly used Microsoft APIs* section, select **Microsoft Graph**
    1. In the **Delegated permissions** section, select **User.Read.All** in the list. Use the search box if necessary.
    1. Select the **Add permissions** button at the bottom.
1. In the app's registration screen, select the **Expose an API** blade to the left to open the page where you can publish the permission as an API for which client applications can obtain [access tokens](https://aka.ms/access-tokens) for. The first thing that we need to do is to declare the unique [resource](https://docs.microsoft.com/azure/active-directory/develop/v2-oauth2-auth-code-flow) URI that the clients will be using to obtain access tokens for this API. To declare an resource URI(Application ID URI), follow the following steps:
    1. Select **Set** next to the **Application ID URI** to generate a URI that is unique for this app.
    1. For this sample, accept the proposed Application ID URI (`api://{clientId}`) by selecting **Save**.
        > :information_source: Read more about Application ID URI at [Validation differences by supported account types (signInAudience)](https://docs.microsoft.com/azure/active-directory/develop/supported-accounts-validation).
    
##### Publish Delegated Permissions

1. All APIs must publish a minimum of one [scope](https://docs.microsoft.com/azure/active-directory/develop/v2-oauth2-auth-code-flow#request-an-authorization-code), also called [Delegated Permission](https://docs.microsoft.com/azure/active-directory/develop/v2-permissions-and-consent#permission-types), for the client apps to obtain an access token for a *user* successfully. To publish a scope, follow these steps:
1. Select **Add a scope** button open the **Add a scope** screen and Enter the values as indicated below:
    1. For **Scope name**, use `ToDoList.Read`.
    1. Select **Admins and users** options for **Who can consent?**.
    1. For **Admin consent display name** type in *Read users ToDo list using the 'WebApi-MultiTenant-v2'*.
    1. For **Admin consent description** type in *Allow the app to read the user's ToDo list using the 'WebApi-MultiTenant-v2'*.
    1. For **User consent display name** type in *Read your ToDo list items via the 'WebApi-MultiTenant-v2'*.
    1. For **User consent description** type in *Allow the app to read your ToDo list items via the 'WebApi-MultiTenant-v2'*.
    1. Keep **State** as **Enabled**.
    1. Select the **Add scope** button on the bottom to save this scope.
    > Repeat the steps above for another scope named **ToDoList.ReadWrite**
1. Select the **Manifest** blade on the left.
    1. Set `accessTokenAcceptedVersion` property to **2**.
    1. Select on **Save**.

> :information_source:  Follow [the principle of least privilege when publishing permissions](https://learn.microsoft.com/security/zero-trust/develop/protected-api-example) for a web API.

##### Publish Application Permissions

1. All APIs should publish a minimum of one [App role for applications](https://docs.microsoft.com/azure/active-directory/develop/howto-add-app-roles-in-azure-ad-apps#assign-app-roles-to-applications), also called [Application Permission](https://docs.microsoft.com/azure/active-directory/develop/v2-permissions-and-consent#permission-types), for the client apps to obtain an access token as *themselves*, i.e. when they are not signing-in a user. **Application permissions** are the type of permissions that APIs should publish when they want to enable client applications to successfully authenticate as themselves and not need to sign-in users. To publish an application permission, follow these steps:
1. Still on the same app registration, select the **App roles** blade to the left.
1. Select **Create app role**:
    1. For **Display name**, enter a suitable name for your application permission, for instance **ToDoList.Read.All**.
    1. For **Allowed member types**, choose **Application** to ensure other applications can be granted this permission.
    1. For **Value**, enter **ToDoList.Read.All**.
    1. For **Description**, enter *Allow the app to read every user's ToDo list using the 'WebApi-MultiTenant-v2'*.
    1. Select **Apply** to save your changes.

    > Repeat the steps above for another app permission named **ToDoList.ReadWrite.All**

##### Configure Optional Claims

1. Still on the same app registration, select the **Token configuration** blade to the left.
1. Select **Add optional claim**:
    1. Select **optional claim type**, then choose **Access**.
     1. Select the optional claim **idtyp**.
    > Indicates token type. This claim is the most accurate way for an API to determine if a token is an app token or an app+user token. This is not issued in tokens issued to users.
    1. Select **Add** to save your changes.

##### Configure the service app (WebApi-MultiTenant-v2) to use your app registration

Open the project in your IDE (like Visual Studio or Visual Studio Code) to configure the code.

> In the steps below, "ClientID" is the same as "Application ID" or "AppId".

1. Open the `ToDoListService\appsettings.json` file.
1. Find the key `Domain` and replace the existing value with your Azure AD tenant domain, ex. `contoso.onmicrosoft.com`.
1. Find the key `TenantId` and replace the existing value with 'common'.
1. Find the key `ClientId` and replace the existing value with the application ID (clientId) of `WebApi-MultiTenant-v2` app copied from the Azure portal.
1. Find the key `ClientSecret` and replace the existing value with the generated secret that you saved during the creation of `WebApi-MultiTenant-v2` copied from the Azure portal.

#### Register the client app (WebApp-MultiTenant-v2)

1. Navigate to the [Azure portal](https://portal.azure.com) and select the **Azure Active Directory** service.
1. Select the **App Registrations** blade on the left, then select **New registration**.
1. In the **Register an application page** that appears, enter your application's registration information:
    1. In the **Name** section, enter a meaningful application name that will be displayed to users of the app, for example `WebApp-MultiTenant-v2`.
    1. Under **Supported account types**, select **Accounts in any organizational directory**
    1. Select **Register** to create the application.
1. In the **Overview** blade, find and note the **Application (client) ID**. You use this value in your app's configuration file(s) later in your code.
1. In the app's registration screen, select the **Authentication** blade to the left.
1. If you don't have a platform added, select **Add a platform** and select the **Web** option.
    1. In the **Redirect URI** section enter the following redirect URIs:
        1. `https://localhost:44321/`
        1. `https://localhost:44321/signin-oidc`
    1. In the **Front-channel logout URL** section, set it to `https://localhost:44321/signout-oidc`.
    1. Click **Save** to save your changes.
1. In the app's registration screen, select the **Certificates & secrets** blade in the left to open the page where you can generate secrets and upload certificates.
1. In the **Client secrets** section, select **New client secret**:
    1. Type a key description (for instance `app secret`).
    1. Select one of the available key durations (**6 months**, **12 months** or **Custom**) as per your security posture.
    1. The generated key value will be displayed when you select the **Add** button. Copy and save the generated value for use in later steps.
    1. You'll need this key later in your code's configuration files. This key value will not be displayed again, and is not retrievable by any other means, so make sure to note it from the Azure portal before navigating to any other screen or blade.
    > :bulb: For enhanced security, instead of using client secrets, consider [using certificates](./README-use-certificate.md) and [Azure KeyVault](https://azure.microsoft.com/services/key-vault/#product-overview).
    1. Since this app signs-in users, we will now proceed to select **delegated permissions**, which is is required by apps signing-in users.
    1. In the app's registration screen, select the **API permissions** blade in the left to open the page where we add access to the APIs that your application needs:
    1. Select the **Add a permission** button and then:
    1. Ensure that the **My APIs** tab is selected.
    1. In the list of APIs, select the API `WebApi-MultiTenant-v2`.
    1. In the **Delegated permissions** section, select **ToDoList.Read**, **ToDoList.ReadWrite** in the list. Use the search box if necessary.
    1. Select the **Add permissions** button at the bottom.

##### Configure Optional Claims

1. Still on the same app registration, select the **Token configuration** blade to the left.
1. Select **Add optional claim**:
    1. Select **optional claim type**, then choose **ID**.
    1. Select the optional claim **acct**.
    > Provides user's account status in tenant. If the user is a **member** of the tenant, the value is *0*. If they're a **guest**, the value is *1*.
    1. Select **Add** to save your changes.

##### Configure the client app (WebApp-MultiTenant-v2) to use your app registration

Open the project in your IDE (like Visual Studio or Visual Studio Code) to configure the code.

> In the steps below, "ClientID" is the same as "Application ID" or "AppId".

1. Open the `ToDoListClient\appsettings.json` file.
1. Find the key `ClientId` and replace the existing value with the application ID (clientId) of `WebApp-MultiTenant-v2` app copied from the Azure portal.
1. Find the key `TenantId` and replace the existing value with 'common'.
1. Find the key `Domain` and replace the existing value with your Azure AD tenant domain, ex. `contoso.onmicrosoft.com`.
1. Find the key `ClientSecret` and replace the existing value with the generated secret that you saved during the creation of `WebApp-MultiTenant-v2` copied from the Azure portal.
1. Find the key `RedirectUri` and replace the existing value with the base address of `WebApp-MultiTenant-v2` (by default `https://localhost:44321/`).
1. Find the key `TodoListScope` and replace the existing value with ScopeDefault.
1. Find the key `TodoListAppId` and replace the existing value with the application ID (clientId) of `WebApi-MultiTenant-v2` app copied from the Azure portal.
1. Find the key `TodoListBaseAddress` and replace the existing value with the base address of `WebApi-MultiTenant-v2` (by default `https://localhost:44351/`).
1. Find the key `AdminConsentRedirectApi` and replace the existing value with the Redirect URI for `WebApi-MultiTenant-v2`. (by default `https://localhost:44351/`).

#### Configure Known Client Applications for service (WebApi-MultiTenant-v2)

For a middle-tier web API (`WebApi-MultiTenant-v2`) to be able to call a downstream web API, the middle tier app needs to be granted the required permissions as well. However, since the middle-tier cannot interact with the signed-in user, it needs to be explicitly bound to the client app in its **Azure AD** registration. This binding merges the permissions required by both the client and the middle-tier web API and presents it to the end user in a single consent dialog. The user then consent to this combined set of permissions. To achieve this, you need to add the **Application Id** of the client app to the `knownClientApplications` property in the **manifest** of the web API. Here's how:

1. In the [Azure portal](https://portal.azure.com), navigate to your `WebApi-MultiTenant-v2` app registration, and select the **Manifest** blade.
1. In the manifest editor, change the `knownClientApplications: []` line so that the array contains the Client ID of the client application (`WebApp-MultiTenant-v2`) as an element of the array.

For instance:

```json
    "knownClientApplications": ["ca8dca8d-f828-4f08-82f5-325e1a1c6428"],
```

1. **Save** the changes to the manifest.

### Step 3: Running the sample

You can run the sample by using either Visual Studio or command line interface as shown below:

#### Run the sample using Visual Studio

Clean the solution, rebuild the solution, and run it. You might want to go into the solution properties and set both projects as startup projects, with the service project starting first.

When you start the Web API from Visual Studio, depending on the browser you use, you'll get:

- an empty web page (with Microsoft Edge)
- or an error HTTP 401 (with Chrome)

This behavior is expected as the browser is not authenticated. The Web application will be authenticated, so it will be able to access the Web API.

#### Run the sample using a command line interface such as VS Code integrated terminal

##### Step 1. Install .NET Core dependencies

```console
   cd ToDoListService
   dotnet restore
```

Then:  
In a separate console window, execute the following commands

```console
   cd ../
   cd ToDoListClient
   dotnet restore
```

##### Step 2. Trust development certificates

```console
   dotnet dev-certs https --clean
   dotnet dev-certs https --trust
```

Learn more about [HTTPS in .NET Core](https://docs.microsoft.com/aspnet/core/security/enforcing-ssl).

##### Step 3. Run the applications

In both the console windows execute the below command:

```console
    dotnet run
```

Open your browser and navigate to `https://localhost:44321`.

> NOTE: Remember, the To-Do list is stored in memory in this `ToDoListService` app. Each time you run the projects, your To-Do list will get emptied.

### Testing the Application

To properly test this application, you need *at least* **two** tenants, and on each tenant, *at least* **one** administrator and **one** non-administrator account.

### The different ways of obtaining admin consent

A service principal of your multi-tenant app and API is provisioned after the tenant admin manually or programmatically consents. The consent can be obtained from a tenant admin by using one of the following methods:

   1. By using the [/adminconsent](https://docs.microsoft.com/azure/active-directory/develop/v2-admin-consent) endpoint.
   2. By Using the PowerShell command [New-AzADServicePrincipal](https://docs.microsoft.com/powershell/module/Az.Resources/New-AzADServicePrincipal).

#### Obtain Consent using the `/adminconsent` endpoint

You can try the **/adminconsent** endpoint on the home page of the sample by clicking on the `Consent as Admin` link. Web API is provisioned first because the Web App is dependent on the Web API. The admin consent endpoint allows developers to programmatically build links to obtain consent.

![admin consent endpoint](./ReadmeFiles/AdminConsentBtn.png)
> **The `.default` scope**
>
> Did you notice the scope here is set to `.default`, as opposed to `User.Read.All` for Microsoft Graph and `access_as_user` for Web API? This is a built-in scope for every application that refers to the static list of permissions configured on the application registration. Basically, it *bundles* all the permissions in one scope. The `/.default` scope can be used in any OAuth 2.0 flow, but is necessary when using the v2 admin consent endpoint to request application permissions. Read about `scopes` usage at [Scopes and permissions in the Microsoft Identity Platform](https://docs.microsoft.com/azure/active-directory/develop/v2-permissions-and-consent#the-default-scope).  
  
Since both the web app and API needs to be consented by the tenant admin, the admin will need to consent twice.

1. First, the tenant admin will consent for the Web API. The Web API is consented first as the client Web app depends on the Web API and not the other way around.
2. Then, the code will redirect the tenant admin to consent for the client web app.

When redirected to the `/adminconsent` endpoint, the tenant admin will see the sign-in or the coose account screen:

![redirect](./ReadmeFiles/admin_redirect_api.png)

After you choose an admin account, it will lead to the following prompt to consent for the **Web API** :

![consent](./ReadmeFiles/admin_consent_api.png)

When you click `Accept`, it will redirects to `/adminconsent` endpoint again to obtain consent for the **Web App**:

![redirect](./ReadmeFiles/admin_redirect_app.png)

After you choose an admin account, it will lead to the Web App consent as below:

![consent](./ReadmeFiles/admin_consent_app.png)

Once it finishes, your applications service principals will be provisioned in the tenant admin's tenant.

#### Consent using PowerShell

The tenant administrators of a tenant can provision service principals for the applications in their tenant using the AAD PowerShell Module. After installing the AAD PowerShell Module v2, you can run the following cmdlet:

```console
Connect-AzureAD -TenantId "[The tenant Id]"
New-AzureADServicePrincipal -AppId '<client/app id>'
```

If you get errors during admin consent, consider deleting the  **service principal** of your apps in the tenant(s) you are about to test, in order to remove any previously granted consent and to be able to run the **provisioning process** from the beginning.

#### How to delete Service Principals of your apps in a tenant

Steps for deleting a service principal differs with respect to whether the principal is in the **home tenant** of the application or in another tenant. If it is in the **home tenant**, you will find the entry for the application under the **App Registrations** blade. If it is another tenant, you will find the entry under the **Enterprise Applications** blade. Read more about these blades in the [How and why applications are added to Azure AD](https://docs.microsoft.com/azure/active-directory/develop/active-directory-how-applications-are-added).The screenshot below shows how to access the service principal from a **home tenant**:
>
> ![principal1](./ReadmeFiles/Home_Tenant_SP.png)
>
> The rest of the process is the same for both cases. In the next screen, click on **Properties** and then the **Delete** button on the upper side.
>
> ![principal1](./ReadmeFiles/Home_Tenant_SP_Delete.png)
>
> You have now deleted the service principal of Web App for that tenant. Similarly, you can delete the service principal for Web API. Next time, admin needs to provision service principal for both the applications in the tenant from which *that* admin belongs.

### Explore the sample

1. Open your browser and navigate to `https://localhost:44321` and sign-in using the link on top-right.
1. Click on `To-Do List`, you can click on `Create New` link. It will redirect to create task screen where you can add a new task and assign it to any user from the list.
1. The `To-Do List` screen also displays tasks that are assigned to and created by signed-in user. The user can edit and delete the created tasks but can only view the assigned tasks.

> Did the sample not work for you as expected? Did you encounter issues trying this sample? Then please reach out to us using the [GitHub Issues](../../../../issues) page.

> [Consider taking a moment to share your experience with us.](https://forms.office.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbRz0h_jLR5HNJlvkZAewyoWxUNEFCQ0FSMFlPQTJURkJZMTRZWVJRNkdRMC4u)

## About the code

### Provisioning your Multi-tenant Apps in another Azure AD Tenant programmatically

Often the user-based consent will be disabled in an Azure AD tenant or your application will be requesting permissions that requires a tenant-admin consent. In these scenarios, your application will need to utilize the `/adminconsent` endpoint to provision both the **ToDoListClient** and the **ToDoListService** before the users from that tenant are able to sign-in to your app.

When provisioning, you have to take care of the dependency in the topology where the **ToDoListClient** is dependent on **ToDoListService**. So in such a case, you would provision the **ToDoListService** before the **ToDoListClient**.

### Code for the Web App (TodoListClient)

In `Startup.cs`, below lines of code enables Microsoft identity platform endpoint. This endpoint is capable of signing-in users both with their Work and School Accounts.

```csharp
services.AddMicrosoftWebAppAuthentication(Configuration)
    .AddMicrosoftWebAppCallsWebApi(Configuration, new string[] { Configuration["TodoList:TodoListScope"] })
   .AddInMemoryTokenCaches();
```

 1. `AddMicrosoftWebAppAuthentication` : This enables your application to use the Microsoft identity platform endpoint. This endpoint is capable of signing-in users both with their Work and School and Microsoft Personal accounts.
 1. `AddMicrosoftWebAppCallsWebApi` : Enables the web app to call the protected API ToDoList Api.
 1. `AddInMemoryTokenCaches`: Adds an in memory token cache provider, which will cache the Access Tokens acquired for the Web API.

The following code enables to add client service to use the HttpClient by dependency injection.

```CSharp
services.AddTodoListService(Configuration);
```

### Admin Consent Endpoint

In `HomeController.cs`, the method `AdminConsentApi` has the code to redirect the user to the admin consent endpoint for the admin to consent for the **Web API**. The state parameter in the URI contains a link for `AdminConsentClient` method.

```csharp
public IActionResult AdminConsentApi()
{
    string adminConsent1 = "https://login.microsoftonline.com/organizations/v2.0/adminconsent?client_id="+ _ApiClientId 
        + "&redirect_uri=" + _ApiRedirectUri
        + "&state=" + _RedirectUri + "Home/AdminConsentClient" + "&scope=" + _ApiScope;

    return Redirect(adminConsent1);
}
```

The method `AdminConsentClient` has the code to redirect the user to the admin consent endpoint for the admin to consent for the **Web App**.

```csharp
public IActionResult AdminConsentClient()
{
    string adminConsent2 = "https://login.microsoftonline.com/organizations/v2.0/adminconsent?client_id=" + _ClientId
        + "&redirect_uri=" + _RedirectUri
        + "&state=123&scope=" + _TodoListScope;

    return Redirect(adminConsent2);
}
```

### Handle the **MsalUiRequiredException** from Web API

If signed-in user does not have consent for a permission on the Web API, for instance "user.read.all" in this sample, then Web API will throw `MsalUiRequiredException`. The response contains the details about consent Uri and proposed action.

The Web App contains a method `HandleChallengeFromWebApi` in `ToDoListService.cs` that handles the exception thrown by API. It creates a consent URI and throws a custom exception i.e., `WebApiMsalUiRequiredException`.

```csharp
private void HandleChallengeFromWebApi(HttpResponseMessage response)
{
    //proposedAction="consent"
    List<string> result = new List<string>();
    AuthenticationHeaderValue bearer = response.Headers.WwwAuthenticate.First(v => v.Scheme == "Bearer");
    IEnumerable<string> parameters = bearer.Parameter.Split(',').Select(v => v.Trim()).ToList();
    string proposedAction = GetParameter(parameters, "proposedAction");

    if (proposedAction == "consent")
    {
        string consentUri = GetParameter(parameters, "consentUri");

        var uri = new Uri(consentUri);

        var queryString = System.Web.HttpUtility.ParseQueryString(uri.Query);
        queryString.Set("redirect_uri", _ApiRedirectUri);
        queryString.Add("prompt", "consent");
        queryString.Add("state", _RedirectUri);

        var uriBuilder = new UriBuilder(uri);
        uriBuilder.Query = queryString.ToString();
        var updateConsentUri = uriBuilder.Uri.ToString();
        result.Add("consentUri");
        result.Add(updateConsentUri);

        throw new WebApiMsalUiRequiredException(updateConsentUri);
    }
}
```

The following code in `ToDoListController.cs` catches the `WebApiMsalUiRequiredException` exception thrown by `HandleChallengeFromWebApi` method as explained above. Further it Redirects to `consentUri` that is retrieved from exception message. Admin needs to consent as `user.read.all` permission requires admin approval.

```csharp
public async Task<IActionResult> Create()
{
    ToDoItem todo = new ToDoItem();
    try
    {
        ...
    }
    catch (WebApiMsalUiRequiredException ex)
    {
        return Redirect(ex.Message);
    }
}
```

### Code for the Web API (ToDoListService)

#### Admin consent Client Redirect

In HomeController.cs, the method `AdminConsent` redirects to the URI passed in the state parameter by Web App. If admin consent is cancelled from API consent screen then it redirects to base address of Web App.

```csharp
public IActionResult AdminConsent()
{
    var decodeUrl = System.Web.HttpUtility.UrlDecode(HttpContext.Request.QueryString.ToString());
    var queryString = System.Web.HttpUtility.ParseQueryString(decodeUrl);
    var clientRedirect = queryString["state"];
    if (!string.IsNullOrEmpty(clientRedirect))
    {
        if (queryString["error"] == "access_denied" && queryString["error_subcode"] == "cancel")
        {
            var clientRedirectUri = new Uri(clientRedirect);
            return Redirect(clientRedirectUri.GetLeftPart(System.UriPartial.Authority));
        }
        else
        {
            return Redirect(clientRedirect);
        }
    }
    else
    {
        return RedirectToAction("GetTodoItems", "TodoList");
    }
}
```

#### Choosing which scopes to expose

This sample exposes a delegated permission (access_as_user) that will be presented in the access token claim. The method `AddMicrosoftWebApi` does not validate the scope, but Microsoft.Identity.Web has a HttpContext extension method, `VerifyUserHasAnyAcceptedScope`, where you can validate the scope as below:

```csharp
HttpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
```

 For delegated permissions how to access scopes

If a token has delegated permission scopes, they will be in the `scp` or `http://schemas.microsoft.com/identity/claims/scope` claim.

#### Custom Token Validation Allowing only Registered Tenants

By marking your application as multi-tenant, your application will be able to sign-in users from any Azure AD tenant out there. Now you would want to restrict the tenants you want to work with. For this, we will now extend token validation to only those Azure AD tenants registered in the application database. Below, the event handler `OnTokenValidated` was configured to grab the `tenantId` from the token claims and check if it has an entry on the records. If it doesn't, an exception is thrown, canceling the authentication.

Another way to control who is allowed into API is to use Policies. This is configured as part of services.AddAuthorization call. See the code below.

```csharp
//get list of allowed tenants from configuration
  var allowedTenants = Configuration.GetSection("AzureAd:AllowedTenants").Get<string[]>();

  //configure OnTokenValidated event to filter the tenants
  //you can use either this approach or the one below through policies
  services.Configure<JwtBearerOptions>(
      JwtBearerDefaults.AuthenticationScheme, options =>
      {
          var existingOnTokenValidatedHandler = options.Events.OnTokenValidated;
          options.Events.OnTokenValidated = async context =>
          {
              await existingOnTokenValidatedHandler(context);
              if (!allowedTenants.Contains(context.Principal.GetTenantId()))
              {
                  throw new UnauthorizedAccessException("This tenant is not authorized");
              }
          };
      });


  // Creating policies that wraps the authorization requirements
  services.AddAuthorization(

      //uncomment this part if you need to filter the tenants by a policy
      //refer to https://github.com/AzureAD/microsoft-identity-web/wiki/authorization-policies#filtering-tenants

      //builder =>
      //{
      //    string policyName = "User belongs to a specific tenant";
      //    builder.AddPolicy(policyName, b =>
      //    {
      //        b.RequireClaim(ClaimConstants.TenantId, allowedTenants);
      //    });
      //    builder.DefaultPolicy = builder.GetPolicy(policyName);
      //}

  );
```

#### Controlling access to API actions with scopes

During registaring Web API Application, two scopes were created **Read.User.Data** and **Write.User.Data**.
For enhanced and secure access we can decide what scope can access what operation. For example Read.User.Data scope is required for GET:

```csharp
    // GET: api/TodoItems
    [HttpGet]
    [RequiredScope("Read.User.Data")]
    public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodoItems()
    {
        string userTenantId = HttpContext.User.GetTenantId();
        var signedInUser = HttpContext.User.GetDisplayName();
        try
        {
            await _context.TodoItems.ToListAsync();
        }
        catch(Exception)
        {
            throw;
        }
        return await _context.TodoItems.Where
            (x => x.TenantId == userTenantId && (x.AssignedTo == signedInUser || x.Assignedby== signedInUser)).ToListAsync();
    }
```

**Write.User.Data** will let user access POST:

```csharp
    [HttpPost]
    [RequiredScope("Write.User.Data")]
    public async Task<ActionResult<TodoItem>> PostTodoItem(TodoItem todoItem)
    {
        var random = new Random();
        todoItem.Id = random.Next();

            
        _context.TodoItems.Add(todoItem);
        await _context.SaveChangesAsync();

        //return CreatedAtAction("GetTodoItem", new { id = todoItem.Id }, todoItem);
        return Ok(todoItem);
    }
```

## Community Help and Support

Use [Stack Overflow](http://stackoverflow.com/questions/tagged/msal) to get support from the community.
Ask your questions on Stack Overflow first and browse existing issues to see if someone has asked your question before.
Make sure that your questions or comments are tagged with [`azure-active-directory` `msal` `dotnet`].

If you find a bug in the sample, please raise the issue on [GitHub Issues](../../issues).

To provide a recommendation, visit the following [User Voice page](https://feedback.azure.com/forums/169401-azure-active-directory).

## Contributing

If you'd like to contribute to this sample, see [CONTRIBUTING.MD](/CONTRIBUTING.md).

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information, see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## More information

To learn more about single and multi-tenant apps, see:

- [Develop multi-tenant applications with Microsoft identity platform](https://www.youtube.com/watch?v=B416AxHoMJ4)
- [National Clouds](https://docs.microsoft.com/azure/active-directory/develop/authentication-national-cloud)
- [Endpoints](https://docs.microsoft.com/azure/active-directory/develop/active-directory-v2-protocols#endpoints)
- [How and why applications are added to Azure AD](https://docs.microsoft.com/azure/active-directory/develop/active-directory-how-applications-are-added)
- [Scopes and permissions in the Microsoft Identity Platform](https://docs.microsoft.com/azure/active-directory/develop/v2-permissions-and-consent#scopes-and-permissions)

To learn more about admin consent experiences, see:

- [Understanding Azure AD application consent experiences](https://docs.microsoft.com/azure/active-directory/develop/application-consent-experience)
- [Understand user and admin consent](https://docs.microsoft.com/azure/active-directory/develop/howto-convert-app-to-be-multi-tenant#understand-user-and-admin-consent)
