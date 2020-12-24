---
page_type: sample
languages:
  - csharp
products:
  - dotnet
  - aspnet-core
  - dotnet-core
  - ms-graph
  - azure-active-directory  
name: Using the Microsoft identity platform to call the Microsoft Graph API from an An ASP.NET Core Web App, on behalf of a user
urlFragment: active-directory-aspnetcore-webapp-openidconnect-v2
description: "This sample demonstrates a ASP.NET Core Web App calling the Microsoft Graph"
---

# Using the Microsoft identity platform to call the Microsoft Graph API from an ASP.NET Core Web App, for a user signing-in using their work and school or Microsoft personal account

1. [Overview](#overview)
2. [Scenario](#scenario)
3. [Contents](#contents)
4. [Prerequisites](#prerequisites)
5. [Setup](#setup)
6. [Registration](#registration)
7. [Running the sample](#running-the-sample)
8. [Explore the sample](#explore-the-sample)
9. [About the code](#about-the-code)
10. [Deployment](#deployment)
11. [More information](#more-information)
12. [Community Help and Support](#community-help-and-support)
13. [Contributing](#contributing)

[![Build status](https://identitydivision.visualstudio.com/IDDP/_apis/build/status/AAD%20Samples/.NET%20client%20samples/ASP.NET%20Core%20Web%20App%20tutorial)](https://identitydivision.visualstudio.com/IDDP/_build/latest?definitionId=819)

## Scenario

The .NET Core MVC Web app that uses OpenID Connect to sign in users, this tutorial shows how to call  Microsoft Graph **/me** endpoint on behalf of the signed-in user. It leverages the ASP.NET Core OpenID Connect middleware and Microsoft Authentication Library for .NET ([MSAL.NET](http://aka.ms/msal-net)) and [Microsoft.Identity.Web](https://aka.ms/microsoft-identity-web).

![Sign in with the Microsoft identity platform](ReadmeFiles/sign-in.png)

## How to run this sample

To run this sample:

## Prerequisites

- [Visual Studio](https://visualstudio.microsoft.com/downloads/)
- An **Azure AD** tenant. For more information see: [How to get an Azure AD tenant](https://docs.microsoft.com/azure/active-directory/develop/quickstart-create-new-tenant)
- A user account in your **Azure AD** tenant.

> Developers who wish to gain good familiarity of programming for Microsoft Graph are advised to go through the [An introduction to Microsoft Graph for developers](https://www.youtube.com/watch?v=EBbnpFdB92A) recorded session.

### Setup

### Step 1: Clone or download this repository

From your shell or command line:

```console
    git clone https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2.git
    cd "2-WebApp-graph-user\2-1-Call-MSGraph"
```

or download and extract the repository .zip file.

> :warning: To avoid path length limitations on Windows, we recommend cloning into a directory near the root of your drive.

### Register the sample application(s) with your Azure Active Directory tenant

There is one project in this sample. To register it, you can:

- follow the steps below for manually register your apps
- or use PowerShell scripts that:
  - **automatically** creates the Azure AD applications and related objects (passwords, permissions, dependencies) for you.
  - modify the projects' configuration files.

<details>
  <summary>Expand this section if you want to use this automation:</summary>

> :warning: If you have never used **Azure AD Powershell** before, we recommend you go through the [App Creation Scripts](./AppCreationScripts/AppCreationScripts.md) once to ensure that your environment is prepared correctly for this step.

1. On Windows, run PowerShell as **Administrator** and navigate to the root of the cloned directory
1. If you have never used Azure AD Powershell before, we recommend you go through the [App Creation Scripts](./AppCreationScripts/AppCreationScripts.md) once to ensure that your environment is prepared correctly for this step.
1. In PowerShell run:

   ```PowerShell
   Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process -Force
   ```

1. Run the script to create your Azure AD application and configure the code of the sample application accordingly.
1. In PowerShell run:

   ```PowerShell
   cd .\AppCreationScripts\
   .\Configure.ps1
   ```

   > Other ways of running the scripts are described in [App Creation Scripts](./AppCreationScripts/AppCreationScripts.md)
   > The scripts also provide a guide to automated application registration, configuration and removal which can help in your CI/CD scenarios.

</details>

### Choose the Azure AD tenant where you want to create your applications

As a first step you'll need to:

1. Sign in to the [Azure Portal](https://portal.azure.com).
1. If your account is present in more than one Azure AD tenant, select your profile at the top right corner in the menu on top of the page, and then **switch directory** to change your portal session to the desired Azure AD tenant.

### Register the webApp app (WebApp-OpenIDConnect-DotNet-code-v2)

1. Navigate to the [Azure Portal](https://portal.azure.com) and select the **Azure AD** service.
1. Select the **App Registrations** blade on the left, then select **New registration**.
1. In the **Register an application page** that appears, enter your application's registration information:
   - In the **Name** section, enter a meaningful application name that will be displayed to users of the app, for example `WebApp-OpenIDConnect-DotNet-code-v2`.
   - Under **Supported account types**, select **Accounts in any organizational directory and personal Microsoft accounts (e.g. Skype, Xbox, Outlook.com)**.
   - In the **Redirect URI (optional)** section, select **Web** in the combo-box and enter the following redirect URI: `https://localhost:44321/`.
     > Note that there are more than one redirect URIs used in this sample. You'll need to add them from the **Authentication** tab later after the app has been created successfully.
1. Select **Register** to create the application.
1. In the app's registration screen, find and note the **Application (client) ID**. You use this value in your app's configuration file(s) later in your code.
1. In the app's registration screen, select **Authentication** in the menu.
   - If you don't have a platform added, select **Add a platform** and select the **Web** option.
   - In the **Redirect URIs** section, enter the following redirect URIs.
      - `https://localhost:44321/signin-oidc`
   - In the **Logout URL** section, set it to `https://localhost:44321/signout-oidc`.
1. Select **Save** to save your changes.
1. In the app's registration screen, select the **Certificates & secrets** blade in the left to open the page where we can generate secrets and upload certificates.
1. In the **Client secrets** section, select **New client secret**:
   - Type a key description (for instance `app secret`),
   - Select one of the available key durations (**In 1 year**, **In 2 years**, or **Never Expires**) as per your security posture.
   - The generated key value will be displayed when you select the **Add** button. Copy the generated value for use in the steps later.
   - You'll need this key later in your code's configuration files. This key value will not be displayed again, and is not retrievable by any other means, so make sure to note it from the Azure Portal before navigating to any other screen or blade.
1. In the app's registration screen, select the **API permissions** blade in the left to open the page where we add access to the APIs that your application needs.
   - Select the **Add a permission** button and then,
   - Ensure that the **Microsoft APIs** tab is selected.
   - In the *Commonly used Microsoft APIs* section, select **Microsoft Graph**
   - In the **Delegated permissions** section, select the **User.Read** in the list. Use the search box if necessary.
   - Select the **Add permissions** button at the bottom.

#### Configure the webApp app (WebApp-OpenIDConnect-DotNet-code-v2) to use your app registration

Open the project in your IDE (like Visual Studio or Visual Studio Code) to configure the code.

> In the steps below, "ClientID" is the same as "Application ID" or "AppId".

1. Open the `appsettings.json` file.
1. Find the key `ClientId` and replace the existing value with the application ID (clientId) of `WebApp-OpenIDConnect-DotNet-code-v2` app copied from the Azure Portal.
1. Find the key `TenantId` and replace the existing value with your Azure AD tenant ID.
1. Find the key `Domain` and replace the existing value with your Azure AD tenant name.
1. Find the key `ClientSecret` and replace the existing value with the key you saved during the creation of `WebApp-OpenIDConnect-DotNet-code-v2` copied from the Azure Portal.

- In case you want to deploy your app in Sovereign or national clouds, ensure the `GraphApiUrl` option matches the one you want. By default this is Microsoft Graph in the Azure public cloud

  ```JSon
   "GraphApiUrl": "https://graph.microsoft.com/v1.0"
  ```

### Step 3: Run the sample

1. Build the solution and run it.

2. Open your web browser and make a request to the app. The app immediately attempts to authenticate you via the Microsoft identity platform endpoint. Sign in with your personal account or with a work or school account.

3. Go to the **Profile** page, you should now see all kind of information about yourself as well as your picture (a call was made to the Microsoft Graph */me* endpoint)

> Did the sample not work for you as expected? Did you encounter issues trying this sample? Then please reach out to us using the [GitHub Issues](../../../../issues) page.

> [Consider taking a moment to share your experience with us.](https://forms.office.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbRz0h_jLR5HNJlvkZAewyoWxUNEFCQ0FSMFlPQTJURkJZMTRZWVJRNkdRMC4u)

## About The code

### Update the `Startup.cs` file to enable TokenAcquisition by a MSAL.NET based service

After the following lines in the ConfigureServices(IServiceCollection services) method, replace `services.AddMicrosoftIdentityPlatformAuthentication(Configuration);`, by the following lines:

```CSharp
 public void ConfigureServices(IServiceCollection services)
{
    . . .
    string[] initialScopes = Configuration.GetValue<string>("DownstreamApi:Scopes")?.Split(' ');

    // Add authentication and Graph
            services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(Configuration.GetSection("AzureAd"))
                .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
                .AddMicrosoftGraph(Configuration.GetSection("DownstreamApi"))
                .AddInMemoryTokenCaches();
```

The two new lines of code:

- enable MSAL.NET to hook-up to the OpenID Connect events and redeem the authorization code obtained by the ASP.NET Core middleware and after obtaining a token, saves it into the token cache, for use by the Controllers.
- Decide which token cache implementation to use. In this part of the phase, we'll use a simple in memory token cache, but next steps will show you other implementations you can benefit from, including distributed token caches based on a SQL database, or a Redis cache.

  > Note that you can replace the *in memory token cache* serialization by a *session token cache*  (stored in a session cookie). To do this replacement, change the following in **Startup.cs**:
  > - replace `using Microsoft.Identity.Web.TokenCacheProviders.InMemory` by `using Microsoft.Identity.Web.TokenCacheProviders.Session`
  > - Replace `.AddInMemoryTokenCaches()` by `.AddSessionTokenCaches()`
  > add `app.UseSession();` in the `Configure(IApplicationBuilder app, IHostingEnvironment env)` method, for instance after `app.UseCookiePolicy();`
  >
  >
  > You can also use a distributed token cache, and choose the serialization implementation. For this,  in **Startup.cs**:
  > - replace `using Microsoft.Identity.Web.TokenCacheProviders.InMemory` by `using Microsoft.Identity.Web.TokenCacheProviders.Distributed`
  > - Replace `.AddInMemoryTokenCaches()` by `.AddDistributedTokenCaches()`
  > - Then choose the distributed cache implementation. For details, see https://docs.microsoft.com/en-us/aspnet/core/performance/caching/distributed?view=aspnetcore-2.2#distributed-memory-cache
  >
  >   ```CSharp
  >   // use a distributed Token Cache by adding
  >      .AddDistributedTokenCaches();
  >
  >   // and then choose your implementation.
  >  
  >   // For instance the distributed in memory cache (not cleaned when you stop the app)
  >   services.AddDistributedMemoryCache()
  >
  >   // Or a Redis cache
  >   services.AddStackExchangeRedisCache(options =>
  >   {
  >    options.Configuration = "localhost";
  >    options.InstanceName = "SampleInstance";
  >   });
  >
  >   // Or even a SQL Server token cache
  >   services.AddDistributedSqlServerCache(options =>
  >   {
  >    options.ConnectionString =_config["DistCache_ConnectionString"];
  >    options.SchemaName = "dbo";
  >    options.TableName = "TestCache";
  >   });
  >   ```

### Add additional files to call Microsoft Graph

Add `Microsoft.Graph` package, to use [Microsoft Graph SDK](https://github.com/microsoftgraph/msgraph-sdk-dotnet/blob/dev/docs/overview.md).

Add the `Services\*.cs` files. The `GraphServiceClientFactory.cs` returns a `GraphServiceClient` with an authentication provider, used for [Microsoft Graph SDK](https://github.com/microsoftgraph/msgraph-sdk-dotnet/blob/dev/docs/overview.md). Given an access token for Microsoft Graph, it's capable of making a request to Graph services sending that access token in the header.

### Codein the `Startup.cs` file to call Microsoft Graph

Still in the `Startup.cs` file, add the following `AddMicrosoftGraph` extension method. This lines ensures that the GraphAPIService benefits from the optimized `HttpClient` management by ASP.NET Core.

// this section in the appsettings.json provides the coordinates to call MS Graph
// Here we request the scopes `user.read` and `Sites.Read.All` to read data in SharePoint sites via Graph

```Json
"DownstreamApi": {
    /*
     'Scopes' contains space separated scopes of the Web API you want to call. This can be:
      - a scope for a V2 application (for instance api:b3682cc7-8b30-4bd2-aaba-080c6bf0fd31/access_as_user)
      - a scope corresponding to a V1 application (for instance <App ID URI>/.default, where  <App ID URI> is the
        App ID URI of a legacy v1 Web application
      Applications are registered in the https:portal.azure.com portal.
    */
    "BaseUrl": "https://graph.microsoft.com/v1.0",
    "Scopes": "user.read Sites.Read.All"
  },
```

```CSharp
    // Add Graph
        // This enables acquisition of an Access Token to call a downstream API like Microsoft Graph
        .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
        // Explicitly add supprot for Microsoft graph
        .AddMicrosoftGraph(Configuration.GetSection("DownstreamApi"))
```

### Change the controller code to acquire a token and call Microsoft Graph

In the `Controllers\HomeController.cs`file:

1. Add a constructor to HomeController, making the ITokenAcquisition service available (used by the ASP.NET dependency injection mechanism)

```CSharp
private readonly GraphServiceClient _graphServiceClient;

public HomeController(ILogger<HomeController> logger,
                    GraphServiceClient graphServiceClient)
{
    _logger = logger;
    _graphServiceClient = graphServiceClient;
}
```

1. Add a `Profile()` action so that it calls the Microsoft Graph *me* endpoint. In case a token cannot be acquired, a challenge is attempted to re-sign-in the user, and have them consent to the requested scopes. This is expressed declaratively by the `AuthorizeForScopes`attribute. This attribute is part of the `Microsoft.Identity.Web` project and automatically manages incremental consent.

```CSharp
[AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
public async Task<IActionResult> Profile()
{
    var me = await _graphServiceClient.Me.Request().GetAsync();
    ViewData["Me"] = me;

    try
    {
        // Get user photo
        using (var photoStream = await _graphServiceClient.Me.Photo.Content.Request().GetAsync())
        {
            byte[] photoByte = ((MemoryStream)photoStream).ToArray();
            ViewData["Photo"] = Convert.ToBase64String(photoByte);
        }
    }
    catch (System.Exception)
    {
        ViewData["Photo"] = null;
    }

    return View();
}
```

### Add a Profile view to display the *me* object

Add a new view `Views\Home\Profile.cshtml` and insert the following code, which creates an
HTML table displaying the properties of the *me* object as returned by Microsoft Graph.

```CSharp
@using Newtonsoft.Json.Linq
@{
    ViewData["Title"] = "Profile";
}
<h2>@ViewData["Title"]</h2>
<h3>@ViewData["Message"]</h3>

<table class="table table-striped table-condensed" style="font-family: monospace">
    <tr>
        <th>Property</th>
        <th>Value</th>
    </tr>
    <tr>
        <td>photo</td>
        <td>
            @{
                if (ViewData["photo"] != null)
                {
                    <img style="margin: 5px 0; width: 150px" src="data:image/jpeg;base64, @ViewData["photo"]" />
                }
                else
                {
                    <h3>NO PHOTO</h3>
                    <p>Check user profile in Azure Active Directory to add a photo.</p>
                }
            }
        </td>
    </tr>
    @{       
        var me = ViewData["me"] as Microsoft.Graph.User;
        var properties = me.GetType().GetProperties();
        foreach (var child in properties)
        {
            object value = child.GetValue(me);
            string stringRepresentation;
            if (!(value is string) && value is IEnumerable<string>)
            {
                stringRepresentation = "["
                    + string.Join(", ", (value as IEnumerable<string>).OfType<object>().Select(c => c.ToString()))
                    + "]";
            }
            else
            {
                stringRepresentation = value?.ToString();
            }

            <tr>
                <td> @child.Name </td>
                <td> @stringRepresentation </td>
            </tr>
        }      
    }
</table>
```

## Next steps

- Learn how to enable distributed caches in [token cache serialization](../2-2-TokenCache)
- Learn how the same principle you've just learned can be used to call:
  - [several Microsoft APIs](../../3-WebApp-multi-APIs), which will enable you to learn how incremental consent and conditional access is managed in your Web App
  - 3rd party, or even [your own Web API](../../4-WebApp-your-API), which will enable you to learn about custom scopes

## Learn more

- Learn how [Microsoft.Identity.Web](https://aka.ms/microsoft-identity-web) works, in particular hooks-up to the ASP.NET Core OIDC events
- [Use HttpClientFactory to implement resilient HTTP requests](https://docs.microsoft.com/dotnet/standard/microservices-architecture/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests) used by the Graph custom service
