 This sample demonstrates an ASP.NET Core client Web App calling an ASP.NET Core Web API that is secured using Azure AD.

 1. The client ASP.NET Core Web App uses the [Microsoft.Identity.Web](https://aka.ms/microsoft-identity-web) to sign-in a user and obtain a JWT [Id Token](https://docs.microsoft.com/azure/active-directory/develop/id-tokens) from **Azure AD**.
 2. The service again uses the the [Microsoft.Identity.Web](https://aka.ms/microsoft-identity-web) to protect the Web api, and validate tokens.
