## About the code

<details>
 <summary>Expand the section</summary>

1. In the `TodoListService` project,  which represents the web api, first the package `Microsoft.Identity.Web`is added from NuGet.

1. Starting with the **Startup.cs** file :

    * at the top of the file, the following using directory was added:

      ```CSharp
      using Microsoft.Identity.Web;
      ```

    * in the `ConfigureServices` method, the following code was added, replacing any existing `AddAuthentication()` code:

      ```CSharp
      services.AddMicrosoftIdentityWebApiAuthentication(Configuration);
      ```

    * `AddMicrosoftIdentityWebApiAuthentication()` protects the Web API by [validating Access tokens](https://docs.microsoft.com/azure/active-directory/develop/access-tokens#validating-tokens) sent tho this API. Check out [Protected web API: Code configuration](https://docs.microsoft.com/azure/active-directory/develop/scenario-protected-web-api-app-configuration) which explains the inner workings of this method in more detail.

    * There is a bit of code (commented) provided under this method that can be used to used do **extended token validation** and do checks based on additional claims, such as:
      * check if the client app's `appid (azp)` is in some sort of an allowed  list via the 'azp' claim, in case you wanted to restrict the API to a list of client apps.
      * check if the caller's account is homed or guest via the `acct` optional claim
      * check if the caller belongs to right roles or groups via the `roles` or `groups` claim, respectively

    See [How to manually validate a JWT access token using the Microsoft identity platform](https://aka.ms/extendtokenvalidation) for more details on to further verify the caller using this method.

1. Then in the controllers `TodoListController.cs`, the `[Authorize]` added on top of the class to protect this route.
    * Further in the controller, the [RequiredScopeOrAppPermission](https://github.com/AzureAD/microsoft-identity-web/wiki/web-apis#checking-for-scopes-or-app-permissions=) is used to list the ([Delegated permissions](https://docs.microsoft.com/azure/active-directory/develop/v2-permissions-and-consent)), that the user should consent for, before the method can be called.  
    * The delegated permissions are checked inside `TodoListService\Controllers\ToDoListController.cs` in the following manner:

      ```CSharp
      [HttpGet]
      [RequiredScopeOrAppPermission(
        AcceptedScope = new string[] { "ToDoList.Read", "ToDoList.ReadWrite" },
        AcceptedAppPermission = new string[] { "ToDoList.Read.All", "ToDoList.ReadWrite.All" }
        )]
      public IEnumerable<Todo> Get()
      {
            if (!IsAppOnlyToken())
          {
              // this is a request for all ToDo list items of a certain user.
              return TodoStore.Values.Where(x => x.Owner == _currentLoggedUser);
          }
          else
          {
              // Its an app calling with app permissions, so return all items across all users
              return TodoStore.Values;
          }
      }
      ```

      The code above demonstrates that to be able to reach a GET REST operation, the access token should contain AT LEAST ONE of the scopes (delegated permissions) listed inside parameter of [RequiredScopeOrAppPermission](https://github.com/AzureAD/microsoft-identity-web/wiki/web-apis#checking-for-scopes-or-app-permissions=) attribute
      Please note that while in this sample, the client app only works with *Delegated Permissions*,  the API's controller is designed to work with both *Delegated* and *Application* permissions.

      The **ToDoList.<*>.All** permissions are **Application Permissions**.

      Here is another example from the same controller:

      ``` CSharp
      [HttpDelete("{id}")]
      [RequiredScopeOrAppPermission(
          AcceptedScope = new string[] { "ToDoList.ReadWrite" },
          AcceptedAppPermission = new string[] { "ToDoList.ReadWrite.All" })]
      public void Delete(int id)
      {
            if (!IsAppOnlyToken())
            {
                // only delete if the ToDo list item belonged to this user
                if (TodoStore.Values.Any(x => x.Id == id && x.Owner == _currentLoggedUser))
                {
                    TodoStore.Remove(id);
                }
            }
            else
            {
                TodoStore.Remove(id);
            }
      }
      ```

      The above code demonstrates that to be able to execute the DELETE REST operation, the access token MUST contain the `ToDoList.ReadWrite` scope. Note that the called is not allowed to access this operation with just `ToDoList.Read` scope only.
      Also note of how we distinguish the **what** a user can delete. When there is a **ToDoList.ReadWrite.All** permission available, the user can delete **ANY** entity from the database,
      but with **ToDoList.ReadWrite**, the user can delete only their own entries.

    * The method *IsAppOnlyToken()* is used by controller method to detect presence of an app only token, i.e a token that was issued to an app using the [Client credentials](https://docs.microsoft.com/azure/active-directory/develop/v2-oauth2-client-creds-grant-flow) flow, i.e no users were signed-in by this client app. 

      ```csharp
        private bool IsAppOnlyToken()
        {
            // Add in the optional 'idtyp' claim to check if the access token is coming from an application or user.
            //
            // See: https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-optional-claims

            if (GetCurrentClaimsPrincipal() != null)
            {
                return GetCurrentClaimsPrincipal().Claims.Any(c => c.Type == "idtyp" && c.Value == "app");
            }

            return false;
        }
      ```

1. In the `TodoListClient` project,  which represents the client app that signs-in a user and makes calls to the web api, first the package `Microsoft.Identity.Web`is added from NuGet.

* The following lines in *Startup.cs* adds the ability to authenticate a user using Microsoft Entra ID.

```csharp
        services.AddMicrosoftIdentityWebAppAuthentication(Configuration)
                .EnableTokenAcquisitionToCallDownstreamApi(
                    Configuration.GetSection("TodoList:TodoListScopes").Get<string>().Split(" ", System.StringSplitOptions.RemoveEmptyEntries)
                    )
                .AddInMemoryTokenCaches();
```

* Specifying Initial scopes (delegated permissions)

The ToDoListClient's *appsettings.json* file contains `ToDoListScopes` key that is used in *startup.cs* to specify which initial scopes (delegated permissions) should be requested for the Access Token when a user is being signed-in:

```csharp
    services.AddMicrosoftIdentityWebAppAuthentication(Configuration)
    .EnableTokenAcquisitionToCallDownstreamApi(Configuration.GetSection("TodoList:TodoListScopes")
    .Get<string>().Split(" ", System.StringSplitOptions.RemoveEmptyEntries))
    .AddInMemoryTokenCaches();
```

* Detecting *Guest* users of a tenant signing-in. This section of code in *Startup.cs* shows you how to detect if the user signing-in is a *member* or *guest*. 
  
  ```CSharp
  app.Use(async (context, next) => {
                if (context.User != null && context.User.Identity.IsAuthenticated)
                {
                    // you can conduct any conditional processing for guest/homes user by inspecting the value of the 'acct' claim
                    // Read more about the 'acct' claim at aka.ms/optionalclaims
                    if (context.User.Claims.Any(x => x.Type == "acct"))
                    {
                        string claimvalue = context.User.Claims.FirstOrDefault(x => x.Type == "acct").Value;
                        string userType = claimvalue == "0" ? "Member" : "Guest";
                        Debug.WriteLine($"The type of the user account from this Microsoft Entra tenant is-{userType}");
                    }
                }
                await next();
            });
  ```

1. There is some commented code in *Startup.cs* that also shows how to user certificates and KeyVault in place, see [README-use-certificate](README-use-certificate.md) for more details on how to use code in this section.
1. Also consider adding [MSAL.NET Logging](https://docs.microsoft.com/azure/active-directory/develop/msal-logging-dotnet) to you project

</details>
