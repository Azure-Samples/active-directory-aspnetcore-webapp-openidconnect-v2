---
page_type: sample
name: How to secure an ASP.NET Core Web API with the Microsoft identity platform
services: active-directory
platforms: dotnet
urlFragment: active-directory-aspnetcore-webapp-openidconnect-v2
description: This sample demonstrates an ASP.NET Core client Web App calling an ASP.NET Core Web API that is secured using Azure AD.
languages:
 - csharp
products:
 - aspnet-core
 - azure-active-directory
---

# How to secure an ASP.NET Core Web API with the Microsoft identity platform

[![Build status](https://identitydivision.visualstudio.com/IDDP/_apis/build/status/AAD%20Samples/.NET%20client%20samples/ASP.NET%20Core%20Web%20App%20tutorial)](https://identitydivision.visualstudio.com/IDDP/_build/latest?definitionId=819)

Table Of Contents

* [Scenario](#Scenario)
* [Prerequisites](#Prerequisites)
* [Setup the sample](#Setup-the-sample)
* [Troubleshooting](#Troubleshooting)
* [Using the sample](#Using-the-sample)
* [About the code](#About-the-code)
* [How the code was created](#How-the-code-was-created)
* [How to deploy this sample to Azure](#How-to-deploy-this-sample-to-Azure)
* [Next Steps](#Next-Steps)
* [Contributing](#Contributing)
* [Learn More](#Learn-More)

## Scenario

 This sample demonstrates an ASP.NET Core client Web App calling an ASP.NET Core Web API that is secured using Azure AD.

 1. The client ASP.NET Core Web App uses the [Microsoft.Identity.Web](https://aka.ms/microsoft-identity-web) to sign-in and obtain a JWT [Access Tokens](https://docs.microsoft.com/azure/active-directory/develop/access-tokens) from **Azure AD**.
 1. The [Access Tokens](https://docs.microsoft.com/azure/active-directory/develop/access-tokens) is used as a bearer token to authorize the user to call the ASP.NET Core Web API protected by **Azure AD**.

![Scenario Image](./ReadmeFiles/topology.png)

## Prerequisites

* Either [Visual Studio](https://visualstudio.microsoft.com/downloads/) or [Visual Studio Code](https://code.visualstudio.com/download) and [.NET Core SDK](https://www.microsoft.com/net/learn/get-started)
* An **Azure AD** tenant. For more information, see: [How to get an Azure AD tenant](https://docs.microsoft.com/azure/active-directory/develop/test-setup-environment#get-a-test-tenant)
* A user account in your **Azure AD** tenant. This sample will not work with a **personal Microsoft account**.  If you're signed in to the [Azure portal](https://portal.azure.com) with a personal Microsoft account and have not created a user account in your directory before, you will need to create one before proceeding.

## Setup the sample

### Step 1: Clone or download this repository

From your shell or command line:

```console
    git clone https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2.git
```

or download and extract the repository .zip file.

>:warning: To avoid path length limitations on Windows, we recommend cloning into a directory near the root of your drive.

### Step 2: Navigate to project folder

```console
    cd 4-WebApp-Your-API\4-1-MyOrg
```

### Step 3: Application Registration

There are two projects in this sample. Each needs to be separately registered in your Azure AD tenant. To register these projects:

You can follow the [manual steps](#Manual-steps)

**OR**

### Run automation scripts

* use PowerShell scripts that:
  * **automatically** creates the Azure AD applications and related objects (passwords, permissions, dependencies) for you.
  * modify the projects' configuration files.

  <details>
   <summary>Expand this section if you want to use this automation:</summary>

    > **WARNING**: If you have never used **Azure AD Powershell** before, we recommend you go through the [App Creation Scripts guide](./AppCreationScripts/AppCreationScripts.md) once to ensure that your environment is prepared correctly for this step.
  
    1. On Windows, run PowerShell as **Administrator** and navigate to the root of the cloned directory
    1. In PowerShell run:

       ```PowerShell
       Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process -Force
       ```

    1. Run the script to create your Azure AD application and configure the code of the sample application accordingly.
    1. For interactive process - in PowerShell run:

       ```PowerShell
       cd .\AppCreationScripts\
       .\Configure.ps1 -TenantId "[Optional] - your tenant id" -Environment "[Optional] - Azure environment, defaults to 'Global'"
       ```

       > Other ways of running the scripts are described in [App Creation Scripts guide](./AppCreationScripts/AppCreationScripts.md)
       > The scripts also provide a guide to automated application registration, configuration and removal which can help in your CI/CD scenarios.

  </details>

### Manual Steps

 > Note: skip this part if you've just used Automation steps

Follow the steps below for manually register and configure your apps

<details>
   <summary>Expand this section if you want to use this automation:</summary>
  1. Sign in to the [Azure portal](https://portal.azure.com).
  1. If your account is present in more than one Azure AD tenant, select your profile at the top right corner in the menu on top of the page, and then **switch directory** to change your portal session to the desired Azure AD tenant.

#### Register the service app (TodoListService-aspnetcore-webapi)

  1. Navigate to the [Azure portal](https://portal.azure.com) and select the **Azure Active Directory** service.
  1. Select the **App Registrations** blade on the left, then select **New registration**.
  1. In the **Register an application page** that appears, enter your application's registration information:
     * In the **Name** section, enter a meaningful application name that will be displayed to users of the app, for example `TodoListService-aspnetcore-webapi`.
  1. Under **Supported account types**, select **Accounts in this organizational directory only**
  1. Click **Register** to create the application.
  1. In the app's registration screen, find and note the **Application (client) ID**. You use this value in your app's configuration file(s) later in your code.
  1. Click **Save** to save your changes.

  1. In the app's registration screen, select the **Expose an API** blade to the left to open the page where you can declare the parameters to expose this app as an API for which client applications can obtain [access tokens](https://aka.ms/access-tokens) for. The first thing that we need to do is to declare the unique [resource](https://docs.microsoft.com/azure/active-directory/develop/v2-oauth2-auth-code-flow) URI that the clients will be using to obtain access tokens for this API. To declare an resource URI(Application ID URI), follow the following steps:
      * Select `Set` next to the **Application ID URI** to generate a URI that is unique for this app.
      * For this sample, accept the proposed Application ID URI (`api://{clientId}`) by selecting **Save**. Read more about Application ID URI at [Validation differences by supported account types \(signInAudience\)](https://docs.microsoft.com/azure/active-directory/develop/supported-accounts-validation).
  1. All APIs have to publish a minimum of two [scopes](https://docs.microsoft.com/azure/active-directory/develop/v2-oauth2-auth-code-flow#request-an-authorization-code), also called [Delegated Permissions](https://docs.microsoft.com/azure/active-directory/develop/v2-permissions-and-consent#permission-types), for the client's to obtain an access token successfully. To publish a scope, follow these steps:
     * Select **Add a scope** button open the **Add a scope** screen and Enter the values as indicated below:
          * For **Scope name**, use `ToDoList.Read`.
          * Select **Admins and users** options for **Who can consent?**.
        - For **Admin consent display name** type `Allow the app TodoListService-aspnetcore-webapi to [ex, read ToDo list items] as the signed-in user`.
        - For **Admin consent description** type `Allow the application to [ex, read ToDo list items] as the signed-in user.`
        - For **User consent display name** type `[ex, Read ToDo list items] as you`.
        - For **User consent description** type `Allow the application to [ex, Read ToDo list items] as the signed-in user on your behalf.`
          * Keep **State** as **Enabled**.
          * Select the **Add scope** button on the bottom to save this scope.
     > Repeat the steps above for scope **ToDoList.Write**

  1. Select the `Manifest` blade on the left.
     * Set `accessTokenAcceptedVersion` property to **2**.
     * Click on **Save**.

##### Configure the service app (TodoListService-aspnetcore-webapi) to use your app registration

  Open the project in your IDE (like Visual Studio or Visual Studio Code) to configure the code.

   > In the steps below, "ClientID" is the same as "Application ID" or "AppId".

  1. Open the `TodoListService\appsettings.json` file.
  1. Find the key `Domain` and replace the existing value with your Azure AD tenant name.
  1. Find the key `TenantId` and replace the existing value with your Azure AD tenant ID.
  1. Find the key `ClientId` and replace the existing value with the application ID (clientId) of `TodoListService-aspnetcore-webapi` app copied from the Azure portal.

#### Register the client app (TodoListClient-aspnetcore-webapi)

  1. Navigate to the [Azure portal](https://portal.azure.com) and select the **Azure Active Directory** service.
  1. Select the **App Registrations** blade on the left, then select **New registration**.
  1. In the **Register an application page** that appears, enter your application's registration information:
     * In the **Name** section, enter a meaningful application name that will be displayed to users of the app, for example `TodoListClient-aspnetcore-webapi`.
  1. Under **Supported account types**, select **Accounts in this organizational directory only**
  1. Click **Register** to create the application.
  1. In the app's registration screen, find and note the **Application (client) ID**. You use this value in your app's configuration file(s) later in your code.
  1. In the app's registration screen, select **Authentication** in the menu.
      * If you don't have a platform added, select **Add a platform** and select the **Web** option.
  1. In the **Redirect URI** section enter the following redirect URIs:
    1. `https://localhost:44321/`
    1. `https://localhost:44321/signin-oidc`

  1. In the **Front-channel logout URL** section, set it to `https://localhost:44321/signout-oidc`.
  1. Click **Save** to save your changes.
  1. In the app's registration screen, select the **Certificates & secrets** blade in the left to open the page where you can generate secrets and upload certificates.
  1. In the **Client secrets** section, select **New client secret**:
     * Type a key description (for instance `app secret`).
     * Select one of the available key durations (**6 months**, **12 months** or **Custom**) as per your security posture.
     * The generated key value will be displayed when you select the **Add** button. Copy and save the generated value for use in later steps.
     * You'll need this key later in your code's configuration files. This key value will not be displayed again, and is not retrievable by any other means, so make sure to note it from the Azure portal before navigating to any other screen or blade.

> :bulb: For enhanced security, consider [using certificates](./README-use-certificate.md) instead of client secrets.

1. In the app's registration screen, select the **API permissions** blade in the left to open the page where we add access to the APIs that your application needs.
      * Select the **Add a permission** button and then,
      * Ensure that the **My APIs** tab is selected.
      * In the list of APIs, select the API `TodoListService-aspnetcore-webapi`.
      * In the **Delegated permissions** section, select the **ToDoList.Read**, **ToDoList.Write** in the list. Use the search box if necessary.
      * Select the **Add permissions** button at the bottom.

##### Configure the client app (TodoListClient-aspnetcore-webapi) to use your app registration

  Open the project in your IDE (like Visual Studio or Visual Studio Code) to configure the code.

   > In the steps below, "ClientID" is the same as "Application ID" or "AppId".

  1. Open the `Client\appsettings.json` file.
  1. Find the key `Domain` and replace the existing value with your Azure AD tenant name.
  1. Find the key `TenantId` and replace the existing value with your Azure AD tenant ID.
  1. Find the key `ClientId` and replace the existing value with the application ID (clientId) of `TodoListClient-aspnetcore-webapi` app copied from the Azure portal.
  1. Find the key `ClientSecret` and replace the existing value with the key you saved during the creation of `TodoListClient-aspnetcore-webapi` copied from the Azure portal.
  1. Find the key `TodoListScopes` and replace the existing value with **"api://<your_service_api_client_id>/ToDoList.Read api://<your_service_api_client_id>/ToDoList.Write"**.
  1. Find the key `TodoListBaseAddress` and replace the existing value with the base address of `TodoListService-aspnetcore-webapi` (by default `https://localhost:44351`).
</details>


### Variation: web app using client certificates

Follow [README-use-keyvault-certificate.md](README-use-keyvault-certificate.md) to know how to use this option.

### Step 4: Running the sample
 To run the samlple, run the next commands:
```console
    cd 4-WebApp-Your-API\4-1-MyOrg\TodoListService
    dotnet run
```

Then open a separate command line terminal and run

```console
    cd 4-WebApp-Your-API\4-1-MyOrg\Client
    dotnet run
```

## Troubleshooting

<details>
 <summary>Expand for troubleshooting info</summary>

Use [Stack Overflow](http://stackoverflow.com/questions/tagged/msal) to get support from the community.
Ask your questions on Stack Overflow first and browse existing issues to see if someone has asked your question before.
Make sure that your questions or comments are tagged with [`azure-active-directory` `adal` `msal` `dotnet`].

If you find a bug in the sample, please raise the issue on [GitHub Issues](../../issues).

To provide a recommendation, visit the following [User Voice page](https://feedback.azure.com/forums/169401-azure-active-directory).
</details>

## Using the sample

<details>
 <summary>Expand to see how to use the sample</summary>
 Open your web browser and navigate to `https://localhost:44321` and sign-in using the link on top-right. The app immediately attempts to authenticate you via the Microsoft identity platform endpoint. Sign in using an user account in that tenant.

1. Click on `TodoList`, you can click on `Create New` link. It will redirect to create task screen where you can add a new task and assign it to any user from the list.
1. The `TodoList` screen also displays tasks that are assigned to and created by signed-in user. The user can edit and delete the created tasks but can only view the assigned tasks.

> NOTE: Remember, the TodoList is stored in memory in this `TodoListService` app. Each time you run the projects, your TodoList will get emptied.

Did the sample not work for you as expected? Did you encounter issues trying this sample? Then please reach out to us using the [GitHub Issues](../../../../issues) page.

[Consider taking a moment to share your experience with us.](https://forms.office.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbRz0h_jLR5HNJlvkZAewyoWxUNEFCQ0FSMFlPQTJURkJZMTRZWVJRNkdRMC4u)
</details>

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

## How the code was created

<details>
 <summary>Expand the section</summary>

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
1. Add the `Microsoft.Identity.Web.csproj` project which is located at the root of this sample repo, to your solution (**Add Existing Project ...**). It's used to simplify signing-in and, in the next tutorial phases, to get a token.
1. Add a reference from your newly generated project to `Microsoft.Identity.Web` (right click on the **Dependencies** node under your new project, and choose **Add Reference ...**, and then in the projects tab find the `Microsoft.Identity.Web` project)

1. Open the **Startup.cs** file and:

   * at the top of the file, add the following using directive were added:

     ```CSharp
      using Microsoft.Identity.Web;
      ```

   * in the `ConfigureServices` method, replace the two following lines:

     ```CSharp
      services.AddAuthentication(AzureADDefaults.AuthenticationScheme)
              .AddAzureAD(options => Configuration.Bind("AzureAd", options));
     ```

     with these lines:

     ```CSharp
     services.AddMicrosoftIdentityWebAppAuthentication(Configuration)
             .EnableTokenAcquisitionToCallDownstreamApi(
               Configuration.GetSection("TodoList:TodoListScopes").Get<string>().Split(" ", System.StringSplitOptions.RemoveEmptyEntries))
             .AddInMemoryTokenCaches();
     ```

    This enables your application to use the Microsoft identity platform endpoint. This endpoint is capable of signing-in users both with their Work and School and Microsoft Personal accounts.

1. Change the `Properties\launchSettings.json` file to ensure that you start your web app from <https://localhost:44321> as registered. For this:
     * update the `sslPort` of the `iisSettings` section to be `44321`
     * in the `applicationUrl` property of use `https://localhost:44321`
     * Then add the following code to inject the ToDoList service implementation in the client

   ```CSharp
     // Add APIs
     services.AddTodoListService(Configuration);
   ```

1. Open the `appsettings.json` file and copy the keys from the sample's corresponding file under the `AzureAd` and `TodoList` sections.

#### Add a model (TodoListItem) and add the controller and views

1. In the TodoListClient project, add a folder named `Models` and then create a new  file named `TodoItem.cs`. Copy the contents of the TodoListClient\Models\TodoItem.cs in this file.
1. Create a new Controller named `TodoListController` and copy and paste the code from the sample (TodoListService\Controllers\TodoListController.cs) to this controller.
1. Copy the files `TodoListService` and `TodoListService.cs` in the **TodoListClient\Services** folder provided in this sample to your project.
1. Copy the contents of **TodoListClient\views\ToDo** folder to the views folder of your project.
1. Modify the `Views\Shared\_Layout.cshtml` to add a link to the **ToDolist** controller. Check the `Views\Shared\_Layout.cshtml` in the sample for reference.
1. Add a section name **TodoList** in the appsettings.json file and add the keys `TodoListScope`, `TodoListBaseAddress`.
1. Update the `configureServices` method in `startup.cs` to add the MSAL library and a token cache.

    ```CSharp
     services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
             .AddMicrosoftIdentityWebApp(Configuration)
             .EnableTokenAcquisitionToCallDownstreamApi(new string[] { Configuration["TodoList:TodoListScopes"] })
             .AddInMemoryTokenCaches();
    ```

1. Update the `Configure` method to include **app.UseAuthentication();** before **app.UseAuthorization();**  

  ```Csharp
     app.UseAuthentication();
     app.AddAuthorization();
  ```

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

### Modify the Startup.cs file to validate bearer access tokens received by the Web API

1. Add the `Microsoft.Identity.Web.csproj` project which is located at the root of this sample repo, to your solution (**Add Existing Project ...**).
1. Add a reference from your newly generated project to `Microsoft.Identity.Web` (right click on the **Dependencies** node under your new project, and choose **Add Reference ...**, and then in the projects tab find the `Microsoft.Identity.Web` project)
Update `Startup.cs` file :

 *Add the following two using statements

```CSharp
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Client.TokenCacheProviders;
```

 *In the `ConfigureServices` method, replace the following code:

  ```CSharp
  services.AddAuthentication(AzureADDefaults.BearerAuthenticationScheme)
          .AddAzureADBearer(options => Configuration.Bind("AzureAd", options));
   ```

  with

  ```Csharp
    services.AddMicrosoftIdentityWebApiAuthentication(Configuration);
  ```

 *Add the method **app.UseAuthentication()** before **app.UseAuthorization()** in the `Configure` method

  ```Csharp
     app.UseAuthentication();
     app.UseAuthorization();
  ```

### Create the TodoListController.cs file

1. Add a folder named `Models` and then create a new  file named `TodoItem.cs`. Copy the contents of the TodoListClient\Models\TodoItem.cs in this file.
1. Create a new Controller named `TodoListController` and copy and paste the code from the sample (TodoListService\Controllers\TodoListController.cs) to this controller.

</details>

## How to deploy this sample to Azure

<details>
 <summary>Expand the section</summary>

This project has two WebApp / Web API projects. To deploy them to Azure Web Sites, you'll need, for each one, to:

 *create an Azure Web Site
 *publish the Web App / Web APIs to the web site, and
 *update its client(s) to call the web site instead of IIS Express.

### Create and publish the `TodoListService-aspnetcore-webapi` to an Azure Web Site

1. Sign in to the [Azure portal](https://portal.azure.com).
1. Click `Create a resource` in the top left-hand corner, select **Web** --> **Web App**, and give your web site a name, for example, `TodoListService-aspnetcore-webapi-contoso.azurewebsites.net`.
1. Thereafter select the `Subscription`, `Resource Group`, `App service plan and Location`. `OS` will be **Windows** and `Publish` will be **Code**.
1. Click `Create` and wait for the App Service to be created.
1. Once you get the `Deployment succeeded` notification, then click on `Go to resource` to navigate to the newly created App service.
1. Once the web site is created, locate it it in the **Dashboard** and click it to open **App Services** **Overview** screen.
1. From the **Overview** tab of the App Service, download the publish profile by clicking the **Get publish profile** link and save it.  Other deployment mechanisms, such as from source control, can also be used.
1. Switch to Visual Studio and go to the TodoListService-aspnetcore-webapi project.  Right click on the project in the Solution Explorer and select **Publish**.  Click **Import Profile** on the bottom bar, and import the publish profile that you downloaded earlier.
1. Click on **Configure** and in the `Connection tab`, update the Destination URL so that it is a `https` in the home page url, for example [https://TodoListService-aspnetcore-webapi-contoso.azurewebsites.net](https://TodoListService-aspnetcore-webapi-contoso.azurewebsites.net). Click **Next**.
1. On the Settings tab, make sure `Enable Organizational Authentication` is NOT selected.  Click **Save**. Click on **Publish** on the main screen.
1. Visual Studio will publish the project and automatically open a browser to the URL of the project.  If you see the default web page of the project, the publication was successful.

### Update the Active Directory tenant application registration for `TodoListService-aspnetcore-webapi`

1. Navigate back to to the [Azure portal](https://portal.azure.com).
In the left-hand navigation pane, select the **Azure Active Directory** service, and then select **App registrations (Preview)**.
1. In the resultant screen, select the `TodoListService-aspnetcore-webapi` application.
1. From the *Branding* menu, update the **Home page URL**, to the address of your service, for example [https://TodoListService-aspnetcore-webapi-contoso.azurewebsites.net](https://TodoListService-aspnetcore-webapi-contoso.azurewebsites.net). Save the configuration.
1. Add the same URL in the list of values of the *Authentication -> Redirect URIs* menu. If you have multiple redirect urls, make sure that there a new entry using the App service's Uri for each redirect url.

### Update the `TodoListClient-aspnetcore-webapi` to call the `TodoListService-aspnetcore-webapi` Running in Azure Web Sites

1. In Visual Studio, go to the `TodoListClient-aspnetcore-webapi` project.
2. Open `Client\appsettings.json`.  Only one change is needed - update the `todo:TodoListBaseAddress` key value to be the address of the website you published,
   for example, [https://TodoListService-aspnetcore-webapi-contoso.azurewebsites.net](https://TodoListService-aspnetcore-webapi-contoso.azurewebsites.net).
3. Run the client! If you are trying multiple different client types (for example, .Net, Windows Store, Android, iOS) you can have them all call this one published web API.

### Create and publish the `TodoListClient-aspnetcore-webapi` to an Azure Web Site

1. Sign in to the [Azure portal](https://portal.azure.com).
1. Click `Create a resource` in the top left-hand corner, select **Web** --> **Web App**, and give your web site a name, for example, `TodoListClient-aspnetcore-webapi-contoso.azurewebsites.net`.
1. Thereafter select the `Subscription`, `Resource Group`, `App service plan and Location`. `OS` will be **Windows** and `Publish` will be **Code**.
1. Click `Create` and wait for the App Service to be created.
1. Once you get the `Deployment succeeded` notification, then click on `Go to resource` to navigate to the newly created App service.
1. Once the web site is created, locate it in the **Dashboard** and click it to open **App Services** **Overview** screen.
1. From the **Overview** tab of the App Service, download the publish profile by clicking the **Get publish profile** link and save it.  Other deployment mechanisms, such as from source control, can also be used.
1. Switch to Visual Studio and go to the TodoListClient-aspnetcore-webapi project.  Right click on the project in the Solution Explorer and select **Publish**.  Click **Import Profile** on the bottom bar, and import the publish profile that you downloaded earlier.
1. Click on **Configure** and in the `Connection tab`, update the Destination URL so that it is a `https` in the home page url, for example [https://TodoListClient-aspnetcore-webapi-contoso.azurewebsites.net](https://TodoListClient-aspnetcore-webapi-contoso.azurewebsites.net). Click **Next**.
1. On the Settings tab, make sure `Enable Organizational Authentication` is NOT selected.  Click **Save**. Click on **Publish** on the main screen.
1. Visual Studio will publish the project and automatically open a browser to the URL of the project.  If you see the default web page of the project, the publication was successful.

### Update the Active Directory tenant application registration for `TodoListClient-aspnetcore-webapi`

1. Navigate back to to the [Azure portal](https://portal.azure.com).
In the left-hand navigation pane, select the **Azure Active Directory** service, and then select **App registrations (Preview)**.
1. In the resultant screen, select the `TodoListClient-aspnetcore-webapi` application.
1. In the **Authentication** | page for your application, update the Logout URL fields with the address of your service, for example [https://TodoListClient-aspnetcore-webapi-contoso.azurewebsites.net](https://TodoListClient-aspnetcore-webapi-contoso.azurewebsites.net)
1. From the *Branding* menu, update the **Home page URL**, to the address of your service, for example [https://TodoListClient-aspnetcore-webapi-contoso.azurewebsites.net](https://TodoListClient-aspnetcore-webapi-contoso.azurewebsites.net). Save the configuration.
1. Add the same URL in the list of values of the *Authentication -> Redirect URIs* menu. If you have multiple redirect urls, make sure that there a new entry using the App service's Uri for each redirect url.

> NOTE: Remember, the To Do list is stored in memory in this TodoListService sample. Azure Web Sites will spin down your web site if it is inactive, and your To Do list will get emptied.
Also, if you increase the instance count of the web site, requests will be distributed among the instances. To Do will, therefore, not be the same on each instance.

</details>

## Next Steps

Learn how to:

* [Change your app to sign-in users from any organization or any Microsoft accounts](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/tree/master/1-WebApp-OIDC/1-3-AnyOrgOrPersonal)
* [Enable users from National clouds to sign-in to your application](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/tree/master/1-WebApp-OIDC/1-4-Sovereign)
* [Enable your Web App to call a Web API on behalf of the signed-in user](https://github.com/Azure-Samples/ms-identity-dotnetcore-ca-auth-context-app)

## Contributing

If you'd like to contribute to this sample, see [CONTRIBUTING.MD](/CONTRIBUTING.md).

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information, see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Learn More

* Microsoft identity platform (Azure Active Directory for developers)](https://docs.microsoft.com/azure/active-directory/develop/)
* Overview of Microsoft Authentication Library (MSAL)](https://docs.microsoft.com/azure/active-directory/develop/msal-overview)
* Authentication Scenarios for Azure AD](https://docs.microsoft.com/azure/active-directory/develop/authentication-flows-app-scenarios)
* Azure AD code samples](https://docs.microsoft.com/azure/active-directory/develop/sample-v2-code)
* Register an application with the Microsoft identity platform](https://docs.microsoft.com/azure/active-directory/develop/quickstart-register-app)
* Building Zero Trust ready apps](https://aka.ms/ztdevsession)

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
  *[Azure Active Directory with ASP.NET Core](https://docs.microsoft.com/aspnet/core/security/authentication/azure-active-directory)



