---
services: active-directory
platforms: dotnet
author: kalyankrishna1
level: 200
client: ASP.NET Core 2.x Web App
service: Microsoft Graph
endpoint: Microsoft identity platform
---

# Call the Microsoft Graph API from an An ASP.NET Core 2.x Web App, using Sql Server for caching tokens

[![Build status](https://identitydivision.visualstudio.com/IDDP/_apis/build/status/AAD%20Samples/.NET%20client%20samples/ASP.NET%20Core%20Web%20App%20tutorial)](https://identitydivision.visualstudio.com/IDDP/_build/latest?definitionId=819)

## Scenario

Starting from a .NET Core 2.2 MVC Web app that uses OpenID Connect to sign in users, this chapter of the tutorial shows how to make a call to Microsoft Graph `/me` endpoint on behalf of the signed-in user. This sample additionally provides instructions on how to use Sql Server for caching tokens.

It leverages the ASP.NET Core OpenID Connect middleware and Microsoft Authentication Library for .NET (MSAL.NET). The complexities of the library's integration with the ASP.NET Core dependency Injection patterns is encapsultated into the `Microsoft.Identity.Web` library project, which is a part of this tutorial.

![Sign in with the Microsoft identity platform](ReadmeFiles/sign-in.png)

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
git clone https://github.com/Azure-Samples/microsoft-identity-platform-aspnetcore-webapp-tutorial webapp
cd webapp
```
> Given that the name of the sample is pretty long, and so are the name of the referenced NuGet packages, you might want to clone it in a folder close to the root of your hard drive, to avoid file size limitations on Windows.

Go to the `"2-WebApp-graph-user\2-2-TokenCache"` folder

 ```Sh
  cd "2-WebApp-graph-user\2-2-TokenCache"
  ```

#### In the appsettings.json file, configure a Sql server database for token caching, if you have not already done so:

1. In the `TokenCacheDbConnStr` key, provide the Sql server connection string to the database you wish to use for token caching.
   > Note:
   > If you want to test this sample locally with Visual Studio, you might want to use localdb, which is installed with Visual Studio.
   > In that case, use the following connection string:
   >
   > ```XML
   >  "ConnectionStrings": {
   >   "TokenCacheDbConnStr": "Data Source=(LocalDb)\\MSSQLLocalDB;Database=MY_TOKEN_CACHE_DATABASE;Trusted_Connection=True;"
   > },
   > ```
1. If you do not have an existing database and tables needed for token caching, this sample can use  [EF Core- code first](https://docs.microsoft.com/en-us/ef/core/get-started/aspnetcore/new-db?tabs=visual-studio) to create a database and tables for you. to do that, follow the steps below.
    1. In the file `Microsoft.Identity.Web\Client\TokenCacheProviders\Sql\MSALAppSqlTokenCacheProviderExtension.cs`, uncomment the code under the **// Uncomment the following lines to create the database.**. This comment exists once in the **AddSqlAppTokenCache** and **AddSqlPerUserTokenCache**  methods.
    1. Run the solution again, when a user signs-in the very first time, the Entity Framework will create the database and tables  `AppTokenCache` and `UserTokenCache` for app and user token caching respectively.

- In case you want to deploy your app in Sovereign or national clouds, ensure the `GraphApiUrl` option matches the one you want. By default this is Microsoft Graph in the Azure public cloud

  ```JSon
   "GraphApiUrl": "https://graph.microsoft.com"
  ```


### Step 3: Run the sample

1. Clean the solution, rebuild the solution, and run it. 

2. Open your web browser and make a request to the app. The app immediately attempts to authenticate you via the Microsoft identity platform endpoint. Sign in with your personal account or with a work or school account.

3. Go to the **Profile** page, you should now see all kind of information about yourself as well as your picture (a call was made to the Microsoft Graph */me* endpoint)

> Did the sample not work for you as expected? Did you encounter issues trying this sample? Then please reach out to us using the [GitHub Issues](../../../../issues) page.

## About The code

Starting from the [previous phase of the tutorial](../../2-WebApp-graph-user/2-1-Call-MSGraph), the code was incrementally updated with the following steps:

### Update the `Startup.cs` file to enable Token caching using Sql database.

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
1. The last two lines hook up the Sql server database based token caching solution to MSAL.NET. The Sql based token cache requires a **Connection string** named `TokenCacheDbConnStr` available in the **ConnectionStrings** collections of the **appsettings.json** configuration file. 

The files `MSALAppSqlTokenCacheProvider.cs` and `MSALPerUserSqlTokenCacheProvider` of the `Microsoft.Identity.Web` project contains the app and per-user token cache implementations that use Sql server as the token cache.

## Next steps

- Learn how to enable distributed caches in [token cache serialization](../2.2.%20token%20cache%20serialization)
- Learn how the same principle you've just learnt can be used to call:
  - [several Microsoft APIs](../../3-WebApp-multi-APIs), which will enable you to learn how incremental consent and conditional access is managed in your Web App
  - 3rd party, or even [your own Web API](../../4-WebApp-your-API), which will enable you to learn about custom scopes

## Learn more

- Learn how [Microsoft.Identity.Web](../../Microsoft.Identity.Web) works, in particular hooks-up to the ASP.NET Core ODIC events
- [Use HttpClientFactory to implement resilient HTTP requests](https://docs.microsoft.com/en-us/dotnet/standard/microservices-architecture/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests) used by the Graph custom service
