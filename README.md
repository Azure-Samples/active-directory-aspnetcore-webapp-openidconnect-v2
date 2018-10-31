---
services: active-directory
platforms: dotnet
author: jmprieur
level: 100
service: ASP.NET Core Web App
endpoint: AAD V2
---
# Integrating Azure AD V2 into an ASP.NET Core web app and calling the Microsoft Graph API on behalf of the user.

> This sample is for ASP.NET Core 2.1
> A previous version for ASP.NET 2.0 is available from the [aspnetcore2-1](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/tree/aspnetcore2-1) branch

![Build badge](https://identitydivision.visualstudio.com/_apis/public/build/definitions/a7934fdd-dcde-4492-a406-7fad6ac00e17/514/badge)

## Scenario

This sample shows how to build a .NET Core 2.1 MVC Web app that uses OpenID Connect to sign in users. Users can use personal accounts (including outlook.com, live.com, and others) as well as work and school accounts from any company or organization that has integrated with Azure Active Directory. It leverages the ASP.NET Core OpenID Connect middleware.

![Sign in with Azure AD](ReadmeFiles/sign-in.png)

<!-- Activate when the signInAndCallMsGraph branch is ready
> This is the first of a set of tutorials. Once you understand how to sign-in users in an ASP.NET Core Web App with Open Id Connect, learn how to enable you [Web App to call a Web API in the name of the user](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/tree/signInAndCallMsGraph)
-->

## How to run this sample

To run this sample:

> Pre-requisites: go through how to run [this sample from the aspnetcore2-2 branch](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/tree/aspnetcore2-2
). This page shows the incremental change required to call the Microsoft Graph API on behalf of a user that has successfully signed in to the web app.

### Step 1: Register the sample with your Azure AD tenant

When you have [registered](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/tree/aspnetcore2-2#step-1-register-the-sample-with-your-azure-ad-tenant) your app as described in [the first tutorial](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/tree/aspnetcore2-2), you need an extra step:

1. In the Application Secrets, section, click on **Generate New Password** and copy the value of the generated password. This value is needed for your Web App to call a Web API. For this to happen, the Web App needs to get an access token for the Web API. This will be done using MSAL.NET `ConfidentialClientApplication` which will share a secret with Azure AD to prove the Web Apps identity.

1. Save the changes

### Step 2: Download/ Clone this sample code or build the application using a template

You can clone this sample from your shell or command line:

  ```console
  git clone https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2
  git checkout aspnetcore2-2-signInAndCallGraph
  ```

  In the appsettings.json file, replace:

- the `ClientID` value with the *Application ID* from the application you registered in Application Registration portal,
- the `TenantId` by `common`,
- and the `ClientSecret` by the password you generated.

### Step 3: Run the sample

1. Build the solution and run it.

2. Open your web browser and make a request to the app. The app immediately attempts to authenticate you via the Azure AD v2 endpoint. Sign in with your personal account or with a work or school account.

3. Go to the Contacts page, you should now see all kind of information about yourself (a call was made to the Microsoft Graph *me* endpoint)

## About The code

Starting from the [previous tutorial](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/tree/aspnetcore2-2), the code was incrementally updated by following these steps:

### Add a NuGet package reference to Microsoft.Identity.Client

Using the NuGet package manager, reference Microsoft.Identity.Client (which is still in preview, so check "include preview" box)

### Add additional files to support token acquisition

1. Add the `Extensions\ITokenAcquisition.cs`, `Extensions\TokenAcquisition.cs`. These files define a token acquisition service leveraging MSAL.NET, which is used in the existing application by dependency injection.
1. Add the `Extensions\AuthPropertiesTokenCacheHelper.cs` file. This file proposes a cache for MSAL.NET Confidential client application based on AuthProperties (which are an ASP.NET concept) backed by Cookies.

### Update the `Startup.cs` file to enable TokenAcquisition service

In the `Startup.cs` file, in the `ConfigureServices(IServiceCollection services)` method:

#### Enable a cookie cache

```CSharp
services.AddAuthentication(AzureADDefaults.AuthenticationScheme)
   .AddAzureAD(options => Configuration.Bind("AzureAd", options));
```

by:

```CSharp
services.AddAuthentication(AzureADDefaults.AuthenticationScheme)
   .AddAzureAD(options => Configuration.Bind("AzureAd", options)) 
   .AddCookie();

services.AddTokenAcquisition();
```

this last line adss a token acquisition service that leverages MSAL.NET to acquire tokens.

#### Hook-up to the `OnAuthorizationCodeReceived` event to populate the token cache with a token for the user

Once the user has signed-in, the ASP.NET middleware provides the Web App with the ability to be notified of events such as the fact that an authorization code was received. The following code hooks-up to the `OnAuthorizationCodeReceived` event in order to redeem the code itself and therefore acquire a token, which then will be cached so that it can be used later in the application (in particular in the controllers). For this, after:

```CSharp
options.TokenValidationParameters.ValidateIssuer = false
```

insert:
```CSharp
 // Response type 
 options.ResponseType = "code id_token"; 
 
 // Handling the auth code 
 var handler = options.Events.OnAuthorizationCodeReceived; 
 options.Events.OnAuthorizationCodeReceived = async context => 
 { 
  var _tokenAcquisition = context.HttpContext.RequestServices.GetRequiredService<ITokenAcquisition>(); 
  await _tokenAcquisition.AddAccountToCacheFromAuthorizationCode(context, new string[] { "user.read" }); 
  await handler(context); 
 };
```

### Change the controller code to acquire a token and call Microsoft Graph

In the `Controllers\HomeController.cs`file:
1. Add a constructor to HomeController, making the ITokenAcquisition service available (used by the ASP.NET dependency injection mechanism)

   ```CSharp
   public HomeController(ITokenAcquisition tokenAcquisition)
   {
     this.tokenAcquisition = tokenAcquisition;
   }
   private ITokenAcquisition tokenAcquisition;
   ```

1. Change the `Contact()` action so that it calls the Microsoft Graph *me* endpoint. In case a token cannot be acquired, a challenge is attempted
  to re-sign-in the user.

      ```CSharp
      public async Task<IActionResult> Contact()
      {
       string[] scopes = new string[] { "user.read" };
       try
        {
         string accessToken = await tokenAcquisition.GetAccessTokenOnBehalfOfUser(HttpContext, User, scopes);
         dynamic me = await CallGraphApiOnBehalfOfUser(accessToken);

         ViewData["Me"] = me;
         return View();
        }
        catch(MsalException)
        {
         var redirectUrl = Url.Action(nameof(HomeController.Contact), "Home");
         return Challenge(
                new AuthenticationProperties { RedirectUri = redirectUrl, IsPersistent = true },
                                               OpenIdConnectDefaults.AuthenticationScheme);
        }
       }

       private static async Task<dynamic> CallGraphApiOnBehalfOfUser(string accessToken)
       {
        //
        // Call the Graph API and retrieve the user's profile.
        //
        HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        HttpResponseMessage response = await client.GetAsync("https://graph.microsoft.com/Beta/me");
        string content = await response.Content.ReadAsStringAsync();
        if (response.StatusCode == HttpStatusCode.OK)
        {
            dynamic me = JsonConvert.DeserializeObject(content);
            return me;
        }
        else
        {
            throw new Exception(content);
        }
       }
       ```

### Change the code of the Contacts view to display the *me* object

In `Views\Home\Contacts.cshtml`, insert the following code, which creates an
HTML table displaying the properties of the *me* object as returned by Microsoft Graph.

```CSharp
<table>
    <tr>
        <td>Property</td>
        <td>Value</td>
    </tr>
    @{
        Newtonsoft.Json.Linq.JObject me = ViewData["me"] as Newtonsoft.Json.Linq.JObject;
        IEnumerable<Newtonsoft.Json.Linq.JProperty> children = me.Properties();
        foreach (Newtonsoft.Json.Linq.JProperty child in children)
        {
                <tr>
                    <td>@child.Name</td>
                    <td>@child.Value<td>
                </tr>
        }
    }
</table>
```

## Learn more

You can learn more about the tokens by looking at the following topics in MSAL.NET's conceptual documentation:

- The [Authorization code flow](https://aka.ms/msal-net-authorization-code) which is used, after the user signed-in with Open ID Connect, in order to get a token and cache it for a later use. See [TokenAcquisition L 107](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/blob/f99e913cc032e16c59b748241111e97108e87918/Extensions/TokenAcquisition.cs#L107) for details of this code
- [AcquireTokenSilent](https://aka.ms/msal-net-acquiretokensilent ) which is used by the controller to get an access token for the downstream API. See [TokenAcquisition L 168](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/blob/f99e913cc032e16c59b748241111e97108e87918/Extensions/TokenAcquisition.cs#L168) for details of this code
- [Token cache serialization](msal-net-token-cache-serialization)