 This sample demonstrates an ASP.NET Core client Web App calling an ASP.NET Core Web API that is secured using Azure AD.

 1. The client ASP.NET Core Web App uses the [Microsoft.Identity.Web](https://aka.ms/microsoft-identity-web) to sign-in and obtain a JWT [Access Tokens](https://docs.microsoft.com/azure/active-directory/develop/access-tokens) from **Azure AD**.
 1. The [Access Tokens](https://docs.microsoft.com/azure/active-directory/develop/access-tokens) is used as a bearer token to authorize the user to call the ASP.NET Core Web API protected by **Azure AD**.
