# Microsoft Identity Web

This library contains a set of reusable classes useful in Web Applications and Web APIs (collectively referred to as Web resources) that sign-in users and call Web APIs

The library contains helper classes to:

- **Bootstrap the web resource from the Startup.cs file** in your web application by just calling a few methods
  - `AddAzureAdV2Authentication` to add authentication with the Microsoft Identity platform (AAD v2.0), including managing the authority validation, and the sign-out.
  
    ```CSharp
    services.AddAzureAdV2Authentication();
    ```    
    
  - `AddMsal` to add support for token acquistion with MSAL.NET. This should be followed by one of the AddXXXTokenCache methods to express the token cache technology to use
  
      ```CSharp
    services.AddAzureAdV2Authentication() 
            .AddMsal()
            .AddInMemoryTokenCache();
    ```    
  
    ![image](https://user-images.githubusercontent.com/13203188/53899064-a100ab80-4039-11e9-8869-fa9cffcd345a.png)
  
- Protect Web resources (in the `Resources` folder)
  - `AadIssuerValidator` is used to validate the issuer in multi-tenant applications, taking into account the aliases for authorities exising in Azure AD. This class works both for Azure AD v1.0 and Microsoft Identity platform v2.0 web resources. You should not need to use it directly, as it's used by `AddAzureAdV2Authentication`
  - `OpenIdConnectMiddlewareDiagnostics` helps you understand what happens in the Open Id Connect Middleware. This is a diagnostics class that can help you troubleshooting your Web apps.
  - `ClaimsPrincipalExtensions` provides a set of extension methods on `ClaimsPrincipal` helping getting information from the signed-in user. It's used in the other classes of the libraries.

- **Acquire a token to call protected APIs** (in the `Client` folder)
  -  `ITokenAcquisition` is an interface implemented by a wrapper to MSAL.NET in confidential client applications, enabling you to simply get a token from the controllers, after adding them to the cache from OpenIDConnect events (in Web Apps), or JwtBearerMiddleware events (in the case of Web APIs)
  - Extensions methods allow you to choose the token cache implementation you want to have in your web resource (`AddSessionBasedTokenCache`, or `AddInMemoryTokenCache` for the moment)
  - `MsalUiRequiredExceptionFilterAttribute` allows for incremental consent by declaratively adding the attribute with the required scopes, on a controller action.
  
## Learn more:
You can learn more about the tokens by looking at the following articles in MSAL.NET's conceptual documentation:

- The [Authorization code flow](https://aka.ms/msal-net-authorization-code), which is used, after the user signed-in with Open ID Connect, in order to get a token and cache it for a later use. See [TokenAcquisition L 107](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/blob/f99e913cc032e16c59b748241111e97108e87918/Extensions/TokenAcquisition.cs#L107) for details of this code
- [AcquireTokenSilent](https://aka.ms/msal-net-acquiretokensilent ), which is used by the controller to get an access token for the downstream API. See [TokenAcquisition L 168](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/blob/f99e913cc032e16c59b748241111e97108e87918/Extensions/TokenAcquisition.cs#L168) for details of this code
- [Token cache serialization](msal-net-token-cache-serialization)


The token validation is performed by the classes of the [Identity Model Extensions for DotNet](https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet) library. Learn about customizing
token validation by reading:

- [Validating Tokens](https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet/wiki/ValidatingTokens) in that library's conceptual documentation
- [TokenValidationParameters](https://docs.microsoft.com/en-us/dotnet/api/microsoft.identitymodel.tokens.tokenvalidationparameters?view=azure-dotnet)'s reference documentation.
