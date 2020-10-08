# Signing out

[![Build status](https://identitydivision.visualstudio.com/IDDP/_apis/build/status/AAD%20Samples/.NET%20client%20samples/ASP.NET%20Core%20Web%20App%20tutorial)](https://identitydivision.visualstudio.com/IDDP/_build/latest?definitionId=819)

This page explains how sign-out works

## What sign out involves

Signing out from a Web app is about more than removing the information about the signed-in account from the Web App's state.
The Web app must also redirect the user to the Microsoft identity platform `logout` endpoint to sign out. When your web app redirects the user to the `logout` endpoint, this endpoint clears the user's session from the browser. If your app did not go to the `logout` endpoint, the user would reauthenticate to your app without entering their credentials again, because they would have a valid single sign-in session with the Microsoft identity platform endpoint.

To learn more, see the [Send a sign-out request](https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-protocols-oidc#send-a-sign-out-request) paragraph in the [Microsoft identity platform and the OpenID Connect protocol](https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-protocols-oidc) conceptual documentation

## Application registration

During the application registration, you will have registered a post logout URI. In our tutorial, you registered `https://localhost:44321/signout-oidc` in the **Logout URL** field of the **Advanced Settings** section in the **Authentication** page. For details see, [
Register the webApp app](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/tree/master/1-WebApp-OIDC/1-1-MyOrg#register-the-webapp-app-webapp)

## About code

### Signout button

The sign out button is exposed in `Views\Shared\_LoginPartial.cshtml` and only displayed when there's an authenticated account (that is when the user has previously signed in).

```html
@using Microsoft.Identity.Web
@if (User.Identity.IsAuthenticated)
{
    <ul class="nav navbar-nav navbar-right">
        <li class="navbar-text">Hello @User.GetDisplayName()!</li>
        <li><a asp-area="MicrosoftIdentity" asp-controller="Account" asp-action="SignOut">Sign out</a></li>
    </ul>
}
else
{
    <ul class="nav navbar-nav navbar-right">
        <li><a asp-area="MicrosoftIdentity" asp-controller="Account" asp-action="SignIn">Sign in</a></li>
    </ul>
}
```

### `Signout()` action of the `AccountController`

Pressing the **Sign out** button on the web app, triggers the `SignOut` action on the `Account` controller. The code for the `AccountController` is available at [Microsoft.Identity.Web repository]
(https://github.com/AzureAD/microsoft-identity-web/blob/master/src/Microsoft.Identity.Web.UI/Areas/MicrosoftIdentity/Controllers/AccountController.cs), and what it does is:

- set an openid redirect URI to `/Account/SignedOut` so that the controller is called back when Azure AD has performed the sign out
- call `Signout()`, which lets the OpenId connect middleware contact the Microsoft identity platform `logout` endpoint which:

  - clears the session cookie from the browser,
  - and finally calls back the **logout URL**, which, by default, displays the signed out view page [SignedOut.html](https://github.com/aspnet/AspNetCore/blob/master/src/Azure/AzureAD/Authentication.AzureAD.UI/src/Areas/AzureAD/Pages/Account/SignedOut.cshtml) also provided as part of ASP.NET Core.

### Intercepting the call to the logout endpoint

The ASP.NET Core OpenIdConnect middleware enables your app to intercept the call to the Microsoft identity platform logout endpoint by providing an OpenIdConnect event named `OnRedirectToIdentityProviderForSignOut`.

```CSharp
services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    options.Events.OnRedirectToIdentityProviderForSignOut = async context =>
    {
        //Your logic here
    };
});
```

### Clearing the token cache

Your application can also intercept the logout event, for instance to clear the entry of the token cache associated with the account that signed out. We'll see in the second part of this tutorial (about the Web app calling a Web API), that the web app will store access tokens for the user in a cache. Intercepting the logout callback enables your web application to remove the user from the token cache. This is illustrated in the `AddMicrosoftWebAppCallsWebApi()` method of [WebAppServiceCollectionExtensions.cs](https://github.com/AzureAD/microsoft-identity-web/blob/master/src/Microsoft.Identity.Web/WebAppServiceCollectionExtensions.cs#L202-L208)

### Single Sign-Out

The **Logout Url** that you have registered for your application enables you to implement single sign-out. Indeed, the Microsoft identity platform logout endpoint will call the **Logout Url** registered with your application. This call happens whether or not the sign-out was initiated from your web app, or from another web app or the browser. For more information, see [Single sign-out](https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-protocols-oidc#single-sign-out) in the conceptual documentation.

In our tutorial, you registered `https://localhost:44321/signout-oidc` as the **Logout Url** but you haven't created the `signout-oidc` endpoint. This endpoint is actually implemented by ASP.NET Core so there is no need to create it, however, if you want to intercept it you should use `OnRemoteSignOut` event:

```CSharp
options.Events.OnRemoteSignOut = async context =>
{
    //Intercepting the signout-oidc endpoint
};
```

> [Consider taking a moment to share your experience with us.](https://forms.office.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbRz0h_jLR5HNJlvkZAewyoWxUNEFCQ0FSMFlPQTJURkJZMTRZWVJRNkdRMC4u)