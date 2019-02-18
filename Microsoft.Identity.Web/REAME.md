# Microsoft Identity Web

This library contains a set of reusable classes useful in Web Applications and Web APIs (collectively referred to as Web resources) that sign-in users and call Web APIs

The library contains help classes to

- Protect Web resources (in the `Resources` folder)
  - `AadIssuerValidator` is used to validate the issuer in multi-tenant applications, taking into account the aliases for authorities exising in Azure AD. This class works both for Azure AD v1.0 and Microsoft Identity platform v2.0 web resources
  - `OpenIdConnectMiddlewareDiagnostics` helps you understand what happens in the Open Id Connect Middleware.
  - `ClaimsPrincipalExtensions` provides a set of extension methods on `ClaimsPrincipal` helping getting information from the signed-in user

- Acquire a token to call protected APIs (in the `Client` folder)
  -  

- Bootstrap the web resource from the Startup.cs file by just calling a few APIs