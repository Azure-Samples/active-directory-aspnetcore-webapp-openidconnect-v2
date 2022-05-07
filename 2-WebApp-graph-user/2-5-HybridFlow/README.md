---
page_type: sample
name: Creating a Hybrid Graph API Application
services: active-directory
platforms: dotnet
urlFragment: active-directory-aspnetcore-webapp-openidconnect-v2
description: This sample demonstrates an ASP.NET Core hybrid calling Microsoft Graph API.
languages:
 - csharp
 - javascript
products:
 - aspnet-core
 - azure-active-directory
---

# How to secure an ASP.NET Core Web API with the Microsoft identity platform

[![Build status](https://identitydivision.visualstudio.com/IDDP/_apis/build/status/AAD%20Samples/.NET%20client%20samples/ASP.NET%20Core%20Web%20App%20tutorial)](https://identitydivision.visualstudio.com/IDDP/_build/latest?definitionId=819)

Table Of Contents

* [Scenario](#Scenario)
* [Prerequisites](#Prerequisites)
* [Setup the sample](#Setup-the-sample)
* [Troubleshooting](#Troubleshooting)
* [Using the sample](#Using-the-sample)
* [About the code](#About-the-code)
* [How the code was created](#How-the-code-was-created)
* [How to deploy this sample to Azure](#How-to-deploy-this-sample-to-Azure)
* [Next Steps](#Next-Steps)
* [Contributing](#Contributing)
* [Learn More](#Learn-More)

## Scenario

 This is a ASP.NET Core single page application/web application hybrid sample that calls the Microsoft Graph API using Razor and MSAL.js.

 1. The client ASP.NET Core Web App uses the [Microsoft.Identity.Web](https://aka.ms/microsoft-identity-web) to sign-in and obtain a JWT [Access Tokens](https://docs.microsoft.com/azure/active-directory/develop/access-tokens) from **Azure AD** as well as an additional [Spa Authorization Code](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/SPA-Authorization-Code) to be passed to a client-side single page application.
 1. The [Access Tokens](https://docs.microsoft.com/azure/active-directory/develop/access-tokens) is used as a bearer token to call the **Microsoft Graph API**.
 1. The [Spa Authorization Code](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/SPA-Authorization-Code) is passed to the razor single page application to be exchanged for an access token client side.

## Prerequisites

* Either [Visual Studio](https://visualstudio.microsoft.com/downloads/) or [Visual Studio Code](https://code.visualstudio.com/download) and [.NET Core SDK](https://www.microsoft.com/net/learn/get-started)
* An **Azure AD** tenant. For more information, see: [How to get an Azure AD tenant](https://docs.microsoft.com/azure/active-directory/develop/test-setup-environment#get-a-test-tenant)
* A user account in your **Azure AD** tenant. This sample will not work with a **personal Microsoft account**.  If you're signed in to the [Azure portal](https://portal.azure.com) with a personal Microsoft account and have not created a user account in your directory before, you will need to create one before proceeding.

## Setup the sample

### Step 1: Clone or download this repository

From your shell or command line:

```console
    git clone https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2.git
```

or download and extract the repository .zip file.

>:warning: To avoid path length limitations on Windows, we recommend cloning into a directory near the root of your drive.

### Step 2: Navigate to project folder

```console
    cd 2-WebApp-graph-user\2-5-HybridFlow
```

### Step 3: Application Registration

There are two projects in this sample. Each needs to be separately registered in your Azure AD tenant. To register these projects:

You can follow the [manual steps](#Manual-steps)

**OR**

### Run automation scripts

* use PowerShell scripts that:
  * **automatically** creates the Azure AD applications and related objects (passwords, permissions, dependencies) for you.
  * modify the projects' configuration files.

  <details>
   <summary>Expand this section if you want to use this automation:</summary>

    > **WARNING**: If you have never used **Azure AD Powershell** before, we recommend you go through the [App Creation Scripts guide](./AppCreationScripts/AppCreationScripts.md) once to ensure that your environment is prepared correctly for this step.
  
    1. On Windows, run PowerShell as **Administrator** and navigate to the root of the cloned directory
    1. In PowerShell run:

       ```PowerShell
       Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process -Force
       ```

    1. Run the script to create your Azure AD application and configure the code of the sample application accordingly.
    1. For interactive process - in PowerShell run:

       ```PowerShell
       cd .\AppCreationScripts\
       .\Configure.ps1 -TenantId "[Optional] - your tenant id" -Environment "[Optional] - Azure environment, defaults to 'Global'"
       ```

       > Other ways of running the scripts are described in [App Creation Scripts guide](./AppCreationScripts/AppCreationScripts.md)
       > The scripts also provide a guide to automated application registration, configuration and removal which can help in your CI/CD scenarios.

  </details>

### Manual Steps

 > Note: You should skip the following steps if you've used the automation provided above.

Follow the steps below for manually register and configure your apps

<details>
   <summary>Expand this section to manually register your app(s):</summary>
  1. Sign in to the [Azure portal](https://portal.azure.com).
  1. If your account is present in more than one Azure AD tenant, select your profile at the top right corner in the menu on top of the page, and then **switch directory** to change your portal session to the desired Azure AD tenant.

#### Register the service app

  1. Navigate to the [Azure portal](https://portal.azure.com) and select the **Azure Active Directory** service.
  1. Select the **App Registrations** blade on the left, then select **New registration**.
  1. In the **Register an application page** that appears, enter your application's registration information:
     * In the **Name** section, enter a meaningful application name that will be displayed to users of the app, for example `HybridFlow-aspnetcore`.
  1. Under **Supported account types**, select **Accounts in this organizational directory only**
  1. Click **Register** to create the application.
  1. In the app's registration screen, find and note the **Application (client) ID**. You'll need to use this value in your app's configuration files.
  1. In the app's registration screen, select **Authentication** in the menu.
     * If you don't have a platform added, select **Add a platform** and select the **Web** option.
     * In the **Redirect URIs** section, enter the following redirect URIs.
        * `https://localhost:44321/signin/`
     * Click on the **Add a platform** button in the **Platform configurations** section of the page
       * Select the **Single-page application** button and enter `https://localhost:7089/` as the **Redirect URI** and click the **Configure** button
  1. Select **Save** to save your changes.
  1. In the app's registration screen, select the **Certificates & secrets** blade in the left to open the page where we can generate secrets and upload certificates.
  2. In the **Client secrets** section, select **New client secret**:
     * Type a key description (for instance `app secret`),
     * Select one of the available key durations (**6 months**, **12 months** or **Custom**) as per your security posture.
     * The generated key value will be displayed when you select the **Add** button. Copy and save the generated value for use in later steps.
     * You'll need this key later in your code's configuration files. This key value will not be displayed again, and is not retrievable by any other means, so make sure to note it from the Azure portal before navigating to any other screen or blade.
  3. In the app's registration screen, select the **API permissions** blade in the left to open the page where we add access to the APIs that your application needs.
     * Select the **Add a permission** button and then,
     * Ensure that the **Microsoft APIs** tab is selected.
     * In the *Commonly used Microsoft APIs* section, select **Microsoft Graph**
     * In the **Delegated permissions** section, select the **User.Read** in the list followed by **Mail.Read**. Use the search box if necessary.
     * Select the **Add permissions** button at the bottom.
     * Click the **Grant admin consent** button in the page to allow your application access to these scopes.

##### Configure the application to use your registration

  Open the project in your IDE (like Visual Studio or Visual Studio Code) to configure the code.

   > In the steps below, "ClientID" is the same as "Application ID" or "AppId".

  1. Open the `appsettings.json` file.
  1. Find the key `Domain` and replace the existing value with your Azure AD tenant name.
  1. Find the key `TenantId` and replace the existing value with your Azure AD tenant ID.
  1. Find the key `ClientId` and replace the existing value with the application ID (clientId) of `HybridFlow-aspnetcore` app copied from the Azure portal.
  1. Find the key `ClientSecret` and replace the existing value with the **client secret** of `HybridFlow-aspnetcore` app copied from the Azure portal.

#### (Optional) Create a self-signed certificate

  To complete this step, you will use the `New-SelfSignedCertificate` Powershell command. You can find more information about the New-SelfSignedCertificate command [here](https://docs.microsoft.com/powershell/module/pkiclient/new-selfsignedcertificate).
  
  1. Open PowerShell and run `New-SelfSignedCertificate` with the following parameters to create a self-signed certificate in the user certificate store on your computer:
  
      ```PowerShell
      $cert=New-SelfSignedCertificate -Subject "CN=HybridFlowCert" -CertStoreLocation "Cert:\CurrentUser\My"  -KeyExportPolicy Exportable -KeySpec Signature
      ```
  
  1. Export this certificate using the "Manage User Certificate" MMC snap-in accessible from the Windows Control Panel. You can also add other options to generate the certificate in a different
  store such as the Computer or service store (See [How to: View Certificates with the MMC Snap-in](https://docs.microsoft.com/dotnet/framework/wcf/feature-details/how-to-view-certificates-with-the-mmc-snap-in)).
  
  Alternatively you can use an existing certificate if you have one (just be sure to record its name for the next steps)
  
  ### Add the certificate for the application in Azure AD
  
  In the application registration blade for your application, in the **Certificates & secrets** page, in the **Certificates** section:
  
  1. Click on **Upload certificate** and, in click the browse button on the right to select the certificate you just exported (or your existing certificate)
  1. Click **Add**
  
  ### Configure the Visual Studio project
  
  To change the visual studio project to enable certificates you need to:
  
  1. Open the `appsettings.json` file
  2. Find the app key `Certificate` in the `AzureAd` section and insert the `CertificateDescription` properties of your certificate. You can see some examples below and read more about how to configure certificate descriptions [here](https://github.com/AzureAD/microsoft-identity-web/wiki/Certificates#specifying-certificates).
  
  ### Get certificate from certificate store
  
  You can retrieve a certificate from your local store by adding the configuration below to the `Certificate` property in the `appsettings.json` file replacing **<CERTIFICATE_STORE_PATH>** with the store path to your certificate and **<CERTIFICATE_DISTINGUISHED_NAME>** with the distinguished name of your certificate. If you used the configuration scripts to generate the application this will be done for you using a sample self-signed certificate. You can read more about certificate stores [here](https://docs.microsoft.com/windows-hardware/drivers/install/certificate-stores).
  
  ```json
  {
    // ... 
    "AzureAd": {
      // ...
        "Certificate":  {
          "SourceType":  "StoreWithDistinguishedName",
          "CertificateStorePath":  "<CERTIFICATE_STORE_PATH>",
          "CertificateDistinguishedName":  "<CERTIFICATE_DISTINGUISHED_NAME>"
        }
    }
  }
  ```
  
  #### Get certificate from file path
  
  It's possible to get a certificate file, such as a **pfx** file, directly from a file path on your machine and load it into the application by using the configuration as shown below. Replace the values in the `Certificate` key of the `appsettings.json` with the snippet shown below also replacing `<PATH_TO_YOUR_CERTIFICATE_FILE>` with the path to your certificate file and `<CERTIFICATE_PASSWORD>` with that certificates password. If you created the application with the `Configure.ps1` script found in the `AppCreationScripts-withCert` a **pfx** file called **HybridFlowCert.pfx** will be generated with the certificate that is associated with  your app and can be used as a credential. If you like, you can use configure the `Certificate` property to reference this file and use it as a credential.
  
  ```json
  {
    // ... 
    "AzureAd": {
      // ... 
      "Certificate":  {
        "SourceType":  "Path",
        "CertificateDiskPath":  "<PATH_TO_YOUR_CERTIFICATE_FILE>",
        "CertificatePassword":  "<CERTIFICATE_PASSWORD>"
      }
    } 
  }
  ```
  
  #### Get certificate from Key Vault
  
  It's also possible to get certificates from an [Azure Key Vault](https://docs.microsoft.com/azure/key-vault/general/overview). Replace the values in the `Certificate` key of the `appsettings.json` file with the snippet shown below also replacing `<YOUR_KEY_VAULT_URL>` with the URL of the Key Vault holding your certificate and `<YOUR_KEY_VAULT_CERTIFICATE_NAME>` with the name of that certificate as shown in your Key Vault. If you created the application with the `Configure.ps1` script found in the `AppCreationScripts-withCert` a **pfx** file called **HybridFlowCert.pfx** will be generated that is associated with the certificate that can be used as a credential for your app. If you like, you can load that certificate into a Key Vault and then access that Key Vault to use as a credential for your application.

  ```json
  {
    // ... 
    "AzureAd": {
      // ... 
      "Certificate":  {
        "SourceType":  "KeyVault",
        "KeyVaultUrl":  "<YOUR_KEY_VAULT_URL>",
        "KeyVaultCertificateName":  "<YOUR_KEY_VAULT_CERTIFICATE_NAME>"
      }
    }
  }
  ```
  
  3. If you had set `ClientSecret` previously, change its value to empty string, `""`.

</details>

### Step 4: Running the sample

 For command line run the next commands:

```console
    cd 2-WebApp-graph-user\2-5-HybridFlow
    dotnet run
```

## Using the sample

<details>
 <summary>Expand to see how to use the sample</summary>
 Open your web browser and navigate to `https://localhost:7089`. You should see the application home page with a link for **Home**, **Privacy** and **Sign in**. Click the **Sign in** link and you'll be redirected to the Microsoft login page. Sign in using an user account in your tenant. After you sign in the client side `MSAL.js` client will receive an **auth** code from the server that will be exchanged for an authorization token and cached immediately in the browser.

1. Click on `Profile` tab to see the signed in user's information and contacts. This information is retrieved using the `MSAL.js` library in the browser.

Did the sample not work for you as expected? Did you encounter issues trying this sample? Then please reach out to us using the [GitHub Issues](../../../../issues) page.

[Consider taking a moment to share your experience with us.](https://forms.office.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbRz0h_jLR5HNJlvkZAewyoWxUNEFCQ0FSMFlPQTJURkJZMTRZWVJRNkdRMC4u)
</details>

## About the code

<details>
 <summary>Expand to see more information about the code</summary>

## Server setup

The entire application is built using the [Microsoft Identity Web](https://docs.microsoft.com/azure/active-directory/develop/microsoft-identity-web) library and [ASP.NET Core](https://docs.microsoft.com/aspnet/core/introduction-to-aspnet-core) using [Razor](https://docs.microsoft.com/aspnet/web-pages/overview/getting-started/introducing-razor-syntax-c) pages.

In other samples you may have noticed that samples leveraged a class called `AuthenticationConfig` to bind the settings in the `appsettings.json` file into an `IConfiguration` object to make the settings available throughout the application.

Using the [options pattern in ASP.Net Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-6.0) the configurations set in `appsettings.json` can instead be bound directly into `Options` objects and injected directly into the application.

```csharp
var azureAdOptions = Options
    .Create(builder.Configuration.GetSection("AzureAd").Get<AzureAdOptions>());

var downStreamApiOptions = Options
    .Create(builder.Configuration.GetSection("DownstreamApi").Get<DownstreamApiOptions>());
```

In this application we're going to need to override how **authorization codes** are redeemed by the server by using a [ConfidentialClientApplication](https://docs.microsoft.com/dotnet/api/microsoft.identity.client.confidentialclientapplication?view=azure-dotnet) instance which is discussed later. This same instance is later injected into the application.

```CSharp
var confidentialClientService = new ConfidentialClientApplicationService(azureAdOptions);

builder.Services.AddSingleton<IConfidentialClientApplicationService>(confidentialClientService);
```

Further into the `Program.cs` file you will see the [WebApplicationBuilder](https://docs.microsoft.com/dotnet/api/microsoft.aspnetcore.builder.webapplicationbuilder) **builder** which injects all dependencies into your application.
  * The authentication dependencies are some of the first to be injected into the application using the `AddAuthentication` method. The `OpenIdConnectDefaults.AuthenticationScheme` is provided to configure the application to use the [OpenID Connect protocol](https://docs.microsoft.com/azure/active-directory/develop/v2-protocols-oidc).
  * The `AddMicrosoftIdentityWebApp` method is called to further configure the application to use the authentication services required by **Microsoft Identity**. Because this is using a hybrid flow the `OnAuthorizationCodeReceived` event is customized as follows:
    * To enable [PKCE](https://datatracker.ietf.org/doc/html/rfc7636) the `code_verifier` value is extracted from the URL that would have been used by the flow automatically to be used for attaining a server-side **authentication token** and client-side **authorization code**
    * The server-side **authentication token** and client-side **authorization code** are retrieved simultaneously using the `ConfidentialClientApplicationService` which is discussed in more detail later.
  *  The `AddMicrosoftIdentityWebApp` also configures a special handler for `OnRedirectToIdentityProviderForSignOut` which is triggered after a re-direct from the OpenID provider after signing out to empty the token cache for the user stored in the `ConfidentialClientApplication` within `ConfidentialClientApplicationService`.


```CSharp
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(options =>
    {
        // Needed to make PKCE possible.
        //
        // https://datatracker.ietf.org/doc/html/rfc7636 
        options.ResponseType = OpenIdConnectResponseType.Code;

        // Scopes need to be added in to get proper claims for user.
        var apiScopes = builder.Configuration.GetSection("DownstreamApi:Scopes").Value;

        foreach (var scope in apiScopes.Split(' '))
        {
            options.Scope.Add(scope);
        }

        // This part of the flow needs to be customized for a hybrid flow. In most flows the OnAuthorizationCodeReceived 
        // receives an authorization code from the authorization end point and exchanges it for an access token.
        // This event will still request an access token when it receives an authorization code but it will also
        // request an access code that will be passed to the front-end so it can be exchanged for an access token.
        // The access code is passed to the front-end via the 'Microsoft.Identity.Hybrid.Authentication' session value.
        options.Events.OnAuthorizationCodeReceived = async context =>
        {
            // The 'code_verifier' is an automatically generated value that is used to obtain the auth code. In this
            // case, this value has already been used to retrieve an auth code and must be used again to redeem the
            // server auth code for its access token.
            //
            // See the 'code_verifier' query parameter at these links for further reading.
            //
            // https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-auth-code-flow#request-an-access-token-with-a-client_secret/
            // https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-auth-code-flow#request-an-access-token-with-a-certificate-credential
            context.TokenEndpointRequest.Parameters.TryGetValue("code_verifier", out var codeVerifier);

            if (string.IsNullOrEmpty(codeVerifier))
            {
                throw new Exception("Unable to retrive verify code challenge.");
            }

            // Exchange the auth code for an access token and client-side auth code.
            //
            // The code used in the front-end to exchange for an authentication token is called the 'SpaAuthCode' which
            // is passed along in the 'Microsoft.Identity.Hybrid.Authentication' value.
            //
            // https://docs.microsoft.com/en-us/dotnet/api/microsoft.identity.client.authenticationresult?view=azure-dotnet
            var authResult = await confidentialClientService.GetAuthenticationResultAsync(options.Scope, context.ProtocolMessage.Code, codeVerifier);

            context.Request.HttpContext.Session.SetString("Microsoft.Identity.Hybrid.Authentication", authResult.SpaAuthCode);

            context.HandleCodeRedemption(authResult.AccessToken, authResult.IdToken);
        };

        // Even though the ASP.NET Core middleware handles the call to the Microsoft Identity Platform logout path
        // automatically this application creates a ConfidentialClientApplication in order to handle the auth token
        // flow which also has it's own cache. In order to ensure that cached tokens for users are removed this handler
        // is activated after a user is redirected to the application from the Microsoft Identity logout endpoint.
        //
        // You can find more information here:
        // https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/tree/master/1-WebApp-OIDC/1-6-SignOut
        options.Events.OnRedirectToIdentityProviderForSignOut = async context =>
        {
            var oid = context.HttpContext.User.GetObjectId();
            var tid = context.HttpContext.User.GetTenantId();

            if (!string.IsNullOrEmpty(oid) && !string.IsNullOrEmpty(tid))
            {
                await confidentialClientService.RemoveAccount($"{oid}.{tid}");
            }
        };

        builder.Configuration.Bind("AzureAd", options);
    });
```

The rest of the file configures a basic **ASP.NET Core** app to use **Razor** pages, **Controllers** and a few other configurations.

```CSharp
builder.Services.AddSession();
builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    options.Filters.Add(new AuthorizeFilter(policy));
}).AddMicrosoftIdentityUI();

builder.Services.AddRazorPages();

var app = builder.Build();

app.UseSession();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
    endpoints.MapRazorPages();
});

app.Run();
```

## Server-side Token Redemption

Server-side token acquisition is handled by the `ConfidentialClientApplicationService`. This service makes use of the [ConfidentialClientApplication](https://docs.microsoft.com/dotnet/api/microsoft.identity.client.confidentialclientapplication?view=azure-dotnet) class from the Microsoft Identity Library to redeem access tokens from Azure and also to store token attributed to individual accounts.

The code below shows how the `ConfidentialClientApplicationService` is created. The `AzureAdOptions` made earlier are used to handle the configuration and pass the relevant values to teh `ConfidentialClientApplication`.

```CSharp
private AzureAdOptions _azureAdOptions;

public ConfidentialClientApplicationService(IOptions<AzureAdOptions> azureAdOptions)
{
    _azureAdOptions = azureAdOptions.Value;
}

private IConfidentialClientApplication? _confidentialClientApplication;
private IConfidentialClientApplication ConfidentialClientApplication
{
    get
    {
        if (_confidentialClientApplication is null)
        {

            var clientSecretPlaceholderValue = "[Enter here a client secret for your application]";

            if (!string.IsNullOrWhiteSpace(_azureAdOptions.ClientSecret) &&
                _azureAdOptions.ClientSecret != clientSecretPlaceholderValue)
            {
                _confidentialClientApplication = ConfidentialClientApplicationBuilder.Create(_azureAdOptions.ClientId)
                    .WithClientSecret(_azureAdOptions.ClientSecret)
                    .WithRedirectUri(_azureAdOptions.RedirectUri)
                    .WithAuthority(new Uri(_azureAdOptions.Authority))
                    .Build();
            }
            else if (_azureAdOptions.Certificate is not null)
            {
                ICertificateLoader certificateLoader = new DefaultCertificateLoader();
                certificateLoader.LoadIfNeeded(_azureAdOptions.Certificate);

                _confidentialClientApplication = ConfidentialClientApplicationBuilder.Create(_azureAdOptions.ClientId)
                    .WithCertificate(_azureAdOptions.Certificate.Certificate)
                    .WithRedirectUri(_azureAdOptions.RedirectUri)
                    .WithAuthority(new Uri(_azureAdOptions.Authority))
                    .Build();
            }
            else
            {
                throw new Exception("You must choose between using client secret or certificate. Please update appsettings.json file.");
            }

            _confidentialClientApplication.AddInMemoryTokenCache();
        }

        return _confidentialClientApplication;
    }
}
```

The acquisition of the server-side **authentication token** and **SPA authorization code** directly from Azure is handled within the `GetAuthenticationResultAsync` method. This app uses [PKCE](https://datatracker.ietf.org/doc/html/rfc7636) by default but this can be disabled.

```CSharp
public async Task<AuthenticationResult> GetAuthenticationResultAsync(string code, string codeVerifier)
{
    return await ConfidentialClientApplication
        .AcquireTokenByAuthorizationCode(ApplicationScopes, code)
        .WithPkceCodeVerifier(codeVerifier)
        .WithSpaAuthorizationCode(true)
        .ExecuteAsync();
}
```

Finally the `RemoveAccount` method is responsible for removing the access token associated with the user currently logged in to the application.

```csharp
public async Task RemoveAccount(string identifier)
{
    var userAccount = await ConfidentialClientApplication.GetAccountAsync(identifier);
    if (userAccount is not null)
    {
        await ConfidentialClientApplication.RemoveAsync(userAccount);
    }
}
```

## Client-side MSAL.js Client

The single page application in this sample is run using the [MSAL.js library](https://github.com/AzureAD/microsoft-authentication-library-for-js).

The razor page generating the client will read the settings within the `appsettings.json` file and configure the application for you.

```JavaScript
const msalInstance = new msal.PublicClientApplication({
    auth: {
    @{
    var clientId = Configuration["AzureAd:ClientId"];
    var instance = Configuration["AzureAd:Instance"];
    var domain = Configuration["AzureAd:Domain"];
    var redirectUri = Configuration["SpaRedirectUri"];

    @Html.Raw($"clientId: '{clientId}',");
    @Html.Raw($"redirectUri: '{redirectUri}',");
    @Html.Raw($"authority: '{instance}{domain}',");
    @Html.Raw($"postLogoutRedirectUri: '{redirectUri}',");
}

    },
cache: {
    cacheLocation: 'sessionStorage',
        storeAuthStateInCookie: false,
    }
});
```

## Client-side Authorization Code Redemption

Because this app is configured to make a simple web application using the `AddMicrosoftIdentityWebApp` this makes it possible to associate sessions with each login instance. The authorization code intended for redemption by the client side application is passed into the client side through the `Microsoft.Identity.Hybrid.Authentication` session property as the server redeems an authorization code for itself.

```CSharp
var authResult = await clientService.GetAuthenticationResultAsync(context.ProtocolMessage.Code, codeVerifier);
context.Request.HttpContext.Session.SetString("Microsoft.Identity.Hybrid.Authentication", authResult.SpaAuthCode);
```

This **authorization code** is then extracted by the main razor page and exchanged for an **authentication token** using the **MSAL.js** client
which is then cached in the application and the `Microsoft.Identity.Hybrid.Authentication` value is removed from the session. The code for redeeming the **authentication token** is only executed if the `Microsoft.Identity.Hybrid.Authentication` value in the session is set.

After the either a token is redeemed for the user from Azure or a token is found withing the cache of the [PublicClientApplication](https://azuread.github.io/microsoft-authentication-library-for-js/ref/classes/_azure_msal_browser.publicclientapplication.html) an event is triggered named `AUTHENTICATED` which alerts other parts of the application a token is available to make requests with.

```JavaScript
(function () {
    const scopes = 
    @{
        var apiScopes = Configuration["DownstreamApi:Scopes"].Split(' ');
        @Html.Raw("[");

        foreach(var scope in apiScopes) {
            @Html.Raw($"'{scope}',")
        }

        @Html.Raw("];");

        Context.Session.TryGetValue("Microsoft.Identity.Hybrid.Authentication", out var spaCode);

        if (spaCode is not null && !string.IsNullOrEmpty(Encoding.Default.GetString(spaCode)))
        {
            @Html.Raw($"const code = '{Encoding.Default.GetString(spaCode)}';");
            Context.Session.Remove("Microsoft.Identity.Hybrid.Authentication");
        }
        else
        {
            @Html.Raw($"const code = '';");
        }
    }

    if (!!code) {
        msalInstance
            .handleRedirectPromise()
            .then(result => {
                if (result) {
                    console.log('MSAL: Returning from login');
                    document.dispatchEvent(new Event('AUTHENTICATED'));
                    return result;
                }

                const timeLabel = "Time for acquireTokenByCode";
                console.time(timeLabel);
                console.log('MSAL: acquireTokenByCode hybrid parameters present');

                return msalInstance.acquireTokenByCode({
                    code,
                    scopes,
                })
                    .then(result => {
                        console.timeEnd(timeLabel);
                        console.log('MSAL: acquireTokenByCode completed successfully', result);
                        document.dispatchEvent(new Event('AUTHENTICATED'));
                    })
                    .catch(error => {
                        console.timeEnd(timeLabel);
                        console.error('MSAL: acquireTokenByCode failed', error);
                        if (error instanceof msal.InteractionRequiredAuthError) {
                            console.log('MSAL: acquireTokenByCode failed, redirecting');

                            @{
                                if (User.Identity is not null)
                                {
                                    @Html.Raw($"const username = '{User.Identity.Name}';");
                                }
                                else
                                {
                                    @Html.Raw($"const username = '';");
                                }
                            }

                            const account = msalInstance.getAllAccounts()
                                .find(account => account.username === username);

                            return msalInstance.acquireTokenRedirect({
                                account,
                                scopes
                            });
                        }
                    });
            })
    }
    else {
        document.dispatchEvent(new Event('AUTHENTICATED'));
    }
})();
```

## Client-side Graph API requests

In order to make a successful call to Microsoft Graph you first need to get an access token from the `PublicClientApplication` cache and then use that access token within the `Authorization` header along with the `Bearer` key word.

The function `getTokenFromCache` defined within `_Layout.cshtml` does just that.

```JavaScript
function getTokenFromCache(scopes) {
    @if (User.Identity is not null)
    {
        @Html.Raw($"const username = '{User.Identity.Name}';");
    }
    else
    {
        @Html.Raw($"const username = '';");
    }

    const account = msalInstance.getAllAccounts()
        .find(account => account.username === username);

    return msalInstance.acquireTokenSilent({
        account,
        scopes
    })
        .catch(error => {
            if (error instanceof msal.InteractionRequiredAuthError) {
                return msalInstance.acquireTokenRedirect({
                    account,
                    scopes
                });
            }
        });
};
```

Actual requests made to the Graph API are handled by the `callMSGraph` function in the `_Layout.cshtml` file.

```JavaScript
function callMSGraph(path, token) {
    @{
        var graphEndpoint = Configuration["DownstreamApi:BaseUrl"];
        if (!string.IsNullOrEmpty(graphEndpoint))
        {
            @Html.Raw($"const graphEndpoint = '{graphEndpoint}';");
        }
        else
        {
            @Html.Raw($"const graphEndpoint = '';");
        }
    }

    const headers = new Headers();
    const bearer = `Bearer ${token}`;
    headers.append("Authorization", bearer);

    const options = {
        method: "GET",
        headers
    };

    console.log('request made to Graph API at: ' + new Date().toString());

    return fetch(`${graphEndpoint}${path}`, options)
        .catch(error => console.log(error))
}
```

All requests made by this application to the Graph API are made within the `Profile.cshtml` page.

Before making any requests the JavaScript of the page has a listener for the `AUTHENTICATED` event mentioned earlier. After this event is received requests for the signed in user's profile and contacts is made and rendered into the HTML of the page.

```JavaScript
async function updateUI() {
    await Promise.all([
        loadUserData(),
        loadUserContacts()
    ]);
}

// Only need to run this once to ensure the token has been loaded.
document.addEventListener('AUTHENTICATED', async () => await updateUI(), { once: true });
```

</details>

## Troubleshooting

<details>
 <summary>Expand for troubleshooting info</summary>

Use [Stack Overflow](http://stackoverflow.com/questions/tagged/msal) to get support from the community.
Ask your questions on Stack Overflow first and browse existing issues to see if someone has asked your question before.
Make sure that your questions or comments are tagged with [`azure-active-directory` `adal` `msal` `dotnet`].

If you find a bug in the sample, please raise the issue on [GitHub Issues](../../issues).

To provide a recommendation, visit the following [User Voice page](https://feedback.azure.com/forums/169401-azure-active-directory).
</details>

## Next Steps

Learn how to:

* [Change your app to sign-in users from any organization or any Microsoft accounts](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/tree/master/1-WebApp-OIDC/1-3-AnyOrgOrPersonal)
* [Enable users from National clouds to sign-in to your application](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/tree/master/1-WebApp-OIDC/1-4-Sovereign)
* [Enable your Web App to call a Web API on behalf of the signed-in user](https://github.com/Azure-Samples/ms-identity-dotnetcore-ca-auth-context-app)

## Contributing

If you'd like to contribute to this sample, see [CONTRIBUTING.MD](/CONTRIBUTING.md).

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information, see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Learn More

* Microsoft identity platform (Azure Active Directory for developers)](https://docs.microsoft.com/azure/active-directory/develop/)
* Overview of Microsoft Authentication Library (MSAL)](https://docs.microsoft.com/azure/active-directory/develop/msal-overview)
* Authentication Scenarios for Azure AD](https://docs.microsoft.com/azure/active-directory/develop/authentication-flows-app-scenarios)
* Azure AD code samples](https://docs.microsoft.com/azure/active-directory/develop/sample-v2-code)
* Register an application with the Microsoft identity platform](https://docs.microsoft.com/azure/active-directory/develop/quickstart-register-app)
* Building Zero Trust ready apps](https://aka.ms/ztdevsession)

For more information, visit the following links:

 *To lean more about the application registration, visit:
  *[Quickstart: Register an application with the Microsoft identity platform](https://docs.microsoft.com/azure/active-directory/develop/quickstart-register-app)
  *[Quickstart: Configure a client application to access web APIs](https://docs.microsoft.com/azure/active-directory/develop/quickstart-configure-app-access-web-apis)
  *[Quickstart: Configure an application to expose web APIs](https://docs.microsoft.com/azure/active-directory/develop/quickstart-configure-app-expose-web-apis)

  *To learn more about the code, visit:
  *[Conceptual documentation for MSAL.NET](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki#conceptual-documentation) and in particular:
  *[Acquiring tokens with authorization codes on web apps](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/Acquiring-tokens-with-authorization-codes-on-web-apps)
  *[Customizing Token cache serialization](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/token-cache-serialization)

  *To learn more about security in aspnetcore,
  *[Introduction to Identity on ASP.NET Core](https://docs.microsoft.com/aspnet/core/security/authentication/identity)
  *[AuthenticationBuilder](https://docs.microsoft.com/dotnet/api/microsoft.aspnetcore.authentication.authenticationbuilder)
  *[Azure Active Directory with ASP.NET Core](https://docs.microsoft.com/aspnet/core/security/authentication/azure-active-directory)
