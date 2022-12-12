
## About the code

<details>
 <summary>Expand the section</summary>

 ### Support in ASP.NET Core middleware libraries

The ASP.NET middleware supports roles populated from claims by specifying the claim in the `RoleClaimType` property of `TokenValidationParameters`.

```CSharp

// Startup.cs
public void ConfigureServices(IServiceCollection services)
{
            // [removed for brevity]

            // This is required to be instantiated before the OpenIdConnectOptions starts getting configured.
            // By default, the claims mapping will map claim names in the old format to accommodate older SAML applications.
            // 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role' instead of 'roles'
            // This flag ensures that the ClaimsIdentity claims collection will be built from the claims in the token
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

            // The following lines code instruct the asp.net core middleware to use the data in the "groups" claim in the Authorize attribute and User.IsInrole()
            // See https://docs.microsoft.com/aspnet/core/security/authorization/roles?view=aspnetcore-2.2 for more info.
            services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
            {
                // Use the groups claim for populating roles
                options.TokenValidationParameters.RoleClaimType = "roles";
            });

            // Adding authorization policies that enforce authorization using Azure AD roles.
            services.AddAuthorization(options => 
            {
                options.AddPolicy(AuthorizationPolicies.AssignmentToUserReaderRoleRequired, policy => policy.RequireRole(AppRole.UserReaders));
                options.AddPolicy(AuthorizationPolicies.AssignmentToDirectoryViewerRoleRequired, policy => policy.RequireRole(AppRole.DirectoryViewers));
            });

            // [removed for brevity]
}

// In code..(Controllers & elsewhere)
[Authorize(Policy = AuthorizationPolicies.AssignmentToDirectoryViewerRoleRequired)]
// or
User.IsInRole("UserReaders"); // In methods
```

The class *GraphHelper.cs* is where the logic to initialize the MS Graph SDK along with logic to enable this app for [Continuous Access Evaluation](http://aka.ms/clientcaet)


 </details>