
This sample demonstrates how to secure a [multi-tenant](https://docs.microsoft.com/azure/active-directory/develop/howto-convert-app-to-be-multi-tenant) ASP.NET Core MVC web application (TodoListClient) which calls another protected **multi-tenant** ASP.NET Core Web API (ToDoListService) with the Microsoft Identity Platform. This sample builds on the concepts introduced in the [Integrate an app that authenticates users and calls Microsoft Graph using the multi-tenant integration pattern (SaaS)](../../2-WebApp-graph-user/2-3-Multi-Tenant) sample. We advise you go through that sample once before trying this sample.  
  
In this sample, we would protect an ASP.Net Core Web API using the Microsoft Identity Platform. The Web API will be protected using Azure Active Directory [OAuth 2.0 Bearer Authorization](https://docs.microsoft.com/azure/active-directory/develop/v2-oauth2-auth-code-flow). The API will support authenticated users with Work and School accounts. Further on the API will also call a downstream API ([Microsoft Graph](https://aka.ms/graph)) **on behalf of the signed-in user** using the [OAuth 2.0 on-behalf-of flow](https://docs.microsoft.com/azure/active-directory/develop/v2-oauth2-on-behalf-of-flow) to provide additional value to its client apps.

The Web API is marked as a [multi-tenant](https://docs.microsoft.com/azure/active-directory/develop/single-and-multi-tenant-apps) app, so that it can be [provisioned](#provisioning-your-multi-tenant-apps-in-another-azure-ad-tenant-programmatically) into Azure AD tenants where the registered client applications in that tenant can then obtain [Access Tokens](https://docs.microsoft.com/azure/active-directory/develop/access-tokens) for this web API and make calls to it.

> Note that the client applications that want to call this web API do not need to be multi-tenant themselves to be able to do so.

### Overview

This sample presents a client Web application that signs-in users and obtains an Access Token for this protected Web API.

Both applications use the [Microsoft.Identity.Web](https://aka.ms/microsoft-identity-web) and Microsoft Authentication Library [MSAL.NET](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet) to sign-in user and obtain a JWT access token through the [OAuth 2.0](https://docs.microsoft.com/azure/active-directory/develop/active-directory-protocols-oauth-code) protocol.

The client Web App:

1. Signs-in users using the [MSAL.NET](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet) and [Microsoft.Identity.Web](https://github.com/AzureAD/microsoft-identity-web) libraries.
1. Acquires an [Access Token](https://docs.microsoft.com/azure/active-directory/develop/access-tokens) for the protected Web API.
1. Calls the ASP.NET Core Web API by using the access token as a bearer token in the authentication header of the Http request.

The Web API:

1. Authorizes the caller (user) using the [Microsoft.Identity.Web](https://github.com/AzureAD/microsoft-identity-web).
1. Acquires another access token on-behalf-of the signed-in user using the [on-behalf of flow](https://docs.microsoft.com/azure/active-directory/develop/v2-oauth2-on-behalf-of-flow).
1. The Web API then uses this new Access token to call Microsoft Graph.

You can run the sample by using either Visual Studio or command line interface as shown below:

#### Running the sample using Visual Studio

Clean the solution, rebuild the solution, and run it. You might want to go into the solution properties and set both projects as startup projects, with the service project starting first.

When you start the Web API from Visual Studio, depending on the browser you use, you'll get:

- an empty web page (with Microsoft Edge)
- or an error HTTP 401 (with Chrome)

This behavior is expected as the browser is not authenticated. The Web application will be authenticated, so it will be able to access the Web API.

> A recording of a Microsoft Identity Platform developer session that covered this topic of developing a multi-tenant app with Azure Active Directory is available at [Develop multi-tenant applications with Microsoft identity platform](https://www.youtube.com/watch?v=B416AxHoMJ4).