## About The code

<details>
 <summary>Expand the section</summary>

This sample details the following aspects of a multi-tenant app.

- usage of the `/common` endpoint.
- Service principal provisioning of an app in Azure AD tenants
- Custom Token Validation to allow users from onboarded tenants only.
- Data partitioning in multi-tenant apps.
- Acquiring Access tokens for Microsoft Graph for each tenant.

This sample is using the OpenID Connect ASP.NET Core middleware to sign in users from multiple Azure AD tenants. The middleware is initialized in the `Startup.cs` file by passing it the Client ID of the app, and the URL of the Azure AD tenant where the app is registered. These values are read from the `appsettings.json` file.

You can trigger the middleware to send an OpenID Connect sign-in request by decorating a class or method with the `[Authorize]` attribute or by issuing a challenge (see the [AccountController.cs](https://github.com/aspnet/AspNetCore/blob/master/src/Azure/AzureAD/Authentication.AzureAD.UI/src/Areas/AzureAD/Controllers/AccountController.cs) file which is part of ASP.NET Core):

These steps are encapsulated in the [Microsoft.Identity.Web](https://github.com/AzureAD/microsoft-identity-web) project, and in particular in the [WebAppServiceCollectionExtensions.cs](https://github.com/AzureAD/microsoft-identity-web/blob/master/src/Microsoft.Identity.Web/WebAppExtensions/WebAppServiceCollectionExtensions.cs) file

### Usage of `/common` endpoint

In order to be able to sign-in users from multiple tenants, the [/common endpoint](https://docs.microsoft.com/azure/active-directory/develop/howto-convert-app-to-be-multi-tenant#update-your-code-to-send-requests-to-common) must be used. In the sample, this endpoint is used as a result of setting the value for `TenantId` as `organizations` on the `appsettings.json` file, and configuring the middleware to read the values from it.

```csharp
services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApp(Configuration)
```

 You can read about the various endpoints of the Microsoft Identity Platform [here](https://docs.microsoft.com/azure/active-directory/develop/active-directory-v2-protocols#endpoints).

#### Implications of signing-in guest users on `/common` endpoint

Please note that if you sign-in guest users at the `/common` endpoint, they will be directed to their home tenant for signing-in. So, if your multi-tenant app cares about applying tenant specific conditional access policies, group assignments or app roles to be applied to the guest users, the app should sign-in the guest user on the **tenanted endpoint** (https://login.microsoftonline.com/{tenantId}) instead of the `/common` endpoint.

### Service principal provisioning for new tenants (onboarding process)

For a multi-tenant app to work across tenants, its service principal will need to be provisioned in the users' tenant. It can either happen when the first user signs in, or most tenant admins only allow a tenant admin to carry out the service principal provisioning. For provisioning, we will be using the [admin consent endpoint](https://docs.microsoft.com/azure/active-directory/develop/v2-admin-consent) for the onboarding process. The code for this is provided in the `OnboardingController.cs`. The `Onboard` action and corresponding view, simulate the onboarding flow and experience.

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult Onboard()
{
  ...
  string authorizationRequest = string.Format(
                "{0}organizations/v2.0/adminconsent?client_id={1}&redirect_uri={2}&state={3}&scope={4}",
                azureADOptions.Instance,
                Uri.EscapeDataString(azureADOptions.ClientId),
                Uri.EscapeDataString(currentUri + "Onboarding/ProcessCode"),
                Uri.EscapeDataString(stateMarker),
                Uri.EscapeDataString("https://graph.microsoft.com/.default"));
  return Redirect(authorizationRequest);
}
```

This results in an OAuth2 code grant request that triggers the admin consent flow and creates the service principal in the admin's tenant. The `state` parameter is used to validate the response, preventing a man-in-the-middle attack. Then, the `ProcessCode` action receives the authorization code from Azure AD and, if they appear valid, we create an entry in the application database for the new customer.

The `https://graph.microsoft.com/.default` is a static scope that allows the tenant admin to consent for all permissions in one go. You can find more about static scope on [this link.](https://docs.microsoft.com/azure/active-directory/develop/v2-admin-consent#request-the-permissions-from-a-directory-admin)

### Custom token validation allowing only registered tenants

On the `Startup.cs` we are calling `AddMicrosoftWebAppAuthentication` to configure the authentication, and within that method, we validates that the token issuer is from AAD.

```csharp
options.TokenValidationParameters.IssuerValidator = AadIssuerValidator.GetIssuerValidator(options.Authority).Validate;
```

To extend this validation to only Azure AD tenants registered in the application database, the event handler `OnTokenValidated` was configured to grab the `tenantId` from the token claims and check if it has an entry on the database. If it doesn't, a custom exception `UnauthorizedTenantException` is thrown, canceling the authentication, and the user is redirected to the `UnauthorizedTenant` view. At this stage, the user is not authenticated in the application.

```csharp
services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    options.Events.OnTokenValidated = async context => 
    {
        string tenantId = context.SecurityToken.Claims.FirstOrDefault(x => x.Type == "tid" || x.Type == "http://schemas.microsoft.com/identity/claims/tenantid")?.Value;

        if (string.IsNullOrWhiteSpace(tenantId))
            throw new UnauthorizedAccessException("Unable to get tenantId from token.");

        var dbContext = context.HttpContext.RequestServices.GetRequiredService<SampleDbContext>();

        var authorizedTenant = await dbContext.AuthorizedTenants.FirstOrDefaultAsync(t => t.TenantId == tenantId);

        if (authorizedTenant == null)
            throw new UnauthorizedTenantException("This tenant is not authorized");

    };
    options.Events.OnAuthenticationFailed = (context) =>
    {
        if (context.Exception != null && context.Exception is UnauthorizedTenantException)
        {
            context.Response.Redirect("/Home/UnauthorizedTenant");
            context.HandleResponse(); // Suppress the exception
        }

        return Task.FromResult(0);
    };
});
```

### Partitioning data by tenant

There are two common scenarios regarding data partition on a multi-tenant app. Having a separate database for each tenant or having a single database and using the **tenantId** to separate the data of each tenant. In this sample, we have taken the single database approach to save the ToDo items for all users from all tenants.

If you want to read more about data architecture on multi-tenant apps, please refer to [Multi-tenant SaaS database tenancy patterns](https://docs.microsoft.com/azure/sql-database/saas-tenancy-app-design-patterns)

`TodoListController.cs` has the basic CRUD actions for `ToDoItem` and each operation takes into account the signed user's **tenantId** to separate data from each tenant. The tenantId can be found in the user' claims.

### Acquiring Access token for Microsoft Graph for each tenant

If a multi-tenant app needs to acquire an access token for Microsoft Graph to be able to read data from the signed user's tenant, the token must be issued from their tenanted authority and not from the tenant where the SaaS application is registered. This feature is being showed on the **Edit** action result on `TodoListController.cs`.

```csharp
var userTenant = User.GetTenantId();
// Acquiring token for graph using the user's tenant, so it can return all the users from their tenant
var graphAccessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(new string[] { GraphScope.UserReadAll }, userTenant);
```

We are acquiring an access token for Graph with the scope `User.Read.All`, to list all the users from the tenant so you can assign a todo item to them. `GetAccessTokenForUserAsync` is a helper method found on `Microsoft.Identity.Web` project, and it receives a **tenantId** as parameter to acquire a token for the desired authority. For that, we get the current authority from the built `IConfidentialClientApplication` and replace the tenantId. Below is an example of this logic.

```csharp
string signedUserAuthority = confidentialClientApplication.Authority.Replace(new Uri(confidentialClientApplication.Authority).PathAndQuery, $"/{tenant}/");
AuthenticationResult result = await confidentialClientApplication
    .AcquireTokenSilent(new string[] { "User.Read.All" }, account)
    .WithAuthority(signedUserAuthority)
    .ExecuteAsync()
    .ConfigureAwait(false);
```

</details>
