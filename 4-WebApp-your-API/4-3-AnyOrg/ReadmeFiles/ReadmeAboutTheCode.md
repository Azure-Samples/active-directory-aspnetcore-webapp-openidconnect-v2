
## About the code

<details>
 <summary>Expand the section</summary>
  ### Provisioning your Multi-tenant Apps in another Azure AD Tenant programmatically

Often the user-based consent will be disabled in an Azure AD tenant or your application will be requesting permissions that requires a tenant-admin consent. In these scenarios, your application will need to utilize the `/adminconsent` endpoint to provision both the **ToDoListClient** and the **ToDoListService** before the users from that tenant are able to sign-in to your app.

When provisioning, you have to take care of the dependency in the topology where the **ToDoListClient** is dependent on **ToDoListService**. So in such a case, you would provision the **ToDoListService** before the **ToDoListClient**.

### Code for the Web App (TodoListClient)

In `Startup.cs`, below lines of code enables Microsoft identity platform endpoint. This endpoint is capable of signing-in users both with their Work and School Accounts.

```csharp
services.AddMicrosoftWebAppAuthentication(Configuration)
    .AddMicrosoftWebAppCallsWebApi(Configuration, new string[] { Configuration["TodoList:TodoListServiceScope"] })
   .AddInMemoryTokenCaches();
```

 1. `AddMicrosoftWebAppAuthentication` : This enables your application to use the Microsoft identity platform endpoint. This endpoint is capable of signing-in users both with their Work and School and Microsoft Personal accounts.
 1. `AddMicrosoftWebAppCallsWebApi` : Enables the web app to call the protected API ToDoList Api.
 1. `AddInMemoryTokenCaches`: Adds an in memory token cache provider, which will cache the Access Tokens acquired for the Web API.

The following code enables to add client service to use the HttpClient by dependency injection.

```CSharp
services.AddTodoListService(Configuration);
```

### Admin Consent Endpoint

In `HomeController.cs`, the method `AdminConsentApi` has the code to redirect the user to the admin consent endpoint for the admin to consent for the **Web API**. The state parameter in the URI contains a link for `AdminConsentClient` method.

```csharp
public IActionResult AdminConsentApi()
{
    string adminConsent1 = "https://login.microsoftonline.com/organizations/v2.0/adminconsent?client_id="+ _ApiClientId 
        + "&redirect_uri=" + _ApiRedirectUri
        + "&state=" + _RedirectUri + "Home/AdminConsentClient" + "&scope=" + _ApiScope;

    return Redirect(adminConsent1);
}
```

The method `AdminConsentClient` has the code to redirect the user to the admin consent endpoint for the admin to consent for the **Web App**.

```csharp
public IActionResult AdminConsentClient()
{
    string adminConsent2 = "https://login.microsoftonline.com/organizations/v2.0/adminconsent?client_id=" + _ClientId
        + "&redirect_uri=" + _RedirectUri
        + "&state=123&scope=" + _TodoListServiceScope;

    return Redirect(adminConsent2);
}
```

### Handle the **MsalUiRequiredException** from Web API

If signed-in user does not have consent for a permission on the Web API, for instance "user.read.all" in this sample, then Web API will throw `MsalUiRequiredException`. The response contains the details about consent Uri and proposed action.

The Web App contains a method `HandleChallengeFromWebApi` in `ToDoListService.cs` that handles the exception thrown by API. It creates a consent URI and throws a custom exception i.e., `WebApiMsalUiRequiredException`.

```csharp
private void HandleChallengeFromWebApi(HttpResponseMessage response)
{
    //proposedAction="consent"
    List<string> result = new List<string>();
    AuthenticationHeaderValue bearer = response.Headers.WwwAuthenticate.First(v => v.Scheme == "Bearer");
    IEnumerable<string> parameters = bearer.Parameter.Split(',').Select(v => v.Trim()).ToList();
    string proposedAction = GetParameter(parameters, "proposedAction");

    if (proposedAction == "consent")
    {
        string consentUri = GetParameter(parameters, "consentUri");

        var uri = new Uri(consentUri);

        var queryString = System.Web.HttpUtility.ParseQueryString(uri.Query);
        queryString.Set("redirect_uri", _ApiRedirectUri);
        queryString.Add("prompt", "consent");
        queryString.Add("state", _RedirectUri);

        var uriBuilder = new UriBuilder(uri);
        uriBuilder.Query = queryString.ToString();
        var updateConsentUri = uriBuilder.Uri.ToString();
        result.Add("consentUri");
        result.Add(updateConsentUri);

        throw new WebApiMsalUiRequiredException(updateConsentUri);
    }
}
```

The following code in `ToDoListController.cs` catches the `WebApiMsalUiRequiredException` exception thrown by `HandleChallengeFromWebApi` method as explained above. Further it Redirects to `consentUri` that is retrieved from exception message. Admin needs to consent as `user.read.all` permission requires admin approval.

```csharp
public async Task<IActionResult> Create()
{
    ToDoItem todo = new ToDoItem();
    try
    {
        ...
    }
    catch (WebApiMsalUiRequiredException ex)
    {
        return Redirect(ex.Message);
    }
}
```

### Code for the Web API (ToDoListService)

#### Admin consent Client Redirect

