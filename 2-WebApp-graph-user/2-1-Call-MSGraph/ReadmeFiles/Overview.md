[![Build status](https://identitydivision.visualstudio.com/IDDP/_apis/build/status/AAD%20Samples/.NET%20client%20samples/ASP.NET%20Core%20Web%20App%20tutorial)](https://identitydivision.visualstudio.com/IDDP/_build/latest?definitionId=819)

## Overview

This sample demonstrates an ASP.NET Core web app that calls the Microsoft Graph API for a signed-in user.

## Scenario

1. The ASP.NET Core client web app uses the [Microsoft.Identity.Web](https://aka.ms/microsoft-identity-web) to sign a user in, and obtain a JWT [access Tokens](https://aka.ms/access-tokens) from **Azure AD**.
1. The access token is used by the client app as a bearer token to call Microsoft Graph.

![Sign in with the Microsoft identity platform and call Graph](ReadmeFiles/sign-in.png)