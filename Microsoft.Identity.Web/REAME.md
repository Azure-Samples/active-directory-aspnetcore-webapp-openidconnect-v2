# Microsoft Identity Web

This library contains a set of reusable classes useful in Web Applications and Web APIs (collectively referred to as Web resources) that sign-in users and call Web APIs

The library contains help classes to

- Protect Web resources (in the `Resources` folder)
  - `AadIssuerValidator` is used to validate the issuer in multi-tenant applications, taking into account the aliases for authorities exising in Azure AD. This class works both for Azure AD v1.0 and Microsoft Identity platform v2.0 web resources
  - `OpenIdConnectMiddlewareDiagnostics` helps you understand what happens in the Open Id Connect Middleware.
  - `ClaimsPrincipalExtensions` provides a set of extension methods on `ClaimsPrincipal` helping getting information from the signed-in user

- Acquire a token to call protected APIs (in the `Client` folder)
  -  `ITokenAcquisition` is a wrapper to MSAL.NET in confidential client applications, enabling you to simply get a token from the controllers, after 
     having populated the cache from the OIDC flow (in the case of Web Apps), or the JWT flow (in the case of Web APIs)
  - Extensions methods allow you to choose the token cache implementation you want to have in your web resource (`AddSessionBasedTokenCache`, or `AddInMemoryTokenCache` for the moment)
  - `MsalUiRequiredExceptionFilterAttribute` allows for incremental consent by declaratively adding the attribute with the required scopes, on a controller action.
  
- Bootstrap the web resource from the Startup.cs file in your web application by just calling a few methods
  - `AddAzureAdV2Authentication` to add authentication with the Microsoft Identity platform (AAD v2.0)
  - `AddMsal` to add support for token acquistion with MSAL. This should be followed by one of the AddXXXTokenCache methods to express the token cache technology to use.

  