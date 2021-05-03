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

# Using the Microsoft identity platform to call the Microsoft Graph API from an ASP.NET Core Web App, on behalf of a user signing-in using their work and school account

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

This sample demonstrates a ASP.NET Core Web App calling the Microsoft Graph on behalf of a signed-in user.

## Scenario

1. The client ASP.NET Core Web App uses the [Microsoft.Identity.Web](https://aka.ms/microsoft-identity-web) to sign-in and obtain a JWT [access Tokens](https://aka.ms/access-tokens) from **Azure AD**.
2. The access token is used as a bearer token to authorize the user to call the Microsoft Graph API.

![Sign in with the Microsoft identity platform and call Graph](ReadmeFiles/sign-in.png)


## Prerequisites

- Either [Visual Studio](https://visualstudio.microsoft.com/downloads/) or [Visual Studio Code](https://code.visualstudio.com/download) and [.NET Core SDK](https://www.microsoft.com/net/learn/get-started)
- An **Azure AD** tenant. For more information see: [How to get an Azure AD tenant](https://docs.microsoft.com/azure/active-directory/develop/quickstart-create-new-tenant)
- A user account in your **Azure AD** tenant. This sample will not work with a **personal Microsoft account**. Therefore, if you signed in to the [Azure portal](https://portal.azure.com) with a personal account and have never created a user account in your directory before, you need to do that now.

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
    dotnet restore WebApp-OpenIDConnect-DotNet.csproj
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

### Register the webApp app (WebApp-OpenIDConnect-DotNet-graph-v2)

1. Navigate to the [Azure portal](https://portal.azure.com) and select the **Azure AD** service.
1. Select the **App Registrations** blade on the left, then select **New registration**.
1. In the **Register an application page** that appears, enter your application's registration information:
   - In the **Name** section, enter a meaningful application name that will be displayed to users of the app, for example `WebApp-OpenIDConnect-DotNet-graph-v2`.
   - Under **Supported account types**, select **Accounts in this organizational directory only**.
   - In the **Redirect URI (optional)** section, select **Web** in the combo-box and enter the following redirect URI: `https://localhost:44321/`.
     > Note that there are more than one redirect URIs used in this sample. You'll need to add them from the **Authentication** tab later after the app has been created successfully.
1. Select **Register** to create the application.
1. In the app's registration screen, find and note the **Application (client) ID**. You use this value in your app's configuration file(s) later in your code.
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
   - The generated key value will be displayed when you select the **Add** button. Copy the generated value for use in the steps later.
   - You'll need this key later in your code's configuration files. This key value will not be displayed again, and is not retrievable by any other means, so make sure to note it from the Azure portal before navigating to any other screen or blade.
1. In the app's registration screen, select the **API permissions** blade in the left to open the page where we add access to the APIs that your application needs.
   - Select the **Add a permission** button and then,
   - Ensure that the **Microsoft APIs** tab is selected.
   - In the *Commonly used Microsoft APIs* section, select **Microsoft Graph**
   - In the **Delegated permissions** section, select the **User.Read** in the list. Use the search box if necessary.
   - Select the **Add permissions** button at the bottom.

#### Configure the webApp app (WebApp-OpenIDConnect-DotNet-graph-v2) to use your app registration

Open the project in your IDE (like Visual Studio or Visual Studio Code) to configure the code.

Open the project in your IDE (like Visual Studio) to configure the code.
>In the steps below, "ClientID" is the same as "Application ID" or "AppId".

1. Open the `appsettings.json` file.
1. Find the key `ClientId` and replace the existing value with the application ID (clientId) of `WebApp-OpenIDConnect-DotNet-graph-v2` app copied from the Azure portal.
1. Find the key `TenantId` and replace the existing value with your Azure AD tenant ID.
1. Find the key `Domain` and replace the existing value with your Azure AD tenant name.
1. Find the key `ClientSecret` and replace the existing value with the key you saved during the creation of `WebApp-OpenIDConnect-DotNet-graph-v2` copied from the Azure portal.

- In case you want to deploy your app in Sovereign or national clouds, ensure the `GraphApiUrl` option matches the one you want. By default this is Microsoft Graph in the Azure public cloud

  ```JSon
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

1. Open your web browser and make a request to the app at url `https://localhost:44321`. The app immediately attempts to authenticate you via the Microsoft identity platform endpoint. Sign in with a work or school account.

1. Provide consent to the screen presented

1. Click on the **Profile** link on the top menu, you should now see all kind of information about yourself as well as your picture (a call was made to the Microsoft Graph */me* endpoint)

> Did the sample not work for you as expected? Did you encounter issues trying this sample? Then please reach out to us using the [GitHub Issues](../../../../issues) page.

> [Consider taking a moment to share your experience with us.](https://forms.office.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbRz0h_jLR5HNJlvkZAewyoWxUNEFCQ0FSMFlPQTJURkJZMTRZWVJRNkdRMC4u)

## About The code

1. In your aspnet core web project, add the `Microsoft.Identity.Web` and `Microsoft.Identity.Web.UI` NuGet packages. It's used to simplify signing-in and acquiring tokens for Microsoft Graph.

1. Open the **Startup.cs** file and:

   - at the top of the file, add the following using directive:

     ```CSharp
      using Microsoft.Identity.Web;
      using Microsoft.Identity.Web.UI;
      ```

   - in the `ConfigureServices` method, replace the  following lines:

     ```CSharp
           services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(Configuration.GetSection("AzureAd"))
                .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
                .AddInMemoryTokenCaches();
     ```

     These enables your application to use the Microsoft identity platform endpoint. This endpoint is capable of signing-in users both with their Work and School and Microsoft Personal accounts (if required).

    1. Change the `Properties\launchSettings.json` file to ensure that you start your web app from <https://localhost:44321> as registered. For this:
    - update the `sslPort` of the `iisSettings` section to be `44321`
    - in the `applicationUrl` property of use `https://localhost:44321`

### Update the `Startup.cs` file to enable TokenAcquisition by [Microsoft.Identity.Web](https://aka.ms/microsoft-identity-web)

After the following lines in the ConfigureServices(IServiceCollection services) method, replace `services.AddMicrosoftIdentityPlatformAuthentication(Configuration);`, by the following lines:

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

### Add additional files to call Microsoft Graph

Add the `Microsoft.Identity.Web.MicrosoftGraph` package, to use [Microsoft Graph SDK](https://github.com/microsoftgraph/msgraph-sdk-dotnet/blob/dev/docs/overview.md).

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

In the `Controllers\HomeController.cs`file:

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

1. Install the VS Code extension [Azure App Service](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azureappservice).
1. Sign-in to App Service using Azure AD Account.
1. Open the WebApp-OpenIDConnect-DotNet-graph-v2 project folder.
1. Choose View > Terminal from the main menu.
1. The terminal opens in the WebApp-OpenIDConnect-DotNet-graph-v2 folder.
1. Run the following command:

    ```console
    dotnet publish --configuration Release
    ```

1. Publish folder is created under path ``bin/Release/<Enter_Framework_FolderName>``.
1. Right Click on **Publish** folder and select **Deploy to Web App**.
1. Select **Create New Web App**, enter unique name for the app.
1. Select Windows as the OS. Press Enter.

#### Disable Azure App Services default authentication (WebApp-OpenIDConnect-DotNet-graph-v2)

1. Go to [Azure portal](https://portal.azure.com), and locate your project there.
    - On the Settings tab, select **Authentication/Authorization**. Make sure **App Service Authentication** is `off` (as we are using our own **custom** authentication logic). 
    - Select **Save**.
1. Browse your website. If you see the default web page of the project, then the publication was successful.

#### Update the Azure AD app registration (WebApp-OpenIDConnect-DotNet-graph-v2)

1. Navigate back to to the [Azure portal](https://portal.azure.com).
In the left-hand navigation pane, select the **Azure Active Directory** service, and then select **App registrations**.
1. In the resulting screen, select the `WebApp-OpenIDConnect-DotNet-graph-v2` application.
1. In the app's registration screen, select **Authentication** in the menu.
   - In the **Redirect URIs** section, update the reply URLs to match the site URL of your Azure deployment. For example:
      - `https://WebApp-OpenIDConnect-DotNet-graph-v2.azurewebsites.net/`
      - `https://WebApp-OpenIDConnect-DotNet-graph-v2.azurewebsites.net/signin-oidc`
   - Update the **Front-channel logout URL** fields with the address of your service, for example [https://WebApp-OpenIDConnect-DotNet-graph-v2.azurewebsites.net](https://WebApp-OpenIDConnect-DotNet-graph-v2.azurewebsites.net)

> :warning: If your app is using an *in-memory* storage, **Azure App Services** will spin down your web site if it is inactive, and any records that your app was keeping will emptied. In addition, if you increase the instance count of your website, requests will be distributed among the instances. Your app's records, therefore, will not be the same on each instance.

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

For more information about how OAuth 2.0 protocols work in this scenario and other scenarios, see [Authentication Scenarios for Azure AD](https://docs.microsoft.com/azure/active-directory/develop/authentication-flows-app-scenarios).

## Community Help and Support

Use [Stack Overflow](http://stackoverflow.com/questions/tagged/msal) to get support from the community.
Ask your questions on Stack Overflow first and browse existing issues to see if someone has asked your question before.
Make sure that your questions or comments are tagged with [`azure-active-directory` `azure-ad-b2c` `ms-identity` `adal` `msal`].

If you find a bug in the sample, raise the issue on [GitHub Issues](../../../issues).

To provide feedback on or suggest features for Azure Active Directory, visit [User Voice page](https://feedback.azure.com/forums/169401-azure-active-directory).

## Contributing

If you'd like to contribute to this sample, see [CONTRIBUTING.MD](/CONTRIBUTING.md).

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information, see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

