## How the code was created

<details>
 <summary>Expand the section</summary>

### Creating the Web API project (TodoListService)

The code for the TodoListService was created in the following way:

#### Step 1: Create the web api using the ASP.NET Core templates

```Text
md TodoListService
cd TodoListService
dotnet new webapi -au=SingleOrg
```

1. Open the generated project (.csproj) in Visual Studio, and save the solution.

#### Add a model (TodoListItem) and modify the controller

In the TodoListService project, add a folder named `Models` and then create a new  file named `TodoItem.cs`. Copy the contents of the TodoListService\Models\TodoItem.cs in this file.

### Modify the Program.cs file to validate bearer access tokens received by the Web API

Update `Program.cs` file :

 *replace the following code:

  ```CSharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
   ```

  with

  ```Csharp
    services.AddMicrosoftIdentityWebApiAuthentication(Configuration);
  ```

### Create the TodoListController and associated Models

1. Add a reference to the ToDoListService.
1. Create a new Controller named `TodoListController` and copy and paste the code from the sample (TodoListService\Controllers\TodoListController.cs) to this controller.
1. Open the `appsettings.json` file and copy the keys from the sample's corresponding file under the `AzureAd` and `TodoList` sections.

### Creating the client web app (TodoListClient)

#### Step 1: the sample from the command line

1. Run the following command to create a sample from the command line using the `SingleOrg` template:

    ```Sh
    md TodoListClient
    cd TodoListClient
    dotnet new mvc --auth SingleOrg --client-id <Enter_the_Application_Id_here> --tenant-id <yourTenantId>
    ```

    > Note: Replace *`Enter_the_Application_Id_here`* with the *Application Id* from the application Id you just registered in the Application Registration Portal and *`<yourTenantId>`* with the *Directory (tenant) ID* where you created your application.

#### Step 2: Modify the generated code

1. Open the generated project (.csproj) in Visual Studio, and save the solution.
1. Add the `Microsoft.Identity.Web` via Nuget.
1. Open the **Program.cs** file and:

   * Replace the two following lines:

```CSharp
services.AddAuthentication(AzureADDefaults.AuthenticationScheme)
    .AddAzureAD(options => Configuration.Bind("AzureAd", options));
```

with these lines:

```CSharp
services.AddMicrosoftIdentityWebAppAuthentication(Configuration)
    .EnableTokenAcquisitionToCallDownstreamApi(
        Configuration.GetSection("TodoList:TodoListScopes").Get<string>().Split(" ", System.StringSplitOptions.RemoveEmptyEntries)
        )
    .AddInMemoryTokenCaches();
```

* This enables your application to use the Microsoft identity platform endpoint. This endpoint is capable of signing-in users both with their Work and School and Microsoft Personal accounts.

1. Change the `Properties\launchSettings.json` file to ensure that you start your web app from <https://localhost:44321> as registered. For this:
     * update the `sslPort` of the `iisSettings` section to be `44321`
     * in the `applicationUrl` property of use `https://localhost:44321`
     * Then add the following code to inject the ToDoList service implementation in the client

   ```CSharp
     // Add APIs
     services.AddTodoListService(Configuration);
   ```

2. Open the `appsettings.json` file and copy the keys from the sample's corresponding file under the `AzureAd` and `TodoList` sections.

#### Add a model (TodoListItem) and add the controller and views

1. In the TodoListClient project, add a folder named `Models` and then create a new  file named `TodoItem.cs`. Copy the contents of the TodoListClient\Models\TodoItem.cs in this file.
1. Create a new Controller named `TodoListController` and copy and paste the code from the sample (TodoListService\Controllers\TodoListController.cs) to this controller.
1. Copy the files `TodoListService` and `TodoListService.cs` in the **TodoListClient\Services** folder provided in this sample to your project.
1. Copy the contents of **TodoListClient\views\ToDo** folder to the views folder of your project.
1. Modify the `Views\Shared\_Layout.cshtml` to add a link to the **ToDolist** controller. Check the `Views\Shared\_Layout.cshtml` in the sample for reference.
1. Add a section name **TodoList** in the appsettings.json file and add the keys `TodoListScope`, `TodoListBaseAddress`.
1. Update the `configureServices` method in `startup.cs` to add the MSAL library and a token cache.

    ```CSharp
            services.AddMicrosoftIdentityWebAppAuthentication(Configuration)
                    .EnableTokenAcquisitionToCallDownstreamApi(
                        Configuration.GetSection("TodoList:TodoListScopes").Get<string>().Split(" ", System.StringSplitOptions.RemoveEmptyEntries)
                     )
                    .AddInMemoryTokenCaches();

            // Add APIs
            services.AddTodoListService(Configuration);
    ```

</details>
