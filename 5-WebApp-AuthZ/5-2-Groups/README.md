---
services: active-directory
platforms: dotnet
author: kalyankrishna1
level: 400
client: ASP.NET Core 2.x Web App
service: Microsoft Graph
endpoint: Microsoft identity platform
---

# Add authorization using groups & group claims to an ASP.NET Core Web app that signs-in users with the Microsoft identity platform

## About this sample

### Overview

This sample shows how a .NET Core 2.2 MVC Web app that uses [OpenID Connect](https://docs.microsoft.com/en-us/azure/active-directory/develop/v1-protocols-openid-connect-code) to sign in users. It also obtains the security group the signed-in user is assigned to as a claim in their token.
Security groups are a popular means to implement authorization.

Authorization in Azure AD can also be done with Application Roles, as shown in [WebApp-RoleClaims](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/tree/master/5-WebApp-AuthZ/5-1-Roles). Azure AD Groups and Application Roles are by no means mutually exclusive - they can be used in tandem to provide even finer grained access control.

[![Build status](https://identitydivision.visualstudio.com/IDDP/_apis/build/status/AAD%20Samples/.NET%20client%20samples/ASP.NET%20Core%20Web%20App%20tutorial)](https://identitydivision.visualstudio.com/IDDP/_build/latest?definitionId=819)

## Scenario

This sample first leverages the ASP.NET Core OpenID Connect middleware to sign in the user. On the home page it displays the various `claims` that the user's [ID Token](https://docs.microsoft.com/en-us/azure/active-directory/develop/id-tokens) contained. The ID token is used by the asp.net security middleware to build the [ClaimsPrincipal](https://docs.microsoft.com/en-us/dotnet/api/system.security.claims.claimsprincipal), accessible via **HttpContext.User**.

![Sign in with the Microsoft identity platform](ReadmeFiles/sign-in.png)

> This is the fifth chapter of a set of tutorials. Once you understand how to receive the group memberships in a user's claims, you can try the sample [Add authorization using app roles & roles claims to an ASP.NET Core Web app thats signs-in users with the Microsoft identity platform](../../5-WebApp-AuthZ/5-1-Roles) to learn about how to use the App roles in an app using the Microsoft Identity Platform to authenticate users. 

> Pre-requisites:
>
> go through the previous phase of the tutorial showing how the [Using the Microsoft identity platform to call the Microsoft Graph API from an An ASP.NET Core 2.x Web App](../../2-WebApp-graph-user/2-1-Call-MSGraph). This page shows the incremental change needed to set up application roles and retrieve them in your app when a user signs in..


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
  cd "5-2-Groups"
  ```

### Step 3: Configure your application to receive the **groups** claims

1. In your application settings page on the Application Registration Portal (preview), click on "Manifest" to open the inline manifest editor.
2. Edit the manifest by locating the "groupMembershipClaims" setting, and setting its value to "SecurityGroup".
3. Save the manifest.

```JSON
{
  ...
  "errorUrl": null,
  "groupMembershipClaims": "SecurityGroup",
  ...
}
```

4. To receive the `groups` claim with the object id of the security groups, make sure that the user accounts you plan to sign-in to this app is assigned to a few security groups in this AAD tenant.

### Step 3: Run the sample

1. Clean the solution, rebuild the solution, and run it.

2. Open your web browser and make a request to the app. The app immediately attempts to authenticate you via the Microsoft identity platform endpoint. Sign in with a work or school account.

1. On the home page, the app lists the various claims it obtained from your ID token. You'd notice one more claims named `groups`. If **Overage** occurred, then you'd see a different claim by the name `_claim_names`. The **Overage** scenario is discussed in detail below. 
1. On the top menu, click on the signed-in user's name **user@domain.com**, you should now see all kind of information about yourself including your picture. Beneath that, a list of all the security groups that the signed-in user is assigned to are listed as well. All of this was obtained by making calls to Microsoft Graph. This list is useful, if an **Overage** occurs with this signed-in user. The **overage** scenario is discussed later in this article. 

> Did the sample not work for you as expected? Did you encounter issues trying this sample? Then please reach out to us using the [GitHub Issues](../../../../issues) page.

#### Processing Groups claim in tokens, including handling **overage**.

#### The `groups` claim.

The object id of the security groups the signed in user is member of is returned in the `groups` claim of the token.

```JSON
{
  ...
  "groups": [
    "0bbe91cc-b69e-414d-85a6-a043d6752215",
    "48931dac-3736-45e7-83e8-015e6dfd6f7c",]
  ...
}
```

#### Groups overage claim

To ensure that the token size doesn’t exceed HTTP header size limits, the Microsoft Identity Platform limits the number of object Ids that it includes in the **groups** claim.

If a user is member of more groups than the overage limit (**150 for SAML tokens, 200 for JWT tokens**), then the Microsoft Identity Platform does not emit the groups claim in the token. Instead, it includes an **overage** claim in the token that indicates to the application to query the [Graph API](https://graph.microsoft.com) to retrieve the user’s group membership.

```JSON
{
  ...
  "_claim_names": {
    "groups": "src1"
    },
    {
   "_claim_sources": {
    "src1": {
        "endpoint":"[Graph Url to get this user's group membership from]"
        }
    }
  ...
}
```
> An Identity Office Hours session covered Azure AD App roles and security groups, featuring this scenario and how to handle the overage claim. Watch the video [Using Security Groups and Application Roles in your apps](https://www.youtube.com/watch?v=V8VUPixLSiM)

 > You can use the `BulkCreateGroups.ps1` provided in the [App Creation Scripts](./AppCreationScripts/) folder to help test overage scenarios.

##### Order of processing the overage claim

1. Check for the claim `_claim_names` with one of the values being `groups`. This indicates overage.

1. If found, make a call to the endpoint specified in `_claim_sources` to fetch user’s groups.

1. If none found, look into the `groups`  claim for user’s groups.

> When attending to overage scenarios, which requires a call to [Microsoft Graph](https://graph.microsoft.com) to read the signed-in user's group memberships, you app will need to have the [Directory.Read.All](https://docs.microsoft.com/en-us/graph/permissions-reference#group-permissions) for the [getMemberObjects](https://docs.microsoft.com/en-us/graph/api/user-getmemberobjects?view=graph-rest-1.0) function to execute successfully.

#### When using the implicit_grant flow to authenticate

In case, you are authenticating using the [implicit grant flow](https://docs.microsoft.com/en-us/azure/active-directory/develop/v1-oauth2-implicit-grant-flow), the **overage** indication and limits are different than the apps using other flows.

1. A claim named `hasgroups` with a value of true will be present in the token instead of the `groups` claim .
1. The maximum number of groups provided in the `groups` claim is limited to 6. This is done to prevent  the URI fragment beyond the URL length limits.

### Support in ASP.NET OWIN middleware libraries

The asp.net middleware supports roles populated from claims by specifying the claim in the `RoleClaimType` property of `TokenValidationParameters`. 
Since the `groups` claim contains the object ids of the security groups than actual names, the following code will not work. 

```CSharp

// Startup.Auth.cs
public void ConfigureAuth(IAppBuilder app)
{
    app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);
    app.UseCookieAuthentication(new CookieAuthenticationOptions());

    //Configure OpenIDConnect, register callbacks for OpenIDConnect Notifications
    app.UseOpenIdConnectAuthentication(
    new OpenIdConnectAuthenticationOptions
    {
        ClientId = ConfigHelper.ClientId,
        Authority = String.Format(CultureInfo.InvariantCulture, ConfigHelper.AadInstance, ConfigHelper.Tenant),
                PostLogoutRedirectUri = ConfigHelper.PostLogoutRedirectUri,
        TokenValidationParameters = new System.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = false,
            RoleClaimType = "groups",
        },
        
        // [removed for] brevity
    });
}


// In code..(Controllers & elsewhere)
[Authorize(Roles = “Group-object-id")]
// or
User.IsInRole("Group-object-id");

```

You’d have to either write your own IAuthorizationFilter or override User.IsInRole to use Azure AD security groups in your code.

## About The code

The following files have the code that would be of interest to you..

1. HomeController.cs
    1. Passes the **HttpContext.User** (the signed-in user) to the view.
1. UserProfileController.cs
    1. Uses the **IMSGraphService** methods to fetch the signed-in user's group memberships.
1 IMSGraphService.cs, MSGraphService.cs and UserGroupsAndDirectoryRoles.cs
    1. Uses the [Microsoft Graph SDK](https://github.com/microsoftgraph/msgraph-sdk-dotnet) to carry out various operations with [Microsoft Graph](https://graph.microsoft.com).
1. Home\Index.cshtml
    1. This has some code to print the current user's claims
1. UserProfile\Index.cshtml
    1. Has some client code that prints the signed-in user's information obtained from the [/me](https://docs.microsoft.com/en-us/graph/api/user-get?view=graph-rest-1.0), [/me/photo](https://docs.microsoft.com/en-us/graph/api/profilephoto-get?view=graph-rest-1.0) and [/memberOf](https://docs.microsoft.com/en-us/graph/api/user-list-memberof?view=graph-rest-1.0) endpoints.
1. Startup.cs
    1.  in the `ConfigureServices` method, replace the two following lines:

     ```CSharp
        .AddMsal(new string[] { "User.Read", "Directory.Read.All" }) // Adds support for the MSAL library with the permissions necessary to retrieve the signed-in user's group info in case of a token overage

        services.AddMSGraphService(Configuration);    // Adds the IMSGraphService as an available service for this app.
     ```
1. if you used the Powershell scripts provided in the [AppCreationScripts](.\AppCreationScripts) folder, then note the extra parameter `-GroupMembershipClaims` in the  `Configure.ps1` script.

     ```PowerShell
       -Oauth2AllowImplicitFlow $true `
       -GroupMembershipClaims "SecurityGroup" `
       -PublicClient $False
     ```


## Community Help and Support

Use [Stack Overflow](http://stackoverflow.com/questions/tagged/msal) to get support from the community.
Ask your questions on Stack Overflow first and browse existing issues to see if someone has asked your question before.
Make sure that your questions or comments are tagged with [ `msal` `azure-active-directory`].

If you find a bug in the sample, please raise the issue on [GitHub Issues](../../../../issues).

To provide a recommendation, visit the following [User Voice page](https://feedback.azure.com/forums/169401-azure-active-directory).

## Contributing

If you'd like to contribute to this sample, see [CONTRIBUTING.MD](/CONTRIBUTING.md).

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information, see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Next steps

- Learn how to use app roles. [Add authorization using app roles & roles claims to a Web app thats signs-in users with the Microsoft identity platform](../../5-WebApp-AuthZ/5-1-Roles).

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
- [Azure AD Connect sync: Understanding Users, Groups, and Contacts](https://docs.microsoft.com/en-us/azure/active-directory/connect/active-directory-aadconnectsync-understanding-users-and-contacts)
- [Configure Office 365 Groups with on-premises Exchange hybrid](https://docs.microsoft.com/en-us/exchange/hybrid-deployment/set-up-office-365-groups)
