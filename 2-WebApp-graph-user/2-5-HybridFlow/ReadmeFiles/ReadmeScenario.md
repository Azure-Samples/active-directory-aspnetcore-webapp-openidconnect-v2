 This is a ASP.NET Core single page application/web application hybrid sample that calls the Microsoft Graph API using Razor and MSAL.js.

 Use the hybrid-SPA code flow to obtain an Access token for your Web API in he backend and use it in the client SPA [without re-authenticating the user]

 1. The client ASP.NET Core Web App uses the [Microsoft.Identity.Web](https://aka.ms/microsoft-identity-web) to sign-in and obtain a JWT [Access Token](https://docs.microsoft.com/azure/active-directory/develop/access-tokens) from **Azure AD** as well as an additional [Spa Authorization Code](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/SPA-Authorization-Code) to be passed to a client-side single page application.
 1. The [Spa Authorization Code](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/SPA-Authorization-Code) is passed to the client-side application using the session configuration for the application.
 1. The [Spa Authorization Code](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/SPA-Authorization-Code) is exchanged for another [Access Token](https://docs.microsoft.com/azure/active-directory/develop/access-tokens) in the client-side application.
 1. The [Access Token](https://docs.microsoft.com/azure/active-directory/develop/access-tokens) is used by the client-side application as a bearer token to call the **Microsoft Graph API**.
