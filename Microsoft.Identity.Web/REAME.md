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

## Learn more:
You can learn more about the tokens by looking at the following articles in MSAL.NET's conceptual documentation:

- The [Authorization code flow](https://aka.ms/msal-net-authorization-code), which is used, after the user signed-in with Open ID Connect, in order to get a token and cache it for a later use. See [TokenAcquisition L 107](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/blob/f99e913cc032e16c59b748241111e97108e87918/Extensions/TokenAcquisition.cs#L107) for details of this code
- [AcquireTokenSilent](https://aka.ms/msal-net-acquiretokensilent ), which is used by the controller to get an access token for the downstream API. See [TokenAcquisition L 168](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/blob/f99e913cc032e16c59b748241111e97108e87918/Extensions/TokenAcquisition.cs#L168) for details of this code
- [Token cache serialization](msal-net-token-cache-serialization)

  
