## How to deploy this sample to Azure

This tutorial has one WebApp and some chapters have a Web API project. To deploy them to Azure Web Sites, you'll need to perform these steps for **each** project:

- create an Azure Web Site with a unique name
- publish the Web App / Web APIs to the web site, and
- update its client(s) to call the web site instead of IIS Express.

### Create and publish the `WebApp-OpenIDConnect-DotNet-code-v2` to an Azure Web Site

1. Sign in to the [Azure portal](https://portal.azure.com).
1. Click `Create a resource` in the top left-hand corner, select **Web** --> **Web App**, and give your web site a name, for example, `WebApp-OpenIDConnect-DotNet-code-v2-contoso.azurewebsites.net`.
1. Thereafter select the `Subscription`, `Resource Group`, `App service plan and Location`. `OS` will be **Windows** and `Publish` will be **Code**.
1. Click `Create` and wait for the App Service to be created.
1. Once you get the `Deployment succeeded` notification, then click on `Go to resource` to navigate to the newly created App service.
1. Do not activate App service authentication: your application handles everything by itself

### If your project uses **SQL Server**, please follow these steps

1. The following steps provide instructions to create a Sql database that the sample needs. If you already have a Sql Server and database present and a connection string available, skip the steps till we ask you to provide the connections string in the `Application Settings`.
1. Click `Create a resource` in the top left-hand corner again, select **Databases** --> **SQL Database**, to create a new database. Follow the `Quickstart tutorial` if needed.
1. You can name the Sql server and database whatever you want to.
1. Select or create a database server, and enter server login credentials. Carefully note down the username and password for the Sql server as you'll need it when constructing your Sql connection string later.
1. Wait for the `Deployment succeeded` notification, then click on `Go to resource` to navigate to the newly created database's manage screen.
1. Click on **Connection Strings** on left menu and copy the **ADO.NET (SQL authentication)** connection string. Populate  **User ID={your_username};Password={your_password};** with values your provided during database creation.Copy this connection string.
1. Click on **Application settings** in the left menu of the App service and add the copied Sql connection string in the **Connection strings** section as `DefaultConnection`.
1. Choose `SQLAzure` in the **Type** dropdown. **Save** the setting.

### Update the redirect URLs

1. Navigate back to to the [Azure portal](https://portal.azure.com).
In the left-hand navigation pane, select the **Azure Active Directory** service, and then select **App registrations**.
1. In the resultant screen, select the `WebApp-OpenIDConnect-DotNet-code-v2` application.
1. In the **Authentication** tab:
   - In the **Redirect URIs** section, select **Web** in the combo-box and add the following redirect URIs.
       - `https://WebApp-OpenIDConnect-DotNet-code-v2-contoso.azurewebsites.net`
       - `https://WebApp-OpenIDConnect-DotNet-code-v2-contoso.azurewebsites.net/signin-oidc`
   - In the **Advanced settings** section set **Logout URL** to `https://WebApp-OpenIDConnect-DotNet-code-v2-contoso.azurewebsites.net/signout-oidc`
1. In the **Branding** tab:
    - Update the **Home page URL** to the address of your app service, for example `https://WebApp-OpenIDConnect-DotNet-code-v2-contoso.azurewebsites.net`.
    - Save the configuration.
1. If your application calls a web api, make sure to apply the necessary changes on the project `appsettings.json`, so it calls the published API URL instead of `localhost`.

### Publishing the sample

1. From the **Overview** tab of the App Service, download the publish profile by clicking the **Get publish profile** link and save it.  Other deployment mechanisms, such as from source control, can also be used.
1. Switch to Visual Studio and go to the WebApp-OpenIDConnect-DotNet-code-v2 project.  Right click on the project in the Solution Explorer and select **Publish**.  Click **Import Profile** on the bottom bar, and import the publish profile that you downloaded earlier.
1. Click on **Configure** and in the `Connection tab`, update the Destination URL so that it is a `https` in the home page url, for example [https://WebApp-OpenIDConnect-DotNet-code-v2-contoso.azurewebsites.net](https://WebApp-OpenIDConnect-DotNet-code-v2-contoso.azurewebsites.net). Click **Next**.
1. On the Settings tab, make sure `Enable Organizational Authentication` is NOT selected.  Click **Save**. Click on **Publish** on the main screen.
1. Visual Studio will publish the project and automatically open a browser to the URL of the project.  If you see the default web page of the project, the publication was successful.

### Case of web apps deployed to App Services as Linux containers

#### What is the issue?

Normally, Microsoft Identity Web computes the redirect URI automatically depending on the deployed URL.

However, when you deploy web apps to App Services as Linux containers, your application will be called by App Services on an HTTP address, whereas its registered redirect URI in the app registration will be HTTPS.

This means that when a user browses to the web app, they will be redirected to `login.microsoftonline.com` as expected, but with:

```
redirect_uri=http://<your app service name>.azurewebsites.net/signin-oidc
```

instead of 

```
redirect_uri=https://<your app service name>.azurewebsites.net/signin-oidc
```

#### How to fix it?

In order to get the right result, the guidance from the ASP.NET Core team for working with proxies is in [Configure ASP.NET Core to work with proxy servers and load balancers](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer). You should address the issue centrally by using `UseForwardedHeaders` to fix the request fields, like scheme.

The container scenario should have been addressed by default in .NET Core 3.0. See [Forwarded Headers Middleware Updates in .NET Core 3.0 preview 6](https://devblogs.microsoft.com/aspnet/forwarded-headers-middleware-updates-in-net-core-3-0-preview-6). If there are issues with this for you, please contact the ASP .NET Core team <https://github.com/dotnet/aspnetcore>, as they will be the right team to assist with this.

## Key Vault and Managed Service Identity (MSI)

Secure key management is essential to protect data in the cloud. Use [Azure Key Vault](https://azure.microsoft.com/services/key-vault/) to encrypt certicates/keys and small secrets like passwords that use keys stored in hardware security modules (HSMs). Then Microsoft.Identity.Web leverages Managed Service Identity to retrieve these certificates. For details see [https://aka.ms/ms-id-web-certificates](https://aka.ms/ms-id-web-certificates)

If you want to retrieve passwords, instead of certificates, see the [app-service-msi-keyvault-dotnet](https://github.com/Azure-Samples/app-service-msi-keyvault-dotnet) sample as a guide on how to use Azure Key Vault from App Service with Managed Service Identity (MSI).

## MSAL token cache on distributed environments

The samples in this tutorial have their token cache providers configured for apps running on a single machine. On a production environment, these apps could be deployed in many machines for scalability purpose, so the token cache provider needs to be configured accordingly for this distributed architecture.

These are the necessary changes for each cache provider option:

### In memory

If you want to use in memory cache, use this configuration on `Startup.cs`:

```csharp
services.AddDistributedTokenCaches()
.AddDistributedMemoryCache();
```

### Redis

If you want to use a distributed Redis cache, use this configuration on `Startup.cs`:

```csharp
services.AddDistributedTokenCaches()
.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "<your_redis_primary_connection_string_here>";
    options.InstanceName = "<your_redis_instance_name>";
});
```

### SQL Server

There are two options for distributed SQL cache:

- [using .Net Core distributed cache extensions](https://docs.microsoft.com/en-us/aspnet/core/performance/caching/distributed?view=aspnetcore-2.2)
- [configuring DataProtection for distributed environments](https://docs.microsoft.com/en-us/aspnet/core/security/data-protection/configuration/overview?view=aspnetcore-2.2)

#### If you want to use .Net Core distributed cache extensions

Create the cache database by running the CLI (change the parameters according to your configurations)

```csharp
dotnet tool install --global dotnet-sql-cache
dotnet sql-cache create "<your DB connection string>" dbo <cacheTableName>
//For example: dotnet sql-cache create "Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=My_Database;Integrated Security=True;" dbo TokenCache
```

Then use this configuration on `Startup.cs`:

```csharp
services.AddDistributedTokenCaches()
.AddDistributedSqlServerCache(options =>
{
    options.ConnectionString = "<your_sql_connection_string_here>";
    options.SchemaName = "dbo";
    options.TableName = "<your_cache_table_name_here>";
});
```

#### If you want to configure `DataProtection` for distributed environments

You have to configure the key ring storage to a centralized location. It could be in [Azure Key Vault](https://azure.microsoft.com/services/key-vault/) or on a [UNC share](https://docs.microsoft.com/en-us/openspecs/windows_protocols/ms-dfsc/149a3039-98ce-491a-9268-2f5ddef08192).

> **Note**: If you change the key persistence location, the system no longer automatically encrypts keys at rest. It is recommended that you use one of the ProtectKeysWith* methods listed [in this doc](https://docs.microsoft.com/en-us/aspnet/core/security/data-protection/configuration/overview?view=aspnetcore-2.2).

For Azure Key Vault, configure the system with [PersistKeysToAzureBlobStorage](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.dataprotection.azuredataprotectionbuilderextensions.persistkeystoazureblobstorage?view=aspnetcore-2.2) (also consider using [ProtectKeysWithAzureKeyVault](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.dataprotection.azuredataprotectionbuilderextensions.protectkeyswithazurekeyvault)) in the `Startup` class:

```csharp
services.AddDataProtection()
.PersistKeysToAzureBlobStorage("<storage account connection or uri>");
```

> **Note**: Your app must have **Unwrap Key** and **Wrap Key** permissions to the Azure Key Vault.

For UNC share, configure the system with [PersistKeysToFileSystem](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.dataprotection.dataprotectionbuilderextensions.persistkeystofilesystem) (also consider using [ProtectKeysWithCertificate](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.dataprotection.dataprotectionbuilderextensions.protectkeyswithcertificate?view=aspnetcore-2.2)) in the `Startup` class:

```csharp
services.AddDataProtection()
.PersistKeysToFileSystem(new DirectoryInfo(@"\\server\share\directory\"));
```

#### If you (really) want to enable App services authentication

You don't need to enable app service authentication. If you do, depending on whether you enable or not App service authentication, the redirect URI will be different:

 Scenario |  Redirect URI
-----------   | -----------  
Run on your developer box with IIS |  ` https://localhost:44321/signin-oidc`
Run on your developer box with Kestrel profile |  ` https://localhost:5001/signin-oidc`
Deployed to app service without app service authentication | `https://appServiceBaseUri/signin-oidc`
Deployed to app service **with** app service authentication | `https://appServiceBaseUri/.auth/login/aad/callback`

Therefore depending on the scenarios you want to run, you should add the corresponding redirect URI to the app registration

## Community Help and Support

Use [Stack Overflow](http://stackoverflow.com/questions/tagged/msal) to get support from the community.
Ask your questions on Stack Overflow first and browse existing issues to see if someone has asked your question before.
Make sure that your questions or comments are tagged with [`azure-active-directory`] [`msal`] [`dotnet`].

If you find a bug in the sample, please raise the issue on [GitHub Issues](../../issues).

To provide a recommendation, visit the following [User Voice page](https://feedback.azure.com/forums/169401-azure-active-directory).

> [Consider taking a moment to share your experience with us.](https://forms.office.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbRz0h_jLR5HNJlvkZAewyoWxUNEFCQ0FSMFlPQTJURkJZMTRZWVJRNkdRMC4u)

## More information

For more information, see MSAL.NET's conceptual documentation:

- [MSAL.NET's conceptual documentation](https://aka.ms/msal-net)
- [National Clouds](https://docs.microsoft.com/en-us/azure/active-directory/develop/authentication-national-cloud#app-registration-endpoints)
