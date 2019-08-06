# Microsoft Identity Web

[![Build status](https://identitydivision.visualstudio.com/IDDP/_apis/build/status/AAD%20Samples/.NET%20client%20samples/ASP.NET%20Core%20Web%20App%20tutorial)](https://identitydivision.visualstudio.com/IDDP/_build/latest?definitionId=819)

This library contains a set of reusable classes useful in ASP.NET Core:

- [Web applications](#web-apps) that sign-in users, and, optionally, call web APIs
- [Protected web APIs](#web-apis), which, optionally call downstream web APIs

to enable them to work with the Microsoft identity platform (formerly named Azure AD v2.0 endpoint). In the library web apps and protected web APIs are collectively referred to as web resources.

## Web apps

As of today, ASP.NET Core web apps templates (`dot net new mvc -auth`) create web apps that sign-in users with the Azure AD v1.0 endpoint (allowing to sign-in users with their organizational accounts, also named Work or school accounts). This library brings `ServiceCollection` extension methods to be used in the ASP.NET Core web app **Startup.cs** file to enable the web app to sign-in users with the Microsoft identity platform (formerly Azure AD v2.0 endpoint), and, optionally enable the web app to call APIs on behalf of the signed-in user.

![image](https://user-images.githubusercontent.com/13203188/62526924-0a563780-b7ef-11e9-8ce0-db284db3f02c.png)

### Web apps that sign-in users - Startup.cs

To enable sign-in users with the Microsoft identity platform, all you need to do is, in your web application Startup.cs file, to replace the call to:

```CSharp
using Microsoft.Identity.Web;

public class Startup
{
  ...
  public void ConfigureServices(IServiceCollection services)
  {
   ...
   services.AddAuthentication(AzureADDefaults.AuthenticationScheme)
           .AddAzureAD(options => Configuration.Bind("AzureAd", options));
   ...
  }
  ...
}
```

by

```CSharp
using Microsoft.Identity.Web;

public class Startup
{
  ...
  public void ConfigureServices(IServiceCollection services)
  {
   ...
   services.AddAzureAdV2Authentication(Configuration);
   ...
  }
  ...
}
```

This adds authentication with the Microsoft Identity platform (formerly Azure AD v2.0), including validating the token in all scenarios (single tenant application, multi tenant applications, in Azure public cloud as well as national clouds)

See also:

- the [ASP.NET Core Web app incremental tutorial](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/tree/master/1-WebApp-OIDC/1-1-MyOrg) in chapter 1.1 (sign-in user in an organization)
- The [Web App that signs-in users](https://docs.microsoft.com/en-us/azure/active-directory/develop/scenario-web-app-sign-user-overview) scenario landing page in the Microsoft identity platform documentation and the following pages.

### Web apps that sign-in users and call web apis on behalf of the signed-in user - startup.cs

If, moreover you want your Web app to call web APIS, you'll need to add a line with `.AddMsal()`, and choose a token cache implementation, for instance `.AddInMemoryTokenCaches()`

```CSharp
using Microsoft.Identity.Web;

public class Startup
{
 const string scopesToRequest = "user.read";
  ...
  public void ConfigureServices(IServiceCollection services)
  {
   ...
   services.AddAzureAdV2Authentication(Configuration)
           .AddMsal(new string[] { scopesToRequest })
           .AddInMemoryTokenCaches();
   ...
  }
  ...
}
```

### Web app controller

For your web app to call web APIs on behalf of the signed-in user, you will need to add a parameter of type `ITokenAcquisition` to the constructor of your controller (the `ITokenAcquisition` service will be injected by dependency injection by ASP.NET Core)

![image](https://user-images.githubusercontent.com/13203188/62526943-14783600-b7ef-11e9-9913-ca79bf7a5cee.png)

```CSharp
using Microsoft.Identity.Web;

[Authorize]
public class HomeController : Controller
{
  readonly ITokenAcquisition tokenAcquisition;

  public HomeController(ITokenAcquisition tokenAcquisition)
  {
   this.tokenAcquisition = tokenAcquisition;
  }
  ...
```

Then in your controller actions, you will need to call: `ITokenAcquisition.GetAccessTokenOnBehalfOfUserAsync` passing the HttpContext of the controller, and the scopes for which to request a token. The other methods of ITokenAcquisition are used from the `AddMsal()` method and similar methods for web APIs (see below).

```CSharp
[Authorize]
public class HomeController : Controller
{
  readonly ITokenAcquisition tokenAcquisition;
  ...
  [MsalUiRequiredExceptionFilter(Scopes = new[] { "user.read" })]
  public async Task<IActionResult> Action()
  {
   string[] scopes = new []{"user.read"};
   string token = await tokenAcquisition.GetAccessTokenOnBehalfOfUserAsync(
                       HttpContext, scopes);
   ...
   // call the downstream API with the bearer token in the Authorize header
  }
```

The controller action is decorated by an attribute `MsalUiRequiredExceptionFilterAttribute` which enables to process the `MsalUiRequiredException` can could be thrown by the service implementing `ITokenAcquisition.GetAccessTokenOnBehalfOfUserAsync` so that the web app interacts with the user, and ask them to consent to the scopes, or re-sign-in if needed.

![image](https://user-images.githubusercontent.com/13203188/62526956-18a45380-b7ef-11e9-99f3-c75085d61ce5.png)

### Samples and documentation

You can see in details how the library is used in the following samples:

- [ASP.NET Core Web app incremental tutorial](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2) in chapter 2.1 ([call Microsoft Graph on behalf of signed in user](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/tree/master/2-WebApp-graph-user/2-1-Call-MSGraph))
- [ASP.NET Core Web app incremental tutorial](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2) in chapter 2.2 ([call Microsoft Graph on behalf of signed in user with a SQL token cache](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/tree/master/2-WebApp-graph-user/2-2-TokenCache))
- The [Web app that calls web apis](https://docs.microsoft.com/en-us/azure/active-directory/develop/scenario-web-app-sign-user-overview) scenario landing page in the Microsoft identity platform documentation

## Web APIs

The library also enables web APIs to work with the Microsoft identity platform, enabling them to process access tokens for both work and school and Microsoft personal accounts.

![image](https://user-images.githubusercontent.com/13203188/62526937-10e4af00-b7ef-11e9-9fee-c205c97653c5.png)

### Protected web APIS - Startup.cs

To enable the web API to accept tokens for the Microsoft identity platform, all you need to do is, in your web API Startup.cs file, to replace the call to:

```CSharp
using Microsoft.Identity.Web;

public class Startup
{
  ...
  public void ConfigureServices(IServiceCollection services)
  {
   ...
   services.AddAuthentication(AzureADDefaults.AuthenticationScheme)
           .AddAzureAdBearer(options => Configuration.Bind("AzureAd", options));
   ...
  }
  ...
}
```

by

```CSharp
using Microsoft.Identity.Web;

public class Startup
{
  ...
  public void ConfigureServices(IServiceCollection services)
  {
   ...
   services.AddProtectWebApiWithMicrosoftIdentityPlatformV2(Configuration);
   ...
  }
  ...
}
```

This enables your web API to be protected using the the Microsoft Identity platform (formerly Azure AD v2.0), including validating the token in all scenarios (single tenant application, multi tenant applications, in Azure public cloud as well as national clouds)

See also:

- the [ASP.NET Core Web API incremental tutorial](https://github.com/Azure-Samples/active-directory-dotnet-native-aspnetcore-v2) in chapter 1.1 ([Protect the web api](https://github.com/Azure-Samples/active-directory-dotnet-native-aspnetcore-v2/tree/master/1.%20Desktop%20app%20calls%20Web%20API))
- The [Protected web API](https://docs.microsoft.com/en-us/azure/active-directory/develop/scenario-protected-web-api-overview) scenario landing page in the Microsoft identity platform documentation and the following pages.

### Protected web APIs that call downstream APIs on behalf of a user - Startup.cs

If, moreover you want your web API to call downstream web APIS, you'll need to add lines with `.AddProtectedApiCallsWebApis()`, and choose a token cache implementation, for instance `.AddInMemoryTokenCaches()`

```CSharp
using Microsoft.Identity.Web;

public class Startup
{
  ...
  public void ConfigureServices(IServiceCollection services)
  {
   ...
   services.AddProtectWebApiWithMicrosoftIdentityPlatformV2(Configuration)
           .AddProtectedApiCallsWebApis()
           .AddInMemoryTokenCaches();
   ...
  }
  ...
}
```

If you are certain that your web API will need some specific scopes, you can optionally pass them as arguments to `AddProtectedApiCallsWebApis`.

### Web API controller

For your web API to call downstream APIs, you will need to:

- add (like in web apps), a parameter of type `ITokenAcquisition` to the constructor of your controller (the `ITokenAcquisition` service will be injected by dependency injection by ASP.NET Core)
- verify, in your controller actions, that the token contains the scopes expected by the action. For this you'll call the `VerifyUserHasAnyAcceptedScope` extension method on the `HttpContext`

  ![image](https://user-images.githubusercontent.com/13203188/62527104-60c37600-b7ef-11e9-8dcb-66bb982fe147.png)

- in your controller actions, to call: `ITokenAcquisition.GetAccessTokenOnBehalfOfUserAsync` passing the HttpContext of the controller, and the scopes for which to request a token.

The following code snippet shows these:

```CSharp
[Authorize]
public class HomeController : Controller
{
  readonly ITokenAcquisition tokenAcquisition;

  static string[] scopeRequiredByAPI = new string[] { "access_as_user" };
  ...
  public async Task<IActionResult> Action()
  {
   HttpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByAPI);
   string[] scopes = new []{"user.read"};
   try
   {
      string accessToken = await _tokenAcquisition.GetAccessTokenOnBehalfOfUser(HttpContext, scopes);
      // call the downstream API with the bearer token in the Authorize header
    }
    catch (MsalUiRequiredException ex)
    {
      _tokenAcquisition.ReplyForbiddenWithWwwAuthenticateHeader(HttpContext, scopes, ex);
    }
   ...
  }
```

#### Handle conditional access

It can happen that when your web api tries to get a token for the downstream API, the token acquisition service throws a `MsalUiRequiredException` meaning that the user on the client calling the web API needs to perform more actions such as multi-factor authentication. Given that the web API is not capable of doing interaction itself, this exception needs to be passed to the client. To propagate back this exception to the client, you can catch the exception and call the `ITokenAcquisition.ReplyForbiddenWithWwwAuthenticateHeader` method.

## Token cache serialization

For web apps calling web apis, as well as web api calling downstream APIs, the code snippets above show the use of the In Memory token cache serialization. The library proposes alternate token cache serialization methods:

| Extension Method | Microsoft.Identity.Web sub Namespace | Description  |
| ---------------- | --------- | ------------ |  
| `AddInMemoryTokenCaches` | `TokenCacheProviders.InMemory` | In memory token cache serialization. This is great in samples, and in production applications where you don't mind if the token cache is lost when the web app is restarted. `AddInMemoryTokenCaches` takes an optional parameter of type `MsalMemoryTokenCacheOptions` that enables you to specify the duration after which the cache entry will expire unless it's used.
| `AddSqlTokenCaches` | `TokenCacheProviders.Sql` | The token cache maintained in a SQL database. This is ideal for production applications which need to keep their token caches. AddSqlTokenCaches takes a parameter of type `MsalSqlTokenCacheOptions` that let you specify the SQL connection string  
| `AddSessionTokenCaches` | `TokenCacheProviders.Session` | The token cache is bound to the user session. This option is not ideal if the ID token is too big because it contains too many claims as the cookie would be too big.

## Other utilities

The library also contains additional classes which you might find useful.

### Troubleshooting your web app or web API

To troubleshoot your web app you can set the `subscribeToOpenIdConnectMiddlewareDiagnosticsEvents` optional boolean to `true` when you call `AddAzureAdV2Authentication`.



### ClaimsPrincipalExtensions

In web apps that sign-in users, ASP.NET Core transforms the claims in the IDToken to a `ClaimsPrincipal` instance, held by the `HttpContext.User` property. In the same way, in protected Web APIs, the claims from the Jwt bearer token used to call the API are available in `HttpContext.User`.

The library proposes extension methods to retrieve some of the relevant information about the user in the `ClaimsPrinciapExtensions` class:

### ClaimsPrincipalFactory



- Protect Web resources (in the `Resources` folder)
  - `AadIssuerValidator` is used to validate the issuer in multi-tenant applications, taking into account the aliases for authorities exising in Azure AD. This class works both for Azure AD (v1.0) and Microsoft identity platform (v2.0) web resources. You should not need to use it directly, as it's used by `AddAzureAdV2Authentication`
  - `OpenIdConnectMiddlewareDiagnostics` helps you understand what happens in the Open Id Connect Middleware. This is a diagnostics class that can help you troubleshooting your Web apps.
  - `ClaimsPrincipalExtensions` provides a set of extension methods on `ClaimsPrincipal` helping getting information from the signed-in user. It's used in the other classes of the libraries.

## Learn more how the library works

You can learn more about the tokens by looking at the following articles in MSAL.NET's conceptual documentation:

- The [Authorization code flow](https://aka.ms/msal-net-authorization-code), which is used, after the user signed-in with Open ID Connect, in order to get a token and cache it for a later use. See [TokenAcquisition L 107](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/blob/f99e913cc032e16c59b748241111e97108e87918/Extensions/TokenAcquisition.cs#L107) for details of this code
- [AcquireTokenSilent](https://aka.ms/msal-net-acquiretokensilent ), which is used by the controller to get an access token for the downstream API. See [TokenAcquisition L 168](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/blob/f99e913cc032e16c59b748241111e97108e87918/Extensions/TokenAcquisition.cs#L168) for details of this code
- [Token cache serialization](msal-net-token-cache-serialization)

The token validation is performed by the classes of the [Identity Model Extensions for DotNet](https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet) library. Learn about customizing
token validation by reading:

- [Validating Tokens](https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet/wiki/ValidatingTokens) in that library's conceptual documentation
- [TokenValidationParameters](https://docs.microsoft.com/en-us/dotnet/api/microsoft.identitymodel.tokens.tokenvalidationparameters?view=azure-dotnet)'s reference documentation.
