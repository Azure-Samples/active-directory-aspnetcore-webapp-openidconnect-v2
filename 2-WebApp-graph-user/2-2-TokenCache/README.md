---
services: active-directory
platforms: dotnet
author: kalyankrishna1
level: 200
client: ASP.NET Core 2.x Web App
service: Microsoft Graph
endpoint: AAD v2.0
---

# Using the Microsoft identity platform to call the Microsoft Graph API from an An ASP.NET Core 2.x Web App, on behalf of a user signing-in using their work and school or Microsoft personal account

![Build badge](https://identitydivision.visualstudio.com/_apis/public/build/definitions/a7934fdd-dcde-4492-a406-7fad6ac00e17/514/badge)

## Scenario

Starting from a .NET Core 2.2 MVC Web app that uses OpenID Connect to sign in users, this chapter of the tutorial shows how to make a call to Microsoft Graph `/me` endpoint on behalf of the signed-in user. This sample additionally provides instructions on how to use Sql Server for caching tokens.

It leverages the ASP.NET Core OpenID Connect middleware and Microsoft Authentication Library for .NET (MSAL.NET). The complexities of the library's integration with the ASP.NET Core dependency Injection patterns is encapsultated into the `Microsoft.Identity.Web` library project, which is a part of this tutorial. 

![Sign in with the Microsoft identity platform for developers (fomerly Azure AD v2.0)](ReadmeFiles/sign-in.png)

## How to run this sample

To run this sample, you'll need:

- [Visual Studio 2017](https://aka.ms/vsdownload) or just the [.NET Core SDK](https://www.microsoft.com/net/learn/get-started)
- An Internet connection
- A Windows machine (necessary if you want to run the app on Windows)
- An OS X machine (necessary if you want to run the app on Mac)
- A Linux machine (necessary if you want to run the app on Linux)
- An Azure Active Directory (Azure AD) tenant. For more information on how to get an Azure AD tenant, see [How to get an Azure AD tenant](https://azure.microsoft.com/en-us/documentation/articles/active-directory-howto-tenant/)
- A user account in your Azure AD tenant. This sample will not work with a Microsoft account (formerly Windows Live account). Therefore, if you signed in to the [Azure portal](https://portal.azure.com) with a Microsoft account and have never created a user account in your directory before, you need to do that now.

### Step 1:  Clone or download this repository

From your shell or command line:

```Shell
git clone https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2.git
```

or download and exact the repository .zip file.

> Given that the name of the sample is pretty long, and so are the name of the referenced NuGet packages, you might want to clone it in a folder close to the root of your hard drive, to avoid file size limitations on Windows.

### Step 2:  Register the sample application with your Azure Active Directory tenant

There is one project in this sample. To register it, you can:

- either follow the steps [Step 2: Register the sample with your Azure Active Directory tenant](#step-2-register-the-sample-with-your-azure-active-directory-tenant) and [Step 3:  Configure the sample to use your Azure AD tenant](#choose-the-azure-ad-tenant-where-you-want-to-create-your-applications)
- or use PowerShell scripts that:
  - **automatically** creates the Azure AD applications and related objects (passwords, permissions, dependencies) for you
  - modify the Visual Studio projects' configuration files.

If you want to use this automation:
1. On Windows run PowerShell and navigate to the root of the cloned directory

1. In PowerShell run:
   ```PowerShell
   Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process -Force
   ```
1. Run the script to create your Azure AD application and configure the code of the sample application accordinly. 
   ```PowerShell
   .\AppCreationScripts\Configure.ps1
   ```
   > Other ways of running the scripts are described in [App Creation Scripts](./AppCreationScripts/AppCreationScripts.md)

1. Open the Visual Studio solution and click start

If you don't want to use this automation, follow the steps below

#### Choose the Azure AD tenant where you want to create your applications

As a first step you'll need to:

1. Sign in to the [Azure portal](https://portal.azure.com) using either a work or school account or a personal Microsoft account.
1. If your account is present in more than one Azure AD tenant, select your profile at the top right corner in the menu on top of the page, and then **switch directory**.
   Change your portal session to the desired Azure AD tenant.

#### Register the webApp app (WebApp-OpenIDConnect-DotNet-code-v2)

1. Navigate to the Microsoft identity platform for developers [App registrations](https://go.microsoft.com/fwlink/?linkid=2083908) page.
1. Select **New registration**.
1. When the **Register an application page** appears, enter your application's registration information:
   - In the **Name** section, enter a meaningful application name that will be displayed to users of the app, for example `WebApp-OpenIDConnect-DotNet-code-v2`.
   - In the **Supported account types** section, select **Accounts in any organizational directory and personal Microsoft accounts (e.g. Skype, Xbox, Outlook.com)**.
     > Note that there are more than one redirect URIs. You'll need to add them from the **Authentication** tab later after the app has been created succesfully.
1. Select **Register** to create the application.
1. On the app **Overview** page, find the **Application (client) ID** value and record it for later. You'll need it to configure the Visual Studio configuration file for this project.
1. In the list of pages for the app, select **Authentication**..
   - In the Redirect URIs section, select **Web** in the combo-box and enter the following redirect URIs.
       - `https://localhost:44321/`
       - `https://localhost:44321/signin-oidc`
   - In the **Advanced settings** section set **Logout URL** to `https://localhost:44321/signout-oidc`
   - In the **Advanced settings** | **Implicit grant** section, check **ID tokens** as this sample requires
     the [Implicit grant flow](https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-implicit-grant-flow) to be enabled to
     sign-in the user, and call an API.
1. Select **Save**.
1. From the **Certificates & secrets** page, in the **Client secrets** section, choose **New client secret**:

   - Type a key description (of instance `app secret`),
   - Select a key duration of either **In 1 year**, **In 2 years**, or **Never Expires**.
   - When you press the **Add** button, the key value will be displayed, copy, and save the value in a safe location.
   - You'll need this key later to configure the project in Visual Studio. This key value will not be displayed again, nor retrievable by any other means,
     so record it as soon as it is visible from the Azure portal.
1. In the list of pages for the app, select **API permissions**
   - Click the **Add a permission** button and then,
   - Ensure that the **Microsoft APIs** tab is selected
   - In the *Commonly used Microsoft APIs* section, click on **Microsoft Graph**
   - In the **Delegated permissions** section, ensure that the right permissions are checked: **User.Read**. Use the search box if necessary.
   - Select the **Add permissions** button

### Step 3:  Configure the sample to use your Azure AD tenant

In the steps below, "ClientID" is the same as "Application ID" or "AppId".

Open the solution in Visual Studio to configure the projects

#### Configure the webApp project

> Note: if you used the setup scripts, the changes below will have been applied for you

1. Open the `appsettings.json` file
1. Find the app key `ClientId` and replace the existing value with the application ID (clientId) of the `WebApp-OpenIDConnect-DotNet-code-v2` application copied from the Azure portal.
1. Find the app key `TenantId` and replace the existing value with your Azure AD tenant ID.
1. Find the app key `Domain` and replace the existing value with your Azure AD tenant name.
1. Find the app key `ClientSecret` and replace the existing value with the key you saved during the creation of the `WebApp-OpenIDConnect-DotNet-code-v2` app, in the Azure portal.
1. In the `TokenCacheDbConnStr` key, provide the Sql server conenction string to the database you wish to use for token caching.
1. In case you want to deploy your app in Sovereign or national clouds, ensure the `GraphApiUrl` option matches the one you want. By default this is Microsoft Graph in the Azure public cloud

### Step 3: Run the sample

1. Clean the solution, rebuild the solution, and run it. 

2. Open your web browser and make a request to the app. The app immediately attempts to authenticate you via the Microsoft identity platform (fomerly Azure AD v2.0) endpoint. Sign in with your personal account or with a work or school account.

3. Go to the **Profile** page, you should now see all kind of information about yourself as well as your picture (a call was made to the Microsoft Graph */me* endpoint)

> Did the sample not work for you as expected? Did you encounter issues trying this sample? Then please reach out to us using the [GitHub Issues](../../issues) page. 

## About The code

Starting from the [previous phase of the tutorial](../../1.%20WebApp%20signs-in%20users%20with%20Microsoft%20Identity%20(OIDC)), the code was incrementally updated with the following steps:

### Update the `Startup.cs` file to enable TokenAcquisition by a MSAL.NET based service

After the following lines in the ConfigureServices(IServiceCollection services) method, replace the following line of code

```CSharp
public void ConfigureServices(IServiceCollection services)
{
    . . .
    services.AddAzureAdV2Authentication(Configuration);
```

with

```CSharp
public void ConfigureServices(IServiceCollection services)
{
    . . .
    // Token acquisition service based on MSAL.NET 
    // and the Sql server based token cache implementation
    services.AddAzureAdV2Authentication(Configuration)
            .AddMsal(new string[] { Constants.ScopeUserRead })
            .AddSqlAppTokenCache(Configuration)
            .AddSqlPerUserTokenCache(Configuration);
            
```

The aforementioned four lines of code are explained below.

1. The first two lines enable MSAL.NET to hook-up to the OpenID Connect events to redeem the authorization code obtained by the ASP.NET Core middleware. After obtaining a token for Microsoft Graph, it saves it into the token cache, for use by the Controllers.
1. The last two lines hook up the Sql server database based token caching solution to MSAL.NET. The Sql based token cache requires a **Connection string** named `TokenCacheDbConnStr` available in the **ConnectionStrings** collections of the **appsettings.js** configuration file. 

### Add additional files to call Microsoft Graph

Add the `Services\Microsoft-Graph-Rest\*.cs` files. This is an implementation of a custom service which encapsultes the call to the Microsoft Graph /me endpoint. Given an access token for Microsoft Graph, it's capable of getting the user information and the photo of the user.

```CSharp
public interface IGraphApiOperations
{
 Task<dynamic> GetUserInformation(string accessToken);
 Task<string> GetPhotoAsBase64Async(string accessToken);
}
```

### Update the `Startup.cs` file to enable the Microsoft Graph custom service

Still in the `Startup.cs` file, add the following lines just after the following. This lines ensures that the GraphAPIService benefits from the optimized `HttpClient` management by ASP.NET Core.

```CSharp
    // Add Graph
    services.AddGraphService(Configuration);
```

### Change the controller code to acquire a token and call Microsoft Graph

In the `Controllers\HomeController.cs`file:

1. Add a constructor to HomeController, making the ITokenAcquisition service available (used by the ASP.NET dependency injection mechanism)

   ```CSharp
   public HomeController(ITokenAcquisition tokenAcquisition, IGraphApiOperations graphApiOperations)
   {
     this.tokenAcquisition = tokenAcquisition;
     this.graphApiOperations = graphApiOperations;

   }
   private ITokenAcquisition tokenAcquisition;
   private readonly IGraphApiOperations graphApiOperations;
   ```

1. Add a `Profile()` action so that it calls the Microsoft Graph *me* endpoint. In case a token cannot be acquired, a challenge is attempted to re-sign-in the user, and have them consent to the requested scopes. This is expressed declaratively by the `MsalUiRequiredExceptionFilter`attribute. This attribute is part of the `Microsoft.Identity.Web` project and automatically manages incremental consent.

   ```CSharp
   [MsalUiRequiredExceptionFilter(Scopes = new[] {Constants.ScopeUserRead})]
   public async Task<IActionResult> Profile()
   {
    var accessToken =
    await tokenAcquisition.GetAccessTokenOnBehalfOfUser(HttpContext, 
                                                     new[] {Constants.ScopeUserRead});

    var me = await graphApiOperations.GetUserInformation(accessToken);
    var photo = await graphApiOperations.GetPhotoAsBase64Async(accessToken);

    ViewData["Me"] = me;
    ViewData["Photo"] = photo;

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
    var me = ViewData["me"] as JObject;
    var children = me.Properties();
    foreach (var child in children)
    {
     <tr>
       <td>@child.Name</td>
       <td>@child.Value</td>
     </tr>
    }
   }
</table>
```

The files `MSALAppSqlTokenCacheProvider.cs` and `MSALPerUserSqlTokenCacheProvider` of the `Microsoft.Identity.Web` project contains the app and per-user token cache implementations that use Sql server as the token cache.

## Next steps

- Learn how to enable distributed caches in [token cache serialization](../2.2.%20token%20cache%20serialization)
- Learn how the same principle you've just learnt can be used to call:
  - [several Microsoft APIs](../../3.%20WebApp%20calls%20several%20APIS%20(incremental%20consent%20and%20CA)), which will enable you to learn how incremental consent and conditional access is managed in your Web App
  - 3rd party, or even [your own Web API](../../4.%20WebApp%20calls%20your%20own%20Web%20API), which will enable you to learn about custom scopes

## Learn more

- Learn how [Microsoft.Identity.Web](../../Microsoft.Identity.Web) works, in particular hooks-up to the ASP.NET Core ODIC events
- [Use HttpClientFactory to implement resilient HTTP requests](https://docs.microsoft.com/en-us/dotnet/standard/microservices-architecture/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests) used by the Graph custom service
