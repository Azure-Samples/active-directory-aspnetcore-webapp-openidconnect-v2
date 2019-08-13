---
services: active-directory
platforms: dotnet
author: kalyankrishna1
level: 300
client: ASP.NET Core 2.x Web App
service: Microsoft Graph
endpoint: Microsoft identity platform
---

# Add authorization using **app roles** & **roles** claims to an ASP.NET Core web app thats signs-in users with the Microsoft identity platform

## About this sample

### Overview

This sample shows how a .NET Core 2.2 MVC Web app that uses [OpenID Connect](https://docs.microsoft.com/en-us/azure/active-directory/develop/v1-protocols-openid-connect-code) to sign in users and use Azure AD Application Roles (app roles) for authorization. App roles, along with Security groups are popular means to implement authorization.

This application implements RBAC using Azure AD's Application Roles & Role Claims feature. Another approach is to use Azure AD Groups and Group Claims, as shown in [WebApp-GroupClaims](../../5-WebApp-AuthZ/5-2-Groups). Azure AD Groups and Application Roles are by no means mutually exclusive; they can be used in tandem to provide even finer grained access control.

[![Build status](https://identitydivision.visualstudio.com/IDDP/_apis/build/status/AAD%20Samples/.NET%20client%20samples/ASP.NET%20Core%20Web%20App%20tutorial)](https://identitydivision.visualstudio.com/IDDP/_build/latest?definitionId=819)

Using RBAC with Application Roles and Role Claims, developers can securely enforce authorization policies with minimal effort on their part.

- A Microsoft Identity Platform Office Hours session covered Azure AD App roles and security groups, featuring this scenario and this sample. A recording of the session is is provided in this video [Using Security Groups and Application Roles in your apps](https://www.youtube.com/watch?v=V8VUPixLSiM)

For more information about how the protocols work in this scenario and other scenarios, see [Authentication Scenarios for Azure AD](http://go.microsoft.com/fwlink/?LinkId=394414).

## Scenario

This sample first leverages the ASP.NET Core OpenID Connect middleware to sign in the user. On the home page it displays the various `claims` that the user's [ID Token](https://docs.microsoft.com/en-us/azure/active-directory/develop/id-tokens) contained. The ID token is used by the asp.net security middleware to build the [ClaimsPrincipal](https://docs.microsoft.com/en-us/dotnet/api/system.security.claims.claimsprincipal), accessible via **HttpContext.User** in the code.

This web application allows users to list all users in their tenant or a list of all the roles and groups the signed in user is assigned to depending on the app role they have been assigned to. The idea is to provide an example of how, within an application, access to certain functionality is restricted to subsets of users depending on which role they belong to.

This kind of authorization is implemented using role-based access control (RBAC). When using RBAC, an administrator grants permissions to roles, not to individual users or groups. The administrator can then assign roles to different users and groups to control who has then access to certain content and functionality.  

This sample application defines the following two *Application Roles*:

- `DirectoryViewers`: Have the ability to view any directory user's roles and security group assignments.
- `UserReaders`: Have the ability to view a list of users in the directory.

These application roles are defined in the [Azure portal](https://portal.azure.com) in the application's registration manifest.  When a user signs into the application, Azure AD emits a `roles` claim for each role that the user has been granted individually to the user in the from of role membership.  Assignment of users and groups to roles can be done through the portal's UI, or programmatically using the [Microsoft Graph](https://graph.microsoft.com) and [Azure AD PowerShell](https://docs.microsoft.com/en-us/powershell/module/azuread/?view=azureadps-2.0).  In this sample, application role management is done through the Azure portal or using PowerShell.

NOTE: Role claims will not be present for guest users in a tenant if the `/common` endpoint is used as the authority.

![Sign in with the Microsoft identity platform](ReadmeFiles/sign-in.png)

> This is the sixth chapter of a set of tutorials. In the chapter before this one, you learned how to receive the group memberships in a user's claims. In this one you will learn about how to use the App roles in an app using the Microsoft Identity Platform to authenticate users.

## How to run this sample

To run this sample:

> Pre-requisites:
>
> go through the previous phase of the tutorial showing how the [Using the Microsoft identity platform to call the Microsoft Graph API from an An ASP.NET Core 2.x Web App](../../2-WebApp-graph-user/2-1-Call-MSGraph). This page shows the incremental change needed to set up application roles and retrieve them in your app when a user signs in.

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
git clone https://github.com/Azure-Samples/microsoft-identity-platform-aspnetcore-webapp-tutorial.git
```

or download and exact the repository .zip file.

> Given that the name of the sample is pretty long, and so are the name of the referenced NuGet packages, you might want to clone it in a folder close to the root of your hard drive, to avoid file size limitations on Windows.

Navigate to the `"5-WebApp-AuthZ"` folder

 ```Sh
  cd "5-1-Roles"
  ```

### Step 2: Configure your application to receive the **roles** claims

1. To receive the `roles` claim with the name of the app roles this user is assigned to, make sure that the user accounts you plan to sign-in to this app is assigned to the app roles of this app.

#### Step 3: Define your Application Roles

1. In the blade for your  application in Azure Portal, click **Manifest**.
1. Edit the manifest by locating the `appRoles` setting and adding the two Application Roles.  The role definitions are provided in the JSON code block below.  Leave the `allowedMemberTypes` to **User** only.  Each role definition in this manifest must have a different valid **Guid** for the "id" property. Note that the `"value"` property of each role is set to the exact strings **DirectoryViewers** and **UserReaders** (as these strings are used in the code in the application).
1. Save the manifest.

The content of `appRoles` should be the following (the `id` can be any unique Guid)

```JSon
{
  ...
    "appRoles": [
        {
            "allowedMemberTypes": [
                "User"
            ],
            "description": "User readers can read basic profiles of all users in the directory",
            "displayName": "UserReaders",
            "id": "a816142a-2e8e-46c4-9997-f984faccb625",
            "isEnabled": true,
            "lang": null,
            "origin": "Application",
            "value": "UserReaders"
        },
        {
            "allowedMemberTypes": [
                "User"
            ],
            "description": "Directory viewers can view objects in the whole directory.",
            "displayName": "DirectoryViewers",
            "id": "72ff9f52-8011-49e0-a4f4-cc1bb26206fa",
            "isEnabled": true,
            "lang": null,
            "origin": "Application",
            "value": "DirectoryViewers"
        }
    ],
 ...
}
```

- You can also use PowerShell scripts that **automatically** creates these two roles for your app. They additionally create two users in your tenant and assign them to these two roles. If you want to use this automation:

  1. In PowerShell run the following command to create the two roles and users.

     ```PowerShell
     .\AppCreationScripts\CreateUsersAndRoles.ps1
     ```

  1. Run the following command to remove the two roles and users once you done testing.

     ```PowerShell
     .\AppCreationScripts\CleanupUsersAndRoles.ps1
     ```

> Remember to make changes in the following line (the app name) in the powershell scripts to ensure that the scripts target the app you intended to use.

 ```PowerShell
      $app = Get-AzureADApplication -Filter "DisplayName eq 'WebApp-RolesClaims'"
 ```

   > More details on how to run the scripts are described in [App Creation Scripts](./AppCreationScripts/AppCreationScripts.md)

### Step 4: Run the sample

1. Clean the solution, rebuild the solution, and run it.

1. Open your web browser and make a request to the app. The app immediately attempts to authenticate you via the Microsoft identity platform endpoint. Sign in with a work or school account.

1. You will be prompted to consent during the sign-in process.

![First time Consent](ReadmeFiles/Sign-in-Consent.png)

1. On the home page, the app lists the various claims it obtained from your [ID token](https://docs.microsoft.com/en-us/azure/active-directory/develop/id-tokens). You'd notice a claim named `roles`. There will be one `roles` claim for each app role the signed-in use is assigned to.

1. There also are two links provided on the home page under the **Try one of the following Azure App Role driven operations** heading. These links will result in an access denied error if the signed-in user is not present in the expected role. Sign-out and sign-in with a user account with the correct role assignment to view the contents of these pages.

> Note: You need to be a tenant admin to view the page that lists all the groups and roles the signed-in user is assigned to. It requires the **Directory.Read.All** permission to work. If you run into the **AADSTS65001: The user or administrator has not consented to use the application** error, provide [admin consent](https://docs.microsoft.com/en-us/azure/active-directory/manage-apps/configure-user-consent#grant-admin-consent-when-registering-an-app-in-the-azure-portal) to your app in the portal. Sign-out and sign-in again to make the page work as expected.

When you click on the page that fetches the signed-in user's roles and group assignments, the sample will attempt to obtain consent from you for the **Directory.Read.All** permission using [incremental consent](https://docs.microsoft.com/en-us/azure/active-directory/develop/azure-ad-endpoint-comparison#incremental-and-dynamic-consent).

> Did the sample not work for you as expected? Did you encounter issues trying this sample? Then please reach out to us using the [GitHub Issues](../../../../issues) page.

## About The code

1. In the `ConfigureServices` method of `Startup.cs', add the following line:

     ```CSharp
            // This is required to be instantiated before the OpenIdConnectOptions starts getting configured.
            // By default, the claims mapping will map claim names in the old format to accommodate older SAML applications.
            // 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role' instead of 'roles'
            // This flag ensures that the ClaimsIdentity claims collection will be built from the claims in the token
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
     ```

1. In the `HomeController.cs`, the following method is added with the `Authorize` attribute with the name of the app role **UserReaders**, that permits listing of users in the tenant.

    ```CSharp
        [Authorize(Roles = AppRoles.UserReaders )]
        public async Task<IActionResult> Users()
        {
     ```

1. In the `ConfigureServices` method of `Startup.cs', the following line instructs the asp.net security middleware to use the **roles** claim to fetch roles for authorization:

     ```CSharp
                // The claim in the Jwt token where App roles are available.
                options.TokenValidationParameters.RoleClaimType = "roles";
     ```

1. A new class called `AccountController.cs` is introduced. This contains the code to intercept the default AccessDenied error's route and present the user with an option to sign-out and sign-back in with a different account that has access to the required role.

    ```CSharp
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
     ```

1. The following method is also added with the `Authorize` attribute with the name of the app role **DirectoryViewers**, that permits listing of roles and groups the signed-in user is assigned to.

    ```CSharp
        [Authorize(Roles = AppRoles.DirectoryViewers)]
        public async Task<IActionResult> Groups()
        {
     ```

1. The views, `Users.cshtml` and `Groups.cshtml` have the code to display the users in a tenant and roles and groups the signed-in user is assigned to respectively.

## Next steps

- Learn how to use app groups. [Add authorization using security groups & groups claims to a Web app thats signs-in users with the Microsoft identity platform](../../5-WebApp-AuthZ/5-2-Groups).

## Learn more

- Learn how [Microsoft.Identity.Web](../../Microsoft.Identity.Web) works, in particular hooks-up to the ASP.NET Core ODIC events

To understand more about app registration, see:

- [Quickstart: Register an application with the Microsoft identity platform (Preview)](https://docs.microsoft.com/azure/active-directory/develop/quickstart-register-app)
- [Quickstart: Configure a client application to access web APIs (Preview)](https://docs.microsoft.com/azure/active-directory/develop/quickstart-configure-app-access-web-apis)

To understand more about groups roles and the various claims in tokens, see:

- [Azure Active Directory app manifest](https://docs.microsoft.com/en-us/azure/active-directory/develop/reference-app-manifest)
- [ID tokens](https://docs.microsoft.com/en-us/azure/active-directory/develop/id-tokens)
- [Azure Active Directory access tokens](https://docs.microsoft.com/en-us/azure/active-directory/develop/access-tokens)
- [Microsoft Graph permissions reference](https://docs.microsoft.com/en-us/graph/permissions-reference)
- [user: getMemberObjects function](https://docs.microsoft.com/en-us/graph/api/user-getmemberobjects?view=graph-rest-1.0)
- [Application roles](https://docs.microsoft.com/en-us/azure/architecture/multitenant-identity/app-roles)
- [Token validation](https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet/wiki/ValidatingTokens)
