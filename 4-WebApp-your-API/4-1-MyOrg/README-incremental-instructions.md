---
services: active-directory
platforms: dotnet
author: kalyankrishna1
level: 200
client: ASP.NET Core Web App
service: ASP.NET Core Web API
endpoint: Microsoft identity platform
---

# How to secure a Web API built with ASP.NET Core using the Microsoft identity platform (formerly Microsoft Entra ID for developers)

[![Build status](https://identitydivision.visualstudio.com/IDDP/_apis/build/status/aad%20Samples/.NET%20client%20samples/ASP.NET%20Core%20Web%20App%20tutorial)](https://identitydivision.visualstudio.com/IDDP/_build/latest?definitionId=819)

> The sample in this folder is part of a multi-chapter tutorial. The first phase is available at [An ASP.NET Core Web app signing-in users with the Microsoft identity platform in your organization](../1-WebApp-OIDC/1-1-MyOrg).

## About this sample

This sample has a web api and a client web app, both built using the asp.net core platform. The client app signs in users using the [OpenID Connect protocol](https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-protocols-oidc) flow and in this process obtains (and caches) an [access token](https://docs.microsoft.com/en-us/azure/active-directory/develop/access-tokens) for the web api. The client app has a ToDo list that the web app users can work with. This ToDo list is maintained in an in-memory list on the Web API. The client app calls the webApi for all operations on the ToDo list.

### Scenario

You expose a Web API and you want to protect it so that only authenticated user can access it. You want to enable authenticated users with  work and school accounts
to use your Web API. Your API calls a downstream API (Microsoft Graph) to provide added value to its client apps.

### Overview

This sample presents a Web API running on ASP.NET Core, protected by [Microsoft Entra ID OAuth Bearer](https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-v2-protocols) Authentication. The client application uses [MSAL.NET](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet) library to obtain a JWT access token through using the [OAuth 2.0](https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-auth-code-flow) protocol flow.

The client web application essentially takes the following steps to sign-in the user and obtain a bearer token for the Web API:

1. Signs-in the user.
1. Acquires an access token for the Web API.
1. Calls the Web API using the access token as a bearer token in the authentication header of the Http request. The Web API validates the caller using the ASP.NET JWT Bearer Authentication middleware.

![Topology](./ReadmeFiles/topology.png)

### The end user experience when using this sample

The Web API (TodoListService) maintains an in-memory collection of to-do items per authenticated user. Multiple client applications signing-in users under the same identities, will share the same to-do list.

The client web application (TodoListClient) enables a user to:

- Sign in to the client app.
- After the sign-in, the user sees the list of to-do items exposed by Web API for the signed-in user.
- The user can add/edit/delete to-do items by clicking on the various options presented.

## How to run this sample

### Step 1:  Clone or download this repository

From your shell or command line:

```Shell
git clone https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2.git

cd "4-WebApp-your-API\4-1-Your-API"
```

or download and exact the repository .zip file.

> Given that the name of the sample is pretty long, and so are the name of the referenced NuGet packages, you might want to clone it in a folder close to the root of your hard drive, to avoid file size limitations on Windows.

### Step 2:  Register the sample application with your Microsoft Entra tenant

There are two projects in this sample. Each needs to be separately registered in your Microsoft Entra tenant. To register these projects, you can:

- either follow the steps [Step 2: Register the sample with your Microsoft Entra tenant](#step-2-register-the-sample-with-your-azure-active-directory-tenant) and [Step 3:  Configure the sample to use your Microsoft Entra tenant](#choose-the-azure-ad-tenant-where-you-want-to-create-your-applications)
- or use PowerShell scripts that:
  - **automatically** creates the Microsoft Entra applications and related objects (passwords, permissions, dependencies) for you
  - modify the Visual Studio projects' configuration files.

If you want to use this automation:

1. On Windows run PowerShell and navigate to the root of the cloned directory

1. In PowerShell run:

   ```PowerShell
   Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process -Force
   ```

1. Since you've already created an app using the automation earlier, run the following script to clean up the existing registration and prepare your tenant for the additional web API app.

   ```PowerShell
   .\AppCreationScripts\Cleanup.ps1
   ```

1. Run the script to create your Microsoft Entra applications and configure the code of the sample application accordingly.

   ```PowerShell
   cd .\AppCreationScripts\ 
   .\Configure.ps1
   ```

   > Other ways of running the scripts are described in [App Creation Scripts](./AppCreationScripts/AppCreationScripts.md)

1. Open the Visual Studio solution and click start

If you don't want to use this automation, follow the steps below

#### Choose the Microsoft Entra tenant where you want to create your applications

These instructions only list the additional set of steps needed over the previous chapter.

#### Register the service app (TodoListService-aspnetcore-webapi)

1. Navigate to the Microsoft identity platform for developers [App registrations](https://go.microsoft.com/fwlink/?linkid=2083908) page.
1. Select **New registration**.
1. When the **Register an application page** appears, enter your application's registration information:
   - In the **Name** section, enter a meaningful application name that will be displayed to users of the app, for example `TodoListService-aspnetcore-webapi`.
   - Leave **Supported account types** on the default setting of **Accounts in this organizational directory only**.
1. Select **Register** to create the application.
1. On the app **Overview** page, find the **Application (client) ID** value and record it for later. You'll need it to configure the Visual Studio configuration file for this project.

1. Select the **Expose an API** section, and:
   - Select **Add a scope**
   - accept the proposed Application ID URI (api://{clientId}) by selecting **Save and Continue**
   - Enter the following parameters
     - for **Scope name** use `user_impersonation`
     - Keep **Admins and users** for **Who can consent**
     - in **Admin consent display name** type `Access TodoListService-aspnetcore-webapi as an admin`
     - in **Admin consent description** type `Accesses the TodoListService-aspnetcore-webapi Web API as an admin`
     - in **User consent display name** type `Access TodoListService-aspnetcore-webapi as an user`
     - in **User consent description** type `Accesses the TodoListService-aspnetcore-webapi Web API as an user`
     - Keep **State** as **Enabled**
     - Select **Add scope**

#### Update the client app (TodoListClient-aspnetcore-webapi)

1. Navigate to the Microsoft identity platform for developers [App registrations](https://go.microsoft.com/fwlink/?linkid=2083908) page.
1. Select the **API permissions** section
   - Click the **Add a permission** button and then,
   - Ensure that the **My APIs** tab is selected
   - In the list of APIs, select the API `TodoListService-aspnetcore-webapi`.
   - In the **Delegated permissions** section, ensure that the right permissions are checked: **Access 'TodoListService-aspnetcore-webapi'**. Use the search box if necessary.
   - Select the **Add permissions** button

### Step 3:  Configure the sample to use your Microsoft Entra tenant

In the steps below, "ClientID" is the same as "Application ID" or "AppId".

Open the solution in Visual Studio to configure the projects

#### Configure the service project

> Note: if you used the setup scripts, the changes below will have been applied for you

1. Open the `TodoListService\appsettings.json` file
1. Find the app key `Domain` and replace the existing value with your Microsoft Entra tenant name.
1. Find the app key `TenantId` and replace the existing value with your Microsoft Entra tenant ID.
1. Find the app key `ClientId` and replace the existing value with the application ID (clientId) of the `TodoListService-aspnetcore-webapi` application copied from the Microsoft Entra admin center.

#### Configure the client project

> Note: if you used the setup scripts, the changes below will have been applied for you

1. Open the `Client\appsettings.json` file
1. Find the app key `Domain` and replace the existing value with your Microsoft Entra tenant name.
1. Find the app key `TenantId` and replace the existing value with your Microsoft Entra tenant ID.
1. Find the app key `ClientId` and replace the existing value with the application ID (clientId) of the `TodoListClient-aspnetcore-webapi` application copied from the Microsoft Entra admin center.
1. Find the app key `ClientSecret` and replace the existing value with the key you saved during the creation of the `TodoListClient-aspnetcore-webapi` app, in the Microsoft Entra admin center.
1. Find the app key `TodoListScope` and replace the existing value with Scope if you changed the name from `api://<client id>/user_impersonation`.
1. Find the app key `TodoListBaseAddress` and replace the existing value with the base address of the TodoListService-aspnetcore-webapi project (or use the default `https://localhost:44351/`).

### Step 4: Run the sample

Clean the solution, rebuild the solution, and run it.  You will have to go into the solution properties and set both projects as startup projects, with the service project starting first.

When you start the Web API from Visual Studio, depending on the browser you use, you'll get:

- an empty web page (case with Microsoft Edge)
- or an error HTTP 401 (case with Chrome)

This behavior is expected as you are not authenticated. The client application will be authenticated, so it will be able to access the Web API.

Explore the sample by signing in into the TodoList client, adding items to the To Do list. If you stop the application without signing out, the next time you run the application, you won't be prompted to sign in again.

NOTE: Remember, the To-Do list is stored in memory in this `TodoListService` app. Each time you run the projects, your To-Do list will get emptied.

> Did the sample not work for you as expected? Did you encounter issues trying this sample? Then please reach out to us using the [GitHub Issues](../../../issues) page.

> [Consider taking a moment to share your experience with us.](https://forms.office.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbRz0h_jLR5HNJlvkZAewyoWxUNEFCQ0FSMFlPQTJURkJZMTRZWVJRNkdRMC4u)

## Variation: web app using client certificates

Follow [README-use-certificate.md](README-use-certificate.md) to know how to use this option.

## How was the code created

For details about the way the code to sign-in users was created, see [Create the sample from command line](../1-WebApp-OIDC/1-1-MyOrg/README.md#option-2-create-the-sample-from-the-command-line) section, of the README.md file located in the sibling folder named **1. Enable your Web Apps to sign-in users**.

In the following section, we will only cover the additional code added to:

1. Enable the client app to obtain a token for the web Api.
1. Create a new asp.net core Web API project and configure it to secure with Microsoft Entra ID.

### Code changes in the the Client Web app (TodoListClient)

### Add a reference to MSAL.NET

Calling a Web API involves getting a token for this Web API. Acquiring a token is achieved using the MSAL.NET SDK.
Add a reference to the `Microsoft.Identity.Client` NuGet package in the TodoListClient project.
Add a reference to the `Microsoft.Identity.Web` library if not already present. It contains reusable code that you can use in your Web APIs (and web apps)

#### Add a model (TodoListItem) and add the controller and views

1. In the TodoListClient project, add a folder named `Models` and then create a new  file named `TodoItem.cs`. Copy the contents of the TodoListClient\Models\TodoItem.cs in this file.
1. Create a new Controller named `TodoListController` and copy and paste the code from the sample (TodoListService\Controllers\TodoListController.cs) to this controller.
1. Copy the files `TodoListService` and `TodoListService.cs` in the **TodoListClient\Services** folder provided in this sample to your project .
1. Copy the contents of **TodoListClient\views\ToDo** folder to the views folder of your project.
1. Modify the `Views\Shared\_Layout.cshtml` to add a link to the ***ToDolist* controller. Check the `Views\Shared\_Layout.cshtml` in the sample for reference.
1. Add a section name **TodoList** in the appsettings.json file and add the keys `TodoListScope`, `TodoListBaseAddress`.
1. Update the `configureServices` method in `startup.cs` to add the MSAL library and a token cache.

```CSharp
     services.AddMicrosoftIdentityWebAppAuthentication(Configuration)
             .EnableTokenAcquisitionToCallDownstreamApi(new string[] { Configuration["TodoList:TodoListScope"] })
             .AddInMemoryTokenCaches();
 ```

### Creating the Web API project (TodoListService)

The code for the TodoListService was created in the following way:

#### Create the web api using the ASP.NET Core templates

```Text
md TodoListService
cd TodoListService
dotnet new webapi -au=SingleOrg
```

#### Add a model (TodoListItem) and modify the controller

In the TodoListService project, add a folder named `Models` and then create a new  file named `TodoItem.cs`. Copy the contents of the TodoListService\Models\TodoItem.cs in this file.

### Modify the startup.cs file to validate bearer access tokens received by the Web API

Update `Startup.cs` file :

- Add the following two using statements

```CSharp
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Client.TokenCacheProviders;
```

- In the `ConfigureServices` method, replace the following code:

  ```CSharp
  services.AddAuthentication(AzureADDefaults.BearerAuthenticationScheme)
          .AddAzureADBearer(options => Configuration.Bind("AzureAd", options));
   ```

  with

  ```Csharp
    services.AddMicrosoftWebApi(Configuration)
         .AddInMemoryTokenCaches();
  ```
  
  - Add the method **app.UseAuthentication()** before **app.UseMvc()** in the `Configure` method

  ```Csharp
     app.UseAuthentication();
     app.UseMvc();
  ```
  `AddMicrosoftWebApi` does the following:
  - add the **Jwt**BearerAuthenticationScheme (Note the replacement of **BearerAuthenticationScheme** by **Jwt**BearerAuthenticationScheme)
  - set the authority to be the Microsoft identity platform 
  - sets the audiences to validate
  - register an issuer validator that accepts issuers to be in the Microsoft identity platform clouds.

The implementations of these classes are in the Microsoft.Identity.Web library (and folder), and they are designed to be reusable in your applications (Web apps and Web apis). You are encouraged to browse the code in the library to understand the changes in detail.

  - Then add the following code to inject the ToDoList service implementation in the client

   ```CSharp
        // Add APIs
        services.AddTodoListService(Configuration);
  ```

### Create the TodoListController.cs file

1. Add a folder named `Models` and then create a new  file named `TodoItem.cs`. Copy the contents of the TodoListClient\Models\TodoItem.cs in this file.
1. Create a new Controller named `TodoListController` and copy and paste the code from the sample (\TodoListService\Controllers\TodoListController.cs) to this controller.

> NOTE: Remember, the To Do list is stored in memory in this TodoListService sample. Azure Web Sites will spin down your web site if it is inactive, and your To Do list will get emptied.
Also, if you increase the instance count of the web site, requests will be distributed among the instances. To Do will, therefore, not be the same on each instance.

## Community Help and Support

Use [Stack Overflow](http://stackoverflow.com/questions/tagged/msal) to get support from the community.
Ask your questions on Stack Overflow first and browse existing issues to see if someone has asked your question before.
Make sure that your questions or comments are tagged with [`msal` `dotnet` `azure-active-directory`].

If you find a bug in the sample, please raise the issue on [GitHub Issues](../../../issues).

To provide a recommendation, visit the following [User Voice page](https://feedback.azure.com/forums/169401-azure-active-directory).

## Contributing

If you'd like to contribute to this sample, see [CONTRIBUTING.MD](../CONTRIBUTING.md).

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information, see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## More information

For more information, visit the following links:

- Articles about the new Microsoft identity platform are at [http://aka.ms/aaddevv2](http://aka.ms/aaddevv2), with a focus on:
  - [Microsoft Entra ID OAuth Bearer protocol](https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-v2-protocols)
  - [The OAuth 2.0 protocol in Microsoft Entra ID](https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-auth-code-flow)
  - [Access token](https://docs.microsoft.com/en-us/azure/active-directory/develop/access-tokens)
  - [The OpenID Connect protocol](https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-protocols-oidc)

- To lean more about the application registration, visit:
  - [Quickstart: Register an application with the Microsoft identity platform (Preview)](https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-register-app)
  - [Quickstart: Configure a client application to access web APIs (Preview)](https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-configure-app-access-web-apis)
  - [Quickstart: Configure an application to expose web APIs (Preview)](https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-configure-app-expose-web-apis)

- To learn more about the code, visit:
  - [Conceptual documentation for MSAL.NET](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki#conceptual-documentation) and in particular:
  - [Acquiring tokens with authorization codes on web apps](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/Acquiring-tokens-with-authorization-codes-on-web-apps)
  - [Customizing Token cache serialization](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/token-cache-serialization)

- To learn more about security in aspnetcore,
  - [Introduction to Identity on ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-2.1&tabs=visual-studio%2Caspnetcore2x)
  - [AuthenticationBuilder](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authentication.authenticationbuilder?view=aspnetcore-2.0)
  - [Microsoft Entra ID with ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/azure-active-directory/?view=aspnetcore-2.1)
