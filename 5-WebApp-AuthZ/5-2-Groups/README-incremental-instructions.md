---
services: active-directory
platforms: dotnet
author: kalyankrishna1
level: 300
client: ASP.NET Core Web App
service: Microsoft Graph
endpoint: Microsoft identity platform
page_type: sample
languages:
  - csharp  
products:
  - azure
  - azure-active-directory  
  - dotnet
  - office-ms-graph
description: "Add authorization using groups & group claims to an ASP.NET Core Web app that signs-in users with the Microsoft identity platform"
---

# Add authorization using groups & group claims to an ASP.NET Core Web app that signs-in users with the Microsoft identity platform

[![Build status](https://identitydivision.visualstudio.com/IDDP/_apis/build/status/AAD%20Samples/.NET%20client%20samples/ASP.NET%20Core%20Web%20App%20tutorial)](https://identitydivision.visualstudio.com/IDDP/_build/latest?definitionId=819)

## About this sample

### Overview

This sample shows how a .NET Core MVC Web app that uses [OpenID Connect](https://docs.microsoft.com/azure/active-directory/develop/v1-protocols-openid-connect-code) to sign in users also obtains the security groups the signed-in user is assigned to as a claim in their token. Security groups are a popular means to implement authorization.

Authorization in Azure AD can also be done with `Application Roles` as well, as shown in [WebApp-RoleClaims](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/tree/master/5-WebApp-AuthZ/5-1-Roles/README.md). `Groups` and `Application Roles` in Azure AD are by no means mutually exclusive - they can be used in tandem to provide even finer grained access control.

## Scenario

This sample first leverages the ASP.NET Core OpenID Connect middleware to sign in the user. On the home page it displays the various `claims` that the signed-in user's [ID Token](https://docs.microsoft.com/azure/active-directory/develop/id-tokens) contained. The ID token is used by the asp.net security middleware to build the [ClaimsPrincipal](https://docs.microsoft.com/dotnet/api/system.security.claims.claimsprincipal), accessible via **HttpContext.User**.

![Sign in with the Microsoft identity platform](ReadmeFiles/sign-in.png)

> An Identity Developer session covered Azure AD App roles and security groups, featuring this scenario and how to handle the overage claim. Watch the video [Using Security Groups and Application Roles in your apps](https://www.youtube.com/watch?v=LRoc-na27l0)
> Prerequisites:
>
> This guide assumes that you've already went through the previous chapter of the tutorial [Using the Microsoft identity platform to call the Microsoft Graph API from an An ASP.NET Core Web App](../../2-WebApp-graph-user/2-1-Call-MSGraph). This page shows the incremental change needed to set up group membership claims and retrieve them in your app when a user signs in.

To run this sample, you'll need:

- [Visual Studio 2019](https://aka.ms/vsdownload) or just the [.NET Core SDK](https://www.microsoft.com/net/learn/get-started)
- An Internet connection
- A Windows machine (necessary if you want to run the app on Windows)
- An OS X machine (necessary if you want to run the app on Mac)
- A Linux machine (necessary if you want to run the app on Linux)
- An Azure Active Directory (Azure AD) tenant. For more information on how to get an Azure AD tenant, see [How to get an Azure AD tenant](https://azure.microsoft.com/documentation/articles/active-directory-howto-tenant/)
- A user account in your Azure AD tenant. This sample will not work with a Microsoft account (formerly Windows Live account). Therefore, if you signed in to the [Azure portal](https://portal.azure.com) with a Microsoft account and have never created a user account in your directory before, you need to do that now.

 > Please make sure to have one or more user accounts in the tenant assigned to a few security groups in your tenant. Please follow the instructions in [Create a basic group and add members using Azure Active Directory](https://docs.microsoft.com/azure/active-directory/fundamentals/active-directory-groups-create-azure-portal) to create a few groups and assign users to them if not already done.

### Step 1:  Clone or download this repository

From your shell or command line:

Navigate to the `"5-WebApp-AuthZ"` folder

 ```Sh
  cd 5-WebApp-AuthZ\5-2-Groups
  ```

### Step 3: Configure your application to receive the **groups** claim

Now you have two different options available to you on how you can further configure your application to receive the `groups` claim.

1. [Receive **all the groups** that the signed-in user is assigned to in an Azure AD tenant, included nested groups](#configure-your-application-to-receive-all-the-groups-the-signed-in-user-is-assigned-to-included-nested-groups).
1. [Receive the **groups** claim values from a **filtered set of groups** that your application is programmed to work with](#configure-your-application-to-receive-the-groups-claim-values-from-a-filtered-set-of-groups-a-user-may-be-assigned-to). (Not available in the [Azure AD Free edition](https://azure.microsoft.com/pricing/details/active-directory/)).

> To get the on-premise group's `samAccountName` or `On Premises Group Security Identifier` instead of Group id, please refer to the document [Configure group claims for applications with Azure Active Directory](https://docs.microsoft.com/azure/active-directory/hybrid/how-to-connect-fed-group-claims#prerequisites-for-using-group-attributes-synchronized-from-active-directory).

#### Configure your application to receive **all the groups** the signed-in user is assigned to, included nested groups

1. In the app's registration screen, click on the **Token Configuration** blade in the left to open the page where you can configure the claims provided tokens issued to your application.
1. Click on the **Add groups claim** button on top to open the **Edit Groups Claim** screen.
1. Select `Security groups` **or** the `All groups (includes distribution lists but not groups assigned to the application)` option. Choosing both negates the effect of `Security Groups` option.
1. Under the **ID** section, select `Group ID`. This will result in Azure AD sending the [object id](https://docs.microsoft.com/graph/api/resources/group?view=graph-rest-1.0) of the groups the user is assigned to in the **groups** claim of the [ID Token](https://docs.microsoft.com/azure/active-directory/develop/id-tokens) that your app receives after signing-in a user.
1. If you are exposing a Web API using the **Expose an API** option, then you can also choose the `Group ID` option under the **Access** section. This will result in Azure AD sending the [object id](https://docs.microsoft.com/graph/api/resources/group?view=graph-rest-1.0) of the groups the user is assigned to in the `groups` claim of the [Access Token](https://docs.microsoft.com/azure/active-directory/develop/access-tokens) issued to the client applications of your API.

#### Configure your application to receive the `groups` claim values from a **filtered set of groups** a user may be assigned to

##### Prerequisites, benefits and limitations of using this option

1. This option is useful when your application is interested in a selected set of groups that a signing-in user may be assigned to and not every security group this user is assigned to in the tenant.  This option also saves your application from running into the [overage](#groups-overage-claim) issue.
1. This feature is not available in the [Azure AD Free edition](https://azure.microsoft.com/pricing/details/active-directory/).
1. **Nested group assignments** are not available when this option is utilized.

##### Steps to enable this option in your app

1. In the app's registration screen, click on the **Token Configuration** blade in the left to open the page where you can configure the claims provided tokens issued to your application.
1. Click on the **Add groups claim** button on top to open the **Edit Groups Claim** screen.
1. Select `Groups assigned to the application`.
    1. Choosing additional options like `Security Groups` or `All groups (includes distribution lists but not groups assigned to the application)` will negate the benefits your app derives from choosing to use this option.
1. Under the **ID** section, select `Group ID`. This will result in Azure AD sending the object [id](https://docs.microsoft.com/graph/api/resources/group?view=graph-rest-1.0) of the groups the user is assigned to in the `groups` claim of the [ID Token](https://docs.microsoft.com/azure/active-directory/develop/id-tokens) that your app receives after signing-in a user.
1. If you are exposing a Web API using the **Expose an API** option, then you can also choose the `Group ID` option under the **Access** section. This will result in Azure AD sending the object [id](https://docs.microsoft.com/graph/api/resources/group?view=graph-rest-1.0) of the groups the user is assigned to in the `groups` claim of the [Access Token](https://docs.microsoft.com/azure/active-directory/develop/access-tokens) issued to the client applications of your API.
1. In the app's registration screen, click on the **Overview** blade in the left to open the Application overview screen. Select the hyperlink with the name of your application in **Managed application in local directory** (note this field title can be truncated for instance `Managed application in ...`). When you select this link you will navigate to the **Enterprise Application Overview** page associated with the service principal for your application in the tenant where you created it. You can navigate back to the app registration page by using the back button of your browser.
1. Select the **Users and groups** blade in the left to open the page where you can assign users and groups to your application.
    1. Click on the **Add user** button on the top row.
    1. Select **User and Groups** from the resultant screen.
    1. Choose the groups that you want to assign to this application.
    1. Click **Select** in the bottom to finish selecting the groups.
    1. Click **Assign** to finish the group assignment process.  
    1. Your application will now receive these selected groups in the `groups` claim when a user signing in to your app is a member of  one or more these **assigned** groups.
1. Select the **Properties** blade in the left to open the page that lists the basic properties of your application.Set the **User assignment required?** flag to **Yes**.

   > **Important security tip**
   >
   > When you set **User assignment required?** to **Yes**, Azure AD will check that only users assigned to your application in the **Users and groups** blade are able to sign-in to your app. You can assign users directly or by assigning security groups they belong to.

### Step 4: Run the sample

1. Clean and rebuild the solution, and run it.

1. Open your web browser and make a request to the app. The app immediately attempts to authenticate you to the Microsoft identity platform. Sign in with a *work or school account* from the tenant where you created this app.
1. On the home page, the app lists the various claims it obtained from your ID token. You'd notice one more claims named `groups`. 
1. On the top menu, click on the signed-in user's name **user@domain.com**, you should now see all kind of information about yourself including their picture. Beneath that, a list of all the security groups that the signed-in user is assigned to are listed as well. All of this was obtained by making calls to Microsoft Graph. This list is useful if the **Overage** scenario occurs with this signed-in user. The [overage](#groups-overage-claim) scenario is discussed later in this article.

> Did the sample not work for you as expected? Did you encounter issues trying this sample? Then please reach out to us using the [GitHub Issues](../../../../issues) page.

### Processing Groups claim in tokens, including handling **overage**

#### The `groups` claim

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

### Support in ASP.NET Core middleware libraries

The asp.net middleware supports roles populated from claims by specifying the claim in the `RoleClaimType` property of `TokenValidationParameters`.
Since the `groups` claim contains the object ids of the security groups than actual names by default, you'd use the group id's instead of group names. See [Role-based authorization in ASP.NET Core](https://docs.microsoft.com/aspnet/core/security/authorization/roles) for more info.

```CSharp
// Startup.cs

// The following lines code instruct the asp.net core middleware to use the data in the "groups" claim in the [Authorize] attribute and for User.IsInrole()
// See https://docs.microsoft.com/aspnet/core/security/authorization/roles
services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    // Use the groups claim for populating roles
    options.TokenValidationParameters.RoleClaimType = "groups";
});

// In code..(Controllers & elsewhere)
[Authorize(Roles = "Group-object-id")] // In controllers
// or
User.IsInRole("Group-object-id"); // In methods

```

### The groups overage claim

To ensure that the token size doesn’t exceed HTTP header size limits, the Microsoft Identity Platform limits the number of object Ids that it includes in the **groups** claim.

If a user is member of more groups than the overage limit (**150 for SAML tokens, 200 for JWT tokens, 6 for Single Page applications**, ), then the Microsoft Identity Platform does not emit the group ids in the `groups` claim in the token. Instead, it includes an **overage** claim in the token that indicates to the application to query the [Graph API](https://graph.microsoft.com) to retrieve the user’s group membership.

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

##### Create the overage scenario in this sample for testing

1. You can use the `BulkCreateGroups.ps1` provided in the [App Creation Scripts](./AppCreationScripts/) folder to create a large number of groups and assign users to them. This will help test overage scenarios during development. Remember to change the user's objectId provided in the `BulkCreateGroups.ps1` script.
1. When you run this sample and an overage occurred, then you'd see the  `_claim_names` in the home page after the user signs-in.
1. We strongly advise you use the [group filtering feature](#configure-your-application-to-receive-the-groups-claim-values-from-a-filtered-set-of-groups-a-user-may-be-assigned-to) (if possible) to avoid running into group overages.
1. In case you cannot avoid running into group overage, we suggest you use the following logic to process groups claim in your token.  
    1. Check for the claim `_claim_names` with one of the values being `groups`. This indicates overage.
    1. If found, make a call to the endpoint specified in `_claim_sources` to fetch user’s groups.
    1. If none found, look into the `groups`  claim for user’s groups.

> When attending to overage scenarios, which requires a call to [Microsoft Graph](https://graph.microsoft.com) to read the signed-in user's group memberships, your app will need to have the [GroupMember.Read.All](https://docs.microsoft.com/graph/permissions-reference#group-permissions) for the [getMemberObjects](https://docs.microsoft.com/graph/api/user-getmemberobjects?view=graph-rest-1.0) function to execute successfully.

> Developers who wish to gain good familiarity of programming for Microsoft Graph are advised to go through the [An introduction to Microsoft Graph for developers](https://www.youtube.com/watch?v=EBbnpFdB92A) recorded session.

##### When you are a single page application and using the implicit grant flow to authenticate

In case, you are authenticating using the [implicit grant flow](https://docs.microsoft.com/azure/active-directory/develop/v1-oauth2-implicit-grant-flow), the **overage** indication and limits are different than the apps using other flows.

1. A claim named `hasgroups` with a value of true will be present in the token instead of the `groups` claim .
1. The maximum number of groups provided in the `groups` claim is limited to 6. This is done to prevent  the URI fragment beyond the URL length limits.

## About the code

### Create the sample from the command line

> The following code used an older version of `[Microsoft.Identity.Web](https://github.com/AzureAD/microsoft-identity-web) library and would be updated when the library becomes Generally Available.

This project was created using the following command.


## About the code

The following files have the code that would be of interest to you:

1. HomeController.cs
    1. Passes the **HttpContext.User** (the signed-in user) to the view.
1. UserProfileController.cs
    1. Uses the **IMSGraphService** methods to fetch the signed-in user's group memberships.
1. IMSGraphService.cs, MSGraphService.cs and UserGroupsAndDirectoryRoles.cs
    1. Uses the [Microsoft Graph SDK](https://github.com/microsoftgraph/msgraph-sdk-dotnet) to carry out various operations with [Microsoft Graph](https://graph.microsoft.com).
1. Home\Index.cshtml
    1. This has some code to print the current user's claims
1. UserProfile\Index.cshtml
    1. Has some client code that prints the signed-in user's information obtained from the [/me](https://docs.microsoft.com/graph/api/user-get?view=graph-rest-1.0), [/me/photo](https://docs.microsoft.com/graph/api/profilephoto-get) and [/memberOf](https://docs.microsoft.com/graph/api/user-list-memberof) endpoints.
1. Startup.cs

    - at the top of the file, add the following using directive:

      ```CSharp
         using Microsoft.Identity.Web;
      ```

   - in the `ConfigureServices` method, the following lines:

     ```CSharp
      services.AddAuthentication(AzureADDefaults.AuthenticationScheme)
              .AddAzureAD(options => Configuration.Bind("AzureAd", options));
     ```

    - have been replaced by these lines::

      ```CSharp
      services.AddMicrosoftIdentityWebAppAuthentication(Configuration)
              .EnableTokenAcquisitionToCallDownstreamApi( new string[] { "User.Read", "Directory.Read.All" })
              .AddInMemoryTokenCaches();

      services.AddMSGraphService(Configuration);    // Adds the IMSGraphService as an available service for this app.
      ```

1. if you used the PowerShell scripts provided in the [AppCreationScripts](.\AppCreationScripts) folder, then note the extra parameter `-GroupMembershipClaims` in the  `Configure.ps1` script.

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

## Next steps

- Learn how to use app roles. [Add authorization using app roles & roles claims to a Web app that signs-in users with the Microsoft identity platform](../../5-WebApp-AuthZ/5-1-Roles/README.md).

## Learn more

- Learn how [Microsoft.Identity.Web](../../Microsoft.Identity.Web) works, in particular hooks-up to the ASP.NET Core ODIC events

- To understand more about groups roles and the various claims in tokens, see:
  - [Configure group claims for applications with Azure Active Directory (Public Preview)](https://docs.microsoft.com/azure/active-directory/hybrid/how-to-connect-fed-group-claims#configure-the-azure-ad-application-registration-for-group-attributes)
  - [A .NET 4.5 MVC web app that uses Azure AD groups for authorization.](https://github.com/Azure-Samples/active-directory-dotnet-webapp-groupclaims/blob/master/README.md)
  - [Azure Active Directory app manifest](https://docs.microsoft.com/azure/active-directory/develop/reference-app-manifest)
  - [Application roles](https://docs.microsoft.com/azure/architecture/multitenant-identity/app-roles)
  - [user: getMemberObjects function](https://docs.microsoft.com/graph/api/user-getmemberobjects?view=graph-rest-1.0)
  - [Microsoft Graph permissions reference](https://docs.microsoft.com/graph/permissions-reference)

- Learn more about on-prem groups synchronization to Azure AD  
  - [Azure AD Connect sync: Understanding Users, Groups, and Contacts](https://docs.microsoft.com/azure/active-directory/connect/active-directory-aadconnectsync-understanding-users-and-contacts)
  - [Configure Office 365 Groups with on-premises Exchange hybrid](https://docs.microsoft.com/exchange/hybrid-deployment/set-up-office-365-groups)

- To learn more about Azure AD's supported protocols and tokens  
  - [Azure AD protocols](https://docs.microsoft.com/azure/active-directory/develop/active-directory-v2-protocols)
  - [The OAuth 2.0 protocol in Azure AD](https://docs.microsoft.com/azure/active-directory/develop/v2-oauth2-auth-code-flow)
  - [The OpenID Connect protocol](https://docs.microsoft.com/azure/active-directory/develop/v2-protocols-oidc)
  - [ID tokens](https://docs.microsoft.com/azure/active-directory/develop/id-tokens)
  - [Azure Active Directory access tokens](https://docs.microsoft.com/azure/active-directory/develop/access-tokens)

- To lean more about the application registration, visit:
  - [Quickstart: Register an application with the Microsoft identity platform (Preview)](https://docs.microsoft.com/azure/active-directory/develop/quickstart-register-app)
  - [Quickstart: Configure a client application to access web APIs (Preview)](https://docs.microsoft.com/azure/active-directory/develop/quickstart-configure-app-access-web-apis)
  - [Quickstart: Configure an application to expose web APIs](https://docs.microsoft.com/azure/active-directory/develop/quickstart-configure-app-expose-web-apis)

- To learn more about the code, visit:
  - [Conceptual documentation for MSAL.NET](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki#conceptual-documentation) and in particular:
  - [Acquiring tokens with authorization codes on web apps](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/Acquiring-tokens-with-authorization-codes-on-web-apps)
  - [Customizing Token cache serialization](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/token-cache-serialization)

- To learn more about security in aspnetcore:
  - [Introduction to Identity on ASP.NET Core](https://docs.microsoft.com/aspnet/core/security/authentication/identity)
  - [Role-based authorization in ASP.NET Core](https://docs.microsoft.com/aspnet/core/security/authorization/roles)
  - [AuthenticationBuilder](https://docs.microsoft.com/dotnet/api/microsoft.aspnetcore.authentication.authenticationbuilder)
  - [Azure Active Directory with ASP.NET Core](https://docs.microsoft.com/aspnet/core/security/authentication/azure-active-directory/)

## Contributing

If you'd like to contribute to this sample, see [CONTRIBUTING.MD](/CONTRIBUTING.md).

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information, see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
