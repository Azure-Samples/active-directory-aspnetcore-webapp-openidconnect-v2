---
services: active-directory
platforms: dotnet
author: jmprieur
---

# Integrating Azure AD into an ASP.NET Core web app

This sample shows how to build a .NET MVC web app that uses OpenID Connect to sign-in users from a single Azure Active Directory (Azure AD) tenant using the ASP.NET Core OpenID Connect middleware.

For more information on how the protocols work in this scenario and other scenarios, see [Authentication Scenarios for Azure AD](http://go.microsoft.com/fwlink/?LinkId=394414).

## How to run this sample

If you are interested in ASP.NET Core 1.1, please look at branch [aspnet_core_1_1](https://github.com/Azure-Samples/active-directory-dotnet-webapp-openidconnect-aspnetcore/tree/aspnet_core_1_1).

To run this sample:
- Install .NET Core for Windows by following the instructions at [.NET and C# - Get Started in 10 Minutes](https://www.microsoft.com/net/core). In addition to developing on Windows, you can develop on [Linux](https://www.microsoft.com/net/core#linuxredhat), [Mac](https://www.microsoft.com/net/core#macos), or [Docker](https://www.microsoft.com/net/core#dockercmd).
- An Azure AD tenant. For more information on how to obtain an Azure AD tenant, see [How to get an Azure AD tenant](https://azure.microsoft.com/documentation/articles/active-directory-howto-tenant/).

### Step 1: Register the sample with your Azure AD tenant

1. Sign in to the [Azure portal](https://portal.azure.com).

2. On the top bar, select your account. Under the **DIRECTORY** list, choose the Active Directory tenant where you wish to register your app. If there isn't a **DIRECTORY** list in the drop down menu, skip this step, as you only have a single tenant associated with your Azure account. For more information, see [How to get an Azure Active Directory tenant](https://docs.microsoft.com/azure/active-directory/develop/active-directory-howto-tenant).

3. In the left navigation sidebar, select **Azure Active Directory**. If you don't see **Azure Active Directory** in the list, select **More Services** and choose **Azure Active Directory** in the **SECURITY + IDENTITY** section of the service list.

4. From the sidebar, select **App registrations**.

5. Select **New application registration** and provide a friendly name for the app, app type, and sign-on URL:
   **Name**: **WebApp-OpenIDConnect-DotNet**
   **Application Type**: **Web app / API**
   **Sign-on URL**: `http://localhost:5000/signin-oidc`
   Select **Create** to register the app.

6. On the **Properties** blade, set the **Logout URL** to `http://localhost:5000/signout-oidc` and select **Save**.

7. From the Azure portal, note the following information:

   **The Tenant domain:** See the **App ID URI** base URL. For example: `contoso.onmicrosoft.com`
   
   **The Tenant ID:** See the **Endpoints** blade. Record the GUID from any of the endpoint URLs. For example: `da41245a5-11b3-996c-00a8-4d99re19f292`
   
   **The Application ID (Client ID):** See the **Properties** blade. For example: `ba74781c2-53c2-442a-97c2-3d60re42f403`

> [!NOTE]
> The base address in the **Sign-on URL** and **Logout URL** settings is `http://localhost:5000`. This localhost address allows the sample app to run insecurely from your local system. Port 5000 is the default port for the [Kestrel server](https://docs.microsoft.com/aspnet/core/fundamentals/servers/kestrel). Update these URLs if you configure the app for production use (for example, `https://www.contoso.com/signin-oidc` and `https://www.contoso.com/signout-oidc`).

### Step 2: Create the sample

This sample was created from the 2.0 [dotnet new mvc](https://docs.microsoft.com/dotnet/core/tools/dotnet-new?tabs=netcore2x) template with `SingleOrg` authentication. You can create the sample from the command line or clone/download this repository:

- To create the sample from the command line, execute the following command:

  ```console
  dotnet new mvc --auth SingleOrg --client-id <CLIENT_ID_(APP_ID)> --tenant-id <TENANT_ID> --domain <TENANT_DOMAIN>
  ```
  Use the values that you recorded from the Azure portal for \<CLIENT\_ID\_(APP\_ID)>, \<TENANT\_ID>, and \<TENANT\_DOMAIN>.

- To clone/download this sample, execute the following command from your shell or command line:

  ```console
  git clone https://github.com/Azure-Samples/active-directory-dotnet-webapp-openidconnect-aspnetcore.git
  ```

  In the **appsettings.json* file, provide values for the `Domain`, `TenantId`, and `ClientID` that you recorded earlier from the Azure portal.

### Step 3: Run the sample

Build the solution and run it.

Make a request to the app. The app immediately attempts to authenticate you via Azure AD. Sign in with the username and password of a user account that is in your Azure AD tenant. You can also use your tenant's Global Administrator account. If you wish to create a user in the tenant, select **Add a user** from the **Quick tasks** panel. The **Quick tasks** panel is found on the Azure AD tenant's blade in the portal.

## About The code

This sample shows how to use the OpenID Connect ASP.NET Core middleware to sign-in users from a single Azure AD tenant. The middleware is initialized in the `Startup.cs` file by passing it the Client ID of the app and the URL of the Azure AD tenant where the app is registered, which is read from the `appsettings.json` file. The middleware takes care of:
- Downloading the Azure AD metadata, finding the signing keys, and finding the issuer name for the tenant.
- Processing OpenID Connect sign-in responses by validating the signature and issuer in an incoming JWT, extracting the user's claims, and putting the claims in `ClaimsPrincipal.Current`.
- Integrating with the session cookie ASP.NET Core middleware to establish a session for the user. 

You can trigger the middleware to send an OpenID Connect sign-in request by decorating a class or method with the `[Authorize]` attribute or by issuing a challenge (see the `AccountController.cs` file):

```csharp
return Challenge(
    new AuthenticationProperties { RedirectUri = redirectUrl }, 
    OpenIdConnectDefaults.AuthenticationScheme);
```

Similarly, you can send a signout request:

```csharp
return SignOut(
    new AuthenticationProperties { RedirectUri = callbackUrl }, 
    CookieAuthenticationDefaults.AuthenticationScheme, 
    OpenIdConnectDefaults.AuthenticationScheme);
```

The middleware in this project is created as a part of the open source [ASP.NET Security](https://github.com/aspnet/Security) project.
