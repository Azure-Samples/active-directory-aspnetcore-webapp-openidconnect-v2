<!--
IF SUFFICIENT INFORMATION IS NOT PROVIDED VIA THE FOLLOWING TEMPLATE THE ISSUE MIGHT BE CLOSED WITHOUT FURTHER CONSIDERATION OR INVESTIGATION
-->
> Please provide us with the following information:
> ---------------------------------------------------------------

### This issue is for a: (mark with an `x`)
```
- [ ] bug report -> please search issues before submitting
- [ ] feature request
- [ ] documentation issue or request
- [ ] regression (a behavior that used to work and stopped in a new release)
```

### The issue was found for the following scenario:

Please add an 'x' for the scenario(s) where you found an issue

1. Web app that signs in users
   1. [ ] with a work and school account in your organization: [1-WebApp-OIDC/1-1-MyOrg](../blob/master/1-WebApp-OIDC/1-1-MyOrg)
   1. [ ] with any work and school account: [/1-WebApp-OIDC/1-2-AnyOrg](../blob/master/1-WebApp-OIDC/1-2-AnyOrg)
   1. [ ] with any work or school account or Microsoft personal account: [1-WebApp-OIDC/1-3-AnyOrgOrPersonal](../blob/master/1-WebApp-OIDC/1-3-AnyOrgOrPersonal)
   1. [ ] with users in National or sovereign clouds [1-WebApp-OIDC/1-4-Sovereign](../blob/master/1-WebApp-OIDC/1-4-Sovereign)
   1. [ ] with B2C users [1-WebApp-OIDC/1-5-B2C](../blob/master/1-WebApp-OIDC/1-5-B2C)
1. Web app that calls Microsoft Graph
   1. [ ] Calling graph with the Microsoft Graph SDK: [2-WebApp-graph-user/2-1-Call-MSGraph](../blob/master/2-WebApp-graph-user/2-1-Call-MSGraph)
   1. [ ] With specific token caches: [2-WebApp-graph-user/2-2-TokenCache](../blob/master/2-WebApp-graph-user/2-2-TokenCache)
   1. [ ] Calling Microsoft Graph in national clouds: [2-WebApp-graph-user/2-4-Sovereign-Call-MSGraph](../blob/master/2-WebApp-graph-user/2-4-Sovereign-Call-MSGraph)
1. [ ] Web app calling several APIs [3-WebApp-multi-APIs](../blob/master/3-WebApp-multi-APIs)
1. [ ] Web app calling your own Web API
   1. [ ] with a work and school account in your organization: [4-WebApp-your-API/4-1-MyOrg](../blob/master/4-WebApp-your-API/4-1-MyOrg)
   1. [ ] with B2C users: [4-WebApp-your-API/4-2-B2C](../blob/master/4-WebApp-your-API/4-2-B2C)
   1. [ ] with any work and school account: [4-WebApp-your-API/4-3-AnyOrg](../blob/master/4-WebApp-your-API/4-3-AnyOrg)
1. Web app restricting users
   1. [ ] by Roles: [5-WebApp-AuthZ/5-1-Roles](../blob/master/5-WebApp-AuthZ/5-1-Roles)
   1. [ ] by Groups: [5-WebApp-AuthZ/5-2-Groups](../blob/master/5-WebApp-AuthZ/5-2-Groups)
1. [ ] Deployment to Azure
1. [ ] Other (please describe)

### Repro-ing the issue

**Repro steps**

<!-- the minimal steps to reproduce -->

**Expected behavior**
<!-- A clear and concise description of what you expected to happen (or code).-->

**Actual behavior**
<!-- A clear and concise description of what happens, e.g. exception is thrown, UI freezes -->

**Possible Solution**
<!--- Only if you have suggestions on a fix for the bug -->

**Additional context/ Error codes / Screenshots**

### Any log messages given by the failure
Add any other context about the problem here, such as logs. 

- You can enable Middleware diagnostics by uncommenting the following [lines](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/blob/418e4880ce3307befb25c7af600a886560cadcaa/Microsoft.Identity.Web/StartupHelpers.cs#L81-L83)
- You can enable personally identifiable information in your exceptions to get more information in the open id connect middleware see [Seeing [PII is hidden] in log messages](https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet/wiki/PII) 
- Logging for MSAL.NET is described at [Loggin in MSAL.NET](https://docs.microsoft.com/en-us/azure/active-directory/develop/msal-logging#logging-in-msalnet)

### OS and Version?
> Windows 7, 8 or 10. Linux (which distribution). macOS (Yosemite? El Capitan? Sierra?)

### Versions
> of ASP.NET Core, of MSAL.NET

### Attempting to troubleshooting yourself:

- did you go through the README.md in the folder where you found the issue?
- did you go through the documentation:
  - [Scenario: Web app that signs in users](https://docs.microsoft.com/en-us/azure/active-directory/develop/scenario-web-app-sign-user-overview)
  - [Scenario: Web app that calls web APIs](https://docs.microsoft.com/en-us/azure/active-directory/develop/scenario-web-app-call-api-overview)
  
### Mention any other details that might be useful

> ---------------------------------------------------------------
> Thanks! We'll be in touch soon.
