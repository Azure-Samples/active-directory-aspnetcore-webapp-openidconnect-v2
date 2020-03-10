---
services: active-directory
platforms: dotnet
author: jennyf19
level: 200
client: ASP.NET Core Web App
service: ASP.NET Core Web API
endpoint: Microsoft identity platform
---

# How to secure a Web API built with ASP.NET Core using the Azure AD B2C

[![Build status](https://identitydivision.visualstudio.com/IDDP/_apis/build/status/AAD%20Samples/.NET%20client%20samples/ASP.NET%20Core%20Web%20App%20tutorial)](https://identitydivision.visualstudio.com/IDDP/_build/latest?definitionId=819)

> The sample in this folder is part of a multi-chapter tutorial. The first phase is available at [An ASP.NET Core Web app signing-in users with the Microsoft identity platform in your organization](../1-WebApp-OIDC/1-1-MyOrg).
>
> This article (README.md) contains the full instructions on how to configure the sample. If you have gone through the [first chapter](../1-WebApp-OIDC/1-1-MyOrg) and already configured the client web application to sigh-in users, read through the [README-incremental-instructions.md](README-incremental-instructions.md) instead.

## About this sample

This sample is essentially a guide for developers who want to secure their Web APIs using the Microsoft identity platform (formerly Azure Active Directory for developers) using Azure AD B2C. This sample lays down the all the steps developers need to take to secure their web api with the Microsoft identity platform. Additionally it also explains the steps and processes for a client to obtain the necessary permissions and tokens to make successful calls to this secured web api.  

### Scenario

This sample has a web api and a client web app, both built using the asp.net core platform. The client app signs in users using the [OpenID Connect protocol](https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-protocols-oidc) flow and in this process obtains (and caches) an [access token](https://docs.microsoft.com/en-us/azure/active-directory/develop/access-tokens) for the web api. The client app has a ToDo list that the web app users can work with. This ToDo list is maintained in an in-memory list on the Web API. The client app calls the webApi for all operations on the ToDo list.

### Overview

This sample presents a Web API running on ASP.NET Core, protected by [Azure AD OAuth Bearer](https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-v2-protocols) Authentication. The client application uses [MSAL.NET](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet) library to obtain a JWT access token through using the [OAuth 2.0](https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-auth-code-flow) protocol flow.

The client web application essentially takes the following steps to sign-in the user and obtain a bearer token for the Web API:

1. Signs-in the user with local or social identities. When the user signs-in for the first time , a consent screen is presented. This consent screen lets the user consent for the application to access the web API( TodoListService).
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

### Pre-requisites

- Install .NET Core for Windows by following the instructions at [dot.net/core](https://dot.net/core), which will include [Visual Studio 2017](https://aka.ms/vsdownload).
- An Internet connection
- An Azure Active Directory (Azure AD) tenant. For more information on how to get an Azure AD tenant, see [How to get an Azure AD tenant](https://azure.microsoft.com/en-us/documentation/articles/active-directory-howto-tenant/)
- A user account in your Azure AD tenant.

### Step 1:  Clone or download this repository

From your shell or command line:

```Shell
git clone https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2.git

cd "4-WebApp-your-API\4-2-B2C"
```

or download and exact the repository .zip file.

> Given that the name of the sample is pretty long, and so are the name of the referenced NuGet packages, you might want to clone it in a folder close to the root of your hard drive, to avoid file size limitations on Windows.

### Step 2:  Register the sample application with your Azure Active B2C Directory tenant


## Option 1 - Run the pre-configured sample

1. Build the solution and run it.
1. Open your web browser and make a request to the app. Accept the IIS Express SSL certificate if needed. Click on **SignIn/Up** button.
1. Click on Sign-In

## Option 2 - Configure the sample with your own B2C app

### Step 2: Get your own Azure AD B2C tenant

If you don't have an Azure AD B2C tenant yet, you'll need to create an Azure AD B2C tenant by following the [Tutorial: Create an Azure Active Directory B2C tenant](https://azure.microsoft.com/documentation/articles/active-directory-b2c-get-started).

### Step 3: Create your own user flow (policy)

This sample uses a unified sign-up/sign-in user flow (policy). Create this policy by following [these instructions on creating an AAD B2C tenant](https://azure.microsoft.com/documentation/articles/active-directory-b2c-reference-policies). You may choose to include as many or as few identity providers as you wish, but make sure **DisplayName** is checked in `User attributes` and `Application claims`.

If you already have an existing unified sign-up/sign-in user flow (policy) in your Azure AD B2C tenant, feel free to re-use it. The is no need to create a new one just for this sample.

Copy this policy name, so you can use it in step 5.

### Step 4: Create your own Web app

Now you need to [register your web app in your B2C tenant](https://docs.microsoft.com/azure/active-directory-b2c/active-directory-b2c-app-registration#register-a-web-application), so that it has its own Application ID.

Your web application registration should include the following information:

- Enable the **Web App/Web API** setting for your application.
- Set the **Reply URL** to `https://localhost:44316/signin-oidc`.
- Copy the Application ID generated for your application, so you can use it in the next step.

### Step 5: Configure the sample with your app coordinates

1. Open the solution in Visual Studio.
1. Open the `appsettings.json` file.
1. Find the assignment for `Instance` and replace the value with your tenant name. For example, `https://fabrikam.b2clogin.com`
1. Find the assignment for `Domain` and replace the value with your Azure AD B2C domain name. For example, `fabrikam.onmicrosoft.com`
1. Find the assignment for `ClientID` and replace the value with the Application ID from Step 4.
1. Find the assignment for `SignUpSignInPolicyId` and replace with the name of the `Sign up and sign in` policy you created in Step 3.

```JSon
{
  "AzureAdB2C": {
    "Instance": "https://<your-tenant-name>.b2clogin.com",
    "ClientId": "<web-app-application-id>",
    "Domain": "<your-b2c-domain>",
    "CallbackPath": "/signin/B2C_1_sign_up_in",
    "SignedOutCallbackPath": "/signout/B2C_1_sign_up_in",
    "SignUpSignInPolicyId": "<your-sign-up-in-policy>"
  }
}
```

### Step 6: Run the sample

1. Build the solution and run it.
1. Open your web browser and make a request to the app. Accept the IIS Express SSL certificate if needed. Click on **SignIn/Up** button.
1. If you don't have an account registered on the **Azure AD B2C** used in this sample, follow the sign up process. Otherwise, input the email and password for your account and click on **Sign in**.

When you start the Web API from Visual Studio, depending on the browser you use, you'll get:

- an empty web page (case with Microsoft Edge)
- or an error HTTP 401 (case with Chrome)

This behavior is expected as you are not authenticated. The client application will be authenticated, so it will be able to access the Web API.

Explore the sample by signing in into the TodoList client, adding items to the To Do list. If you stop the application without signing out, the next time you run the application, you won't be prompted to sign in again.

NOTE: Remember, the To-Do list is stored in memory in this `TodoListService` app. Each time you run the projects, your To-Do list will get emptied.

> Did the sample not work for you as expected? Did you encounter issues trying this sample? Then please reach out to us using the [GitHub Issues](../../../issues) page.

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
     services.AddSignIn(Configuration)
          .AddWebAppCallsProtectedWebApi(new string[] { Configuration["TodoList:TodoListScope"] })
          .AddInMemoryTokenCaches();
     ```

     This enables your application to use the Microsoft identity platform endpoint. This endpoint is capable of signing-in users both with their Work and School and Microsoft Personal accounts.

    1. Change the `Properties\launchSettings.json` file to ensure that you start your web app from <https://localhost:44321> as registered. For this:
    - update the `sslPort` of the `iisSettings` section to be `44321`
    - in the `applicationUrl` property of use `https://localhost:44321`

  - Then add the following code to inject the ToDoList service implementation in the client

   ```CSharp
     // Add APIs
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
1. Update the `configureServices` method in `startup.cs` to add the MSAL library and a token cache.

    ```CSharp
     services.AddSignIn(Configuration)
          .AddWebAppCallsProtectedWebApi(new string[] { Configuration["TodoList:TodoListScope"] })
          .AddInMemoryTokenCaches();
    ```
1. Update the `Configure` method to include **app.UseAuthentication();** before **app.UseMvc();**  

  ```Csharp
     app.UseAuthentication();
     app.UseMvc();
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
    services.AddProtectedWebApi(Configuration)
         .AddInMemoryTokenCaches();
  ```
- Add the method **app.UseAuthentication()** before **app.UseMvc()** in the `Configure` method

  ```Csharp
     app.UseAuthentication();
     app.UseMvc();
  ```

  `AddProtectedWebApi` does the following:
  - add the **Jwt**BearerAuthenticationScheme (Note the replacement of **BearerAuthenticationScheme** by **Jwt**BearerAuthenticationScheme)
  - set the authority to be the Microsoft identity platform identity
  - sets the audiences to validate
  - register an issuer validator that accepts issuers to be in the Microsoft identity platform clouds.

The implementations of these classes are in the `Microsoft.Identity.Web` library (and folder), and they are designed to be reusable in your applications (Web apps and Web apis). You are encouraged to browse the code in the library to understand the changes in detail.

### Create the TodoListController.cs file

1. Add a folder named `Models` and then create a new  file named `TodoItem.cs`. Copy the contents of the TodoListClient\Models\TodoItem.cs in this file.
1. Create a new Controller named `TodoListController` and copy and paste the code from the sample (\TodoListService\Controllers\TodoListController.cs) to this controller.

## How to deploy this sample to Azure

This project has two WebApp / Web API projects. To deploy them to Azure Web Sites, you'll need, for each one, to:

- create an Azure Web Site
- publish the Web App / Web APIs to the web site, and
- update its client(s) to call the web site instead of IIS Express.

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
1. Once the web site is created, locate it it in the **Dashboard** and click it to open **App Services** **Overview** screen.
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

## Next steps

If you're interested in the Web API calling a downstream API, you might want to have a look at the [ASP.NET Core Web API tutorial](https://github.com/Azure-Samples/active-directory-dotnet-native-aspnetcore-v2), in chapter 2 [2. Web API now calls Microsoft Graph/](https://github.com/Azure-Samples/active-directory-dotnet-native-aspnetcore-v2/tree/master/2.%20Web%20API%20now%20calls%20Microsoft%20Graph). The client is a desktop app there, whereas you have a Web App, but apart from that all the app registration steps apply.

## Community Help and Support

Use [Stack Overflow](http://stackoverflow.com/questions/tagged/msal) to get support from the community.
Ask your questions on Stack Overflow first and browse existing issues to see if someone has asked your question before.
Make sure that your questions or comments are tagged with [`msal` `dotnet` `azure-active-directory`].

If you find a bug in the sample, please raise the issue on [GitHub Issues](../../issues).

To provide a recommendation, visit the following [User Voice page](https://feedback.azure.com/forums/169401-azure-active-directory).

## Contributing

If you'd like to contribute to this sample, see [CONTRIBUTING.MD](../CONTRIBUTING.md).

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information, see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## More information

For more information, visit the following links:

- Articles about the Microsoft identity platform are at [http://aka.ms/aaddevv2](http://aka.ms/aaddevv2), with a focus on:
  - [Azure AD OAuth Bearer protocol](https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-v2-protocols)
  - [The OAuth 2.0 protocol in Azure AD](https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-auth-code-flow)
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
  - [Azure Active Directory with ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/azure-active-directory/?view=aspnetcore-2.1)
