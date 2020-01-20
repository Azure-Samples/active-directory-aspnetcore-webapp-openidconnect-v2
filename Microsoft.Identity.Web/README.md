# Microsoft Identity Web

[![Build status](https://identitydivision.visualstudio.com/IDDP/_apis/build/status/AAD%20Samples/.NET%20client%20samples/ASP.NET%20Core%20Web%20App%20tutorial)](https://identitydivision.visualstudio.com/IDDP/_build/latest?definitionId=819)

This library contains a set of reusable classes useful in ASP.NET Core:

- [Web applications](#web-apps) that sign in users, and, optionally, call web APIs
- [Protected web APIs](#web-apis), which, optionally call downstream web APIs

to enable them to work with the Microsoft identity platform (formerly named Azure AD v2.0 endpoint). In the library, web apps and protected web APIs are collectively referred to as web resources.

## Web apps

As of today, ASP.NET Core web apps templates (`dot net new mvc -auth`) create web apps that sign in users with the Azure AD v1.0 endpoint (allowing to sign in users with their organizational accounts, also named *Work or school accounts*). This library brings `ServiceCollection` extension methods to be used in the ASP.NET Core web app **Startup.cs** file to enable the web app to sign in users with the Microsoft identity platform (formerly Azure AD v2.0 endpoint), and, optionally enable the web app to call APIs on behalf of the signed-in user.

![WebAppServiceCollectionExtensions](https://user-images.githubusercontent.com/13203188/64252959-82ae3680-cf1c-11e9-8a01-0a0be728a78e.png)

### Web apps that sign in users - Startup.cs

To enable users to sign in with the Microsoft identity platform, you need to replace, in your web application Startup.cs file, the following code:

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
   services.AddMicrosoftIdentityPlatformAuthentication(Configuration);
   ...
  }
  ...
}
```

This method adds authentication with the Microsoft Identity platform (formerly Azure AD v2.0). This includes validating the token in all scenarios (single tenant application, multi tenant applications) in Azure public cloud as well as national clouds.

See also:

- the [ASP.NET Core Web app incremental tutorial](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/tree/master/1-WebApp-OIDC/1-1-MyOrg) in chapter 1.1 (sign-in user in an organization)
- The [Web App that signs-in users](https://docs.microsoft.com/azure/active-directory/develop/scenario-web-app-sign-user-overview) scenario landing page in the Microsoft identity platform documentation and the following pages.

### Web apps that sign in users and call web apis on behalf of the signed-in user - startup.cs

If moreover you want your Web app to call web APIS, you'll need to add a line with `.AddMsal()`, and choose a token cache implementation, for instance `.AddInMemoryTokenCaches()`

```CSharp
using Microsoft.Identity.Web;

public class Startup
{
 const string scopesToRequest = "user.read";
  ...
  public void ConfigureServices(IServiceCollection services)
  {
   ...
   services.AddMicrosoftIdentityPlatformAuthentication(Configuration)
           .AddMsal(new string[] { scopesToRequest })
           .AddInMemoryTokenCaches();
   ...
  }
  ...
}
```

Note that by default, `AddMicrosoftIdentityPlatformAuthentication` gets the configuration from the "AzureAD" section of the configuration files. It has
several parameters that you can change.

Also the proposed token cache serialization is in memory. you can also use the session cache, or various distributed caches

### Web app controller

For your web app to call web APIs on behalf of the signed-in user, you'll need to add a parameter of type `ITokenAcquisition` to the constructor of your controller (the `ITokenAcquisition` service will be injected by dependency injection by ASP.NET Core)

![ITokenAcquisition](https://user-images.githubusercontent.com/13203188/62526943-14783600-b7ef-11e9-9913-ca79bf7a5cee.png)

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

Then in your controller actions, you'll need to call: `ITokenAcquisition.GetAccessTokenOnBehalfOfUserAsync` passing the scopes for which to request a token. The other methods of ITokenAcquisition are used from the `AddMsal()` method and similar methods for web APIs (see below).

```CSharp
[Authorize]
public class HomeController : Controller
{
  readonly ITokenAcquisition tokenAcquisition;
  ...
  [AuthorizeForScopes(Scopes = new[] { "user.read" })]
  public async Task<IActionResult> Action()
  {
   string[] scopes = new []{"user.read"};
   string token = await tokenAcquisition.GetAccessTokenOnBehalfOfUserAsync(scopes);
   ...
   // call the downstream API with the bearer token in the Authorize header
  }
```

The controller action is decorated by an attribute `AuthorizeForScopesAttribute` which enables to process the `MsalUiRequiredException` that could be thrown by the service implementing `ITokenAcquisition.GetAccessTokenOnBehalfOfUserAsync` so that the web app interacts with the user, and ask them to consent to the scopes, or re-sign-in if needed.

<img alt="AuthorizeForScopesAttribute" src="https://user-images.githubusercontent.com/13203188/64253212-0bc56d80-cf1d-11e9-9666-2e72b78886ed.png" width="50%"/>

### Samples and documentation

You can see in details how the library is used in the following samples:

- [ASP.NET Core Web app incremental tutorial](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2) in chapter 2.1 ([call Microsoft Graph on behalf of signed in user](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/tree/master/2-WebApp-graph-user/2-1-Call-MSGraph))
- [ASP.NET Core Web app incremental tutorial](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2) in chapter 2.2 ([call Microsoft Graph on behalf of signed in user with a SQL token cache](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/tree/master/2-WebApp-graph-user/2-2-TokenCache))
- The [Web app that calls web apis](https://docs.microsoft.com/azure/active-directory/develop/scenario-web-app-sign-user-overview) scenario landing page in the Microsoft identity platform documentation

## Web APIs

The library also enables web APIs to work with the Microsoft identity platform, enabling them to process access tokens for both work and school and Microsoft personal accounts.

![image](https://user-images.githubusercontent.com/13203188/64253058-ba1ce300-cf1c-11e9-8f01-88180fc0faed.png)

### Protected web APIS - Startup.cs

To enable the web API to accept tokens emitted by  the Microsoft identity platform, you need to replace, in your web API Startup.cs file, the call to:

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
   services.AddProtectedWebApi(Configuration);
   ...
  }
  ...
}
```

This method enables your web API to be protected using the Microsoft Identity platform (formerly Azure AD v2.0). It takes care of validating the token in all scenarios (single tenant application, multi tenant applications), in Azure public cloud as well as national clouds.

See also:

- the [ASP.NET Core Web API incremental tutorial](https://github.com/Azure-Samples/active-directory-dotnet-native-aspnetcore-v2) in chapter 1.1 ([Protect the web api](https://github.com/Azure-Samples/active-directory-dotnet-native-aspnetcore-v2/tree/master/1.%20Desktop%20app%20calls%20Web%20API))
- The [Protected web API](https://docs.microsoft.com/azure/active-directory/develop/scenario-protected-web-api-overview) scenario landing page in the Microsoft identity platform documentation and the following pages.

### Protected web APIs that call downstream APIs on behalf of a user - Startup.cs

If moreover you want your web API to call downstream web APIS, you'll need to add lines with `.AddProtectedApiCallsWebApis()`, and choose a token cache implementation, for instance `.AddInMemoryTokenCaches()`

```CSharp
using Microsoft.Identity.Web;

public class Startup
{
  ...
  public void ConfigureServices(IServiceCollection services)
  {
   ...
   services.AddProtectedWebApi(Configuration)
           .AddProtectedApiCallsWebApis()
           .AddInMemoryTokenCaches();
   ...
  }
  ...
}
```

Like for Web Apps, you can choose various token cache implementations.

If you're certain that your web API will need some specific scopes, you can optionally pass them as arguments to `AddProtectedApiCallsWebApis`.

### Web API controller

For your web API to call downstream APIs, you'll need to:

- add (like in web apps), a parameter of type `ITokenAcquisition` to the constructor of your controller (the `ITokenAcquisition` service will be injected by dependency injection by ASP.NET Core)
- verify, in your controller actions, that the token contains the scopes expected by the action. For this, you'll call the `VerifyUserHasAnyAcceptedScope` extension method on the `HttpContext`

  <img alt="ScopesRequiredHttpContextExtensions" src="https://user-images.githubusercontent.com/13203188/64253176-f9e3ca80-cf1c-11e9-8fe9-df06cee11c25.png" width="80%"/>

- in your controller actions, to call: `ITokenAcquisition.GetAccessTokenOnBehalfOfUserAsync` passing the scopes for which to request a token.

The following code snippet shows how to combine these steps:

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
      string accessToken = await _tokenAcquisition.GetAccessTokenOnBehalfOfUser(scopes);
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

It can happen that when your web api tries to get a token for the downstream API, the token acquisition service throws a `MsalUiRequiredException` meaning that the user on the client calling the web API needs to perform more actions such as multi-factor authentication. Given that the web API isn't capable of doing interaction itself, this exception needs to be passed to the client. To propagate back this exception to the client, you can catch the exception and call the `ITokenAcquisition.ReplyForbiddenWithWwwAuthenticateHeader` method.

## Token cache serialization

For web apps that calls web apis, and web APIs that call downstream APIs, the code snippets above show the use of the In Memory token cache serialization. The library proposes alternate token cache serialization methods:

| Extension Method | Microsoft.Identity.Web sub Namespace | Description  |
| ---------------- | --------- | ------------ |  
| `AddInMemoryTokenCaches` | `TokenCacheProviders.InMemory` | In memory token cache serialization. This implementation is great in samples. It's also good in production applications provided you don't mind if the token cache is lost when the web app is restarted. `AddInMemoryTokenCaches` takes an optional parameter of type `MsalMemoryTokenCacheOptions` that enables you to specify the duration after which the cache entry will expire unless it's used.
| `AddSessionTokenCaches` | `TokenCacheProviders.Session` | The token cache is bound to the user session. This option isn't ideal if the ID token is too large because it contains too many claims as the cookie would be too large.
| `AddDistributedTokenCaches` | `TokenCacheProviders.Distributed` | The token cache is an adapter against the ASP.NET Core `IDistributedCache` implementation, therefore enabling you to choose between a distributed memory cache, a Redis cache, or a SQL Server cache. For details about the IDistributedCache` implementations, see https://docs.microsoft.com/aspnet/core/performance/caching/distributed?view=aspnetcore-2.2#distributed-memory-cache.

Examples of possible distributed cache:

```CSharp
// or use a distributed Token Cache by adding 
    services.AddMicrosoftIdentityPlatformAuthentication(Configuration)
            .AddMsal(new string[] { scopesToRequest })
            .AddDistributedTokenCaches();

// and then choose your implementation

// For instance the distributed in memory cache (not cleared when you stop the app)
services.AddDistributedMemoryCache()

// Or a Redis cache
services.AddStackExchangeRedisCache(options =>
{
 options.Configuration = "localhost";
 options.InstanceName = "SampleInstance";
});

// Or even a SQL Server token cache
services.AddDistributedSqlServerCache(options =>
{
 options.ConnectionString = _config["DistCache_ConnectionString"];
 options.SchemaName = "dbo";
 options.TableName = "TestCache";
});
```

## Other utility classes

The library also contains additional classes that you might find useful.

### ClaimsPrincipalExtensions

In web apps that sign in users, ASP.NET Core transforms the claims in the IDToken to a `ClaimsPrincipal` instance, held by the `HttpContext.User` property. In the same way, in protected Web APIs, the claims from the Jwt bearer token used to call the API are available in `HttpContext.User`.

The library proposes extension methods to retrieve some of the relevant information about the user in the `ClaimsPrincipalExtensions` class.

<img alt="ClaimsPrincipalExtensions" src="https://user-images.githubusercontent.com/13203188/62538243-2bc31d80-b807-11e9-8689-085c5dc78f7e.png" width="60%"/>

If you want to implement your own token cache serialization, you might want to use this class, for instance to get the key of the token cache to serialize (typically `GetMsalAccountId()`)

### ClaimsPrincipalFactory

In the other direction `ClaimsPrincipalFactory` instantiates a `ClaimsPrincipal` from an account objectId and tenantId. These methods can be useful when the web app or the web API subscribes to another service on behalf of the user, and then is called back by a notification where the users are identified by only their tenant ID and object ID. This is, for instance the case of [Microsoft Graph Web Hooks](https://docs.microsoft.com/graph/api/resources/webhooks) [notifications](https://docs.microsoft.com/graph/webhooks#notification-example). 

<img alt="ClaimsPrincipalFactory" src="https://user-images.githubusercontent.com/13203188/62538251-2fef3b00-b807-11e9-912f-2674972e9f48.png" width="70%"/>

### AccountExtensions

Finally, you can create a `ClaimsPrincipal` from an instance of MSAL.NET `IAccount`, using the   `ToClaimsPrincipal` method in `AccountExtensions`.

<img alt="AccountExtensions" src="https://user-images.githubusercontent.com/13203188/62538259-341b5880-b807-11e9-9328-a094f79a0874.png" width="60%"/>

### Troubleshooting your web app or web API

In order to troubleshoot your web app you can set the `subscribeToOpenIdConnectMiddlewareDiagnosticsEvents` optional boolean to `true` when you call `AddMicrosoftIdentityPlatformAuthentication`. This will display on the output window the progression of the OpenID connect message through OpenID Connect middleware (from the reception of the message from Azure Active directory to the availability of the user identity in `HttpContext.User`)  

<img alt="OpenIdConnectMiddlewareDiagnostics" src="https://user-images.githubusercontent.com/13203188/62538366-75ac0380-b807-11e9-9ce0-d0eec9381b78.png" width="75%"/>

In order to troubleshoot your web API you can set the `subscribeToJwtBearerMiddlewareDiagnosticsEvents` optional boolean to `true` when you call `AddProtectedWebApi`. Enabling these diagnostics will display on the output window the progression of the OAuth 2.0 message through the JWTBearer middleware (from the reception of the message from Azure Active directory to the availability of the user identity in `HttpContext.User`)  

<img alt="JwtBearerMiddlewareDiagnostics" src="https://user-images.githubusercontent.com/13203188/62538382-7d6ba800-b807-11e9-9540-560e7129197b.png" width="65%"/>

In both cases, you can set a breakpoint in the methods of the  `OpenIdConnectMiddlewareDiagnostics` and `JwtBearerMiddlewareDiagnostics` classes respectively to observe values under the debugger.

## More customizations

If you want to customize the `OpenIdConnectOption` or `JwtBearerOption` but still want to benefit from the implementation provided by Microsoft.Identity.Web, you can easily do it from your `Startup.cs` file:

Let's take for example the method `AddProtectedWebApi`. If you check the code inside it, you have this event setup:

```
options.Events.OnTokenValidated = async context =>
{
    // This check is required to ensure that the Web API only accepts tokens from tenants where it has been consented and provisioned.
    if (!context.Principal.Claims.Any(x => x.Type == ClaimConstants.Scope)
    && !context.Principal.Claims.Any(y => y.Type == ClaimConstants.Scp)
    && !context.Principal.Claims.Any(y => y.Type == ClaimConstants.Roles))
    {
         throw new UnauthorizedAccessException("Neither scope or roles claim was found in the bearer token.");
    }

    await Task.FromResult(0);
};
```

Let's say you want to augment the current `ClaimsPrincipal` by adding claims to it, and you have to do it on `OnTokenValidated `, however you don't want to lose this `UnauthorizedAccessException` check existing in the event. To do so, on your `Startup.cs` you would have:

```
services.AddProtectedWebApi(Configuration);
services.Configure<JwtBearerOptions>(AzureADDefaults.JwtBearerAuthenticationScheme, options => 
{
  var existingOnTokenValidatedHandler = options.Events.OnTokenValidated ;
  options.Events.OnTokenValidated = async context =>
  {
       await existingOnTokenValidatedHandler(context);
      // your code to add extra claims that will be executed after the current event implementation.
  }    
}

```

## Learn more about how the library works

You can learn more about the tokens by looking at the following articles in MSAL.NET's conceptual documentation:

- The [Authorization code flow](https://aka.ms/msal-net-authorization-code), which is used, after the user signed-in with Open ID Connect, in order to get a token and cache it for a later use. See [TokenAcquisition L 107](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/blob/f99e913cc032e16c59b748241111e97108e87918/Extensions/TokenAcquisition.cs#L107) for details of this code
- [AcquireTokenSilent](https://aka.ms/msal-net-acquiretokensilent), which is used by the controller to get an access token for the downstream API. See [TokenAcquisition L 168](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/blob/f99e913cc032e16c59b748241111e97108e87918/Extensions/TokenAcquisition.cs#L168) for details of this code
- [Token cache serialization](msal-net-token-cache-serialization)

The token validation is performed by the classes of the [Identity Model Extensions for DotNet](https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet) library. Learn about customizing
token validation by reading:

- [Validating Tokens](https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet/wiki/ValidatingTokens) in that library's conceptual documentation
- [TokenValidationParameters](https://docs.microsoft.com/dotnet/api/microsoft.identitymodel.tokens.tokenvalidationparameters?view=azure-dotnet)'s reference documentation.
