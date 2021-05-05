---
page_type: sample
languages:
  - csharp
products:
  - aspnet-core
  - ms-graph
  - azure-active-directory
name: Using the Microsoft identity platform to call the Microsoft Graph API from an ASP.NET Core Web App, on behalf of a user signing-in using their work and school account
urlFragment: active-directory-aspnetcore-webapp-openidconnect-v2
description: "This sample demonstrates a ASP.NET Core Web App calling the Microsoft Graph"
---

# Enable your ASP.NET Core web app to sign in users and call Microsoft Graph with the Microsoft identity platform

 1. [Overview](#overview)
 1. [Scenario](#scenario)
 1. [Contents](#contents)
 1. [Prerequisites](#prerequisites)
 1. [Setup](#setup)
 1. [Registration](#registration)
 1. [Running the sample](#running-the-sample)
 1. [Explore the sample](#explore-the-sample)
 1. [About the code](#about-the-code)
 1. [Deployment](#deployment)
 1. [More information](#more-information)
 1. [Community Help and Support](#community-help-and-support)
 1. [Contributing](#contributing)

[![Build status](https://identitydivision.visualstudio.com/IDDP/_apis/build/status/AAD%20Samples/.NET%20client%20samples/ASP.NET%20Core%20Web%20App%20tutorial)](https://identitydivision.visualstudio.com/IDDP/_build/latest?definitionId=819)

## Overview

This sample demonstrates an ASP.NET Core web app that calls the Microsoft Graph API for a signed-in user.

## Scenario

1. The ASP.NET Core client web app uses the [Microsoft.Identity.Web](https://aka.ms/microsoft-identity-web) to sign a user in, and obtain a JWT [access Tokens](https://aka.ms/access-tokens) from **Azure AD**.
1. The access token is used by the client app as a bearer token to call Microsoft Graph.

![Sign in with the Microsoft identity platform and call Graph](ReadmeFiles/sign-in.png)

## Prerequisites

- Either [Visual Studio](https://visualstudio.microsoft.com/downloads/) or [Visual Studio Code](https://code.visualstudio.com/download) and [.NET Core SDK](https://www.microsoft.com/net/learn/get-started)
- An **Azure AD** tenant. For more information see: [How to get an Azure AD tenant](https://docs.microsoft.com/azure/active-directory/develop/quickstart-create-new-tenant)
- A user account in _your_ **Azure AD** tenant. This sample will not work with a **personal Microsoft account**.  If you're signed in to the [Azure portal](https://portal.azure.com) with a personal account and have not created a _user account in your directory_ before, you will need need to create one before proceeding.

## Setup

### Step 1: Clone or download this repository

From your shell or command line:

```console
    git clone https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2.git
```

or download and extract the repository .zip file.

> :warning: To avoid path length limitations on Windows, we recommend cloning into a directory near the root of your drive.

Go to the `"2-WebApp-graph-user\2-1-Call-MSGraph"` folder

 ```Sh
  cd "2-WebApp-graph-user\2-1-Call-MSGraph"
  ```

> Developers who wish to gain good familiarity of programming for Microsoft Graph are advised to go through the [An introduction to Microsoft Graph for developers](https://www.youtube.com/watch?v=EBbnpFdB92A) recorded session.

### Step 2: Install project dependencies

```console
    dotnet restore WebApp-OpenIDConnect-DotNet-graph.csproj
```

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

1. Sign in to the [Azure portal](https://portal.azure.com).
1. If your account is present in more than one Azure AD tenant, select your profile at the top right corner in the menu on top of the page, and then **switch directory** to change your portal session to the desired Azure AD tenant.

### ### Register the client web app (WebApp-OpenIDConnect-DotNet-graph-v2)

1. Navigate to the [Azure portal](https://portal.azure.com) and select the **Azure AD** service.
1. Select the **App Registrations** blade on the left, then select **New registration**.
1. In the **Register an application page** that appears, enter your application's registration information:
   - In the **Name** section, enter a meaningful application name that will be displayed to users of the app, for example `WebApp-OpenIDConnect-DotNet-graph-v2`.
   - Under **Supported account types**, select **Accounts in this organizational directory only**.
   - In the **Redirect URI (optional)** section, select **Web** in the combo-box and enter the following redirect URI: `https://localhost:44321/`.
     > Note that there are more than one redirect URIs used in this sample. You'll need to add them from the **Authentication** tab later after the app has been created successfully.
1. Select **Register** to create the application.
1. In the app's registration screen, find and note the **Application (client) ID**. You'll need to use this value in your app's configuration files.
1. In the app's registration screen, select **Authentication** in the menu.
   - If you don't have a platform added, select **Add a platform** and select the **Web** option.
   - In the **Redirect URIs** section, enter the following redirect URIs.
      - `https://localhost:44321/signin-oidc`
   - In the **Front-channel logout URL** section, set it to `https://localhost:44321/signout-oidc`.
1. Select **Save** to save your changes.
1. In the app's registration screen, select the **Certificates & secrets** blade in the left to open the page where we can generate secrets and upload certificates.
1. In the **Client secrets** section, select **New client secret**:
   - Type a key description (for instance `app secret`),
   - Select one of the available key durations (**6 months**, **12 months** or **Custom**) as per your security posture.
   - The generated key value will be displayed when you select the **Add** button. Copy and save the generated value for use in later steps.
   - You'll need this key later in your code's configuration files. This key value will not be displayed again, and is not retrievable by any other means, so make sure to note it from the Azure portal before navigating to any other screen or blade.
1. In the app's registration screen, select the **API permissions** blade in the left to open the page where we add access to the APIs that your application needs.
   - Select the **Add a permission** button and then,
   - Ensure that the **Microsoft APIs** tab is selected.
   - In the *Commonly used Microsoft APIs* section, select **Microsoft Graph**
   - In the **Delegated permissions** section, select the **User.Read** in the list. Use the search box if necessary.
   - Select the **Add permissions** button at the bottom.

#### Configure the client web app (WebApp-OpenIDConnect-DotNet-graph-v2) to use your app registration

Open the project in your IDE (like Visual Studio or Visual Studio Code) to configure the code.

>In the steps below, "ClientID" is the same as "Application ID" or "AppId".

1. Open the `appsettings.json` file.
1. Find the key `ClientId` and replace the existing value with the application ID (clientId) of `WebApp-OpenIDConnect-DotNet-graph-v2` app copied from the Azure portal.
1. Find the key `TenantId` and replace the existing value with your Azure AD tenant ID.
1. Find the key `Domain` and replace the existing value with your Azure AD tenant name.
1. Find the key `ClientSecret` and replace the existing value with the key you saved during the creation of `WebApp-OpenIDConnect-DotNet-graph-v2` copied from the Azure portal.

- In case you want to deploy your app in [Sovereign or national clouds](https://docs.microsoft.com/graph/deployments), ensure the `GraphApiUrl` and `Instance` option matches the your requirements. The default values are set to Microsoft Graph in the Azure public cloud. You may skip this point if it does not apply to you.

  ```Json
  "Instance": "https://login.microsoftonline.com/",
  "GraphApiUrl": "https://graph.microsoft.com/v1.0"
  ```

## Running the sample

> For Visual Studio Users
>
> Clean the solution, rebuild the solution, and run it.  You might want to go into the solution properties and set the right startup project first.

```console
    dotnet run
```

## Explore the sample

1. Open your web browser and make a request to the app at url `https://localhost:44321`. The app immediately attempts to authenticate you via the Microsoft identity platform. Sign in with a work or school account.

2. Provide consent to the screen presented.

3. Click on the **Profile** link on the top menu. The web app will make a call to the Microsoft Graph */me* endpoint. You should see information about the signed-in user's account, as well as its picture, if these values are set in the account's profile.

> Did the sample not work for you as expected? Did you encounter issues trying this sample? Then please reach out to us using the [GitHub Issues](../../../../../issues) page.

> [Consider taking a moment to share your experience with us.](https://forms.office.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbRz0h_jLR5HNJlvkZAewyoWxUNEFCQ0FSMFlPQTJURkJZMTRZWVJRNkdRMC4u)

## About The code

1. In this aspnetcore web project, first the packages `Microsoft.Identity.Web` and `Microsoft.Identity.Web.UI` were added from NuGet. These libraries are used to simplify the process of signing-in a user and acquiring tokens for Microsoft Graph.

1. Staring with the **Startup.cs** file :

   - at the top of the file, the following two using directives were added:

     ```CSharp
      using Microsoft.Identity.Web;
      using Microsoft.Identity.Web.UI;
      ```

   - in the `ConfigureServices` method, the following code was added, replacing any existing `AddAuthentication()` code.:

     ```CSharp
           services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(Configuration.GetSection("AzureAd"))
                .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
                .AddInMemoryTokenCaches();
     ```

     These enable your application to sign-in a user with the Microsoft identity platform endpoint. This endpoint is capable of signing-in users both with their Work and School and Microsoft Personal accounts (if required).

### Update launchSetting.json

Change the following values in the `Properties\launchSettings.json` file to ensure that you start your web app from `https://localhost:44321`:
    - update the `sslPort` of the `iisSettings` section to be `44321`
    - update the `applicationUrl` property to `https://localhost:44321`

### Update the `Startup.cs` file to enable the Microsoft Graph custom service

After adding the `Microsoft.Identity.Web.MicrosoftGraph` package, to use [Microsoft Graph SDK](https://github.com/microsoftgraph/msgraph-sdk-dotnet/blob/dev/docs/overview.md),  in the `Startup.cs` file, add the following `AddMicrosoftGraph` extension method. This lines ensures that the GraphAPIService benefits from the optimized `HttpClient` management by ASP.NET Core.

```CSharp
    // Add Graph
    services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApp(Configuration.GetSection("AzureAd"))
        .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
        .AddMicrosoftGraph(Configuration.GetSection("DownstreamApi"))
        .AddInMemoryTokenCaches();
```

### The controller code to acquire a token and call Microsoft Graph

1. In the `Controllers\HomeController.cs`file, the following code is added to allow calling MS Graph

```CSharp
        private readonly ILogger<HomeController> _logger;
        private readonly GraphServiceClient _graphServiceClient;
        private readonly MicrosoftIdentityConsentAndConditionalAccessHandler _consentHandler;

private readonly GraphServiceClient _graphServiceClient;

        public HomeController(ILogger<HomeController> logger,
                            IConfiguration configuration,
                            GraphServiceClient graphServiceClient,
                            MicrosoftIdentityConsentAndConditionalAccessHandler consentHandler)
{
            _logger = logger;
            _graphServiceClient = graphServiceClient;
            this._consentHandler = consentHandler;
}
```

1. In the `Profile()` action we  make a call to the Microsoft Graph *me* endpoint. In case a token cannot be acquired, a challenge is attempted to re-sign-in the user, and have them consent to the requested scopes. This is expressed declaratively by the `AuthorizeForScopes`attribute. This attribute is part of the `Microsoft.Identity.Web` project and automatically manages incremental consent.

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

## Deployment

### Deploying web app to Azure App Services

There is one web app in this sample. To deploy it to **Azure App Services**, you'll need to:

- create an **Azure App Service**
- publish the projects to the **App Services**, and
- update its client(s) to call the website instead of the local environment.

#### Publish your files (WebApp-OpenIDConnect-DotNet-graph-v2)

##### Publish using Visual Studio

Follow the link to [Publish with Visual Studio](https://docs.microsoft.com/visualstudio/deployment/quickstart-deploy-to-azure).

##### Publish using Visual Studio Code

1. Open an instance of Visual Studio code set to the `WebApp-OpenIDConnect-DotNet-graph-v2` project folder.
1. Install the VS Code extension [Azure App Service](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azureappservice).
1. Using the extension you just installed, sign in to **Azure App Service** using your Azure AD account.
1. Choose `View > Terminal` from the VS Code menu to open a new terminal window in the project directory.
1. Run the following command

    ```console
    dotnet publish WebApp-OpenIDConnect-DotNet-graph.csproj --configuration Release
    ```

1. Publish folder is created under path ``bin/Release/<Enter_Framework_FolderName>``.
1. From the VS Code file explorer, right-click on the **Publish** folder and select **Deploy to Web App**.
1. Select **Create New Web App**.
1. Enter a unique name for the app, for example, `WebApp-OpenIDConnect-DotNet-graph-v2`. If you chose `example-domain` for your app name, your app's domain name will be `https://example-domain.azurewebsites.net`.
1. Select **Windows** as the OS. Press Enter.
1. Select **.NET Core 3.1 (LTS)** as runtime stack.
1. Select `Free` or any other option for your pricing tier.

#### Update the Azure AD app registration (WebApp-OpenIDConnect-DotNet-graph-v2)

1. Navigate back to to the [Azure portal](https://portal.azure.com).
1. Go to the **Azure Active Directory** section, and then select **App registrations**.
1. In the resulting screen, select the `WebApp-OpenIDConnect-DotNet-graph-v2` application.
1. In the app's registration screen, select **Authentication** in the menu.
   - In the **Redirect URIs** section, update both of the reply URLs to match the site URL of your Azure deployment. Using the following examples as a guide, **replace** the text `<replace-this-with-the-app-name-you-created-when-deploying-in-the-previous-step>` with the app name you created while deploying, for example:
   - `https://<replace-this-with-the-app-name-you-created-when-deploying-in-the-previous-step>.azurewebsites.net/`
   - `https://<replace-this-with-the-app-name-you-created-when-deploying-in-the-previous-step>.azurewebsites.net/signin-oidc`
1. Update the **Front-channel logout URL** fields with the address of your service, for example `https://<replace-this-with-the-app-name-you-created-when-deploying-in-the-previous-step>.azurewebsites.net`.

> :warning: If your app is using *in-memory* storage, **Azure App Services** will spin down your web site if it is inactive, and any records that your app was keeping will emptied. In addition, if you increase the instance count of your website, requests will be distributed among the instances. Your app's records, therefore, will not be the same on each instance.

### Enabling your code to pick secrets from Key Vault using a Managed Identity

One of the uber principals of security and Zero Trust is to place credentials out of your code and used in a manner that allows for credentials to be replaced or rotated without incurring a downtime.
To achieve this we'd place our application's credentials in [Azure Key Vault](https://azure.microsoft.com/services/key-vault/) and access it via [Managed Identities for Azure resources](https://docs.microsoft.com/azure/active-directory/managed-identities-azure-resources/overview).

We will follow the steps broadly outlined in [Use Key Vault from App Service with Azure Managed Identity](https://github.com/Azure-Samples/app-service-msi-keyvault-dotnet/blob/master/README.md)

#### Set up your managed Identity and Key vault

1. You would need an [Azure Subscription](https://azure.microsoft.com/free/) first.
1. You should have a working and deployed application as an Azure App Service following the steps in [Deploying web app to Azure App Services](#deploying-web-app-to-azure-app-services) above.
1. [Set up your Key Vault](https://docs.microsoft.com/azure/key-vault/secrets/quick-create-portal) and move the 'ClientSecret' from `appsettings.json` to the Key Vault. Delete the 'ClientSecret' entry in the `appsettings.json`. Note the Key Vault Uri , e.g. **https://<my key vault.azure.net>**.
2. Make 
3. In the Properties\launchSettings.json file add the following entry

  > "KEY_VAULT_URI": "https://keyvault0417.vault.azure.net/" under `environmentVariables`

1. Add the `Azure.Identity` package to the solution.
1. In `startup.cs` add the following method:

```CSharp
        /// Gets the secret from key vault via an enabled Managed Identity.
        /// </summary>
        /// <remarks>https://github.com/Azure-Samples/app-service-msi-keyvault-dotnet/blob/master/README.md</remarks>
        /// <returns></returns>
        private string GetSecretFromKeyVault(string tenantId)
        {
            // this should point to your vault's URI, like https://<yourkeyvault>.vault.azure.net/
            string uri = Environment.GetEnvironmentVariable("KEY_VAULT_URI");
            DefaultAzureCredentialOptions options = new DefaultAzureCredentialOptions();

            // Specify the tenant ID to use the dev credentials when running the app locally
            options.VisualStudioTenantId = tenantId;
            options.SharedTokenCacheTenantId = tenantId;
            SecretClient client = new SecretClient(new Uri(uri), new DefaultAzureCredential(options));

            // The secret name, for example if the full url to the secret is https://<yourkeyvault>.vault.azure.net/secrets/Graph-App-Secret
            Response<KeyVaultSecret> secret = client.GetSecretAsync("Graph-App-Secret").Result;

            return secret.Value.Value;
        }
```

1. In ConfugureServices method, add the following lines of code 

```CSharp
            // client secret is picked from KeyVault
            string tenantId = Configuration.GetValue<string>("AzureAd:TenantId");
            services.Configure<MicrosoftIdentityOptions>(
                options => { options.ClientSecret = GetSecretFromKeyVault(tenantId); });

```

## More information

- [Microsoft identity platform (Azure Active Directory for developers)](https://docs.microsoft.com/azure/active-directory/develop/)
- [Overview of Microsoft Authentication Library (MSAL)](https://docs.microsoft.com/azure/active-directory/develop/msal-overview)
- [Microsoft.Identity.Web](https://aka.ms/microsoft-identity-web)
- [Quickstart: Register an application with the Microsoft identity platform (Preview)](https://docs.microsoft.com/azure/active-directory/develop/quickstart-register-app)
- [Understanding Azure AD application consent experiences](https://docs.microsoft.com/azure/active-directory/develop/application-consent-experience)
- [Understand user and admin consent](https://docs.microsoft.com/azure/active-directory/develop/howto-convert-app-to-be-multi-tenant#understand-user-and-admin-consent)
- [Application and service principal objects in Azure Active Directory](https://docs.microsoft.com/azure/active-directory/develop/app-objects-and-service-principals)
- [National Clouds](https://docs.microsoft.com/azure/active-directory/develop/authentication-national-cloud#app-registration-endpoints)
- [MSAL code samples](https://docs.microsoft.com/azure/active-directory/develop/sample-v2-code)
- [Managed Identities for Azure resources](https://docs.microsoft.com/azure/active-directory/managed-identities-azure-resources/overview)
For more information about how OAuth 2.0 protocols work in this scenario and other scenarios, see [Authentication Scenarios for Azure AD](https://docs.microsoft.com/azure/active-directory/develop/authentication-flows-app-scenarios).
[Azure Key Vault](https://azure.microsoft.com/services/key-vault/)
[Use Key Vault from App Service with Azure Managed Identity](https://github.com/Azure-Samples/app-service-msi-keyvault-dotnet/blob/master/README.md)

## Community Help and Support

Use [Stack Overflow](http://stackoverflow.com/questions/tagged/msal) to get support from the community.
Ask your questions on Stack Overflow first and browse existing issues to see if someone has asked your question before.
Make sure that your questions or comments are tagged with [`azure-active-directory` `azure-ad-b2c` `ms-identity` `adal` `msal`].

If you find a bug in the sample, raise the issue on [GitHub Issues](../../../issues).

To provide feedback on or suggest features for Azure Active Directory, visit [User Voice page](https://feedback.azure.com/forums/169401-azure-active-directory).

## Contributing

If you'd like to contribute to this sample, see [CONTRIBUTING.MD](/CONTRIBUTING.md).

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information, see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.