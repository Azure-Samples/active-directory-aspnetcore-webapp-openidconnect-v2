---
page_type: sample
languages:
  - csharp
products:
  - dotnet
  - aspnet-core
  - azure-active-directory-b2c
description: "How to secure a Web API built with ASP.NET Core using the Azure AD B2C"
---

# How to secure a Web API built with ASP.NET Core using the Azure AD B2C

[![Build status](https://identitydivision.visualstudio.com/IDDP/_apis/build/status/AAD%20Samples/.NET%20client%20samples/ASP.NET%20Core%20Web%20App%20tutorial)](https://identitydivision.visualstudio.com/IDDP/_build/latest?definitionId=819)

> The sample in this folder is part of a multi-chapter tutorial. The first phase is available at [An ASP.NET Core Web app signing-in users with the Microsoft identity platform in your organization](../../1-WebApp-OIDC/1-1-MyOrg).

## Overview

This sample demonstrates an ASP.NET Core web app calling an ASP.NET Core web API that is secured using Azure AD B2C.

1. The client ASP.NET Core web app application uses the Microsoft Authentication Library [Microsoft Authentication Library (MSAL) for .NET](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet) to sign-in a user and obtain a JWT access token from **Azure AD B2C**:
1. The [Access Token](https://docs.microsoft.com/azure/active-directory/develop/access-tokens) is used as a bearer token to authenticate the user when calling the ASP.NET Core Web API.

The client web application essentially takes the following steps to sign-in the user and obtain a bearer token for the Web API:

1. Signs-in the user with local or social identities.
1. Acquires an access token for the Web API.
1. Calls the Web API using the access token as a bearer token in the authentication header of the Http request. The Web API authorizes the caller (user) using the ASP.NET JWT Bearer Authorization middleware.

![Topology](./ReadmeFiles/topology.png)

## Scenario

This sample has a web API and a client web app, both built using the asp.net core platform. The client app signs in users using the [OpenID Connect protocol](https://docs.microsoft.com/azure/active-directory/develop/v2-protocols-oidc) flow and in this process obtains (and caches) an [access token](https://docs.microsoft.com/azure/active-directory/develop/access-tokens) for the web API. The client app has a ToDo list that the web app users can work with. This ToDo list is maintained in an in-memory list on the Web API. The client app calls the web API for all operations on the ToDo list.

## Prerequisites

- Either [Visual Studio](https://visualstudio.microsoft.com/downloads/) or [Visual Studio Code](https://code.visualstudio.com/download) and [.NET Core SDK](https://www.microsoft.com/net/learn/get-started)
- An **Azure AD B2C** tenant. For more information see: [How to get an Azure AD B2C tenant](https://docs.microsoft.com/azure/active-directory-b2c/tutorial-create-tenant)
- A user account in your **Azure AD B2C** tenant.

## Setup

### Clone or download this repository

From your shell or command line:

```Shell
git clone https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2.git

cd "4-WebApp-your-API\4-2-B2C"
```

or download and extract the repository .zip file.

> :warning: To avoid path length limitations on Windows, we recommend cloning into a directory near the root of your drive.

### Register the sample application(s) with your Azure Active Directory tenant

:warning: This sample comes with a pre-registered application for demo purposes. If you would like to use your own **Azure AD B2C** tenant and application, follow the steps below to register and configure the application on **Azure portal**. Otherwise, continue with the steps for [Running the sample](#running-the-sample).

### Choose the Azure AD tenant where you want to create your applications

As a first step you'll need to:

1. Sign in to the [Azure portal](https://portal.azure.com).
1. If your account is present in more than one Azure AD B2C tenant, select your profile at the top right corner in the menu on top of the page, and then **switch directory** to change your portal session to the desired Azure AD B2C tenant.

### Create User Flows and Custom Policies

Please refer to: [Tutorial: Create user flows in Azure Active Directory B2C](https://docs.microsoft.com/azure/active-directory-b2c/tutorial-create-user-flows)

> Current sample uses the [self-service password reset](https://docs.microsoft.com/azure/active-directory-b2c/add-password-reset-policy?pivots=b2c-user-flow#self-service-password-reset-recommended) experience that is configured for **Sign up and sign in (Recommended)** user flow.

### Add External Identity Providers

Please refer to: [Tutorial: Add identity providers to your applications in Azure Active Directory B2C](https://docs.microsoft.com/azure/active-directory-b2c/tutorial-add-identity-providers)

### Register the service app (TodoListService-aspnetcore-webapi)

1. Navigate to the [Azure portal](https://portal.azure.com) and select the **Azure AD B2C** service.
1. Select the **App Registrations** blade on the left, then select **New registration**.
1. In the **Register an application page** that appears, enter your application's registration information:
   - In the **Name** section, enter a meaningful application name that will be displayed to users of the app, for example `TodoListService-aspnetcore-webapi`.
   - Under **Supported account types**, select **Accounts in any identity provider or organizational directory (for authenticating users with user flows)**.
1. Select **Register** to create the application.
1. In the app's registration screen, find and note the **Application (client) ID**. You use this value in your app's configuration file(s) later in your code.
1. Select **Save** to save your changes.
1. In the app's registration screen, select the **Expose an API** blade to the left to open the page where you can declare the parameters to expose this app as an API for which client applications can obtain [access tokens](https://docs.microsoft.com/azure/active-directory/develop/access-tokens) for.
The first thing that we need to do is to declare the unique [resource](https://docs.microsoft.com/azure/active-directory/develop/v2-oauth2-auth-code-flow) URI that the clients will be using to obtain access tokens for this Api. To declare an resource URI, follow the following steps:
   - Select `Set` next to the **Application ID URI** to generate a URI that is unique for this app.
   - For this sample, accept the proposed Application ID URI (`https://{tenantName}.onmicrosoft.com/{clientId}`) by selecting **Save**.
1. All APIs have to publish a minimum of one [scope](https://docs.microsoft.com/azure/active-directory/develop/v2-oauth2-auth-code-flow#request-an-authorization-code) for the client's to obtain an access token successfully. To publish a scope, follow the following steps:
   - Select **Add a scope** button open the **Add a scope** screen and Enter the values as indicated below:
        - For **Scope name**, use `access_as_user`.
        - For **Admin consent display name** type `Access TodoListService-aspnetcore-webapi`.
        - For **Admin consent description** type `Allows the app to access TodoListService-aspnetcore-webapi as the signed-in user.`
        - Keep **State** as **Enabled**.
        - Select the **Add scope** button on the bottom to save this scope.

#### Configure the service app (TodoListService-aspnetcore-webapi) to use your app registration

Open the project in your IDE (like Visual Studio or Visual Studio Code) to configure the code.

> In the steps below, "ClientID" is the same as "Application ID" or "AppId".

1. Open the `TodoListService\appsettings.json` file.
1. Find the key `Instance` and replace the value with your tenant name. For example, `https://fabrikam.b2clogin.com`
1. Find the key `Domain` and replace the existing value with your Azure AD tenant name.
1. Find the key `ClientId` and replace the existing value with the application ID (clientId) of the application copied from the Azure portal.
1. Find the key `SignUpSignInPolicyId` and replace with the name of the `Sign up and sign in` policy you created.

### Register the client app (TodoListClient-aspnetcore-webapi)

1. Navigate to the [Azure portal](https://portal.azure.com) and select the **Azure AD B2C** service.
1. Select the **App Registrations** blade on the left, then select **New registration**.
1. In the **Register an application page** that appears, enter your application's registration information:
   - In the **Name** section, enter a meaningful application name that will be displayed to users of the app, for example `TodoListClient-aspnetcore-webapi`.
   - Under **Supported account types**, select **Accounts in any identity provider or organizational directory (for authenticating users with user flows)**.
   - In the **Redirect URI (optional)** section, select **Web** in the combo-box and enter the following redirect URI: `https://localhost:5000/`.
     > Note that there are more than one redirect URIs used in this sample. You'll need to add them from the **Authentication** tab later after the app has been created successfully.
1. Select **Register** to create the application.
1. In the app's registration screen, find and note the **Application (client) ID**. You use this value in your app's configuration file(s) later in your code.
1. In the app's registration screen, select **Authentication** in the menu.
   - If you don't have a platform added, select **Add a platform** and select the **Web** option.
   - In the **Redirect URIs** section, enter the following redirect URIs.
      - `https://localhost:5000/signin-oidc`
   - In the **Front-channel logout URL** section, set it to `https://localhost:5000/signout-oidc`.
   - In **Implicit grant** section,  select the check boxes for **Access tokens** and **ID tokens**.
1. Select **Save** to save your changes.
1. In the app's registration screen, select the **Certificates & secrets** blade in the left to open the page where we can generate secrets and upload certificates.
1. In the **Client secrets** section, select **New client secret**:
   - Type a key description (for instance `app secret`),
   - Select one of the available key durations (**In 1 year**, **In 2 years**, or **Never Expires**) as per your security posture.
   - The generated key value will be displayed when you select the **Add** button. Copy the generated value for use in the steps later.
   - You'll need this key later in your code's configuration files. This key value will not be displayed again, and is not retrievable by any other means, so make sure to note it from the Azure portal before navigating to any other screen or blade.
1. In the app's registration screen, select the **API permissions** blade in the left to open the page where we add access to the APIs that your application needs.
   - Select the **Add a permission** button and then,
   - Ensure that the **My APIs** tab is selected.
   - In the list of APIs, select the API `TodoListService-aspnetcore-webapi`.
   - In the **Delegated permissions** section, select the **Access 'TodoListService-aspnetcore-webapi'** in the list. Use the search box if necessary.
   - Select the **Add permissions** button at the bottom.
   - Select Grant admin consent for (your tenant name).

#### Configure the client app (TodoListClient-aspnetcore-webapi) to use your app registration

Open the project in your IDE (like Visual Studio or Visual Studio Code) to configure the code.

> In the steps below, "ClientID" is the same as "Application ID" or "AppId".

1. Open the `Client\appsettings.json` file.
1. Find the key `Instance` and replace the value with your tenant name. For example, `https://fabrikam.b2clogin.com`
1. Find the key `Domain` and replace the existing value with your Azure AD tenant name.
1. Find the key `ClientId` and replace the existing value with the application ID (clientId) of the application copied from the Azure portal.
1. Find the key `SignUpSignInPolicyId` and replace with the name of the `Sign up and sign in` policy you created.
1. Find the key `EditProfilePolicyId` and replace with the name of the `Profile editing` policy you created.
1. Find the key `ClientSecret` and replace the existing value with the key you saved during the creation of the app, in the Azure portal.
1. Find the key `TodoListScope` and replace the existing value with the service Scope. For example, `https://{tenantName}.onmicrosoft.com/{service_clientId}/access_as_user`.

 This sample is configured with the password reset experience that is a part of the sign-up or sign-in policy. If separate password reset flow is required then add the key `ResetPasswordPolicyId` under **AzureAdB2C** section, and value of the key as the name of the `Password reset` policy you created.

## Running the sample

You can run the sample by using either Visual Studio or command line interface as shown below:

### Run the sample using Visual Studio

Clean the solution, rebuild the solution, and run it. You might want to go into the solution properties and set both projects as startup projects, with the service project starting first.

### Run the sample using a command line interface such as VS Code integrated terminal

#### Step 1. Install .NET Core dependencies

```console
   cd TodoListService
   dotnet restore
```

Then:  
In a separate console window, execute the following commands

```console
   cd ../
   cd Client
   dotnet restore
```

#### Step 2. Trust development certificates

```console
   dotnet dev-certs https --clean
   dotnet dev-certs https --trust
```

Learn more about [HTTPS in .NET Core](https://docs.microsoft.com/aspnet/core/security/enforcing-ssl).

#### Step 3. Run the applications

In both the console windows execute the below command:

```console
    dotnet run
```

## Explore the sample

1. Open your web browser and navigate to https://localhost:5000. Accept the IIS Express SSL certificate if needed. Click on **Sign In** button.
1. If you don't have an account registered on the **Azure AD B2C** used in this sample, follow the sign up process. Otherwise, input the email and password for your account and click on **Sign in**.

When you start the Web API from Visual Studio, depending on the browser you use, you'll get:

- an empty web page (case with Microsoft Edge)
- or an error HTTP 401 (case with Chrome)

This behavior is expected as the browser is not authenticated. The client application will be authenticated, so it will be authorized to access the Web API.

Explore the sample by signing in into the TodoList client, adding items to the To-Do list. If you stop the application without signing out, the next time you run the application, you won't be prompted to sign in again.

NOTE: Remember, the To-Do list is stored in memory in this `TodoListService` app. Each time you run the projects, your To-Do list will get emptied.

> Did the sample not work for you as expected? Did you encounter issues trying this sample? Then please reach out to us using the [GitHub Issues](../../../issues) page.

> [Consider taking a moment to share your experience with us.](https://forms.office.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbRz0h_jLR5HNJlvkZAewyoWxUNEFCQ0FSMFlPQTJURkJZMTRZWVJRNkdRMC4u)

## How was the code created

### Creating the client web app (TodoListClient)

#### Step 1: Create the sample from the command line

1. Run the following command to create a sample from the command line using the `SingleOrg` template:

    ```Sh
    md TodoListClient
    cd TodoListClient
    dotnet new mvc --auth SingleOrg --client-id <Enter_the_Application_Id_here> --tenant-id <yourTenantId>
    ```

    > Note: Replace *`Enter_the_Application_Id_here`* with the *Application Id* from the application Id you just registered in the Application Registration Portal and *`<yourTenantId>`* with the *Directory (tenant) ID* where you created your application.

#### Step 2: Modified the generated code

1. Open the generated project (.csproj) in Visual Studio, and save the solution.
1. Add the `Microsoft.Identity.Web.csproj` project which is located at the root of this sample repo, to your solution (**Add Existing Project ...**). It's used to simplify signing-in and, in the next tutorial phases, to get a token.
1. Add a reference from your newly generated project to `Microsoft.Identity.Web` (right click on the **Dependencies** node under your new project, and choose **Add Reference ...**, and then in the projects tab find the `Microsoft.Identity.Web` project)

1. Open the **Startup.cs** file and:

   - at the top of the file, add the following using directive:

     ```CSharp
      using Microsoft.Identity.Web;
      ```

   - in the `ConfigureServices` method, replace the two following lines:

     ```CSharp
      services.AddAuthentication(AzureADDefaults.AuthenticationScheme)
              .AddAzureAD(options => Configuration.Bind("AzureAd", options));
     ```

     by this line:

     ```CSharp
      services.AddMicrosoftIdentityWebAppAuthentication(Configuration, "AzureAdB2C")
                    .EnableTokenAcquisitionToCallDownstreamApi(new string[] { Configuration["TodoList:TodoListScope"] })
                    .AddInMemoryTokenCaches();
     services.AddInMemoryTokenCaches();
     ```

1. Update the `Configure` method to include **app.UseAuthentication();** before **app.UseMvc();**  

    ```Csharp
      app.UseAuthentication();
      app.UseMvc();
    ```

1. Change the `Properties\launchSettings.json` file to ensure that you start your web app from <https://localhost:44321> as registered. For this:
    - update the `sslPort` of the `iisSettings` section to be `44321`
    - in the `applicationUrl` property of use `https://localhost:44321`
    - Then add the following code to inject the ToDoList service implementation in the client

   ```CSharp
     services.AddTodoListService(Configuration);
   ```

1. Open the `appsettings.json` file and copy the keys from the sample's corresponding file under the `AzureAd` and `TodoList` sections.

#### Add a model (TodoListItem) and add the controller and views

1. In the TodoListClient project, add a folder named `Models` and then create a new  file named `TodoItem.cs`. Copy the contents of the TodoListClient\Models\TodoItem.cs in this file.
1. Create a new Controller named `TodoListController` and copy and paste the code from the sample (TodoListService\Controllers\TodoListController.cs) to this controller.
1. Copy the files `TodoListService` and `TodoListService.cs` in the **TodoListClient\Services** folder provided in this sample to your project .
1. Copy the contents of **TodoListClient\views\ToDo** folder to the views folder of your project.
1. Modify the `Views\Shared\_Layout.cshtml` to add a link to the ***ToDolist* controller. Check the `Views\Shared\_Layout.cshtml` in the sample for reference.
1. Add a section name **TodoList** in the appsettings.json file and add the keys `TodoListScope`, `TodoListBaseAddress`.

### Creating the Web API project (TodoListService)

The code for the TodoListService was created in the following way:

#### Step 1: Create the web api using the ASP.NET Core templates

1. Run the following command to create a sample from the command line using the `SingleOrg` template:

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
     services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
             .AddMicrosoftIdentityWebApi(options =>
    {
        Configuration.Bind("AzureAdB2C", options);

        options.TokenValidationParameters.NameClaimType = "name";
    },
        options => { Configuration.Bind("AzureAdB2C", options); });
  ```

- Add the method **app.UseAuthentication()** before **app.UseMvc()** in the `Configure` method

  ```Csharp
     app.UseAuthentication();
     app.UseMvc();
  ```

### Create the TodoListController.cs file

1. Add a folder named `Models` and then create a new  file named `TodoItem.cs`. Copy the contents of the TodoListClient\Models\TodoItem.cs in this file.
1. Create a new Controller named `TodoListController` and copy and paste the code from the sample (\TodoListService\Controllers\TodoListController.cs) to this controller. TodoListController requires below code:

```csharp
   [Authorize]
   [RequiredScope(scopeRequiredByAPI)]
    public class TodoListController : Controller
    {
        const string scopeRequiredByAPI = "access_as_user" ;
        ...
    }
      
```

The RequiredScopes attribute validates if token contains the scopes required by a web API. Value for scopeRequiredByAPI is the required scope.

## About the code

### Code for the Web App (TodoListClient)

In `Startup.cs`, below lines of code enables Microsoft identity platform endpoint. This endpoint is capable of signing-in users both with their Work and School Accounts.

```csharp
services.AddMicrosoftIdentityWebAppAuthentication(Configuration, "AzureAdB2C")
                    .EnableTokenAcquisitionToCallDownstreamApi(new string[] { Configuration["TodoList:TodoListScope"] })
                    .AddInMemoryTokenCaches();
```

 1. AddMicrosoftIdentityWebAppAuthentication : This enables your application to use the Microsoft identity platform endpoint. This endpoint is capable of signing-in users both with their Work and School and Microsoft Personal accounts.
 1. EnableTokenAcquisitionToCallDownstreamApi : Enables the web app to call the protected API ToDoList Api.
 1. AddInMemoryTokenCaches: Adds an in memory token cache provider, which will cache the Access Tokens acquired for the Web API.

### Code for the Web API (ToDoListService)

In `Startup.cs`, below lines of code protects the web API with Microsoft identity platform.

```Csharp
     services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
             .AddMicrosoftIdentityWebApi(options =>
    {
        Configuration.Bind("AzureAdB2C", options);

        options.TokenValidationParameters.NameClaimType = "name";
    },
        options => { Configuration.Bind("AzureAdB2C", options); });
```

## Deployment

### Deployment to Azure App Services

There are two web projects in this sample. To deploy them to **Azure App Services**, you'll need, for each one, to:

- create an **Azure App Service**
- publish the projects to the **App Services**, and
- update its client(s) to call the web site instead of the local environment.

#### Create and publish the `TodoListService-aspnetcore-webapi` to an Azure Web Site

1. Sign in to the [Azure portal](https://portal.azure.com).
1. Click `Create a resource` in the top left-hand corner, select **Web** --> **Web App**, and give your web site a name, for example, `TodoListService-aspnetcore-webapi-contoso.azurewebsites.net`.
1. Next, select the `Subscription`, `Resource Group`, `App service plan and Location`. `OS` will be **Windows** and `Publish` will be **Code**.
1. Click `Create` and wait for the App Service to be created.
1. Once you get the `Deployment succeeded` notification, then click on `Go to resource` to navigate to the newly created App service.
1. Once the web site is created, locate it in the **Dashboard** and click it to open **App Services** **Overview** screen.
1. From the **Overview** tab of the App Service, download the publish profile by clicking the **Get publish profile** link and save it.  Other deployment mechanisms, such as from **source control**, can also be used.
1. Switch to Visual Studio and go to the TodoListService-aspnetcore-webapi project.  Right click on the project in the Solution Explorer and select **Publish**.  Click **Import Profile** on the bottom bar, and import the publish profile that you downloaded earlier.
1. Click on **Configure** and in the `Connection tab`, update the Destination URL so that it is a `https` in the home page URL, for example [https://TodoListService-aspnetcore-webapi-contoso.azurewebsites.net](https://TodoListService-aspnetcore-webapi-contoso.azurewebsites.net). Click **Next**.
1. On the Settings tab, make sure `Enable Organizational Authentication` is NOT selected.  Click **Save**. Click on **Publish** on the main screen.
1. Visual Studio will publish the project and automatically open a browser to the URL of the project.  If you see the default web page of the project, the publication was successful.

#### Update the Active Directory tenant application registration for `TodoListService-aspnetcore-webapi`

1. Navigate back to the [Azure portal](https://portal.azure.com).
In the left-hand navigation pane, select the **Azure Active Directory** service, and then select **App registrations (Preview)**.
1. In the resulting screen, select the `TodoListService-aspnetcore-webapi` application.
1. From the *Branding* menu, update the **Home page URL**, to the address of your service, for example [https://TodoListService-aspnetcore-webapi-contoso.azurewebsites.net](https://TodoListService-aspnetcore-webapi-contoso.azurewebsites.net). Save the configuration.
1. Add the same URL in the list of values of the *Authentication -> Redirect URIs* menu. If you have multiple redirect URIs, make sure that there a new entry using the App service's Uri for each redirect Uri.

#### Update the `TodoListClient-aspnetcore-webapi` to call the `TodoListService-aspnetcore-webapi` Running in Azure App Services

1. In your IDE, go to the `TodoListClient-aspnetcore-webapi` project.
1. Open `TodoListClient\appsettings.json`.  Only one change is needed - update the `todo:TodoListBaseAddress` key value to be the address of the website you published,
   for example, [https://TodoListService-aspnetcore-webapi-contoso.azurewebsites.net](https://TodoListService-aspnetcore-webapi-contoso.azurewebsites.net).
1. Run the client! If you are trying multiple different client types (for example, .Net, Windows Store, Android, iOS) you can have them all call this one published web API.

#### Create and publish `TodoListClient-aspnetcore-webapi` to an Azure App Services

1. Sign in to the [Azure portal](https://portal.azure.com).
1. Click `Create a resource` in the top left-hand corner, select **Web** --> **Web App**, and give your web site a name, for example, `TodoListClient-aspnetcore-webapi-contoso.azurewebsites.net`.
1. Next, select the `Subscription`, `Resource Group`, `App service plan and Location`. `OS` will be **Windows** and `Publish` will be **Code**.
1. Click `Create` and wait for the App Service to be created.
1. Once you get the `Deployment succeeded` notification, then click on `Go to resource` to navigate to the newly created App service.
1. Once the web site is created, locate it it in the **Dashboard** and click it to open **App Services** **Overview** screen.
1. From the **Overview** tab of the App Service, download the publish profile by clicking the **Get publish profile** link and save it.  Other deployment mechanisms, such as from **source control**, can also be used.
1. Switch to Visual Studio and go to the TodoListClient-aspnetcore-webapi project.  Right click on the project in the Solution Explorer and select **Publish**.  Click **Import Profile** on the bottom bar, and import the publish profile that you downloaded earlier.
1. Click on **Configure** and in the `Connection tab`, update the Destination URL so that it is a `https` in the home page URL, for example [https://TodoListClient-aspnetcore-webapi-contoso.azurewebsites.net](https://TodoListClient-aspnetcore-webapi-contoso.azurewebsites.net). Click **Next**.
1. On the Settings tab, make sure `Enable Organizational Authentication` is NOT selected.  Click **Save**. Click on **Publish** on the main screen.
1. Visual Studio will publish the project and automatically open a browser to the URL of the project.  If you see the default web page of the project, the publication was successful.

#### Update the Active Directory tenant application registration for `TodoListClient-aspnetcore-webapi`

1. Navigate back to the [Azure portal](https://portal.azure.com).
In the left-hand navigation pane, select the **Azure Active Directory** service, and then select **App registrations (Preview)**.
1. In the resulting screen, select the `TodoListClient-aspnetcore-webapi` application.
1. In the **Authentication** page for your application, update the Logout URL fields with the address of your service, for example [https://TodoListClient-aspnetcore-webapi-contoso.azurewebsites.net](https://TodoListClient-aspnetcore-webapi-contoso.azurewebsites.net)
1. From the *Branding* menu, update the **Home page URL**, to the address of your service, for example [https://TodoListClient-aspnetcore-webapi-contoso.azurewebsites.net](https://TodoListClient-aspnetcore-webapi-contoso.azurewebsites.net). Save the configuration.
1. Add the same URL in the list of values of the *Authentication -> Redirect URIs* menu. If you have multiple redirect URLs, make sure that there a new entry using the App service's Uri for each redirect URL.

> :warning: If your app is using an *in-memory* storage, **Azure App Services** will spin down your web site if it is inactive, and any records that your app was keeping will emptied. In addition, if you increase the instance count of your web site, requests will be distributed among the instances. Your app's records, therefore, will not be the same on each instance.

## More information

Learn more about **Microsoft Identity Platform** and **Azure AD B2C**:

- [Microsoft identity platform (Azure Active Directory for developers)](https://docs.microsoft.com/azure/active-directory/develop/)
- [Overview of Microsoft Authentication Library (MSAL)](https://docs.microsoft.com/azure/active-directory/develop/msal-overview)
- [What is Azure Active Directory B2C?](https://docs.microsoft.com/azure/active-directory-b2c/overview)
- [Azure AD B2C User Flows](https://docs.microsoft.com/azure/active-directory-b2c/user-flow-overview)
- [Azure AD B2C Custom Policies](https://docs.microsoft.com/azure/active-directory-b2c/custom-policy-overview)
- [Tutorial: Grant access to an ASP.NET web API using Azure Active Directory B2C](https://docs.microsoft.com/azure/active-directory-b2c/tutorial-web-api-dotnet?tabs=app-reg-ga)

To learn more about the code, visit:

- [Conceptual documentation for MSAL.NET](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki#conceptual-documentation) and in particular:
- [Acquiring tokens with authorization codes on web apps](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/Acquiring-tokens-with-authorization-codes-on-web-apps)
- [Customizing Token cache serialization](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/token-cache-serialization)

## Community Help and Support

Use [Stack Overflow](http://stackoverflow.com/questions/tagged/msal) to get support from the community.
Ask your questions on Stack Overflow first and browse existing issues to see if someone has asked your question before.
Make sure that your questions or comments are tagged with [`msal` `ms-identity` `azure-active-directory` `azure-ad-b2c`].

If you find a bug in the sample, raise the issue on [GitHub Issues](../../issues).

To provide feedback on or suggest features for Azure Active Directory, visit [User Voice page](https://feedback.azure.com/forums/169401-azure-active-directory).

## Contributing

If you'd like to contribute to this sample, see [CONTRIBUTING.MD](../CONTRIBUTING.md).

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information, see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
