---
page_type: sample
name: An ASP.NET Core Web App which authenticates users with work and school account in your Azure AD tenant
description: An ASP.NET Core Web App which authenticates users with work and school account in your Azure AD tenant
languages:
 - csharp
products:
 - aspnet-core
 - azure-active-directory
urlFragment: microsoft-identity-platform-aspnetcore-webapp-tutorial
extensions:
- services: ms-identity
- platform: DotNet
- endpoint: AAD v2.0
- level: 100
- client: ASP.NET Core Web App
- service: Microsoft Graph
---


# An ASP.NET Core Web App which authenticates users with work and school account in your Azure AD tenant

> This sample is for Azure AD, not Azure AD B2C. See [sample 1-5-B2C](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/tree/master/1-WebApp-OIDC/1-5-B2C), for B2C scenario.

[![Build status](https://identitydivision.visualstudio.com/IDDP/_apis/build/status/AAD%20Samples/.NET%20client%20samples/ASP.NET%20Core%20Web%20App%20tutorial)](https://identitydivision.visualstudio.com/IDDP/_build/latest?definitionId=819)

## Scenario

This sample shows how to build a .NET Core MVC Web app that uses OpenID Connect to authenticate users. Users can only sign-in with their `work and school` accounts in their own organization (tenant). It leverages the ASP.NET Core OpenID Connect middleware.

![Sign in with Azure AD](ReadmeFiles/sign-in.png)

> This is the first chapter of this ASP.NET Core Web App tutorial. Once you understand how to sign-in users in an ASP.NET Core Web App with Open Id Connect, you can learn how to enable your [Web App to call a Web API (MS Graph) on behalf of the signed-in user](../../2-WebApp-graph-user) in a later chapter.
  You can also sign-in users in any or several Azure Active Directory organizations, and even with Microsoft personal accounts or social identities. For more details the parent directory's [Readme.md](../Readme.md)

## Prerequisites

* Install [\.NET](https://dotnet.microsoft.com/en-us/download/dotnet).
* Either [Visual Studio](https://visualstudio.microsoft.com/downloads/) or [Visual Studio Code](https://code.visualstudio.com/download) and [.NET Core SDK](https://www.microsoft.com/net/learn/get-started)
* An **Azure AD** tenant. For more information, see: [How to get an Azure AD tenant](https://docs.microsoft.com/azure/active-directory/develop/test-setup-environment#get-a-test-tenant)
* A user account in your **Azure AD** tenant.

>This sample will not work with a **personal Microsoft account**. If you're signed in to the [Azure portal](https://portal.azure.com) with a personal Microsoft account and have not created a user account in your directory before, you will need to create one before proceeding.

## Setup the sample

### Step 1: Clone or download this repository

From your shell or command line:

```console
git clone https://github.com/Azure-Samples/microsoft-identity-platform-aspnetcore-webapp-tutorial.git
```

or download and extract the repository *.zip* file.

> :warning: To avoid path length limitations on Windows, we recommend cloning into a directory near the root of your drive.

### Step 2: Navigate to project folder

```console
cd 1-WebApp-OIDC\1-1-MyOrg
```

### Step 3: Register the sample application(s) in your tenant

There is one project in this sample. To register it, you can:

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
    
    
</details>

#### Choose the Azure AD tenant where you want to create your applications

To manually register the apps, as a first step you'll need to:

1. Sign in to the [Azure portal](https://portal.azure.com).
1. If your account is present in more than one Azure AD tenant, select your profile at the top right corner in the menu on top of the page, and then **switch directory** to change your portal session to the desired Azure AD tenant.

#### Register the webApp app (WebApp)

1. Navigate to the [Azure portal](https://portal.azure.com) and select the **Azure Active Directory** service.
1. Select the **App Registrations** blade on the left, then select **New registration**.
1. In the **Register an application page** that appears, enter your application's registration information:
    1. In the **Name** section, enter a meaningful application name that will be displayed to users of the app, for example `WebApp`.
    1. Under **Supported account types**, select **Accounts in this organizational directory only**
    1. Select **Register** to create the application.
     <details open=true>
     <summary>Expand/collapse screenshot</summary>

       ![Register app](../../ReadmeFiles/screenshot-register-app.png)

     </details>
1. In the **Overview** blade, find and note the **Application (client) ID**. You use this value in your app's configuration file(s) later in your code.
   <details open=true>
   <summary>Expand/collapse screenshot</summary>

     ![OVerview page](../../ReadmeFiles/screenshot-overview.png)

   </details>

1. In the app's registration screen, select the **Authentication** blade to the left.
1. If you don't have a platform added, select **Add a platform** and select the **Web** option.
    1. In the **Redirect URI** section enter the following redirect URIs:
        1. `https://localhost:44321/`
        1. `https://localhost:44321/signin-oidc`
    1. In the **Front-channel logout URL** section, set it to `https://localhost:44321/signout-oidc`.
     1. In the **Implicit grant** section, check the **ID tokens** option as this sample uses the [Hybrid flow](https://learn.microsoft.com/azure/active-directory/develop/v2-oauth2-implicit-grant-flow).
    1. Select **ID tokens (used for implicit and hybrid flows)** checkbox.
    1. Click **Save** to save your changes.
     <details open=true>
     <summary>Expand/collapse screenshot</summary>

       ![Authentication page](../../ReadmeFiles/screenshot-authentication.png)

     </details>

##### Configure Optional Claims

1. Still on the same app registration, select the **Token configuration** blade to the left.
1. Select **Add optional claim**:
    1. Select **optional claim type**, then choose **ID**.
    1. Select the optional claim **acct**.
    > Provides user's account status in tenant. If the user is a **member** of the tenant, the value is *0*. If they're a **guest**, the value is *1*.
    1. Select **Add** to save your changes.

> Note that unless the Web App calls a Web API, no certificate or secret is needed.

##### Configure the webApp app (WebApp) to use your app registration

Open the project in your IDE (like Visual Studio or Visual Studio Code) to configure the code.

> In the steps below, "ClientID" is the same as "Application ID" or "AppId".

1. Open the `appsettings.json` file.
1. Find the key `ClientId` and replace the existing value with the application ID (clientId) of `WebApp` app copied from the Azure portal.
1. Find the key `TenantId` and replace the existing value with your Azure AD tenant/directory ID.
1. Find the key `Domain` and replace the existing value with your Azure AD tenant domain, ex. `contoso.onmicrosoft.com`.

### Step 4: Running the sample

From your shell or command line, execute the following commands:

```console
    cd 1-WebApp-OIDC\1-1-MyOrg
    dotnet run
```

## Explore the sample

1. Build the solution and run it.

2. Open your web browser and make a request to the app. Accept the IIS Express SSL certificate if needed. The app immediately attempts to authenticate you via the identity platform endpoint. Sign in with your personal account or with work or school account.

#### Option 2: Create the sample from the command line

1. Run the following command to create a sample from the command line using the `SingleOrg` template:

    ```Sh
    dotnet new mvc --auth SingleOrg --client-id <Enter_the_Application_Id_here> --tenant-id <yourTenantId> --domain <domainName.onmicrosoft.com>
    ```

    > Note: Replace *`Enter_the_Application_Id_here`* with the *Application Id* from the application Id you just registered in the Application Registration Portal and *`<yourTenantId>`* with the *Directory (tenant) ID* where you created your application. Specify your actual domain name instead of domainName.onmicrosoft.com

1. Open the generated project (.csproj) in Visual Studio, and save the solution.
1. Change the `Properties\launchSettings.json` file to ensure that you start your web app from <https://localhost:44321> as registered. For this:
    - update the `sslPort` of the `iisSettings` section to be `44321`
    - in the `applicationUrl` property of use `https://localhost:44321`

1. (Optional) If you don't have a custom `AccountController` to handle the *sign-in* and *sign-out* requests, you can use the `Microsoft.Identity.Web.UI` built-in one. For that, please include this change in **Startup.cs**:

    - at the top of the file, add the following using directive:

      ```CSharp
        using Microsoft.Identity.Web;
        using Microsoft.Identity.Web.UI;
      ```
    - in the `ConfigureServices` method, change the **AddControllersWithView** code snippet to this:

      ```CSharp

            services.AddControllersWithViews(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            }).AddMicrosoftIdentityUI();
      ```

    - in **Views/Shared/_LoginPartial.cshtml**, change the **asp-area** tag to: `asp-area="MicrosoftIdentity"`



## Troubleshooting

### known issue on iOS 12

ASP.NET core applications create session cookies that represent the identity of the caller. Some Safari users using iOS 12 had issues which are described in [ASP.NET Core #4467](https://github.com/aspnet/AspNetCore/issues/4647) and the Web kit bugs database [Bug 188165 - iOS 12 Safari breaks ASP.NET Core 2.1 OIDC authentication](https://bugs.webkit.org/show_bug.cgi?id=188165). 

If your web site needs to be accessed from users using iOS 12, you probably want to disable the SameSite protection, but also ensure that state changes are protected with CSRF anti-forgery mechanism. See the how to fix section of [Microsoft Security Advisory: iOS12 breaks social, WSFed and OIDC logins #4647](https://github.com/aspnet/AspNetCore/issues/4647)

> Did the sample not work for you as expected? Did you encounter issues trying this sample? Then please reach out to us using the [GitHub Issues](../../../../issues) page.

> [Consider taking a moment to share your experience with us.](https://forms.office.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbRz0h_jLR5HNJlvkZAewyoWxUNEFCQ0FSMFlPQTJURkJZMTRZWVJRNkdRMC4u)

## About The code

This sample shows how to use the OpenID Connect ASP.NET Core middleware to sign in users from a single Azure AD tenant. The middleware is initialized in the `Startup.cs` file by passing it the Client ID of the app, and the URL of the Azure AD tenant where the app is registered. These values are  read from the `appsettings.json` file. The middleware takes care of:

- Downloading the Azure AD metadata, finding the signing keys, and finding the issuer name for the tenant.
- Processing OpenID Connect sign-in responses by validating the signature and issuer in an incoming JWT, extracting the user's claims, and putting the claims in `ClaimsPrincipal.Current`.
- Integrating with the session cookie ASP.NET Core middleware to establish a session for the user.

You can trigger the middleware to send an OpenID Connect sign-in request by decorating a class or method with the `[Authorize]` attribute or by issuing a challenge (see the [AccountController.cs](https://github.com/aspnet/AspNetCore/blob/master/src/Azure/AzureAD/Authentication.AzureAD.UI/src/Areas/AzureAD/Controllers/AccountController.cs) file which is part of ASP.NET Core):

The middleware in this project is created as a part of the open-source [ASP.NET Core Security](https://github.com/aspnet/aspnetcore) project.

These steps are encapsulated in the [Microsoft.Identity.Web](https://github.com/AzureAD/microsoft-identity-web/wiki) library.

## Next steps

Learn how to:

- Change your app to sign-in users from [any organization](../1-2-AnyOrg/README-1-1-to-1-2.md) or [any Microsoft accounts](../1-3-AnyOrgOrPersonal/README-1-1-to-1-3.md)
- Enable users from [National clouds](../1-4-Sovereign) to sign-in to your application
- enable your [Web App to call a Web API on behalf of the signed-in user](../../2-WebApp-graph-user/README-incremental-instructions.md)


## Contributing

If you'd like to contribute to this sample, see [CONTRIBUTING.MD](/CONTRIBUTING.md).

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information, see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Learn more

* [Microsoft identity platform (Azure Active Directory for developers)](https://docs.microsoft.com/azure/active-directory/develop/)
* [Azure AD code samples](https://docs.microsoft.com/azure/active-directory/develop/sample-v2-code)
* [Overview of Microsoft Authentication Library (MSAL)](https://docs.microsoft.com/azure/active-directory/develop/msal-overview)
* [Register an application with the Microsoft identity platform](https://docs.microsoft.com/azure/active-directory/develop/quickstart-register-app)
* [Configure a client application to access web APIs](https://docs.microsoft.com/azure/active-directory/develop/quickstart-configure-app-access-web-apis)
