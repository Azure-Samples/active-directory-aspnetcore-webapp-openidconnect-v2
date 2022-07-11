## About the code

<details>
 <summary>Expand the section</summary>

1. Consider adding [MSAL.NET Logging](https://docs.microsoft.com/azure/active-directory/develop/msal-logging-dotnet) to you project

1. In the `TodoListService` project, first the package `Microsoft.Identity.Web`is added from NuGet.

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

    * Then in the controllers `TodoListController.cs`, the `[Authorize]` added on top of the class to protect this route.
    * Further in the controller, the `RequiredScope` is used to list the scopes ([Delegated permissions](https://docs.microsoft.com/azure/active-directory/develop/v2-permissions-and-consent)), that the user should consent for, before the method can be called.  
    * The delegated permissions are checked inside `TodoListService\Controllers\ToDoListController.cs`, for example in the following way:

      ```CSharp
      [HttpGet]
      [RequiredScopeOrAppPermission(
        AcceptedScope = new string[] { "ToDoList.Read", "ToDoList.ReadWrite" },
        AcceptedAppPermission = new string[] { "ToDoList.Read.All", "ToDoList.ReadWrite.All" }
        )]
      public IEnumerable<Todo> Get()
      {
        if (HasDelegatedPermissions(new string[] { "ToDoList.Read", "ToDoList.ReadWrite" }))
        {
            return TodoStore.Values.Where(x => x.Owner == GetObjectIdClaim(User));
        }
        else if (HasApplicationPermissions(new string[] { "ToDoList.Read.All", "ToDoList.ReadWrite.All" }))
        {
            return TodoStore.Values;
        }

        return null;
      }
      ```

      The code above demonstrates that to be able to reach a GET REST operation, the access token should contain AT LEAST ONE of the scopes listed inside parameter of [RequiredScopeOrAppPermission attribute](https://github.com/AzureAD/microsoft-identity-web/blob/master/src/Microsoft.Identity.Web/Policy/RequiredScopeOrAppPermissionAttribute.cs)
      Please note that in this specific sample we use only delegated permissions, but also added an app permissions as an additional option for a developer consideration.
      As well, pay attention that **ToDoList.*.All** permissions will list **ALL** entries.

      Here is another example from the same controller:

      ``` CSharp
      [HttpDelete("{id}")]
      [RequiredScopeOrAppPermission(
          AcceptedScope = new string[] { "ToDoList.ReadWrite" },
          AcceptedAppPermission = new string[] { "ToDoList.ReadWrite.All" })]
      public void Delete(int id)
      {
          if (
              (HasDelegatedPermissions(new string[] { "ToDoList.ReadWrite" }) && TodoStore.Values.Any(x => x.Id == id && x.Owner == GetObjectIdClaim(User)))

                ||

              HasApplicationPermissions(new string[] { "ToDoList.ReadWrite.All" }))
          {
              TodoStore.Remove(id);
          }
      }
      ```

      The above code demonstrates that to be able to execute the DELETE REST operation, the access token MUST contain the `ToDoList.ReadWrite` scope. Note that the called is not allowed to access this operation with just `ToDoList.Read` scope only.
      Also note of how we distinguish the **what** a user can delete. When there is a **ToDoList.ReadWrite.All** permission available, the user can delete **ANY** entity from the database,
      but with **ToDoList.ReadWrite**, the user can delete only their own entries.

### Initial scopes

Client [appsettings.json](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/blob/master/4-WebApp-your-API/4-1-MyOrg/Client/appsettings.json) file contains `ToDoListScopes` key that is used in [startup.cs](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/blob/2607df1338a9f7c06fe228c87644b8b456ca708b/4-WebApp-your-API/4-1-MyOrg/Client/Startup.cs#L46) to specify which initial scopes should be requested from Web API when refreshing the token:

```csharp
services.AddMicrosoftIdentityWebAppAuthentication(Configuration)
 .EnableTokenAcquisitionToCallDownstreamApi(Configuration.GetSection("TodoList:TodoListScopes")
 .Get<string>().Split(" ", System.StringSplitOptions.RemoveEmptyEntries))
 .AddInMemoryTokenCaches();
```

</details>