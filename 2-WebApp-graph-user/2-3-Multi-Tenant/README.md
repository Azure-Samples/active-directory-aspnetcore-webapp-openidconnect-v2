---
services: active-directory
platforms: dotnet
author: TiagoBrenck
level: 400
client: ASP.NET Core Web App
endpoint: Microsoft identity platform
---


# An ASP.NET Core Web app signing-in users in any org with the Microsoft identity platform

> This sample is for Azure AD, not Azure AD B2C.

[![Build status](https://identitydivision.visualstudio.com/IDDP/_apis/build/status/AAD%20Samples/.NET%20client%20samples/ASP.NET%20Core%20Web%20App%20tutorial)](https://identitydivision.visualstudio.com/IDDP/_build/latest?definitionId=819)

## Scenario

This sample shows how to build a .NET Core MVC Web app that uses OpenID Connect to sign in users from multi-tenants. Users can use a work and school accounts from any company or organization that has integrated with Azure Active Directory. It leverages the ASP.NET Core OpenID Connect middleware.

![Sign in with Azure AD](ReadmeFiles/sign-in.png)

## How to run this sample

To run this sample:

> Pre-requisites: Install .NET Core 2.2 or later (for example for Windows) by following the instructions at [.NET and C# - Get Started in 10 Minutes](https://www.microsoft.com/net/core). In addition to developing on Windows, you can develop on [Linux](https://www.microsoft.com/net/core#linuxredhat), [Mac](https://www.microsoft.com/net/core#macos), or [Docker](https://www.microsoft.com/net/core#dockercmd).

Ideally, you would want to have two Azure AD tenants so you can test the multi-tenant aspect of this sample. For more information on how to get an Azure AD tenant, see [How to get an Azure AD tenant](https://azure.microsoft.com/documentation/articles/active-directory-howto-tenant/).

### Step 1:  Clone or download this repository

From your shell or command line:

```Shell
git clone https://github.com/Azure-Samples/microsoft-identity-platform-aspnetcore-webapp-tutorial.git
cd "2-WebApp-graph-user\2-3-Multi-Tenant"
```

or download and extract the repository .zip file.

> Given that the name of the sample is quiet long, and so are the names of the referenced NuGet packages, you might want to clone it in a folder close to the root of your hard drive, to avoid file size limitations on Windows.

### Step 2:  Register the sample application with your Azure Active Directory tenant

> :warning: **If you had created this sample in the past already**: [Delete its **enterprise app** from the other tenants before re-creating this application](#error-AADSTS650051).

There is one project in this sample. To register it, you can:

- either follow the steps [Step 2: Register the sample with your Azure Active Directory tenant](#step-2-register-the-sample-with-your-azure-active-directory-tenant) and [Step 3:  Configure the sample to use your Azure AD tenant](#choose-the-azure-ad-tenant-where-you-want-to-create-your-applications)
- or use PowerShell scripts that:
  - **automatically** creates the Azure AD applications and related objects (passwords, permissions, dependencies) for you. Note that this works for Visual Studio only.
  - modify the Visual Studio projects' configuration files.

<details>
  <summary>Expand this section if you want to use this automation:</summary>

1. On Windows, run PowerShell and navigate to the root of the cloned directory
1. In PowerShell run:

   ```PowerShell
   Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process -Force
   ```

1. Run the script to create your Azure AD application and configure the code of the sample application accordingly.
1. In PowerShell run:

   ```PowerShell
   .\AppCreationScripts\Configure.ps1
   ```

   > Other ways of running the scripts are described in [App Creation Scripts](./AppCreationScripts/AppCreationScripts.md)
   > The scripts also provide a guide to automated application registration, configuration and removal which can help in your CI/CD scenarios.

1. Open the Visual Studio solution and click start to run the code.

</details>

Follow the steps below to manually walk through the steps to register and configure the applications.

### Step 3:  Configure the sample to use your Azure AD tenant

As a first step you'll need to:

1. Sign in to the [Azure portal](https://portal.azure.com) using either a work or school account or a personal Microsoft account.
1. If your account is present in more than one Azure AD tenant, select your profile at the top right corner in the menu on top of the page, and then **switch directory**.
   Change your portal session to the desired Azure AD tenant.

#### Register the web app (WebApp-MultiTenant-v2)

1. Navigate to the Microsoft identity platform for developers [App registrations](https://go.microsoft.com/fwlink/?linkid=2083908) page.
1. Click **New registration** on top.
1. In the **Register an application page** that appears, enter your application's registration information:
   - In the **Name** section, enter a meaningful application name that will be displayed to users of the app, for example `WebApp-MultiTenant-v2`.
   - Change **Supported account types** to **Accounts in any organizational directory**.
     > Note that there are more than one redirect URIs used in this sample. You'll need to add them from the **Authentication** tab later after the app has been created successfully.
1. Click on the **Register** button in bottom to create the application.
1. In the app's registration screen, find the **Application (client) ID** value and record it for use later. You'll need it to configure the configuration file(s) later in your code.
1. In the app's registration screen, click on the **Authentication** blade in the left.
   - In the Redirect URIs section, select **Web** in the drop down and enter the following redirect URIs.
           - `https://localhost:44321/`
           - `https://localhost:44321/signin-oidc`
        - In the **Advanced settings** section, set **Logout URL** to `https://localhost:44321/signout-oidc`.
        - In the **Advanced settings** | **Implicit grant** section, check the **ID tokens** option as this sample requires the [Implicit grant flow](https://docs.microsoft.com/azure/active-directory/develop/v2-oauth2-implicit-grant-flow) to be enabled to sign-in the user, and call an API.

1. Click the **Save** button on top to save the changes.
1. In the app's registration screen, click on the **Certificates & secrets** blade in the left to open the page where we can generate secrets and upload certificates.
1. In the **Client secrets** section, click on **New client secret**:
   - Type a key description (for instance `app secret`),
   - Select one of the available key durations (**In 1 year**, **In 2 years**, or **Never Expires**) as per your security concerns.
   - The generated key value will be displayed when you click the **Add** button. Copy the generated value for use in the steps later.
   - You'll need this key later in your code's configuration files. This key value will not be displayed again, and is not retrievable by any other means, so make sure to note it from the Azure portal before navigating to any other screen or blade.
1. In the app's registration screen, click on the **API permissions** blade in the left to open the page where we add access to the Apis that your application needs.
   - Click the **Add a permission** button and then,
   - Ensure that the **Microsoft APIs** tab is selected.
   - In the *Commonly used Microsoft APIs* section, click on **Microsoft Graph**
   - In the **Delegated permissions** section, select the **Directory.Read.All** in the list. Use the search box if necessary.
   - Click on the **Add permissions** button in the bottom.

##### Configure the project (WebApp-OpenIDConnect-DotNet) to use your app registration

Open the project in your IDE (like Visual Studio) to configure the code.
>In the steps below, "ClientID" is the same as "Application ID" or "AppId".

1. Open the `appsettings.json` file
1. Find the app key `ClientId` and replace the existing value with the application ID (clientId) of the `WebApp-MultiTenant-v2` application copied from the Azure portal.
1. Find the app key `TenantId` and replace the existing value with `organizations`.
1. Find the app key `Domain` and replace the existing value with your Azure AD tenant name.
1. Find the app key `ClientSecret` and replace the existing value with the key you saved during the creation of the `WebApp-MultiTenant-v2` app, in the Azure portal.

### Step 4: Run the sample

Clean the solution, rebuild the solution, and run it.
The sample implements two distinct tasks: the onboarding of a new tenant and a basic Todo List CRUD operation.

Ideally, you would want to have two Azure AD tenants so you can test the multi-tenant aspect of this sample. For more information on how to get an Azure AD tenant, see [How to get an Azure AD tenant](https://azure.microsoft.com/documentation/articles/active-directory-howto-tenant/).

#### Sign-in

Users can only sign-in if their tenant had been onboarded. The sample will guide them how to do so, but it requires a **tenant admin account** to complete the onboarding process. Once the admin have consented, all users from their tenant will be able to sign-in.

#### Todo List

Users from one tenant can't see todo items from other tenants. They will be able to perform basic CRUD operations on todo items assigned to them. When editing a todo item, users can assign it to any other user from their tenant. The list of users is coming from Microsoft Graph, using the [Graph SDK](https://github.com/microsoftgraph/msgraph-sdk-dotnet).

## About The code

This sample covers the following topics on a multi-tenant app.

- usage of the `/common` endpoint
- service principle provision for new tenants
- custom token validation allowing only registered tenants
- data partition
- Microsoft Graph token by tenant

It is using the OpenID Connect ASP.NET Core middleware to sign in users from multiple Azure AD tenants. The middleware is initialized in the `Startup.cs` file by passing it the Client ID of the app, and the URL of the Azure AD tenant where the app is registered. These values are read from the `appsettings.json` file.

You can trigger the middleware to send an OpenID Connect sign-in request by decorating a class or method with the `[Authorize]` attribute or by issuing a challenge (see the [AccountController.cs](https://github.com/aspnet/AspNetCore/blob/master/src/Azure/AzureAD/Authentication.AzureAD.UI/src/Areas/AzureAD/Controllers/AccountController.cs) file which is part of ASP.NET Core):

These steps are encapsulated in the [Microsoft.Identity.Web](..\..\Microsoft.Identity.Web) project, and in particular in the [WebAppServiceCollectionExtensions.cs](..\..\Microsoft.Identity.Web\WebAppServiceCollectionExtensions.cs) file

### Usage of `/common` endpoint

In order to be able to sign-in users from multiple tenants using OpenID Connect, the [/common endpoint](https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-protocols-oidc#fetch-the-openid-connect-metadata-document) must be used so Microsoft identity platform can fetch the metadata document. In the sample, this endpoint is used as a result of setting the value for `TenantId` as `organizations` on the `appsettings.json` file, and configuring the middleware to read the values from it.

```csharp
services.AddAuthentication(AzureADDefaults.AuthenticationScheme)
                .AddAzureAD(options => configuration.Bind(configSectionName, options));
```

Read more about [OpenID Connect endpoints here](https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-v2-protocols#endpoints).

### Service principle provision for new tenants (onboarding process)

On a multi-tenant app, its service principle will be created on all the users' tenants that have signed-in at least once. Some might want that only tenant admins accept the service principle provisioning. For that, we are using the [admin consent endpoint](https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-admin-consent) for the onboarding process on the `OnboardingController.cs`. The `Onboard` action and corresponding view simulate a simple onboarding experience.

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult Onboard()
{
  ...
  string authorizationRequest = string.Format(
                "{0}common/v2.0/adminconsent?client_id={1}&redirect_uri={2}&state={3}&scope={4}",
                azureADOptions.Instance,
                Uri.EscapeDataString(azureADOptions.ClientId),
                Uri.EscapeDataString(currentUri + "Onboarding/ProcessCode"),
                Uri.EscapeDataString(stateMarker),
                Uri.EscapeDataString("https://graph.microsoft.com/.default"));
  return Redirect(authorizationRequest);
}
```

This results in an OAuth2 code grant request that triggers the admin consent flow and creates the service principle in the admin's tenant. The `state` parameter is used to validate the response, preventing a man-in-the-middle attack. Then, the `ProcessCode` action receives the authorization code from Azure AD and, if they appear valid, it creates an entry in the application database for the new customer.

### Custom token validation allowing only registered tenants

On the `Startup.cs` we are calling `AddMicrosoftIdentityPlatformAuthentication` to configure the authentication, and it also validates that the token issuer is from AAD.

```csharp
options.TokenValidationParameters.IssuerValidator = AadIssuerValidator.GetIssuerValidator(options.Authority).Validate;
```

To extend this validation to only AAD tenants registered in the application database, the event handler `OnTokenValidated` was configured to grab the `tenantId` from the token claims and check if it has on the database. If it doesn't, a custom exception `UnauthorizedTenantException` is thrown and the user is redirected to the `UnauthorizedTenant` view.

```csharp
services.Configure<OpenIdConnectOptions>(AzureADDefaults.OpenIdScheme, options =>
{
    options.Events.OnTokenValidated = async context => 
    {
        string tenantId = context.SecurityToken.Claims.FirstOrDefault(x => x.Type == "tid" || x.Type == "http://schemas.microsoft.com/identity/claims/tenantid")?.Value;

        if (string.IsNullOrWhiteSpace(tenantId))
            throw new UnauthorizedAccessException("Unable to get tenantId from token.");

        var dbContext = context.HttpContext.RequestServices.GetRequiredService<SampleDbContext>();

        var authorizedTenant = await dbContext.AuthorizedTenants.FirstOrDefaultAsync(t => t.TenantId == tenantId);

        if (authorizedTenant == null)
            throw new UnauthorizedTenantException("This tenant is not authorized");

    };
    options.Events.OnAuthenticationFailed = (context) =>
    {
        if (context.Exception != null && context.Exception is UnauthorizedTenantException)
        {
            context.Response.Redirect("/Home/UnauthorizedTenant");
            context.HandleResponse(); // Suppress the exception
        }

        return Task.FromResult(0);
    };
});
```

### Data partition

There are two common scenarios regarding data partition on a multi-tenant app. Having a separate database for each tenant or having a single database and using the **tenantId** to distinguish the data from each tenant. In this sample, we used the single database approach to save the todo items.

`TodoListController.cs` have basic CRUD actions for `TodoItem` and each operation takes into account the signed user's **tenantId** to separate data from each tenant. The tenantId can be found in the user' claims.

### Microsoft Graph token by tenant

If a multi-tenant app needs to acquire a token from Graph to read data from the signed user's tenant, the token must be issued with their tenantId authority and not where the application is registered. This feature is being showed on the **Edit** action result on `TodoListController.cs`.

```csharp
var userTenant = User.GetTenantId();
// Acquiring token for graph using the user's tenantId, so it can return all the users from their tenant
var graphAccessToken = await _tokenAcquisition.GetAccessTokenOnBehalfOfUserAsync(new string[] { GraphScope.DirectoryReadAll }, userTenant);
```

We are acquiring an access token for Graph with the scope `Directory.Read.All`, to list all the users from the tenant so you can assign a todo item to them. `GetAccessTokenOnBehalfOfUserAsync` is a helper method found on `Microsoft.Identity.Web` project, and it receives a **tenantId** as parameter to acquire a token for the desired authority. For that, we get the current authority from the built `IConfidentialClientApplication` and replace the tenantId. Below is an example of this logic.

```csharp
string signedUserAuthority = confidentialClientApplication.Authority.Replace(new Uri(confidentialClientApplication.Authority).PathAndQuery, $"/{tenant}/");
AuthenticationResult result = await confidentialClientApplication
    .AcquireTokenSilent(new string[] { "Directory.Read.All" }, account)
    .WithAuthority(signedUserAuthority)
    .ExecuteAsync()
    .ConfigureAwait(false);
```

## Troubleshooting

### Error AADSTS650051

If you are receiving the following error message, you might need to **delete older Enterprise Applications**

> OpenIdConnectProtocolException: Message contains error: 'invalid_client', error_description: 'AADSTS650051: Application '{applicationId}' is requesting permissions that are either invalid or out of date.

If you had provisioned a service principle of this app in the past and created a new one, the tenants that had signed-in in the app might still have the previous service principle registered causing a conflict with the new one. The solution for the conflict is to delete the older service principle from each tenant in the **Enterprise Application** menu.

## Contributing

If you'd like to contribute to this sample, see [CONTRIBUTING.MD](/CONTRIBUTING.md).

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information, see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Learn more
To understand more about token validation, see
- [Validating tokens](https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet/wiki/ValidatingTokens)

To understand more about app registration, see:

- [Quickstart: Register an application with the Microsoft identity platform (Preview)](https://docs.microsoft.com/azure/active-directory/develop/quickstart-register-app)
- [Quickstart: Configure a client application to access web APIs (Preview)](https://docs.microsoft.com/azure/active-directory/develop/quickstart-configure-app-access-web-apis)
