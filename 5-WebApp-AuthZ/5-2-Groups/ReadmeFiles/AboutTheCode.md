## About the code

Much of the specifics of implementing **RBAC** with **Security Groups** is the same with implementing **RBAC** with **App Roles** discussed in the [previous tutorial](../5-1-Roles/README.md). In order to avoid redundancy, here we discuss particular issues, such as **groups overage**, that might arise with using the **groups** claim.

#### .NET Core app configuration and how to handle the overage scenario

1. In [Startup.cs](./Startup.cs), `OnTokenValidated` event calls **ProcessAnyGroupsOverage** method defined in [GraphHelper.cs](./Services/GraphHelper.cs) to process groups overage claim.

```csharp
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(options =>
        {
            // code omitted for brevity...
            options.Events.OnTokenValidated = async context =>
            {
                // code omitted for brevity...
                if (context != null)
                {
                    // Calls method to process groups overage claim (before policy checks kick-in)
                    await GraphHelper.ProcessAnyGroupsOverage(context, requiredGroupsIds);
                }
                await Task.CompletedTask;
            };
        })
    .EnableTokenAcquisitionToCallDownstreamApi(options => Configuration.Bind("AzureAd", options))
    .AddMicrosoftGraph(Configuration.GetSection("GraphAPI"))
    .AddInMemoryTokenCaches();
```

`AddMicrosoftGraph` registers the service for `GraphServiceClient`. The values for `BaseUrl` and `Scopes` defined in `GraphAPI` section of the **appsettings.json**.

1. In [GraphHelper.cs](./Services/GraphHelper.cs), **ProcessAnyGroupsOverage** method checks if incoming token contains the *Group Overage* claim. If so, it will call **ProcessUserGroupsForOverage** method to retrieve groups, which in turn calls the Microsoft Graph `/checkMemberGroups` endpoint.

```csharp
public static async Task ProcessAnyGroupsOverage(TokenValidatedContext context)
{
    // Checks if the incoming token contains a groups overage claim.
    if (HasOverageOccurred(context.Principal))
    {
        await ProcessUserGroupsForOverage(context, requiredGroupsIds);
    }
}
```

