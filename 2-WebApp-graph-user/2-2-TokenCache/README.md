---
services: active-directory
platforms: dotnet
author: kalyankrishna1
level: 200
client: ASP.NET Core Web App
service: Microsoft Graph
endpoint: Microsoft identity platform
---

# Call the Microsoft Graph API from an An ASP.NET Core Web App, using Sql Server or Redis for caching tokens

## About this sample

[![Build status](https://identitydivision.visualstudio.com/IDDP/_apis/build/status/AAD%20Samples/.NET%20client%20samples/ASP.NET%20Core%20Web%20App%20tutorial)](https://identitydivision.visualstudio.com/IDDP/_build/latest?definitionId=819)

## Scenario

Starting from a .NET Core MVC Web app that uses OpenID Connect to sign in users, this chapter of the tutorial shows how to make a call to Microsoft Graph `/me` endpoint on behalf of the signed-in user. This sample additionally provides instructions on how to use Sql Server or Redis for caching tokens.

It leverages the ASP.NET Core OpenID Connect middleware and Microsoft Authentication Library for .NET (MSAL.NET). The complexities of the library's integration with the ASP.NET Core dependency Injection patterns is encapsulated into the `Microsoft.Identity.Web` library project, which is a part of this tutorial.

![Sign in with the Microsoft identity platform](ReadmeFiles/sign-in.png)

## How to run this sample

To run this sample, you'll need:

- [Visual Studio](https://aka.ms/vsdownload) or just the [.NET Core SDK](https://www.microsoft.com/net/learn/get-started)
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

or download and extract the repository .zip file.

> Given that the name of the sample is quiet long, and so are the names of the referenced NuGet packages, you might want to clone it in a folder close to the root of your hard drive, to avoid file size limitations on Windows.

### Step 2:  Register the sample application with your Azure Active Directory tenant

There is one project in this sample. To register it, you can:

- either follow the steps [Step 2: Register the sample with your Azure Active Directory tenant](#step-2-register-the-sample-with-your-azure-active-directory-tenant) and [Step 3:  Configure the sample to use your Azure AD tenant](#choose-the-azure-ad-tenant-where-you-want-to-create-your-applications)
- or use PowerShell scripts that:
  - **automatically** creates the Azure AD applications and related objects (passwords, permissions, dependencies) for you
  - modify the Visual Studio projects' configuration files.

If you want to use this automation:

1. On Windows, run PowerShell and navigate to the root of the cloned directory
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

1. Open the Visual Studio solution and click start to run the code.

If you don't want to use this automation, follow the steps below.

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
   - Change **Supported account types** to **Accounts in any organizational directory and personal Microsoft accounts (e.g. Skype, Xbox, Outlook.com)**.
     > Note that there are more than one redirect URIs. You'll need to add them from the **Authentication** tab later after the app has been created successfully.
1. Select **Register** to create the application.
1. On the app **Overview** page, find the **Application (client) ID** value and record it for later. You'll need it to configure the Visual Studio configuration file for this project.
1. From the app's Overview page, select the **Authentication** section.
   - In the Redirect URIs section, select **Web** in the combo-box and enter the following redirect URIs.
       - `https://localhost:44321/`
       - `https://localhost:44321/signin-oidc`
   - In the **Advanced settings** section set **Logout URL** to `https://localhost:44321/signout-oidc`
   - In the **Advanced settings** | **Implicit grant** section, check **ID tokens** as this sample requires
     the [ID Token](https://docs.microsoft.com/azure/active-directory/develop/id-tokens) to be enabled to
     sign-in the user, and call an API.
1. Select **Save**.
1. From the **Certificates & secrets** page, in the **Client secrets** section, choose **New client secret**:

   - Type a key description (of instance `app secret`),
   - Select a key duration of either **In 1 year**, **In 2 years**, or **Never Expires**.
   - When you press the **Add** button, the key value will be displayed, copy, and save the value in a safe location.
   - You'll need this key later to configure the project in Visual Studio. This key value will not be displayed again, nor retrievable by any other means,
     so record it as soon as it is visible from the Azure portal.
1. Select the **API permissions** section
   - Click the **Add a permission** button and then,
   - Ensure that the **Microsoft APIs** tab is selected
   - In the *Commonly used Microsoft APIs* section, click on **Microsoft Graph**
   - In the **Delegated permissions** section, ensure that the right permissions are checked: **User.Read**. Use the search box if necessary.
   - Select the **Add permissions** button

### Step 3:  Configure the sample to use your Azure AD tenant

In the steps below, "ClientID" is the same as "Application ID" or "AppId".

Open the solution in Visual Studio to configure the projects

#### Configure the webApp project

Note: if you had used the automation to setup your application mentioned in [Step 2:  Register the sample application with your Azure Active Directory tenant](#step-2-register-the-sample-application-with-your-azure-active-directory-tenant), the changes below would have been applied by the scripts.

1. Open the `appsettings.json` file
1. Find the app key `ClientId` and replace the existing value with the application ID (clientId) of the `WebApp-OpenIDConnect-DotNet-code-v2` application copied from the Azure portal.
1. Find the app key `TenantId` and replace the existing value with your Azure AD tenant ID.
1. Find the app key `Domain` and replace the existing value with your Azure AD tenant name.
1. Find the app key `ClientSecret` and replace the existing value with the key you saved during the creation of the `WebApp-OpenIDConnect-DotNet-code-v2` app, in the Azure portal.

#### In the appsettings.json file, configure a Sql server database for token caching:

1. In the `TokenCacheDbConnStr` key, provide the Sql server connection string to the database you wish to use for token caching.
   > Note:
   > If you want to test this sample locally with Visual Studio, you might want to use localdb, which is installed with Visual Studio.
   > In that case, use the following connection string:
   >
   > ```XML
   > "ConnectionStrings": {
   >   "TokenCacheDbConnStr": "Data Source=(LocalDb)\\MSSQLLocalDB;Database=MY_TOKEN_CACHE_DATABASE;Trusted_Connection=True;"
   >   // Rest of strings...
   > },
   > ```
   >

1. If you do not have an existing database and tables needed for token caching, you can execute the `dotnet sql-cache create` command to create the table for you. To do that, follow the steps below.
   > 1. In Visual Studio, open the SQL Server Object Explorer, then (localdb)\MSSQLLocalDB
   > 2. Right click on Databases and select "Add New database", and then choose the name of the database: 'MY_TOKEN_CACHE_DATABASE'
    ```powershell
    dotnet tool install --global dotnet-sql-cache
    dotnet sql-cache create "Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=MY_TOKEN_CACHE_DATABASE;Integrated Security=True;" dbo TokenCache
    ```

#### [OPTIONAL] In the appsettings.json file, configure a Redis instance for token caching:

1. In the `TokenCacheRedisConnStr` key, provide the Redis domain for your instance or `localhost` if testing on a local Redis database. Replace the value of the `TokenCacheRedisInstaceName` key with the name of your instance.
   > Note:
   > ```XML
   >"ConnectionStrings": {
   >   // Rest of strings...
   >  "TokenCacheRedisConnStr": "[Replace with your domain like so: <your-domain>.redis.cache.windows.net:6380,password=SomeLongPassword,ssl=True,abortConnect=False or with 'localhost' if running locally]",
   >  "TokenCacheRedisInstaceName": "[Replace with your instance name]"
   > },
   > ```

- In case you want to deploy your app in Sovereign or national clouds, ensure the `GraphApiUrl` option matches the one you want. By default this is Microsoft Graph in the Azure public cloud

  ```JSon
   "GraphApiUrl": "https://graph.microsoft.com"
  ```

### Step 4: Run the sample

1. Clean the solution, rebuild the solution, and run it. 

2. Open your web browser and make a request to the app. The app immediately attempts to authenticate you via the Microsoft identity platform endpoint. Sign in with your personal account or with a work or school account.

3. Go to the **Profile** page, you should now see all kind of information about yourself as well as your picture (a call was made to the Microsoft Graph */me* endpoint)

> Did the sample not work for you as expected? Did you encounter issues trying this sample? Then please reach out to us using the [GitHub Issues](../../../../issues) page.

> [Consider taking a moment to share your experience with us.](https://forms.office.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbRz0h_jLR5HNJlvkZAewyoWxUNEFCQ0FSMFlPQTJURkJZMTRZWVJRNkdRMC4u)

## About the code

Starting from the [previous phase of the tutorial](../../2-WebApp-graph-user/2-1-Call-MSGraph), the code was incrementally updated with the following steps:

### Reference Microsoft.Extensions.Caching.SqlServer

This sample can use a distributed SQL token cache. To use it, you'll need to add a reference to the `Microsoft.Extensions.Caching.SqlServer` NuGet package

### Update the `Startup.cs` file to enable Token caching using Sql database

```CSharp
public void ConfigureServices(IServiceCollection services)
{
    . . .
    services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApp(Configuration.GetSection("AzureAd"))
        .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
        .AddMicrosoftGraph(Configuration.GetSection("DownstreamApi"))
        .AddDistributedTokenCaches();

    services.AddDistributedSqlServerCache(options =>
    {
        options.ConnectionString = Configuration.GetConnectionString("TokenCacheDbConnStr");
        options.SchemaName = "dbo";
        options.TableName = "TokenCache";

       // You don't want the SQL token cache to be purged before the access token has expired. Usually
       // access tokens expire after 1 hour (but this can be changed by token lifetime policies), whereas
       // the default sliding expiration for the distributed SQL database is 20 mins. 
       // Use a value which is above 60 mins (or the lifetime of a token in case of longer lived tokens)
       options.DefaultSlidingExpiration = TimeSpan.FromMinutes(90); 
    });
```

1. The first two lines enable MSAL.NET to hook-up to the OpenID Connect events to redeem the authorization code obtained by the ASP.NET Core middleware. After obtaining a token for Microsoft Graph, it saves it into the token cache, for use by the Controllers.
1. The last two lines hook up the Sql server database based token caching solution to MSAL.NET. The Sql based token cache requires a **Connection string** named `TokenCacheDbConnStr` available in the **ConnectionStrings** collections of the **appsettings.json** configuration file.

### Reference Microsoft.Extensions.Caching.StackExchangeRedis

This sample also has an optional distributed Redis token cache. To use it, you'll need to add a reference to the `Microsoft.Extensions.Caching.StackExchangeRedis` NuGet package

### Update the `Startup.cs` file to enable Token caching using Redis

```CSharp
// Uncomment for Redis configuration. Be sure you DO NOT ADD BOTH an SQL cache and Redis cache
services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = Configuration.GetConnectionString("TokenCacheRedisConnStr");
    options.InstanceName = Configuration.GetConnectionString("TokenCacheRedisInstanceName");
});
```

1. The code above hooks up the Redis database based token caching solution to MSAL.NET. The Redis based token cache requires a **Connection string** named `TokenCacheRedisConnStr` to connect to the Redis database and a **Connection string** named `TokenCacheRedisInstanceName` to name the instance tokens are stored to available in the **ConnectionStrings** collections of the **appsettings.json** configuration file.

## Next steps

- Learn how to enable distributed caches in [token cache serialization](../2.2.%20token%20cache%20serialization)
- Learn more about the [Distributed SQL Server Cache](https://docs.microsoft.com/aspnet/core/performance/caching/distributed#distributed-sql-server-cache)
- Learn more about the [Distributed Redis Cache](https://learn.microsoft.com/azure/azure-cache-for-redis/cache-overview)
- Learn how the same principle you've just learnt can be used to call:
  - [several Microsoft APIs](../../3-WebApp-multi-APIs), which will enable you to learn how incremental consent and conditional access is managed in your Web App
  - 3rd party, or even [your own Web API](../../4-WebApp-your-API), which will enable you to learn about custom scopes

## Learn more

- Learn how [Microsoft.Identity.Web](../../Microsoft.Identity.Web) works, in particular hooks-up to the ASP.NET Core OIDC events
- [Use HttpClientFactory to implement resilient HTTP requests](https://docs.microsoft.com/en-us/dotnet/standard/microservices-architecture/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests) used by the Graph custom service

## How to deploy this sample to Azure

This project has one WebApp project. To deploy it to the Azure Web Site, you'll need to:

- create an Azure Web Site
- publish the Web App to the web site, and
- update its client(s) to call the web site instead of IIS Express.

### Create and publish the `WebApp-OpenIDConnect-DotNet-code-v2` to an Azure Web Site

1. Sign in to the [Azure portal](https://portal.azure.com).
1. Click `Create a resource` in the top left-hand corner, select **Web** --> **Web App**, and give your web site a name, for example, `WebApp-OpenIDConnect-DotNet-code-v2-contoso.azurewebsites.net`.
1. Thereafter select the `Subscription`, `Resource Group`, `App service plan and Location`. `OS` will be **Windows** and `Publish` will be **Code**.
1. Click `Create` and wait for the App Service to be created.
1. Once you get the `Deployment succeeded` notification, then click on `Go to resource` to navigate to the newly created App service.
1. The following steps provide instructions to create a Sql database. Steps for setting up a Redis database are further down. If you already have a Redis or Sql Server and database present and a connection string available, skip the steps till we ask you to provide the connections string in the `Application Settings`.
1. Click `Create a resource` in the top left-hand corner.
1. If you want to setup an SQL database, select **Databases** --> **SQL Database**, to create a new database. Follow the `Quickstart tutorial` if needed.
1. You can name the Sql server and database whatever you want to.
1. Select or create a database server, and enter server login credentials. Carefully note down the username and password for the Sql server as you'll need it when constructing your Sql conenction string later.
1. Wait for the `Deployment succeeded` notification, then click on `Go to resource` to navigate to the newly created database's manage screen.
1. Click on **Application settings** in the left menu of the App service and add the copied Sql connection string in the **Connection strings** section as `DefaultConnection`. Click on **Connection Strings** on left menu and copy the **ADO.NET (SQL authentication)** connection string. Populate  **User ID={your_username};Password={your_password};** with values your provided during database creation.Copy this connection string.
1. Choose `SQLAzure` in the **Type** dropdown. **Save** the setting.
1. If you want to setup a Redis Cache instead search for `Azure Cache for Redis` and select the matching option from the screen.
1. Follow the steps to create a cache that will meet your needs then press the `Create` button and wait for the cache to be successfully deployed.
1. Wait for the `Deployment succeeded` notification, then click o1 `Go to resource` to navigate to the newly created database's manage screen.
1. Navigate to the `Access Keys` option in the menu on the left. Copy either the `Primary connection string` and `Secondary connection string` values are the connection strings to the Redis database.
1. Update the `TokenCacheRedisConnStr` value in the `appsettings.json` file with either the `Primary connection string` and `Secondary connection string` and be sure that the `TokenCacheRedisInstanceName` value is set. You can even test that sessions are stored to the cache by running the application locally if you wish.
1. Once the web site is created, locate it it in the **Dashboard** and click it to open **App Services** **Overview** screen.
1. From the **Overview** tab of the App Service, download the publish profile by clicking the **Get publish profile** link and save it.  Other deployment mechanisms, such as from source control, can also be used.
1. Switch to Visual Studio and go to the WebApp-OpenIDConnect-DotNet-code-v2 project.  Right click on the project in the Solution Explorer and select **Publish**.  Click **Import Profile** on the bottom bar, and import the publish profile that you downloaded earlier.
1. Click on **Configure** and in the `Connection tab`, update the Destination URL so that it is a `https` in the home page url, for example [https://WebApp-OpenIDConnect-DotNet-code-v2-contoso.azurewebsites.net](https://WebApp-OpenIDConnect-DotNet-code-v2-contoso.azurewebsites.net). Click **Next**.
1. On the Settings tab, make sure `Enable Organizational Authentication` is NOT selected.  Click **Save**. Click on **Publish** on the main screen.
1. Visual Studio will publish the project and automatically open a browser to the URL of the project.  If you see the default web page of the project, the publication was successful.

### Update the Active Directory tenant application registration for `WebApp-OpenIDConnect-DotNet-code-v2`

1. Navigate back to to the [Azure portal](https://portal.azure.com).
In the left-hand navigation pane, select the **Azure Active Directory** service, and then select **App registrations (Preview)**.
1. In the resultant screen, select the `WebApp-OpenIDConnect-DotNet-code-v2` application.
1. In the **Authentication** | page for your application, update the Logout URL fields with the address of your service, for example [https://WebApp-OpenIDConnect-DotNet-code-v2-contoso.azurewebsites.net](https://WebApp-OpenIDConnect-DotNet-code-v2-contoso.azurewebsites.net)
1. From the *Branding* menu, update the **Home page URL**, to the address of your service, for example [https://WebApp-OpenIDConnect-DotNet-code-v2-contoso.azurewebsites.net](https://WebApp-OpenIDConnect-DotNet-code-v2-contoso.azurewebsites.net). Save the configuration.
1. Add the same URL in the list of values of the *Authentication -> Redirect URIs* menu. If you have multiple redirect urls, make sure that there a new entry using the App service's Uri for each redirect url.

## Community Help and Support

Use [Stack Overflow](http://stackoverflow.com/questions/tagged/msal) to get support from the community.
Ask your questions on Stack Overflow first and browse existing issues to see if someone has asked your question before.
Make sure that your questions or comments are tagged with [`azure-active-directory` `msal` `dotnet`].

If you find a bug in the sample, please raise the issue on [GitHub Issues](../../issues).

To provide a recommendation, visit the following [User Voice page](https://feedback.azure.com/forums/169401-azure-active-directory).

## Contributing

If you'd like to contribute to this sample, see [CONTRIBUTING.MD](/CONTRIBUTING.md).

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information, see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## More information

- [Microsoft identity platform (Azure Active Directory for developers)](https://docs.microsoft.com/en-us/azure/active-directory/develop/)
- [MSAL.NET's conceptual documentation](https://aka.ms/msal-net)
- [Customizing Token cache serialization](https://aka.ms/msal-net-token-cache-serialization)
- [Types of Applications](https://aka.ms/msal-net-client-applications)
- [Acquiring Tokens](https://aka.ms/msal-net-acquiring-tokens)

- [National Clouds](https://docs.microsoft.com/en-us/azure/active-directory/develop/authentication-national-cloud#app-registration-endpoints)

For more information about how OAuth 2.0 protocols work in this scenario and other scenarios, see [Authentication Scenarios for Azure AD](http://go.microsoft.com/fwlink/?LinkId=394414).
