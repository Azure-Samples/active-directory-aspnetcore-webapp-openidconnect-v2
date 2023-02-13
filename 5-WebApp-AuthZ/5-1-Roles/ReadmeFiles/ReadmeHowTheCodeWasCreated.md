
## How the code was created

<details>
 <summary>Expand the section</summary>

 1. Open the generated project (.csproj) in Visual Studio, and save the solution.
1. Add the `Microsoft.Identity.Web.csproj` project which is located at the root of this sample repo, to your solution (**Add Existing Project ...**). It's used to simplify signing-in and, in the next tutorial phases, to get a token
1. Add a reference from your newly generated project to `Microsoft.Identity.Web` (right click on the **Dependencies** node under your new project, and choose **Add Reference ...**, and then in the projects tab find the `Microsoft.Identity.Web` project)
1. Open the **Startup.cs** file and:

   - in the `ConfigureServices` method, the following lines:

     ```CSharp
      services.AddAuthentication(AzureADDefaults.AuthenticationScheme)
              .AddAzureAD(options => Configuration.Bind("AzureAd", options));

     ```

      have been replaced by these lines:

     ```CSharp
     //This enables your application to use the Microsoft identity platform endpoint. This endpoint is capable of signing-in users both with their Work and School and Microsoft Personal accounts.
            services.AddMicrosoftIdentityWebAppAuthentication(Configuration)
                    .EnableTokenAcquisitionToCallDownstreamApi(new string[] { Constants.ScopeUserRead })
                    .AddInMemoryTokenCaches(); // Adds aspnetcore MemoryCache as Token cache provider for MSAL.

        services.AddGraphService(Configuration);    // Adds the IMSGraphService as an available service for this app.
     ```

1. In the `ConfigureServices` method of `Startup.cs', add the following lines:

     ```CSharp
            // This is required to be instantiated before the OpenIdConnectOptions starts getting configured.
            // By default, the claims mapping will map claim names in the old format to accommodate older SAML applications.
            // 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role' instead of 'roles'
            // This flag ensures that the ClaimsIdentity claims collection will be built from the claims in the token
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

            // Adding authorization policies that enforce authorization using Azure AD roles.
            services.AddAuthorization(options => 
            {
                options.AddPolicy(AuthorizationPolicies.AssignmentToUserReaderRoleRequired, policy => policy.RequireRole(AppRole.UserReaders));
                options.AddPolicy(AuthorizationPolicies.AssignmentToDirectoryViewerRoleRequired, policy => policy.RequireRole(AppRole.DirectoryViewers));
            });
     ```

1. In the `ConfigureServices` method of `Startup.cs', the following lines instruct the ASP.NET security middleware to use the **roles** claim to fetch roles for authorization.

     ```CSharp
    // Add this configuration after the call to `AddMicrosoftWebAppAuthentication`.
    services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        // The claim in the JWT token where App roles are available.
        options.TokenValidationParameters.RoleClaimType = "roles";
    });
     ```

1. In the `HomeController.cs`, the following method is added with the `Authorize` attribute with the name of the policy that enforces that the signed-in user is present in the app role **UserReaders**, that permits listing of users in the tenant.

    ```CSharp
        [Authorize(Policy = AuthorizationPolicies.AssignmentToUserReaderRoleRequired)]
        public async Task<IActionResult> Users()
        {
     ```

1. A new class called `AccountController.cs` is introduced. This contains the code to intercept the default AccessDenied error's route and present the user with an option to sign-out and sign-back in with a different account that has access to the required role.

    ```CSharp
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
     ```

1. The following method is also added with the `Authorize` attribute with the name of the policy that enforces that the signed-in user is present in the app role **DirectoryViewers**, that permits listing of roles and groups the signed-in user is assigned to.

    ```CSharp
        [Authorize(Policy = AuthorizationPolicies.AssignmentToDirectoryViewerRoleRequired)]
        public async Task<IActionResult> Groups()
        {
     ```

1. The views, `Users.cshtml` and `Groups.cshtml` have the code to display the users in a tenant and roles and groups the signed-in user is assigned to respectively.

 </details>