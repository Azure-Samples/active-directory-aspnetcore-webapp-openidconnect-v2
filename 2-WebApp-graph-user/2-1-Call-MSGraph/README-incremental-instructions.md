---
services: active-directory
platforms: dotnet
author: jmprieur
level: 200
client: ASP.NET Core 3.x Web App
service: Microsoft Graph
endpoint: Microsoft identity platform
---

# Using the Microsoft identity platform to call the Microsoft Graph API from an An ASP.NET Core 2.x Web App, on behalf of a user signing-in using their work and school or Microsoft personal account

[![Build status](https://identitydivision.visualstudio.com/IDDP/_apis/build/status/AAD%20Samples/.NET%20client%20samples/ASP.NET%20Core%20Web%20App%20tutorial)](https://identitydivision.visualstudio.com/IDDP/_build/latest?definitionId=819)

## Scenario

Starting from a .NET Core MVC Web app that uses OpenID Connect to sign in users, this phase of the tutorial shows how to call  Microsoft Graph /me endpoint on behalf of the signed-in user. It leverages the ASP.NET Core OpenID Connect middleware and Microsoft Authentication Library for .NET (MSAL.NET). Their complexities where encapsulated into the [Microsoft.Identity.Web](https://github.com/AzureAD/microsoft-identity-web/wiki/Microsoft-Identity-Web-basics). Once again the notion of ASP.NET services injected by dependency injection is heavily used.

![Sign in with the Microsoft identity platform](ReadmeFiles/sign-in.png)

## How to run this sample

To run this sample:

> Pre-requisites:
>
> go through the previous phase of the tutorial showing how the [WebApp signs-in users with Microsoft Identity (OIDC) / with work and school or personal accounts](../../1-WebApp-OIDC/1-3-AnyOrgOrPersonal). This page shows the incremental change required to call the Microsoft Graph API on behalf of a user that has successfully signed in to the web app.

- Developers who wish to gain good familiarity of programming for Microsoft Graph are advised to go through the [An introduction to Microsoft Graph for developers](https://www.youtube.com/watch?v=EBbnpFdB92A) recorded session.

### Step 1: Register the sample with your Azure AD tenant

You first need to [register](../../1-WebApp-OIDC/1-1-MyOrg#step-1-register-the-sample-with-your-azure-ad-tenant) your app as described in [the first tutorial](../../1-WebApp-OIDC/1-1-MyOrg)

Then follow the following extra set of steps:

1. In the app's registration screen, click on the **Certificates & secrets** blade in the left to open the page where we can generate secrets and upload certificates.
1. In the **Client secrets** section, click on **New client secret**:
   - Type a key description (for instance `app secret`),
   - Select one of the available key durations (**In 1 year**, **In 2 years**, or **Never Expires**) as per your security concerns.
   - The generated key value will be displayed when you click the **Add** button. Copy the generated value for use in the steps later.
   - You'll need this key later in your code's configuration files. This key value will not be displayed again, and is not retrievable by any other means, so make sure to note it from the Azure portal before navigating to any other screen or blade.
1. In the app's registration screen, click on the **API permissions** blade in the left to open the page where we add access to the Apis that your application needs.
   - Click the **Add permissions** button and then,
   - Ensure that the **Microsoft APIs** tab is selected.
   - In the *Commonly used Microsoft APIs* section, click on **Microsoft Graph**
   - In the **Delegated permissions** section, select the **User.Read** in the list. Use the search box if necessary.
   - Click on the **Add permissions** button in the bottom.

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

#### Configure the  webApp app (WebApp-OpenIDConnect-DotNet-code-v2) to use your app registration

Open the project in your IDE (like Visual Studio) to configure the code.
>In the steps below, "ClientID" is the same as "Application ID" or "AppId".

1. Open the `appsettings.json` file
1. Find the app key `ClientId` and replace the existing value with the application ID (clientId) of the `WebApp-OpenIDConnect-DotNet-code-v2` application copied from the Azure portal.
1. Find the app key `TenantId` and replace by `common`, as here you chose to sign-in users with their work or school or personal account. In case you want to sign-in different audiences, refer back to the first phase of the tutorial.
1. Find the app key `Domain` and replace the existing value with your Azure AD tenant name.
1. Find the app key `ClientSecret` and replace the existing value with the key you saved during the creation of the `WebApp-OpenIDConnect-DotNet-code-v2` app, in the Azure portal.

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

Starting from the [previous phase of the tutorial](../../1-WebApp-OIDC), the code was incrementally updated with the following steps:

### Update the `Startup.cs` file to enable TokenAcquisition by a MSAL.NET based service

After the following lines in the ConfigureServices(IServiceCollection services) method, replace `services.AddMicrosoftIdentityWebApp(Configuration);`, by the following lines:

```CSharp
 public void ConfigureServices(IServiceCollection services)
{
    . . .
    string[] initialScopes = Configuration.GetValue<string>("DownstreamApi:Scopes")?.Split(' ');

    // Add Graph
    services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApp(Configuration.GetSection("AzureAd"))
         .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
            .AddMicrosoftGraph(Configuration.GetSection("DownstreamApi"))
            .AddInMemoryTokenCaches();
```

The two new lines of code:

- enable MSAL.NET to hook-up to the OpenID Connect events and redeem the authorization code obtained by the ASP.NET Core middleware and after obtaining a token, saves it into the token cache, for use by the Controllers.
- Decide which token cache implementation to use. In this part of the phase, we'll use a simple in memory token cache, but next steps will show you other implementations you can benefit from, including distributed token caches based on a SQL database, Cosmos DB or a Redis cache.
  For details see [Token cache serialization](https://github.com/AzureAD/microsoft-identity-web/wiki/token-cache-serialization).

### Add additional files to call Microsoft Graph

Add the `Microsoft.Identity.Web.GraphServiceClient` package, to use [Microsoft Graph SDK](https://github.com/microsoftgraph/msgraph-sdk-dotnet/blob/dev/docs/overview.md).

### Update the `Startup.cs` file to enable the Microsoft Graph custom service

Still in the `Startup.cs` file, add the following `AddMicrosoftGraph` extension method. This lines ensures that the GraphAPIService benefits from the optimized `HttpClient` management by ASP.NET Core.

```CSharp
    // Add Graph
    services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApp(Configuration.GetSection("AzureAd"))
        .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
        .AddMicrosoftGraph(Configuration.GetSection("DownstreamApi"))
        .AddInMemoryTokenCaches();
```

### Change the controller code to acquire a token and call Microsoft Graph

In the `Controllers\HomeController.cs` file:

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
    var me = await _graphServiceClient.Me.GetAsync();
    ViewData["Me"] = me;

    try
    {
        // Get user photo
        using (var photoStream = await _graphServiceClient.Me.Photo.Content.GetAsync())
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

## Optional - Handle Continuous Access Evaluation (CAE) challenge from Microsoft Graph
Continuous access evaluation (CAE) enables web APIs to do just-in time token validation, for instance enforcing user session revocation in the case of password change/reset but there are other benefits. For details, see [Continuous access evaluation](https://docs.microsoft.com/azure/active-directory/conditional-access/concept-continuous-access-evaluation).
Microsoft Graph is now CAE-enabled in Preview. This means that it can ask its clients for more claims when conditional access policies require it. Your can enable your application to be ready to consume CAE-enabled APIs by:
1. Declaring that the client app is capable of handling claims challenges from the web API.
2. Processing these challenges when thrown.
### Declare the CAE capability in the configuration
This sample declares that it's CAE-capable by adding a `ClientCapabilities` property in the configuration, whose value is `[ "cp1" ]`.
```Json
{
  "AzureAd": {
    // ...
    // the following is required to handle Continuous Access Evaluation challenges
    "ClientCapabilities": [ "cp1" ],
    // ...
  },
  // ...
}
```
### Process the CAE challenge from Microsoft Graph
To process the CAE challenge from Microsoft Graph, the controller actions need to extract it from the `wwwAuthenticate` header. It is returned when MS Graph rejects a seemingly valid Access tokens for MS Graph. For this you need to:
1. Inject and instance of `MicrosoftIdentityConsentAndConditionalAccessHandler` in the controller constructor. The beginning of the HomeController becomes:
   ```CSharp
   public class HomeController : Controller
   {
    private readonly ILogger<HomeController> _logger;
    private readonly GraphServiceClient _graphServiceClient;
    private readonly MicrosoftIdentityConsentAndConditionalAccessHandler _consentHandler;
    private string[] _graphScopes = new[] { "user.read" };
    public HomeController(ILogger<HomeController> logger,
                          IConfiguration configuration,
                          GraphServiceClient graphServiceClient,
                          MicrosoftIdentityConsentAndConditionalAccessHandler consentHandler)
    {
      _logger = logger;
      _graphServiceClient = graphServiceClient;
      this._consentHandler = consentHandler;
      // Capture the Scopes for Graph that were used in the original request for an Access token (AT) for MS Graph as
      // they'd be needed again when requesting a fresh AT for Graph during claims challenge processing
      _graphScopes = configuration.GetValue<string>("DownstreamApi:Scopes")?.Split(' ');
    }
    
    // more code here
    ```

1. The process to handle CAE challenges from MS Graph comprises of the following steps:
    1. Catch a Microsoft Graph SDK's `ServiceException` and extract the required `claims`. This is done by wrapping the call to Microsoft Graph into a try/catch block that processes the challenge:

    ```CSharp
    currentUser = await _graphServiceClient.Me.GetAsync();
    ```

    1. Then redirect the user back to Azure AD with the new requested `claims`. Azure AD will use this `claims` payload to discern what or if any additional processing is required, example being the user needs to sign-in again or do multi-factor authentication.

  ```CSharp
    try
    {
        currentUser = await _graphServiceClient.Me.GetAsync();
    }
    // Catch CAE exception from Graph SDK
    catch (ServiceException svcex) when (svcex.Message.Contains("Continuous access evaluation resulted in claims challenge"))
    {
      try
      {
        Console.WriteLine($"{svcex}");
        string claimChallenge = WwwAuthenticateParameters.GetClaimChallengeFromResponseHeaders(svcex.ResponseHeaders);
        _consentHandler.ChallengeUser(_graphScopes, claimChallenge);
        return new EmptyResult();
      }
      catch (Exception ex2)
      {
        _consentHandler.HandleException(ex2);
      }
    }        
  ```

   The `AuthenticationHeaderHelper` class is available from the `Helpers\AuthenticationHeaderHelper.cs file`.

## Next steps

- Learn how to enable distributed caches in [token cache serialization](../2-2-TokenCache)
- Learn how the same principle you've just learned can be used to call:
  - [several Microsoft APIs](../../3-WebApp-multi-APIs), which will enable you to learn how incremental consent and conditional access is managed in your Web App
  - 3rd party, or even [your own Web API](../../4-WebApp-your-API), which will enable you to learn about custom scopes

## Learn more

- Learn about [Microsoft.Identity.Web](https://github.com/AzureAD/microsoft-identity-web/wiki/Microsoft-Identity-Web-basics) works, in particular hooks-up to the ASP.NET Core OIDC events
- [Use HttpClientFactory to implement resilient HTTP requests](https://docs.microsoft.com/en-us/dotnet/standard/microservices-architecture/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests) used by the Graph custom service
