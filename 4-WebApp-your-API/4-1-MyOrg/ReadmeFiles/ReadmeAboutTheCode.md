## About the code

<details>
 <summary>Expand the section</summary>

1. In the `TodoListService` project, first the package `Microsoft.Identity.Web`is added from NuGet.

1. Starting with the **Startup.cs** file :

    * at the top of the file, the following two using directives were added:

      ```CSharp
      using Microsoft.Identity.Web;
      ```

    * in the `ConfigureServices` method, the following code was added, replacing any existing `AddAuthentication()` code:

      ```CSharp
      services.AddMicrosoftIdentityWebApiAuthentication(Configuration);
      ```

    * `AddMicrosoftIdentityWebApiAuthentication()` protects the Web API by validating Access tokens sent tho this API. Check out [Protected web API: Code configuration](https://docs.microsoft.com/azure/active-directory/develop/scenario-protected-web-api-app-configuration) which explains the inner workings of this method in more detail.

    * Then in the controllers `TodoListController.cs`, the `[Authorize]` added on top of the class to protect this route.
    * Further in the controller, the `RequiredScope` is used to list the scopes ([Delegated permissions](https://docs.microsoft.com/azure/active-directory/develop/v2-permissions-and-consent)), that the user should consent for, before the method can be called.  
    * The delegated permissions are checked inside `TodoListService\Controllers\ToDoListController.cs` in the following way:

      ```CSharp
      [HttpGet]
      [RequiredScope(new string[] { "ToDoList.Read", "ToDoList.Write" })
    
      public IEnumerable<Todo> Get()
      {
      string owner = User.Identity.Name;
      return TodoStore.Values.Where(x => x.Owner == owner);
      }
      ```

      The code above demonstrates that to be able to reach a GET REST operation, the access token should contain AT LEAST ONE of the scopes listed inside parameter of [RequiredScope attribute](https://github.com/AzureAD/microsoft-identity-web/blob/master/src/Microsoft.Identity.Web/Policy/RequiredScopeAttribute.cs)

      ``` CSharp
      [HttpDelete("{id}")]
      [RequiredScope("ToDoList.Write")]
      public void Delete(int id)
      {
      TodoStore.Remove(id);
      }
      ```

      The above code demonstrates that to be able to execute the DELETE REST operation, the access token MUST contain the `ToDoList.Write` scope. Note that the called is not allowed to access this operation with just `ToDoList.Read` scope only.

### Initial scopes

Client [appsettings.json](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/blob/master/4-WebApp-your-API/4-1-MyOrg/Client/appsettings.json) file contains `ToDoListScopes` key that is used in [startup.cs](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/blob/2607df1338a9f7c06fe228c87644b8b456ca708b/4-WebApp-your-API/4-1-MyOrg/Client/Startup.cs#L46) to specify which initial scopes should be requested from Web API when refreshing the token:

```csharp
services.AddMicrosoftIdentityWebAppAuthentication(Configuration)
 .EnableTokenAcquisitionToCallDownstreamApi(Configuration.GetSection("TodoList:TodoListScopes")
 .Get<string>().Split(" ", System.StringSplitOptions.RemoveEmptyEntries))
 .AddInMemoryTokenCaches();
```

</details>