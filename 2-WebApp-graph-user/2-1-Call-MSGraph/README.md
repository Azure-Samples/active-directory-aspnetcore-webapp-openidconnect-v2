---
services: active-directory
platforms: dotnet
author: jmprieur
level: 200
client: ASP.NET Core 2.x Web App
service: Microsoft Graph
endpoint: Microsoft identity platform
---

# Using the Microsoft identity platform to call the Microsoft Graph API from an An ASP.NET Core 2.x Web App, on behalf of a user signing-in using their work and school or Microsoft personal account

[![Build status](https://identitydivision.visualstudio.com/IDDP/_apis/build/status/AAD%20Samples/.NET%20client%20samples/ASP.NET%20Core%20Web%20App%20tutorial)](https://identitydivision.visualstudio.com/IDDP/_build/latest?definitionId=819)

## Scenario

Starting from a .NET Core 2.2 MVC Web app that uses OpenID Connect to sign in users, this phase of the tutorial shows how to call  Microsoft Graph /me endpoint on behalf of the signed-in user. It leverages the ASP.NET Core OpenID Connect middleware and Microsoft Authentication Library for .NET (MSAL.NET). Their complexities where encapsultated into the `Microsoft.Identity.Web` reusable library project part of this tutorial. Once again the notion of ASP.NET services injected by dependency injection is heavily used.

![Sign in with the Microsoft identity platform](ReadmeFiles/sign-in.png)

## How to run this sample

To run this sample:

> Pre-requisites:
>
> go through the previous phase of the tutorial showing how the [WebApp signs-in users with Microsoft Identity (OIDC) / with work and school or personal accounts](../../1-WebApp-OIDC/1-3-AnyOrgOrPersonal). This page shows the incremental change required to call the Microsoft Graph API on behalf of a user that has successfully signed in to the web app.

### Step 1: Register the sample with your Azure AD tenant

You first need to [register](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/tree/aspnetcore2-2#step-1-register-the-sample-with-your-azure-ad-tenant) your app as described in [the first tutorial](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/tree/aspnetcore2-2)

Then follow the following extra set of steps:

1. From the **Certificates & secrets** page, for your app registration, in the **Client secrets** section, choose **New client secret**:

   - Type a key description (of instance `app secret`),
   - Select a key duration of either **In 1 year**, **In 2 years**, or **Never Expires**.
   - When you press the **Add** button, the key value will be displayed, copy, and save the value in a safe location.
   - You'll need this key later to configure the project in Visual Studio. This key value will not be displayed again, nor retrievable by any other means.
1. In the list of pages for the app, select **API permissions**, and notice that a delegated permission is set by default to Microsoft Graph for the scope **User.Read**

### Step 2: Download/Clone/Go to the folder containing the sample code and build the application

If you have not already,  clone this sample from your shell or command line:

  ```Shell
  git clone https://github.com/Azure-Samples/microsoft-identity-platform-aspnetcore-webapp-tutorial webapp
  cd webapp

  ```

Go to the `"2-WebApp-graph-user\2-1-Call-MSGraph"` folder

 ```Sh
  cd "2-WebApp-graph-user\2-1-Call-MSGraph"
  ```

  In the appsettings.json file, replace, if you have not already:

- the `ClientID` value with the *Application ID* from the application you registered in Application Registration portal,
- the `TenantId` by `common`, as here you chose to sign-in users with their work or school or personal account. In case you want to sign-in different audiences, refer back to the first phase of the tutorial
- and the `ClientSecret` by the client secret you generated in Step 1.

- In case you want to deploy your app in Sovereign or national clouds, ensure the `GraphApiUrl` option matches the one you want. By default this is Microsoft Graph in the Azure public cloud

  ```JSon
   "GraphApiUrl": "https://graph.microsoft.com/v1.0"
  ```

### Step 3: Run the sample

1. Build the solution and run it.

2. Open your web browser and make a request to the app. The app immediately attempts to authenticate you via the Microsoft identity platform endpoint. Sign in with your personal account or with a work or school account.

3. Go to the **Profile** page, you should now see all kind of information about yourself as well as your picture (a call was made to the Microsoft Graph */me* endpoint)

## About The code

Starting from the [previous phase of the tutorial](../../1-WebApp-OIDC), the code was incrementally updated with the following steps:

### Update the `Startup.cs` file to enable TokenAcquisition by a MSAL.NET based service

After the following lines in the ConfigureServices(IServiceCollection services) method, replace `services.AddAzureAdV2Authentication(Configuration);`, by the following lines:

```CSharp
 public void ConfigureServices(IServiceCollection services)
{
    . . .
    // Token acquisition service based on MSAL.NET 
    // and chosen token cache implementation
    services.AddAzureAdV2Authentication(Configuration)
            .AddMsal(new string[] { Constants.ScopeUserRead })
            .AddInMemoryTokenCache();
```

The two new lines of code:

- enable MSAL.NET to hook-up to the OpenID Connect events and redeem the authorization code obtained by the ASP.NET Core middleware and after obtaining a token, saves it into the token cache, for use by the Controllers.
- Decide which token cache implementation to use. In this part of the phase, we'll use a simple in memory token cache, but next steps will show you other implementations you can benefit from, including distributed token caches based on a SQL database, or a Redis cache.

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

## Next steps

- Learn how to enable distributed caches in [token cache serialization](../2-2-TokenCache)
- Learn how the same principle you've just learnt can be used to call:
  - [several Microsoft APIs](../../3-WebApp-multi-APIs), which will enable you to learn how incremental consent and conditional access is managed in your Web App
  - 3rd party, or even [your own Web API](../../4-WebApp-your-API), which will enable you to learn about custom scopes

## Learn more

- Learn how [Microsoft.Identity.Web](../../Microsoft.Identity.Web) works, in particular hooks-up to the ASP.NET Core ODIC events
- [Use HttpClientFactory to implement resilient HTTP requests](https://docs.microsoft.com/en-us/dotnet/standard/microservices-architecture/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests) used by the Graph custom service