1. `UserProfileController.cs`
    1. Checks authorization of signed-in user for ```[Authorize(Policy = AuthorizationPolicies.AssignmentToGroupAdminGroupRequired)]```. If authorized successfully then obtain information from the [/me](https://docs.microsoft.com/graph/api/user-get?view=graph-rest-1.0) and [/me/photo](https://docs.microsoft.com/graph/api/profilephoto-get) endpoints by using `GraphServiceClient`.

1. `UserProfile\Index.cshtml`
    1. Has some client code that prints the signed-in user's information.
Much of the specifics of implementing **RBAC** with **Security Groups** is the same with implementing **RBAC** with **App Roles** discussed in the [previous tutorial](../5-2-Roles/README.md). In order to avoid redundancy, here we discuss particular issues, such as **groups overage**, that might arise with using the **groups** claim.

1. AdminController.cs
    1. Checks authorization of signed-in user for ```[Authorize(Policy = AuthorizationPolicies.AssignmentToGroupAdminGroupRequired)]```. If authorized successfully a simple place holder page is displayed.

2. Admin\Index.cshtml
    1. A simple place holder to show how you can store hidden content only available to members of the **GroupAdmin** group

#### Caching user group memberships in overage scenario

Since overaged tokens will not contain group membership IDs, yet these IDs are required for controlling access to pages and/or resources, applications have to call Microsoft Graph whenever a user action (e.g. accessing a page on the UI, accessing a todolist item in the web API etc.) takes place. These network calls are costly and will impact the application performance and user experience. As such the project benefits from caching the group membership IDs once they are fetched from Microsoft Graph for the first time. By default, these are cached for **1 hour** in the sample. Cached groups will miss any changes to a users group membership for this duration. If you need more fine grained control, you can configure cache duration in [appsettings.json](./API/TodoListAPI/appsettings.json). If your scenario requires capturing real-time changes to a user's group membership, consider implementing [Microsoft Graph change notifications](https://learn.microsoft.com/graph/api/resources/webhooks) instead.

##### Group authorization policy

The ASP.NET middleware supports roles populated from claims by specifying the claim in the `RoleClaimType` property of `TokenValidationParameters`. Since the `groups` claim contains the object IDs of the security groups than the actual names by default, you'd use the group IDs instead of group names. See [Role-based authorization in ASP.NET Core](https://docs.microsoft.com/aspnet/core/security/authorization/roles) for more info. See [Startup.cs](./Startup.cs) for more.

```csharp
// The following lines code instruct the asp.net core middleware to use the data in the "groups" claim in the [Authorize] attribute and for User.IsInrole()
// See https://docs.microsoft.com/aspnet/core/security/authorization/roles
services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    // Use the groups claim for populating roles
    options.TokenValidationParameters.RoleClaimType = "groups";
});
// Adding authorization policies that enforce authorization using Azure AD roles.
services.AddAuthorization(options =>
{
    options.AddPolicy(AuthorizationPolicies.AssignmentToGroupMemberGroupRequired, policy => policy.RequireRole(Configuration["Groups:GroupMember"], Configuration["Groups:GroupAdmin"]));
    options.AddPolicy(AuthorizationPolicies.AssignmentToGroupAdminGroupRequired, policy => policy.RequireRole(Configuration["Groups:GroupAdmin"]));
});
```

These policies can be used in controllers as shown below:

```csharp
[Authorize(Policy = AuthorizationPolicies.AssignmentToGroupMemberGroupRequired)]
[AuthorizeForScopes(Scopes = new[] { Constants.ScopeUserRead })]        
public async Task<IActionResult> Index()
{
    try
    {
        User me = await _graphServiceClient.Me.GetAsync();
        ViewData["Me"] = me;

        var photo = await _graphServiceClient.Me.Photo.GetAsync();
        ViewData["Photo"] = photo;
    }
    // See 'Optional - Handle Continuous Access Evaluation (CAE) challenge from Microsoft Graph' for more information.
    catch (ServiceException svcex) when (svcex.Message.Contains("Continuous access evaluation resulted in claims challenge"))
    {
        // Left blank for brevity.
    }

    return View();
}
```

### The Groups Overage Claim

To ensure that the token size doesn’t exceed HTTP header size limits, the Microsoft Identity Platform limits the number of object Ids that it includes in the **groups** claim.

If a user is member of more groups than the overage limit (**150 for SAML tokens, 200 for JWT tokens, 6 for single-page applications using implicit flow**), then the Microsoft Identity Platform does not emit the group IDs in the `groups` claim in the token. Instead, it includes an **overage** claim in the token that indicates to the application to query the [MS Graph API](https://graph.microsoft.com) to retrieve the user’s group membership.

#### Create the Overage Scenario for testing

1. You can use the [BulkCreateGroups.ps1](./AppCreationScripts/BulkCreateGroups.ps1) provided in the [App Creation Scripts](./AppCreationScripts/) folder to create a large number of groups and assign users to them. This will help test overage scenarios during development. You'll need to enter a user's object ID when prompted by the `BulkCreateGroups.ps1` script. If you would like to delete these groups after your testing, run the [BulkRemoveGroups.ps1](./AppCreationScripts/BulkRemoveGroups.ps1).

> When attending to overage scenarios, which requires a call to [Microsoft Graph](https://graph.microsoft.com) to read the signed-in user's group memberships, your app will need to have the [User.Read](https://docs.microsoft.com/graph/permissions-reference#user-permissions) and [GroupMember.Read.All](https://docs.microsoft.com/graph/permissions-reference#group-permissions) for the [getMemberGroups](https://docs.microsoft.com/graph/api/user-getmembergroups) function to execute successfully.
> :warning: For the overage scenario, make sure you have granted **Admin Consent** for the MS Graph API's **GroupMember.Read.All** scope for both the Client and the Service apps (see the **App Registration** steps above).

##### Detecting group overage in your code by examining claims

1. When you run this sample and an overage occurred, then you'd see the `_claim_names` in the home page after the user signs-in.

1. In case you cannot avoid running into group overage, we suggest you use the following logic to process groups claim in your token.  
    1. Check for the claim `_claim_names` with one of the values being `groups`. This indicates overage.
    1. If found, make a call to the endpoint specified in `_claim_sources` to fetch user’s groups.
    1. If none found, look into the `groups` claim for user’s groups.

> You can gain a good familiarity of programming for Microsoft Graph by going through the [An introduction to Microsoft Graph for developers](https://www.youtube.com/watch?v=EBbnpFdB92A) recorded session.
