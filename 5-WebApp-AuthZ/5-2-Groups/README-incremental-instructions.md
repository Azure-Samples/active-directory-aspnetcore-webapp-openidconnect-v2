---
page_type: sample
languages:
  - csharp
products:
		 
  - azure-active-directory  
  - dotnet  
  - aspnet-core
  - ms-graph
name: Add authorization using groups & group claims to an ASP.NET Core Web app that signs-in users with the Microsoft identity platform
description: "This sample demonstrates a ASP.NET Core Web App application calling The Microsoft Graph"
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

- [Visual Studio](https://visualstudio.microsoft.com/downloads/)
- An Azure Active Directory (Azure AD) tenant. For more information on how to get an Azure AD tenant, see [How to get an Azure AD tenant](https://azure.microsoft.com/documentation/articles/active-directory-howto-tenant/)
- A user account in your Azure AD tenant. This sample will not work with a **personal Microsoft account**. Therefore, if you signed in to the [Azure portal](https://portal.azure.com) with a personal account and have never created a user account in your directory before, you need to do that now.

 > Please make sure to have one or more user accounts in the tenant assigned to a few security groups in your tenant. Please follow the instructions in [Create a basic group and add members using Azure Active Directory](https://docs.microsoft.com/azure/active-directory/fundamentals/active-directory-groups-create-azure-portal) to create a few groups and assign users to them if not already done.

### Step 1: In the downloaded folder

From your shell or command line:

Navigate to the `"5-WebApp-AuthZ"` folder

 ```Sh
  cd 5-WebApp-AuthZ\5-2-Groups
  ```

### Step 2: Configure your application to receive the **groups** claim

In the sample, a dashboard component allows signed-in users to see the tasks assigned to them or other users based on their memberships to one of the two security groups, **GroupAdmin** and **GroupMember**. Please use the instructions provided at [Create a basic group and add members using Azure AD](https://docs.microsoft.com/azure/active-directory/fundamentals/active-directory-groups-create-azure-portal) to create these security groups, if not available already. 

You have two different options available to you on how you can further configure your application to receive the `groups` claim.

1. [Receive **all the groups** that the signed-in user is assigned to in an Azure AD tenant, included nested groups](#configure-your-application-to-receive-all-the-groups-the-signed-in-user-is-assigned-to-included-nested-groups).
1. [Receive the **groups** claim values from a **filtered set of groups** that your application is programmed to work with](#configure-your-application-to-receive-the-groups-claim-values-from-a-filtered-set-of-groups-a-user-may-be-assigned-to) (Not available in the [Azure AD Free edition](https://azure.microsoft.com/pricing/details/active-directory/)).

> To get the on-premise group's `samAccountName` or `On Premises Group Security Identifier` instead of Group ID, please refer to the document [Configure group claims for applications with Azure Active Directory](https://docs.microsoft.com/azure/active-directory/hybrid/how-to-connect-fed-group-claims#prerequisites-for-using-group-attributes-synchronized-from-active-directory).

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
1. If you are exposing a Web API using the **Expose an API** option, then you can also choose the `Group ID` option under the **Access** section. This will result in Azure AD sending the [Object ID](https://docs.microsoft.com/graph/api/resources/group?view=graph-rest-1.0) of the groups the user is assigned to in the `groups` claim of the [Access Token](https://docs.microsoft.com/azure/active-directory/develop/access-tokens) issued to the client applications of your API.
1. In the app's registration screen, click on the **Overview** blade in the left to open the Application overview screen. Select the hyperlink with the name of your application in **Managed application in local directory** (note this field title can be truncated for instance `Managed application in ...`). When you select this link you will navigate to the **Enterprise Application Overview** page associated with the service principal for your application in the tenant where you created it. You can navigate back to the app registration page by using the *back* button of your browser.
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

### Step 3: Run the sample

#### Run the sample using Visual Studio

> For Visual Studio Users
>
> Clean the solution, rebuild the solution, and run it.

#### Run the sample using a command line interface such as VS Code integrated terminal

##### Step 1. Install .NET Core dependencies

```console
   cd WebApp-GroupClaims
   dotnet restore
```

##### Step 2. Trust development certificates

```console
   dotnet dev-certs https --clean
   dotnet dev-certs https --trust
```

Learn more about [HTTPS in .NET Core](https://docs.microsoft.com/aspnet/core/security/enforcing-ssl).

##### Step 3. Run the applications

In the console window execute the below command:

```console
    dotnet run
```

1. Open your web browser and make a request to the app. The app immediately attempts to authenticate you to the Microsoft identity platform. You can sign-in with a *work or school account* from the tenant where you created this app. If admin consent to `GroupMember.Read.All` permission from portal is not done then sign-in with admin for the first time and consent for the permission.
1. On the home page, the app lists the various claims it obtained from your ID token. You'd notice one more claims named `groups`.
1. On the top menu, click on the signed-in user's name **user@domain.com**, you should now see all kind of information about yourself including their picture.

> Did the sample not work for you as expected? Did you encounter issues trying this sample? Then please reach out to us using the [GitHub Issues](../../../../issues) page.

> [Consider taking a moment to share your experience with us.](https://forms.office.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbRz0h_jLR5HNJlvkZAewyoWxUNEFCQ0FSMFlPQTJURkJZMTRZWVJRNkdRMC4u)

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

#### Create the overage scenario in this sample for testing

1. You can use the `BulkCreateGroups.ps1` provided in the [App Creation Scripts](./AppCreationScripts/) folder to create a large number of groups and assign users to them. This will help test overage scenarios during development. Remember to change the user's objectId provided in the `BulkCreateGroups.ps1` script.
1. When you run this sample and an overage occurred, then you'd see the  `_claim_names` in the home page after the user signs-in.
1. We strongly advise you use the [group filtering feature](#configure-your-application-to-receive-the-groups-claim-values-from-a-filtered-set-of-groups-a-user-may-be-assigned-to) (if possible) to avoid running into group overages.
1. In case you cannot avoid running into group overage, we suggest you use the following logic to process groups claim in your token.  
    1. Check for the claim `_claim_names` with one of the values being `groups`. This indicates overage.
    1. If found, make a call to the endpoint specified in `_claim_sources` to fetch user’s groups.
    1. If none found, look into the `groups`  claim for user’s groups.

> When attending to overage scenarios, which requires a call to [Microsoft Graph](https://graph.microsoft.com) to read the signed-in user's group memberships, your app will need to have the [GroupMember.Read.All](https://docs.microsoft.com/graph/permissions-reference#group-permissions) for the [getMemberObjects](https://docs.microsoft.com/graph/api/user-getmemberobjects?view=graph-rest-1.0) function to execute successfully.

> Developers who wish to gain good familiarity of programming for Microsoft Graph are advised to go through the [An introduction to Microsoft Graph for developers](https://www.youtube.com/watch?v=EBbnpFdB92A) recorded session.

#### When you are a single page application and using the implicit grant flow to authenticate

In case, you are authenticating using the [implicit grant flow](https://docs.microsoft.com/azure/active-directory/develop/v1-oauth2-implicit-grant-flow), the **overage** indication and limits are different than the apps using other flows.

1. A claim named `hasgroups` with a value of true will be present in the token instead of the `groups` claim .
1. The maximum number of groups provided in the `groups` claim is limited to 6. This is done to prevent  the URI fragment beyond the URL length limits.

## About the code

### Create the sample from the command line

> The following code used an older version of [Microsoft.Identity.Web](https://github.com/AzureAD/microsoft-identity-web) library and would be updated when the library becomes Generally Available.

This project was created using the following command.


## About the code

The following files have the code that would be of interest to you:

1. HomeController.cs
    1. Passes the **HttpContext.User** (the signed-in user) to the view.
    1. Calls method **GetSessionGroupList** of `GraphHelper.cs` to get groups from session and if groups are returned then pass them to the view.

       ```csharp
        public IActionResult Index()
        {
            ViewData["User"] = HttpContext.User;
            var groups = GraphHelper.GetUserGroupsFromSession(HttpContext.Session);
            if (groups?.Count > 0)
            {
                ViewData.Add("groupClaims", groups );
            }
            return View();
        }
       ```

1. Home\Index.cshtml
    1. This has some code to print the current user's claims.

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

    - have been replaced by these lines:

     ```CSharp
      services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
              .AddMicrosoftIdentityWebApp(
          options =>
          {
              Configuration.Bind("AzureAd", options);
              options.Events = new OpenIdConnectEvents();
              options.Events.OnTokenValidated = async context =>
              {
                  var overageGroupClaims = await GraphHelper.GetSignedInUsersGroups(context);
              };
          }, options => { Configuration.Bind("AzureAd", options); })
              .EnableTokenAcquisitionToCallDownstreamApi(options => Configuration.Bind("AzureAd", options), initialScopes)
              .AddMicrosoftGraph(Configuration.GetSection("GraphAPI"))
              .AddInMemoryTokenCaches();
      ```

    `OnTokenValidated` event calls **GetSignedInUsersGroups** method, that is defined in GraphHelper.cs, to process groups overage claim.
  
    `AddMicrosoftGraph` registers the service for `GraphServiceClient`. The values for BaseUrl and Scopes defined in `GraphAPI` section of **appsettings.json**.

    Following lines of code adds authorization policies that enforce authorization using group values.

    ```csharp
          services.AddAuthorization(options =>
          {
            options.AddPolicy("GroupAdmin",
            policy => policy.Requirements.Add(new GroupPolicyRequirement(Configuration["Groups:GroupAdmin"])));
                options.AddPolicy("GroupMember",
              policy => policy.Requirements.Add(new GroupPolicyRequirement(Configuration["Groups:GroupMember"])));
          });
    ```
  
1. In GraphHelper.cs, **GetSignedInUsersGroups** method checks if incoming token contains *Group Overage* claim then returns the list of groups from Microsoft Graph. First **GetUserGroupsFromSession** method is called to get group values from session if exists. If session does not contain groups claim then it will call **ProcessUserGroupsForOverage** method to retrieve groups.
  
      ```csharp
    public static async Task<List<string>> GetSignedInUsersGroups(TokenValidatedContext context)
          {
              List<string> groupClaims = new List<string>();
              if (HasOverageOccurred(context.Principal))
              {
                  // 
                  groupClaims = GetUserGroupsFromSession(context.HttpContext.Session);
                  if (groupClaims?.Count > 0)
                  {
                      return groupClaims;
                  }
                  else
                  {
                      groupClaims = await ProcessUserGroupsForOverage(context);
                  }
              }
              return groupClaims;
          }
      ```

    GraphHelper.cs contains a method **CheckUsersGroupMembership** that is called in `CustomAuthorization.cs` to check if value of GroupName parameter exists in either Session for Overage scenario or in User claims otherwise.

      ```csharp
        public static bool CheckUsersGroupMembership(AuthorizationHandlerContext context, string GroupName, IHttpContextAccessor _httpContextAccessor)
        {
            bool result = false;
            if (HasOverageOccurred(context.User))
            {
                var groups = GetUserGroupsFromSession(_httpContextAccessor.HttpContext.Session);
                if (groups?.Count > 0 && groups.Contains(GroupName))
                {
                    result = true;
                }
            }
            else if (context.User.Claims.Any(x => x.Type == "groups" && x.Value == GroupName))
            {
                result = true;
            }
            return result;
        }
      ```

1. In `CustomAuthorization.cs`, we have **GroupPolicyHandler** class that deals with custom Policy-based authorization. It evaluates the GroupPolicyRequirement against AuthorizationHandlerContext by overriding **HandleRequirementAsync** of **AuthorizationHandler**.

    HandleRequirementAsync calls **CheckUsersGroupMembership** method of `GraphHelper.cs` to determine if authorization is allowed.

    ```csharp
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                   GroupPolicyRequirement requirement)
        {
            if (GraphHelper.CheckUsersGroupMembership(context, requirement.GroupName, _httpContextAccessor))
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    ```

1. UserProfileController.cs
    1. Checks authorization of signed-in user for ```[Authorize(Policy = "GroupAdmin")]```. If authorized successfully then obtain information from the [/me](https://docs.microsoft.com/graph/api/user-get?view=graph-rest-1.0) and [/me/photo](https://docs.microsoft.com/graph/api/profilephoto-get) endpoints by using `GraphServiceClient`.

1. UserProfile\Index.cshtml
    1. Has some client code that prints the signed-in user's information.

1. if you used the PowerShell scripts provided in the [AppCreationScripts](.\AppCreationScripts) folder, then note the extra parameter `-GroupMembershipClaims` in the  `Configure.ps1` script.

     ```PowerShell
       -GroupMembershipClaims "SecurityGroup"
       -PublicClient $False
     ```

## Community Help and Support

Use [Stack Overflow](http://stackoverflow.com/questions/tagged/msal) to get support from the community.
Ask your questions on Stack Overflow first and browse existing issues to see if someone has asked your question before.
Make sure that your questions or comments are tagged with [ `msal` `azure-active-directory` `dotnet`].

If you find a bug in the sample, please raise the issue on [GitHub Issues](../../../../issues).

To provide a recommendation, visit the following [User Voice page](https://feedback.azure.com/forums/169401-azure-active-directory).

## Next steps

- Learn how to use app roles. [Add authorization using app roles & roles claims to a Web app that signs-in users with the Microsoft identity platform](../../5-WebApp-AuthZ/5-1-Roles/README.md).

## Learn more

- Learn how [Microsoft.Identity.Web](https://aka.ms/idweblib) works, in particular hooks-up to the ASP.NET Core ODIC events

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
