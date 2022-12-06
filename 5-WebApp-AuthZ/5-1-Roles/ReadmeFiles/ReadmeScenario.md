## Overview

This sample shows how a .NET Core MVC Web app that uses [OpenID Connect](https://docs.microsoft.com/azure/active-directory/develop/v1-protocols-openid-connect-code) to sign in users and use Azure AD [App Roles](aka.ms/approles) for authorization. [App Roles](aka.ms/approles), along with Security groups are popular means to implement authorization.

This application implements RBAC using Azure AD's App roles & Role Claims feature. Another approach is to use Azure AD Groups and Group Claims, as shown in [WebApp-GroupClaims](../../5-WebApp-AuthZ/5-2-Groups/README.md). Azure AD Groups and [App Roles](aka.ms/approles) are by no means mutually exclusive; they can be used in tandem to provide even finer grained access control.

Using RBAC with App roles and Role Claims, developers can securely enforce authorization policies with minimal effort on their part.

- A Microsoft Identity Platform Office Hours session covered Azure AD App roles and security groups, featuring this scenario and this sample. A recording of the session is is provided in this video [Using Security Groups and App roles in your apps](https://www.youtube.com/watch?v=LRoc-na27l0)

For more information about how the protocols work in this scenario and other scenarios, see [Authentication Scenarios for Azure AD](http://go.microsoft.com/fwlink/?LinkId=394414).

## Scenario

This sample first leverages the ASP.NET Core OpenID Connect middleware to sign in the user. On the home page it displays the various `claims` that the user's [ID Token](https://docs.microsoft.com/azure/active-directory/develop/id-tokens) contained. The ID token is used by the asp.net security middleware to build the [ClaimsPrincipal](https://docs.microsoft.com/dotnet/api/system.security.claims.claimsprincipal), accessible via **HttpContext.User** in the code.

This web application allows users to list all users in their tenant or a list of all the app roles and groups the signed in user is assigned to. The idea is to provide an example of how, within an application, access to certain functionality is restricted to subsets of users based on which role they belong to. The sample also shows how to use the app roles in [Policy-based authorization in ASP.NET Core](https://docs.microsoft.com/aspnet/core/security/authorization/policies).

This type of authorization is implemented using role-based access control (RBAC). When using RBAC, an administrator grants permissions to roles, not to individual users or groups. The administrator can then assign roles to different users and groups to control who has then access to certain content and functionality within the application.  

This sample application defines the following two *App roles*:

- `DirectoryViewers`: Have the ability to view any directory user's roles and security group assignments.
- `UserReaders`: Have the ability to view a list of users in the directory.

These App roles are defined in the [Azure portal](https://portal.azure.com) in the application's registration manifest.  When a user signs into the application, Azure AD emits a `roles` claim for each role that the user has been granted individually to the user in the from of role membership. Assignment of users and groups to roles can be done through the portal's UI, or programmatically using the [Microsoft Graph](https://graph.microsoft.com) and [Azure AD PowerShell](https://docs.microsoft.com/powershell/module/azuread).  In this sample, application role management is done through the Azure portal or using PowerShell.

NOTE: Role claims will not be present for guest users in a tenant if the `https://login.microsoftonline.com/common/` endpoint is used as the authority to sign in users. Azure AD can emit app roles only if the tenanted end point `https://login.microsoftonline.com/{tenant Id}/` is being used.

