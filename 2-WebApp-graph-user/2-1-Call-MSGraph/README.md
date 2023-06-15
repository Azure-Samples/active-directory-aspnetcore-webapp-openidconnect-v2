---
page_type: sample
languages:
  - csharp
products:
  - aspnet-core
  - ms-graph
  - azure-active-directory
name: Enable your ASP.NET Core web app to sign in users and call Microsoft Graph with the Microsoft identity platform
urlFragment: active-directory-aspnetcore-webapp-openidconnect-v2
description: "This sample demonstrates a ASP.NET Core Web App calling the Microsoft Graph"
---

# Enable your ASP.NET Core web app to sign in users and call Microsoft Graph with the Microsoft identity platform

- [Overview](#overview)
- [Scenario](#scenario)
- [Prerequisites](#prerequisites)
- [Setup](#setup)
  - [Step 1: Clone or download this repository](#step-1-clone-or-download-this-repository)
  - [Step 2: Install project dependencies](#step-2-install-project-dependencies)
  - [Step 3: Register the sample application(s) with your Azure Active Directory tenant](#step-3-register-the-sample-applications-with-your-azure-active-directory-tenant)
- [Run the sample](#run-the-sample)
- [Explore the sample](#explore-the-sample)
- [About The code](#about-the-code)
- [Deployment](#deployment)
  - [Deploying web app to Azure App Services](#deploying-web-app-to-azure-app-services)
  - [Enabling your code to get secrets from Key Vault using Managed Identity](#enabling-your-code-to-get-secrets-from-key-vault-using-managed-identity)
- [Optional - Handle Continuous Access Evaluation (CAE) challenge from Microsoft Graph](#optional---handle-continuous-access-evaluation-cae-challenge-from-microsoft-graph)
  - [Declare the CAE capability in the configuration](#declare-the-cae-capability-in-the-configuration)
  - [Process the CAE challenge from Microsoft Graph](#process-the-cae-challenge-from-microsoft-graph)
- [More information](#more-information)
- [Community Help and Support](#community-help-and-support)
- [Contributing](#contributing)

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

> Developers who wish to increase their familiarity with programming for Microsoft Graph are advised to go through the [An introduction to Microsoft Graph for developers](https://www.youtube.com/watch?v=EBbnpFdB92A) recorded session.

### Step 2: Install project dependencies

```console
    dotnet restore WebApp-OpenIDConnect-DotNet-graph.csproj
```

### Step 3: Register the sample application(s) with your Azure Active Directory tenant

There is one project in this sample. To register it, you can:

- follow the steps below for manually register your apps
- or use PowerShell scripts that:
  - **automatically** creates the Azure AD applications and related objects (passwords, permissions, dependencies) for you.
  - modify the projects' configuration files.

<details>
  <summary>Expand this section if you want to use this automation:</summary>

> :warning: If you have never used **Azure AD Powershell** before, we recommend you go through the [App Creation Scripts](./AppCreationScripts/AppCreationScripts.md) once to ensure that your environment is prepared correctly for this step.

1. On Windows, run PowerShell as **Administrator** and navigate to the folder that contains this readme file.
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

#### Choose the Azure AD tenant where you want to create your applications

As a first step you'll need to:

1. Sign in to the [Azure portal](https://portal.azure.com).
1. If your account is present in more than one Azure AD tenant, select your profile at the top right corner in the menu on top of the page, and then **switch directory** to change your portal session to the desired Azure AD tenant.

#### Register the client web app (WebApp-OpenIDConnect-DotNet-graph-v2)

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

## Run the sample

> For Visual Studio Users
>
> Clean the solution, rebuild the solution, and run it.  You might want to go into the solution properties and set the right startup project first.

```console
    dotnet run
```

## Explore the sample

1. Open your web browser and make a request to the app at url `https://localhost:44321`. The app immediately attempts to authenticate you via the Microsoft identity platform. Sign in with a work or school account.
2. Provide consent to the screen presented.
3. Click on the **Profile** link on the top menu. The web app will make a call to the Microsoft Graph `/me` endpoint. You should see information about the signed-in user's account, as well as its picture, if these values are set in the account's profile.

> Did the sample not work for you as expected? Did you encounter issues trying this sample? Then please reach out to us using the [GitHub Issues](../../../../issues) page.

> [Consider taking a moment to share your experience with us.](https://forms.office.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbRz0h_jLR5HNJlvkZAewyoWxUNEFCQ0FSMFlPQTJURkJZMTRZWVJRNkdRMC4u)

## About The code

1. In this aspnetcore web project, first the packages `Microsoft.Identity.Web`,  `Microsoft.Identity.Web.UI` and `Microsoft.Identity.Web.GraphServiceClient` were added from NuGet. These libraries are used to simplify the process of signing-in a user and acquiring tokens for Microsoft Graph.

2. Starting with the **Startup.cs** file :

   - at the top of the file, the following two using directives were added:

     ```CSharp
      using Microsoft.Identity.Web;
      using Microsoft.Identity.Web.UI;
      ```

   - in the `ConfigureServices` method, the following code was added, replacing any existing `AddAuthentication()` code:

    ```CSharp

    services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApp(Configuration.GetSection("AzureAd"))
        .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
        .AddMicrosoftGraph(Configuration.GetSection("DownstreamApi"))
        .AddInMemoryTokenCaches();

    ```

     `AddMicrosoftIdentityWebApp()` enables your application to sign-in a user with the Microsoft identity platform endpoint. This endpoint is capable of signing-in users both with their Work and School and Microsoft Personal accounts (if required).

    `EnableTokenAcquisitionToCallDownstreamApi()` and `AddMicrosoftGraph` adds support to call Microsoft Graph. This lines ensures that the GraphAPIService benefits from the optimized `HttpClient` management by ASP.NET Core.

3. In the `Controllers\HomeController.cs` file, the following code is added to allow calling MS Graph:

 ```CSharp
   private readonly ILogger<HomeController> _logger;
   private readonly GraphServiceClient _graphServiceClient;
  
   public HomeController(ILogger<HomeController> logger,
                      IConfiguration configuration,
                      GraphServiceClient graphServiceClient)
   {
    _logger = logger;
    _graphServiceClient = graphServiceClient;
    this._consentHandler = consentHandler;
   }
   ```

4. In the `Profile()` action we make a call to the Microsoft Graph `/me` endpoint. In case a token cannot be acquired, a challenge is attempted to re-sign-in the user, and have them consent to the requested scopes. This is expressed declaratively by the `AuthorizeForScopes`attribute. This attribute is part of the `Microsoft.Identity.Web` project and automatically manages incremental consent.

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

5. Update `launchSetting.json`. Change the following values in the `Properties\launchSettings.json` file to ensure that you start your web app from `https://localhost:44321`:
    - update the `sslPort` of the `iisSettings` section to be `44321`
    - update the `applicationUrl` property to `https://localhost:44321`

## Deployment

### Deploying web app to Azure App Services

There is one web app in this sample. To deploy it to **Azure App Services**, you'll need to:

- create an **Azure App Service**
- publish the projects to the **App Services**, and
- update its client(s) to call the website instead of the local environment.

#### Publish your files

##### Publish using Visual Studio

Follow the link to [Publish with Visual Studio](https://docs.microsoft.com/visualstudio/deployment/quickstart-deploy-to-azure).

##### Publish using Visual Studio Code

1. Open an instance of Visual Studio code set to the `WebApp-OpenIDConnect-DotNet-graph-v2` project folder.
1. Install the VS Code extension [Azure App Service](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azureappservice).
1. Using the extension you just installed, sign in to **Azure App Service** using your Azure AD account.
1. Choose `Terminal > New Terminal` from the VS Code menu to open a new terminal window in the project directory.
1. Run the following command

    ```console
    dotnet publish WebApp-OpenIDConnect-DotNet-graph.csproj --configuration Release
    ```

1. A `publish` folder is created within the following folder: `bin/Release/netcoreapp3.1/`.
1. From the VS Code file explorer, right-click on the **publish** folder and select **Deploy to Web App**.
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
   - In the **Redirect URIs** section, update both of the reply URLs to match the site URL of your Azure deployment. Using the following examples as a guide, **replace** the text `example-domain` with the app name you created while deploying, for example:
   - `https://example-domain.azurewebsites.net/`
   - `https://example-domain.azurewebsites.net/signin-oidc`
1. Update the **Front-channel logout URL** fields with the address of your service, for example `https://example-domain.azurewebsites.net`.

> :warning: If your app is using *in-memory* storage, **Azure App Services** will spin down your web site if it is inactive, and any records that your app was keeping will emptied. In addition, if you increase the instance count of your website, requests will be distributed among the instances. Your app's records, therefore, will not be the same on each instance.

### Enabling your code to get secrets from Key Vault using Managed Identity

One of the uber principals of security and **Zero Trust** is to place credentials out of your code and use in a manner that allows for credentials to be replaced or rotated without incurring a downtime.

To achieve this we'd place our application's credentials in [Azure Key Vault](https://azure.microsoft.com/services/key-vault/) and access it via [managed Identities for Azure resources](https://docs.microsoft.com/azure/active-directory/managed-identities-azure-resources/overview).

We will follow the steps broadly outlined in the guide: [Use Key Vault from App Service with Azure Managed Identity](https://github.com/Azure-Samples/app-service-msi-keyvault-dotnet/blob/master/README.md)

#### Set up your Managed Identity

1. Navigate to [Azure portal](https://portal.azure.com) and select the **Azure App Service**.
1. Find and select the App Service you've created previously.
1. On App Service portal, select **Identity**.
1. Within the **System assigned** tab, switch **Status** to **On**. Click **Save**.
1. Record the **Object Id** that will appear, as you will need it in the next step.

For more information, see [Add a system-assigned identity](https://docs.microsoft.com/azure/app-service/overview-managed-identity?tabs=dotnet#add-a-system-assigned-identity)

#### Set up your Key vault

Before starting here, make sure:

- You have an [Azure Subscription](https://azure.microsoft.com/free/).
- You have a working and deployed application as an Azure App Service following the steps listed at [Deploying web app to Azure App Services](#deploying-web-app-to-azure-app-services) above.
- Follow the guide to [create an Azure Key Vault](https://docs.microsoft.com/azure/key-vault/general/quick-create-portal).

##### Upload your secret to KeyVault

1. Navigate to your new key vault in the Azure portal.
1. On the Key Vault settings pages, select **Secrets**.
1. Click on **Generate/Import**.
1. On the **Create a secret** screen choose the following values:
    - **Upload options**: Manual.
    - **Name**: Type a name for the secret. The secret name must be unique within a Key Vault. For example, `myClientSecret`
    - **Value**: Copy and paste the value for the `ClientSecret` property (without quotes!) from your `appsettings.json` file.
    - Leave the other values to their defaults. Click **Create**.

##### Provide the managed identity access to Key Vault

1. Navigate to your Key Vault in the portal.
1. Select **Overview** > **Access policies**.
1. Click on **Add Access Policy**.
1. In the input box for **Secret permissions**, select **Get**.
1. Click on **Select Principal**, add the **system-assigned managed identity** that you have created in the [steps before](#set-up-your-managed-identity). You can use the **Object Id** you have recorded previously to search for it.
1. Click on **OK** to add the new Access Policy, then click **Save** to save the Access Policy.

#### Modify your code to connect to Key Vault

1. In the `appsettings.json` file, find and delete the `ClientSecret` property and its value.
1. In the `Properties\launchSettings.json` file, find the string `ENTER_YOUR_KEY_VAULT_URI` and replace it with the URI of your Key Vault, for example: `https://example-vault.vault.azure.net/`
1. Add the `Azure.Identity` NuGet package to the solution. This sample project has already added this package.
1. Add the following directives to your `startup.cs`.
    </p>
    :information_source: this has been already added in the sample project.

```CSharp
using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
```

5. In your `Startup.cs` file, you must create a `GetSecretFromKeyVault` method. This method sets up the Azure Key Vault client and returns the secret that is required.
    </p>
    :information_source: this has already been added in the sample project.

```CSharp
    private string GetSecretFromKeyVault(string tenantId, string secretName)
  {
      // this should point to your vault's URI, like https://<yourkeyvault>.vault.azure.net/
      string uri = Environment.GetEnvironmentVariable("KEY_VAULT_URI");
      DefaultAzureCredentialOptions options = new DefaultAzureCredentialOptions();

      // Specify the tenant ID to use the dev credentials when running the app locally
      options.VisualStudioTenantId = tenantId;
      options.SharedTokenCacheTenantId = tenantId;
      SecretClient client = new SecretClient(new Uri(uri), new DefaultAzureCredential(options));

      // The secret name, for example if the full url to the secret is https://<yourkeyvault>.vault.azure.net/secrets/Graph-App-Secret
        Response<KeyVaultSecret> secret = client.GetSecretAsync(secretName).Result;

      return secret.Value.Value;
  }
```

6. In your `Startup.cs` file, find the `ConfigureServices` method. Add the following code to call the GetSecretFromKeyVault method, right after `services.AddAuthentication`.
    </p>
    :information_source: In the sample project, this code is present but commented out by default. Uncomment it.
    </p>
    :warning: Replace the string `ENTER_YOUR_SECRET_NAME_HERE` with the name of the client secret you entered into Azure Key Vault, for example `myClientSecret`.

```CSharp
    // uncomment the following 3 lines to get ClientSecret from KeyVault
  string tenantId = Configuration.GetValue<string>("AzureAd:TenantId");
  services.Configure<MicrosoftIdentityOptions>(
        options => { options.ClientSecret = GetSecretFromKeyVault(tenantId, "ENTER_YOUR_SECRET_NAME_HERE"); });
```

7. Your `ConfigureServices` method should now look like the following snippet:

```CSharp
       public void ConfigureServices(IServiceCollection services)
        {
            string[] initialScopes = Configuration.GetValue<string>("DownstreamApi:Scopes")?.Split(' ');

            services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(Configuration)
                .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
                .AddMicrosoftGraph(Configuration.GetSection("DownstreamApi"))
                .AddInMemoryTokenCaches();

        // uncomment the following 3 lines to get ClientSecret from KeyVault
            string tenantId = Configuration.GetValue<string>("AzureAd:TenantId");
            services.Configure<MicrosoftIdentityOptions>(
            options => { options.ClientSecret = GetSecretFromKeyVault(tenantId, "myClientSecret"); });

      // ... more method code continues below
    }
```

8. Add an environment variable to your App Service so your web app can find its key vault.

    1. Go to the [Azure portal](https://portal.azure.com). Search for and select **App Service**, and then select your app.
    1. Select **Configuration** blade on the left, then select **New Application Settings**.
    1. Add a new variable, naming it **KEY_VAULT_URI**. Populate the value with the URI of your key vault, for example: `https://example-vault.vault.azure.net/`

1. Re-deploy your project to Azure App Service.

    1. Run the following command:

    ```console
    dotnet publish WebApp-OpenIDConnect-DotNet-graph.csproj --configuration Release
    ```

    1. Then, from the VS Code file explorer, right-click on the **bin/Release/netcoreapp3.1/publish** folder and select **Deploy to Web App**. If you are prompted to select an app, select one you created during this sample.

1. The deployment status is available from the output window. Within a few minutes you'll be able to visit your now-secure app and sign in.

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

## More information

- [Microsoft identity platform (Azure Active Directory for developers)](https://docs.microsoft.com/azure/active-directory/develop/)
- [Overview of Microsoft Authentication Library (MSAL)](https://docs.microsoft.com/azure/active-directory/develop/msal-overview)
- [Microsoft.Identity.Web](https://aka.ms/microsoft-identity-web)
- [Quickstart: Register an application with the Microsoft identity platform (Preview)](https://docs.microsoft.com/azure/active-directory/develop/quickstart-register-app)
- [Understanding Azure AD application consent experiences](https://docs.microsoft.com/azure/active-directory/develop/application-consent-experience)
- [Understand user and admin consent](https://docs.microsoft.com/azure/active-directory/develop/howto-convert-app-to-be-multi-tenant#understand-user-and-admin-consent)
- [Application and service principal objects in Azure Active Directory](https://docs.microsoft.com/azure/active-directory/develop/app-objects-and-service-principals)
- [National Clouds](https://docs.microsoft.com/azure/active-directory/develop/authentication-national-cloud#app-registration-endpoints)
- [Azure AD code samples](https://docs.microsoft.com/azure/active-directory/develop/sample-v2-code)
- [Managed Identities for Azure resources](https://docs.microsoft.com/azure/active-directory/managed-identities-azure-resources/overview)
- [Azure Key Vault](https://azure.microsoft.com/services/key-vault/)
- [Use Key Vault from App Service with Azure Managed Identity](https://github.com/Azure-Samples/app-service-msi-keyvault-dotnet/blob/master/README.md)
- [Authentication Scenarios for Azure AD](https://docs.microsoft.com/azure/active-directory/develop/authentication-flows-app-scenarios).

## Community Help and Support

Use [Stack Overflow](http://stackoverflow.com/questions/tagged/msal) to get support from the community.
Ask your questions on Stack Overflow first and browse existing issues to see if someone has asked your question before.
Make sure that your questions or comments are tagged with [`azure-active-directory` `azure-ad-b2c` `ms-identity` `adal` `msal`].

If you find a bug in the sample, raise the issue on [GitHub Issues](../../../../issues).

To provide feedback on or suggest features for Azure Active Directory, visit [User Voice page](https://feedback.azure.com/forums/169401-azure-active-directory).

## Contributing

If you'd like to contribute to this sample, see [CONTRIBUTING.MD](/CONTRIBUTING.md).

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information, see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