In HomeController.cs, the method `AdminConsent` redirects to the URI passed in the state parameter by Web App. If admin consent is cancelled from API consent screen then it redirects to base address of Web App.

```csharp
public IActionResult AdminConsent()
{
    var decodeUrl = System.Web.HttpUtility.UrlDecode(HttpContext.Request.QueryString.ToString());
    var queryString = System.Web.HttpUtility.ParseQueryString(decodeUrl);
    var clientRedirect = queryString["state"];
    if (!string.IsNullOrEmpty(clientRedirect))
    {
        if (queryString["error"] == "access_denied" && queryString["error_subcode"] == "cancel")
        {
            var clientRedirectUri = new Uri(clientRedirect);
            return Redirect(clientRedirectUri.GetLeftPart(System.UriPartial.Authority));
        }
        else
        {
            return Redirect(clientRedirect);
        }
    }
    else
    {
        return RedirectToAction("GetTodoItems", "TodoList");
    }
}
```

#### Choosing which scopes to expose

This sample exposes a delegated permission (access_as_user) that will be presented in the access token claim. The method `AddMicrosoftWebApi` does not validate the scope, but Microsoft.Identity.Web has a HttpContext extension method, `VerifyUserHasAnyAcceptedScope`, where you can validate the scope as below:

```csharp
HttpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
```

 For delegated permissions how to access scopes

If a token has delegated permission scopes, they will be in the `scp` or `http://schemas.microsoft.com/identity/claims/scope` claim.

#### Custom Token Validation Allowing only Registered Tenants

By marking your application as multi-tenant, your application will be able to sign-in users from any Azure AD tenant out there. Now you would want to restrict the tenants you want to work with. For this, we will now extend token validation to only those Azure AD tenants registered in the application database. Below, the event handler `OnTokenValidated` was configured to grab the `tenantId` from the token claims and check if it has an entry on the records. If it doesn't, an exception is thrown, canceling the authentication.

Another way to control who is allowed into API is to use Policies. This is configured as part of services.AddAuthorization call. See the code below.

```csharp
//get list of allowed tenants from configuration
  var allowedTenants = Configuration.GetSection("AzureAd:AllowedTenants").Get<string[]>();

  //configure OnTokenValidated event to filter the tenants
  //you can use either this approach or the one below through policies
  services.Configure<JwtBearerOptions>(
      JwtBearerDefaults.AuthenticationScheme, options =>
      {
          var existingOnTokenValidatedHandler = options.Events.OnTokenValidated;
          options.Events.OnTokenValidated = async context =>
          {
              await existingOnTokenValidatedHandler(context);
              if (!allowedTenants.Contains(context.Principal.GetTenantId()))
              {
                  throw new UnauthorizedAccessException("This tenant is not authorized");
              }
          };
      });


  // Creating policies that wraps the authorization requirements
  services.AddAuthorization(

      //uncomment this part if you need to filter the tenants by a policy
      //refer to https://github.com/AzureAD/microsoft-identity-web/wiki/authorization-policies#filtering-tenants

      //builder =>
      //{
      //    string policyName = "User belongs to a specific tenant";
      //    builder.AddPolicy(policyName, b =>
      //    {
      //        b.RequireClaim(ClaimConstants.TenantId, allowedTenants);
      //    });
      //    builder.DefaultPolicy = builder.GetPolicy(policyName);
      //}

  );
```

#### Controlling access to API actions with scopes

During startup of Web API Application, four permissions were created:

- 2 for user scopes: **ToDoList.Read** and **ToDoList.ReadWrite**.
- 2 for app permissions: **ToDoList.Read.All** and **ToDoList.ReadWrite.All**
  It's important to note that because current sample is a multi-tenant sample, app permissions won't take effect, but are left here as an example for a single tenant samples

For enhanced and secure access we can decide what scope can access what operation. For example Read and Write scopes and permissions are required for GET:

```csharp
    // GET: api/TodoItems
    [HttpGet]
    [RequiredScopeOrAppPermission(
        AcceptedScope = new string[] { _todoListReadScope, _todoListReadWriteScope },
        AcceptedAppPermission = new string[] { _todoListReadAllPermission, _todoListReadWriteAllPermission }
        )]
    public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodoItems()
    {
         try
            {
                // this is a request for all ToDo list items of a certain user.
                if (!IsAppOnlyToken())
                {
                    return await _context.TodoItems.Where(x => x.TenantId == _userTenantId && (x.AssignedTo == _signedInUser || x.Assignedby == _signedInUser)).ToArrayAsync();
                }

                // Its an app calling with app permissions, so return all items across all users
                return await _context.TodoItems.Where(x => x.TenantId == _userTenantId).ToArrayAsync();
            }
            catch (Exception)
            {
                throw;
            }
    }
```

**Write** scopes and permissions will let user access POST:

```csharp
    [HttpPost]
        [RequiredScopeOrAppPermission(
            AcceptedScope = new string[] { _todoListReadWriteScope },
            AcceptedAppPermission = new string[] { _todoListReadWriteAllPermission })]
        public async Task<ActionResult<TodoItem>> CreateTodoItem(TodoItem todoItem)
        {
            var random = new Random();
            todoItem.Id = random.Next();


            _context.TodoItems.Add(todoItem);
            await _context.SaveChangesAsync();

            return Ok(todoItem);
        }
```

 </details>
