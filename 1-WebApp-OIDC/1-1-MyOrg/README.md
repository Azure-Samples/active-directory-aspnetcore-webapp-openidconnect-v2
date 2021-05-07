---
services: active-directory
platforms: dotnet
author: jmprieur
level: 100
client: ASP.NET Core Web App
endpoint: Microsoft identity platform
---

# An ASP.NET Core Web app signing-in users with the Microsoft identity platform in your organization

> This sample is for Azure AD, not Azure AD B2C. See [sample 1-5-B2C](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/tree/master/1-WebApp-OIDC/1-5-B2C), for B2C scenario.

[![Build status](https://identitydivision.visualstudio.com/IDDP/_apis/build/status/AAD%20Samples/.NET%20client%20samples/ASP.NET%20Core%20Web%20App%20tutorial)](https://identitydivision.visualstudio.com/IDDP/_build/latest?definitionId=819)

## Scenario

This sample shows how to build a .NET Core MVC Web app that uses OpenID Connect to sign in users. Users can only sign-in with their `work and school` accounts in their own organization. It leverages the ASP.NET Core OpenID Connect middleware.

![Sign in with Azure AD](ReadmeFiles/sign-in.png)

> This is the first chapter of this ASP.NET Core Web App tutorial. Once you understand how to sign-in users in an ASP.NET Core Web App with Open Id Connect, can learn how to enable your [Web App to call a Web API on behalf of the signed-in user](../../2-WebApp-graph-user) in a later chapter.
  You can also sign-in users in any or several Azure Active Directory organizations, and even with Microsoft personal accounts or social identities. For more details the parent directory's [Readme.md](../Readme.md)

## How to run this sample

To run this sample:

> Pre-requisites: Install .NET Core 3.0 or later (for example for Windows) by following the instructions at [.NET and C# - Get Started in 10 Minutes](https://www.microsoft.com/net/core). In addition to developing on Windows, you can develop on [Linux](https://www.microsoft.com/net/core#linuxredhat), [Mac](https://www.microsoft.com/net/core#macos), or [Docker](https://www.microsoft.com/net/core#dockercmd).

### Step 1: Register the sample with your Azure AD tenant

There is one project in this sample. To register it, you can:

- either use PowerShell scripts that **automatically** creates the Azure AD applications and related objects (passwords, permissions, dependencies) for you and modify the Visual Studio projects' configuration files.

  <details>
  <summary>Expand to see how to use this automation</summary>

    1. On Windows run PowerShell and navigate to the solution's folder

    2. In PowerShell run:

       ```PowerShell
       Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process -Force
       ```

    3. Run the script to create your Azure AD application and configure the code of the sample application accordingly

       ```PowerShell
       cd .\AppCreationScripts\ 
   .\Configure.ps1
       ```

       > Other ways of running the scripts are described in [App Creation Scripts](./AppCreationScripts/AppCreationScripts.md)

    4. Open the Visual Studio solution and click start. That's it!

    </details>

- or, if you want to register your application with the Azure portal, follow the steps below:

#### Choose the Azure AD tenant where you want to create your applications

As a first step you'll need to:

1. Sign in to the [Azure portal](https://portal.azure.com) using either a work or school account or a personal Microsoft account.
1. If your account is present in more than one Azure AD tenant, select your profile at the top right corner in the menu on top of the page, and then **switch directory**.
   Change your portal session to the desired Azure AD tenant.

#### Register the webApp app (WebApp)

1. Navigate to the Microsoft identity platform for developers [App registrations](https://go.microsoft.com/fwlink/?linkid=2083908) page.
1. Select **New registration**.
1. When the **Register an application page** appears, enter your application's registration information:
   - In the **Name** section, enter a meaningful application name that will be displayed to users of the app, for example `WebApp`.
   - In the **Supported account types** section, select **Accounts in this organizational directory only ({tenant name})**.
     <details open=true>
     <summary>Expand/collapse screenshot</summary>

       ![Register app](../../ReadmeFiles/screenshot-register-app.png)

     </details>
     > Note that there are more than one redirect URIs. You'll need to add them from the **Authentication** tab later after the app has been created successfully.
     
1. Select **Register** to create the application.
1. On the app **Overview** page, find the **Application (client) ID** value and record it for later. You'll need it to configure the Visual Studio configuration file for this project.
   <details open=true>
   <summary>Expand/collapse screenshot</summary>

     ![OVerview page](../../ReadmeFiles/screenshot-overview.png)

   </details>

1. In the list of pages for the app, select **Authentication**..
   - In the Redirect URIs section, select **Web** in the combo-box and enter the following redirect URIs.
       - `https://localhost:44321/`
       - `https://localhost:44321/signin-oidc`
   - In the **Advanced settings** section set **Logout URL** to `https://localhost:44321/signout-oidc`
   - In the **Advanced settings** | **Implicit grant** section, check **ID tokens** as this sample requires
     the [ID Token](https://docs.microsoft.com/azure/active-directory/develop/id-tokens) to be enabled to
     sign-in the user.
     <details open=true>
     <summary>Expand/collapse screenshot</summary>

       ![Authentication page](../../ReadmeFiles/screenshot-authentication.png)

     </details>

1. Select **Save**.

> Note that unless the Web App calls a Web API, no certificate or secret is needed.

### Step 2: Download/ Clone this sample code or build the application using a template

This sample was created from the dotnet core 2.2 [dotnet new mvc](https://docs.microsoft.com/dotnet/core/tools/dotnet-new?tabs=netcore2x) template with `SingleOrg` authentication, and then tweaked to let it support tokens for the Microsoft identity platform endpoint. You can clone/download this repository or create the sample from the command line:

#### Option 1: Download/ clone this sample

You can clone this sample from your shell or command line:

  ```console
git clone https://github.com/Azure-Samples/microsoft-identity-platform-aspnetcore-webapp-tutorial webapp
cd webapp
cd "1-WebApp-OIDC\1-1-MyOrg"
  ```

> Given that the name of the sample is very long, and so are the name of the referenced NuGet packages, you might want to clone it in a folder close to the root of your hard drive, to avoid file size limitations on Windows.

  In the **appsettings.json** file:
  
  - replace the `ClientID` value with the *Application ID* from the application you registered in Application Registration portal on *Step 1*.
  - replace the `TenantId` value with the *Tenant ID* where you registered your Application on *Step 1*.
  - replace the `Domain` value with the *Azure AD domain name*,  e.g. contoso.onmicrosoft.com where you registered your Application on *Step 1*.

#### Option 2: Create the sample from the command line

1. Run the following command to create a sample from the command line using the `SingleOrg` template:

    ```Sh
    dotnet new mvc --auth SingleOrg --client-id <Enter_the_Application_Id_here> --tenant-id <yourTenantId>
    ```

    > Note: Replace *`Enter_the_Application_Id_here`* with the *Application Id* from the application Id you just registered in the Application Registration Portal and *`<yourTenantId>`* with the *Directory (tenant) ID* where you created your application.

1. Open the generated project (.csproj) in Visual Studio, and save the solution.
1. Add the `Microsoft.Identity.Web` NuGet package. It's used to simplify signing-in and, in the next tutorial phases, to get a token.
1. Open the **Startup.cs** file and:

   - at the top of the file, add the following using directive:

     ```CSharp
      using Microsoft.Identity.Web;
      ```

   - in the `ConfigureServices` method, replace the two following lines:

     ```CSharp
      services.AddAuthentication(AzureADDefaults.AuthenticationScheme)
              .AddAzureAD(options => Configuration.Bind("AzureAd", options));
     ```

     by this line:

     ```CSharp
       services.AddMicrosoftIdentityWebAppAuthentication(Configuration);
     ```

     This enables your application to use the Microsoft identity platform endpoint. This endpoint is capable of signing-in users both with their Work and School and Microsoft Personal accounts.

    1. Change the `Properties\launchSettings.json` file to ensure that you start your web app from <https://localhost:44321> as registered. For this:
    - update the `sslPort` of the `iisSettings` section to be `44321`
    - in the `applicationUrl` property of use `https://localhost:44321`

1. (Optional) If you don't have a custom `AccountController` to handle the *sign-in* and *sign-out* requests, you can use the `Microsoft.Identity.Web.UI` built-in one. For that, please include this change in **Startup.cs**:

    - at the top of the file, add the following using directive:

      ```CSharp
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

### Step 3: Run the sample

1. Build the solution and run it.

2. Open your web browser and make a request to the app. Accept the IIS Express SSL certificate if needed. The app immediately attempts to authenticate you via the identity platform endpoint. Sign in with your personal account or with work or school account.

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

## Learn more

To understand more about token validation, see

- [Validating tokens](https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet/wiki/ValidatingTokens)
To understand more about app registration, see:

- [Quickstart: Register an application with the Microsoft identity platform (Preview)](https://docs.microsoft.com/azure/active-directory/develop/quickstart-register-app)
- [Quickstart: Configure a client application to access web APIs (Preview)](https://docs.microsoft.com/azure/active-directory/develop/quickstart-configure-app-access-web-apis)
