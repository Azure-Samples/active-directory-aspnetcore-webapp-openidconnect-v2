---
page_type: sample
name: How to secure an ASP.NET Core Web API with the Microsoft identity platform
description: This sample demonstrates an ASP.NET Core Web App signing-in a user and calling an ASP.NET Core Web API that is secured with Microsoft Entra ID.
languages:
 - csharp
products:
 - aspnet-core
 - microsoft-entra-id
urlFragment: active-directory-aspnetcore-webapp-openidconnect-v2
extensions:
- services: ms-identity
- platform: AspNetCore
- endpoint: Microsoft Entra ID v2.0
- level: 200
- client: ASP.NET Core Web App
- service: ASP.NET Core Web API
---

# How to secure an ASP.NET Core Web API with the Microsoft identity platform

[![Build status](https://identitydivision.visualstudio.com/IDDP/_apis/build/status/aad%20Samples/.NET%20client%20samples/ASP.NET%20Core%20Web%20App%20tutorial)](https://identitydivision.visualstudio.com/IDDP/_build/latest?definitionId=819)

* [Overview](#overview)
* [Scenario](#scenario)
* [Prerequisites](#prerequisites)
* [Setup the sample](#setup-the-sample)
* [Explore the sample](#explore-the-sample)
* [Troubleshooting](#troubleshooting)
* [About the code](#about-the-code)
* [How the code was created](#how-the-code-was-created)
* [How to deploy this sample to Azure](#how-to-deploy-this-sample-to-azure)
* [Next Steps](#next-steps)
* [Contributing](#contributing)
* [Learn More](#learn-more)

## Overview

This sample demonstrates a ASP.NET Core Web App calling a ASP.NET Core Web API that is secured using Microsoft Entra ID.

## Scenario

This sample demonstrates an ASP.NET Core client Web App calling an ASP.NET Core Web API that is secured using Microsoft Entra ID.

1. The client ASP.NET Core Web App uses the [Microsoft.Identity.Web](https://aka.ms/microsoft-identity-web) to sign-in a user and obtain a JWT [ID Token](https://aka.ms/id-tokens) and an [Access Token](https://aka.ms/access-tokens) from **Microsoft Entra ID**.
1. The **access token** is used as a *bearer* token to authorize the user to call the ASP.NET Core Web API protected by **Microsoft Entra ID**.
1. The service uses the [Microsoft.Identity.Web](https://aka.ms/microsoft-identity-web) to protect the Web api, check permissions and validate tokens.

![Scenario Image](./ReadmeFiles/topology.png)

## Prerequisites

* Either [Visual Studio](https://visualstudio.microsoft.com/downloads/) or [Visual Studio Code](https://code.visualstudio.com/download) and [.NET Core SDK](https://www.microsoft.com/net/learn/get-started)
* An **Microsoft Entra ID** tenant. For more information, see: [How to get a Microsoft Entra tenant](https://docs.microsoft.com/azure/active-directory/develop/test-setup-environment#get-a-test-tenant)
* A user account in your **Microsoft Entra ID** tenant. This sample will not work with a **personal Microsoft account**. If you're signed in to the [Microsoft Entra admin center](https://entra.microsoft.com) with a personal Microsoft account and have not created a user account in your directory before, you will need to create one before proceeding.

## Setup the sample

### Step 1: Clone or download this repository

From your shell or command line:

```console
git clone https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2.git
```

or download and extract the repository *.zip* file.

> :warning: To avoid path length limitations on Windows, we recommend cloning into a directory near the root of your drive.

### Step 2: Navigate to project folder

```console
cd 4-WebApp-Your-API\4-1-MyOrg
```

### Step 3: Register the sample application(s) in your tenant

There are two projects in this sample. Each needs to be separately registered in your Microsoft Entra tenant. To register these projects, you can:

- follow the steps below for manually register your apps
- or use PowerShell scripts that:
  - **automatically** creates the Microsoft Entra applications and related objects (passwords, permissions, dependencies) for you.
  - modify the projects' configuration files.

  <details>
   <summary>Expand this section if you want to use this automation:</summary>

    > :warning: If you have never used **Microsoft Graph PowerShell** before, we recommend you go through the [App Creation Scripts Guide](./AppCreationScripts/AppCreationScripts.md) once to ensure that your environment is prepared correctly for this step.
  
    1. On Windows, run PowerShell as **Administrator** and navigate to the root of the cloned directory
    1. In PowerShell run:

       ```PowerShell
       Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process -Force
       ```

    1. Run the script to create your Microsoft Entra application and configure the code of the sample application accordingly.
    1. For interactive process -in PowerShell, run:

       ```PowerShell
       cd .\AppCreationScripts\
       .\Configure.ps1 -TenantId "[Optional] - your tenant id" -AzureEnvironmentName "[Optional] - Azure environment, defaults to 'Global'"
       ```

    > Other ways of running the scripts are described in [App Creation Scripts guide](./AppCreationScripts/AppCreationScripts.md). The scripts also provide a guide to automated application registration, configuration and removal which can help in your CI/CD scenarios.

  </details>

#### Choose the Microsoft Entra tenant where you want to create your applications

To manually register the apps, as a first step you'll need to:

1. Sign in to the [Microsoft Entra admin center](https://entra.microsoft.com).
1. If your account is present in more than one Microsoft Entra tenant, select your profile at the top right corner in the menu on top of the page, and then **switch directory** to change your portal session to the desired Microsoft Entra tenant.

#### Register the service app (TodoListService-aspnetcore-webapi)

1. Navigate to the [Microsoft Entra admin center](https://entra.microsoft.com) and select the **Microsoft Entra ID** service.
1. Select the **App Registrations** blade on the left, then select **New registration**.
1. In the **Register an application page** that appears, enter your application's registration information:
    1. In the **Name** section, enter a meaningful application name that will be displayed to users of the app, for example `TodoListService-aspnetcore-webapi`.
    1. Under **Supported account types**, select **Accounts in this organizational directory only**
    1. Select **Register** to create the application.
1. In the **Overview** blade, find and note the **Application (client) ID**. You use this value in your app's configuration file(s) later in your code.
1. In the app's registration screen, select the **Expose an API** blade to the left to open the page where you can publish the permission as an API for which client applications can obtain [access tokens](https://aka.ms/access-tokens) for. The first thing that we need to do is to declare the unique [resource](https://docs.microsoft.com/azure/active-directory/develop/v2-oauth2-auth-code-flow) URI that the clients will be using to obtain access tokens for this API. To declare an resource URI(Application ID URI), follow the following steps:
    1. Select **Set** next to the **Application ID URI** to generate a URI that is unique for this app.
    1. For this sample, accept the proposed Application ID URI (`api://{clientId}`) by selecting **Save**. Read more about Application ID URI at [Validation differences by supported account types \(signInAudience\)](https://docs.microsoft.com/azure/active-directory/develop/supported-accounts-validation).

##### Publish Delegated Permissions

1. All APIs must publish a minimum of one [scope](https://docs.microsoft.com/azure/active-directory/develop/v2-oauth2-auth-code-flow#request-an-authorization-code), also called [Delegated Permission](https://docs.microsoft.com/azure/active-directory/develop/v2-permissions-and-consent#permission-types), for the client apps to obtain an access token for a *user* successfully. To publish a scope, follow these steps:
1. Select **Add a scope** button open the **Add a scope** screen and Enter the values as indicated below:
    1. For **Scope name**, use `ToDoList.Read`.
    1. Select **Admins and users** options for **Who can consent?**.
    1. For **Admin consent display name** type in *Read users ToDo list using the 'TodoListService-aspnetcore-webapi'*.
    1. For **Admin consent description** type in *Allow the app to read the user's ToDo list using the 'TodoListService-aspnetcore-webapi'*.
    1. For **User consent display name** type in *Read your ToDo list items via the 'TodoListService-aspnetcore-webapi'*.
    1. For **User consent description** type in *Allow the app to read your ToDo list items via the 'TodoListService-aspnetcore-webapi'*.
    1. Keep **State** as **Enabled**.
    1. Select the **Add scope** button on the bottom to save this scope.
    > Repeat the steps above for another scope named **ToDoList.ReadWrite**
1. Select the **Manifest** blade on the left.
    1. Set `accessTokenAcceptedVersion` property to **2**.
    1. Select on **Save**.

> :information_source:  Follow [the principle of least privilege when publishing permissions](https://learn.microsoft.com/security/zero-trust/develop/protected-api-example) for a web API.

##### Publish Application Permissions

1. All APIs should publish a minimum of one [App role for applications](https://docs.microsoft.com/azure/active-directory/develop/howto-add-app-roles-in-azure-ad-apps#assign-app-roles-to-applications), also called [Application Permission](https://docs.microsoft.com/azure/active-directory/develop/v2-permissions-and-consent#permission-types), for the client apps to obtain an access token as *themselves*, i.e. when they are not signing-in a user. **Application permissions** are the type of permissions that APIs should publish when they want to enable client applications to successfully authenticate as themselves and not need to sign-in users. To publish an application permission, follow these steps:
1. Still on the same app registration, select the **App roles** blade to the left.
1. Select **Create app role**:
    1. For **Display name**, enter a suitable name for your application permission, for instance **ToDoList.Read.All**.
    1. For **Allowed member types**, choose **Application** to ensure other applications can be granted this permission.
    1. For **Value**, enter **ToDoList.Read.All**.
    1. For **Description**, enter *Allow the app to read every user's ToDo list using the 'TodoListService-aspnetcore-webapi'*.
    1. Select **Apply** to save your changes.

    > Repeat the steps above for another app permission named **ToDoList.ReadWrite.All**

##### Configure Optional Claims

1. Still on the same app registration, select the **Token configuration** blade to the left.
1. Select **Add optional claim**:
    1. Select **optional claim type**, then choose **Access**.
     1. Select the optional claim **idtyp**.
    > Indicates token type. This claim is the most accurate way for an API to determine if a token is an app token or an app+user token. This is not issued in tokens issued to users.
    1. Select **Add** to save your changes.

##### Configure the service app (TodoListService-aspnetcore-webapi) to use your app registration

Open the project in your IDE (like Visual Studio or Visual Studio Code) to configure the code.

> In the steps below, "ClientID" is the same as "Application ID" or "AppId".

1. Open the `TodoListService\appsettings.json` file.
1. Find the key `Domain` and replace the existing value with your Microsoft Entra tenant domain, ex. `contoso.onmicrosoft.com`.
1. Find the key `TenantId` and replace the existing value with your Microsoft Entra tenant/directory ID.
1. Find the key `ClientId` and replace the existing value with the application ID (clientId) of `TodoListService-aspnetcore-webapi` app copied from the Microsoft Entra admin center.

#### Register the client app (TodoListClient-aspnetcore-webapi)

1. Navigate to the [Microsoft Entra admin center](https://entra.microsoft.com) and select the **Microsoft Entra ID** service.
1. Select the **App Registrations** blade on the left, then select **New registration**.
1. In the **Register an application page** that appears, enter your application's registration information:
    1. In the **Name** section, enter a meaningful application name that will be displayed to users of the app, for example `TodoListClient-aspnetcore-webapi`.
    1. Under **Supported account types**, select **Accounts in this organizational directory only**
    1. Select **Register** to create the application.
1. In the **Overview** blade, find and note the **Application (client) ID**. You use this value in your app's configuration file(s) later in your code.
1. In the app's registration screen, select the **Authentication** blade to the left.
1. If you don't have a platform added, select **Add a platform** and select the **Web** option.
    1. In the **Redirect URI** section enter the following redirect URI:
        1. `https://localhost:44321/signin-oidc`
    1. In the **Front-channel logout URL** section, set it to `https://localhost:44321/signout-oidc`.
    1. Click **Save** to save your changes.
1. In the app's registration screen, select the **Certificates & secrets** blade in the left to open the page where you can generate secrets and upload certificates.
1. In the **Client secrets** section, select **New client secret**:
    1. Type a key description (for instance `app secret`).
    1. Select one of the available key durations (**6 months**, **12 months** or **Custom**) as per your security posture.
    1. The generated key value will be displayed when you select the **Add** button. Copy and save the generated value for use in later steps.
    1. You'll need this key later in your code's configuration files. This key value will not be displayed again, and is not retrievable by any other means, so make sure to note it from the Microsoft Entra admin center before navigating to any other screen or blade.
    > :bulb: For enhanced security, instead of using client secrets, consider [using certificates](./README-use-certificate.md) and [Azure KeyVault](https://azure.microsoft.com/services/key-vault/#product-overview).
    1. Since this app signs-in users, we will now proceed to select **delegated permissions**, which is is required by apps signing-in users.
   1. In the app's registration screen, select the **API permissions** blade in the left to open the page where we add access to the APIs that your application needs:
   1. Select the **Add a permission** button and then:
   1. Ensure that the **My APIs** tab is selected.
   1. In the list of APIs, select the API `TodoListService-aspnetcore-webapi`.
      * Since this app signs-in users, we will now proceed to select **delegated permissions**, which is requested by apps that signs-in users.
      * In the **Delegated permissions** section, select **ToDoList.Read**, **ToDoList.ReadWrite** in the list. Use the search box if necessary.
   1. Select the **Add permissions** button at the bottom.

##### Configure Optional Claims

1. Still on the same app registration, select the **Token configuration** blade to the left.
1. Select **Add optional claim**:
    1. Select **optional claim type**, then choose **ID**.
    1. Select the optional claim **acct**.
    > Provides user's account status in tenant. If the user is a **member** of the tenant, the value is *0*. If they're a **guest**, the value is *1*.
    1. Select **Add** to save your changes.

##### Configure the client app (TodoListClient-aspnetcore-webapi) to use your app registration

Open the project in your IDE (like Visual Studio or Visual Studio Code) to configure the code.

> In the steps below, "ClientID" is the same as "Application ID" or "AppId".

1. Open the `Client\appsettings.json` file.
1. Find the key `Domain` and replace the existing value with your Microsoft Entra tenant domain, ex. `contoso.onmicrosoft.com`.
1. Find the key `TenantId` and replace the existing value with your Microsoft Entra tenant/directory ID.
1. Find the key `ClientId` and replace the existing value with the application ID (clientId) of `TodoListClient-aspnetcore-webapi` app copied from the Microsoft Entra admin center.
1. Find the key `ClientSecret` and replace the existing value with the generated secret that you saved during the creation of `TodoListClient-aspnetcore-webapi` copied from the Microsoft Entra admin center.
1. Find the key `TodoListScopes` and replace the existing value with **"api://<your_service_api_client_id>/ToDoList.Read api://<your_service_api_client_id>/ToDoList.ReadWrite"**.
1. Find the key `TodoListBaseAddress` and replace the existing value with the base address of `TodoListService-aspnetcore-webapi` (by default `https://localhost:44351`).

### Variation: web app using client certificates

Follow [README-use-certificate.md](README-use-certificate.md) to know how to use this option.

### Step 4: Running the sample

From your shell or command line, execute the following commands:

```console
    cd 4-WebApp-Your-API\4-1-MyOrg\TodoListService
    dotnet run
```

Then, open a separate command terminal and run: 

```console
    cd 4-WebApp-Your-API\4-1-MyOrg\Client
    dotnet run
```

## Explore the sample

<details>
 <summary>Expand to see how to use the sample</summary>
 Open your web browser and navigate to `https://localhost:44321` and sign-in using the link on top-right. The app immediately attempts to authenticate you via the Microsoft identity platform endpoint. Sign in using an user account in that tenant.

1. Click on `TodoList`, you can click on `Create New` link. It will redirect to create task screen where you can add a new task and assign it to any user from the list.
1. The `TodoList` screen also displays tasks that are assigned to and created by signed-in user. The user can edit and delete the created tasks but can only view the assigned tasks.

> NOTE: Remember, the TodoList is stored in memory in this `TodoListService` app. Each time you run the projects, your TodoList will get emptied.

Did the sample not work for you as expected? Did you encounter issues trying this sample? Then please reach out to us using the [GitHub Issues](../../../../issues) page.

[Consider taking a moment to share your experience with us.](https://forms.office.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbRz0h_jLR5HNJlvkZAewyoWxUNEFCQ0FSMFlPQTJURkJZMTRZWVJRNkdRMC4u)
</details>

## Troubleshooting

<details>
 <summary>Expand for troubleshooting info</summary>

Use [Stack Overflow](http://stackoverflow.com/questions/tagged/msal) to get support from the community.
Ask your questions on Stack Overflow first and browse existing issues to see if someone has asked your question before.
Make sure that your questions or comments are tagged with [`azure-active-directory` `adal` `msal` `dotnet`].

If you find a bug in the sample, please raise the issue on [GitHub Issues](../../issues).

To provide a recommendation, visit the following [User Voice page](https://feedback.azure.com/forums/169401-azure-active-directory).
</details>

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

## How to deploy this sample to Azure

<details>
<summary>Expand the section</summary>

### Deploying web API to Azure App Services

There is one web API in this sample. To deploy it to **Azure App Services**, you'll need to:

* create an **Azure App Service**
* publish the projects to the **App Services**

> :warning: Please make sure that you have not switched on the *[Automatic authentication provided by App Service](https://docs.microsoft.com/en-us/azure/app-service/scenario-secure-app-authentication-app-service)*. It interferes the authentication code used in this code example.


#### Publish your files (TodoListService-aspnetcore-webapi)

##### Publish using Visual Studio

Follow the link to [Publish with Visual Studio](https://docs.microsoft.com/visualstudio/deployment/quickstart-deploy-to-azure).

##### Publish using Visual Studio Code

1. Install the Visual Studio Code extension [Azure App Service](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azureappservice).
1. Follow the link to [Publish with Visual Studio Code](https://docs.microsoft.com/aspnet/core/tutorials/publish-to-azure-webapp-using-vscode)


> :information_source: When calling the web API, your app may receive an error similar to the following:
>
> *Cross-Origin Request Blocked: The Same Origin Policy disallows reading the remote resource at https://some-url-here. (Reason: additional information here).*
> 
> If that's the case, you'll need enable [cross-origin resource sharing (CORS)](https://developer.mozilla.org/docs/Web/HTTP/CORS) for you web API. Follow the steps below to do this:
>
> - Go to [Microsoft Entra admin center](https://entra.microsoft.com), and locate the web API project that you've deployed to App Service.
> - On the API blade, select **CORS**. Check the box **Enable Access-Control-Allow-Credentials**.
> - Under **Allowed origins**, add the URL of your published web app **that will call this web API**. 

### Deploying Web app to Azure App Service

There is one web app in this sample. To deploy it to **Azure App Services**, you'll need to:

- create an **Azure App Service**
- publish the projects to the **App Services**, and
- update its client(s) to call the website instead of the local environment.

#### Publish your files (TodoListClient-aspnetcore-webapi)

##### Publish using Visual Studio

Follow the link to [Publish with Visual Studio](https://docs.microsoft.com/visualstudio/deployment/quickstart-deploy-to-azure).

##### Publish using Visual Studio Code

1. Install the Visual Studio Code extension [Azure App Service](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azureappservice).
1. Follow the link to [Publish with Visual Studio Code](https://docs.microsoft.com/aspnet/core/tutorials/publish-to-azure-webapp-using-vscode)

#### Update the Microsoft Entra app registration (TodoListClient-aspnetcore-webapi)

1. Navigate back to to the [Microsoft Entra admin center](https://entra.microsoft.com).
In the left-hand navigation pane, select the **Microsoft Entra ID** service, and then select **App registrations (Preview)**.
1. In the resulting screen, select the `TodoListClient-aspnetcore-webapi` application.
1. In the app's registration screen, select **Authentication** in the menu.
    1. In the **Redirect URIs** section, update the reply URLs to match the site URL of your Azure deployment. For example:
        1. `https://TodoListClient-aspnetcore-webapi.azurewebsites.net/signin-oidc`
    1. Update the **Front-channel logout URL** fields with the address of your service, for example [https://TodoListClient-aspnetcore-webapi.azurewebsites.net](https://TodoListClient-aspnetcore-webapi.azurewebsites.net)

#### Update authentication configuration parameters (TodoListClient-aspnetcore-webapi)

1. In your IDE, locate the `TodoListClient-aspnetcore-webapi` project. Then, open `Client\appsettings.json`.
2. Find the key for **redirect URI** and replace its value with the address of the web app you published, for example, [https://TodoListClient-aspnetcore-webapi.azurewebsites.net/redirect](https://TodoListClient-aspnetcore-webapi.azurewebsites.net/redirect).
3. Find the key for **web API endpoint** and replace its value with the address of the web API you published, for example, [https://TodoListService-aspnetcore-webapi.azurewebsites.net/api](https://TodoListService-aspnetcore-webapi.azurewebsites.net/api).

> :warning: If your app is using an *in-memory* storage, **Azure App Services** will spin down your web site if it is inactive, and any records that your app was keeping will be empty. In addition, if you increase the instance count of your website, requests will be distributed among the instances. Your app's records, therefore, will not be the same on each instance.

</details>

## Contributing

If you'd like to contribute to this sample, see [CONTRIBUTING.MD](/CONTRIBUTING.md).

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information, see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Learn More

For more information, visit the following links:

 *To lean more about the application registration, visit:
  *[Quickstart: Register an application with the Microsoft identity platform](https://docs.microsoft.com/azure/active-directory/develop/quickstart-register-app)
  *[Quickstart: Configure a client application to access web APIs](https://docs.microsoft.com/azure/active-directory/develop/quickstart-configure-app-access-web-apis)
  *[Quickstart: Configure an application to expose web APIs](https://docs.microsoft.com/azure/active-directory/develop/quickstart-configure-app-expose-web-apis)

  *To learn more about the code, visit:
  *[Conceptual documentation for MSAL.NET](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki#conceptual-documentation) and in particular:
  *[Acquiring tokens with authorization codes on web apps](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/Acquiring-tokens-with-authorization-codes-on-web-apps)
  *[Customizing Token cache serialization](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/token-cache-serialization)

  *To learn more about security in aspnetcore,
  *[Introduction to Identity on ASP.NET Core](https://docs.microsoft.com/aspnet/core/security/authentication/identity)
  *[AuthenticationBuilder](https://docs.microsoft.com/dotnet/api/microsoft.aspnetcore.authentication.authenticationbuilder)
  *[Microsoft Entra ID with ASP.NET Core](https://docs.microsoft.com/aspnet/core/security/authentication/azure-active-directory)
  *[Protected web API: Verify scopes and app roles](https://docs.microsoft.com/azure/active-directory/develop/scenario-protected-web-api-verification-scope-app-roles?tabs=aspnetcore)
