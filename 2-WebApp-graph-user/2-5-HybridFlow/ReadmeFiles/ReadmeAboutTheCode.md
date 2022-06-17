### Code in Asp.net core web app

The entire application is built on [ASP.NET Core](https://docs.microsoft.com/aspnet/core/introduction-to-aspnet-core) using [Razor](https://docs.microsoft.com/aspnet/web-pages/overview/getting-started/introducing-razor-syntax-c) pages. The web app is then secured using the [Microsoft Identity Web](https://docs.microsoft.com/azure/active-directory/develop/microsoft-identity-web) library.

When configuring the application in the `appsettings.json` file we'll need to set the `WithSpaAuthCode` property of the `AzureAd` object to `true`. This makes it possible for Azure to respond with not only an *access token* for users but also an *access code* for that user which can be sent to the retrieved by the SPA for an access token. This will be discussed in more detail later.

```json
{
  "AzureAd": {
    // ...
    "WithSpaAuthCode": true
  },
  // ...
}
```

Within the `Program.cs` file you will see the [WebApplicationBuilder](https://docs.microsoft.com/dotnet/api/microsoft.aspnetcore.builder.webapplicationbuilder) **builder** which injects all dependencies into your application.

The first thing that needs to be done is to configure a `Session` for your application which will contain the *authorization code* your SPA will exchange for an access token.

```CSharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSession(options =>
 {
   options.Cookie.IsEssential = true;
 });
```

Next, the initial scopes are extracted from the `appsettings.json` file from within the `DownstreamApi` object. These scopes will be used in the access token stored within a cache within your server and also be contained within the token which will be retrieved by the SPA after it exchanges its *access code*. The server-side token cache will be cleared out for users after they sign-out.

```CSharp
var initialScopes = builder.Configuration.GetSection("DownstreamApi:Scopes")
    .Value
    .Split(' ');

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration)
    .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
    .AddDistributedTokenCaches();
```

### Client-side MSAL.js Client

The single page application in this sample is run using the [MSAL.js library](https://github.com/AzureAD/microsoft-authentication-library-for-js).

The razor page generating the client will read the settings within the `appsettings.json` file and configure the application for you.

```JavaScript
const msalInstance = new msal.PublicClientApplication({
    auth: {
    @{
    var clientId = Configuration["AzureAd:ClientId"];
    var instance = Configuration["AzureAd:Instance"];
    var domain = Configuration["AzureAd:Domain"];
    var redirectUri = Configuration["SpaRedirectUri"];

    @Html.Raw($"clientId: '{clientId}',");
    @Html.Raw($"redirectUri: '{redirectUri}',");
    @Html.Raw($"authority: '{instance}{domain}',");
    @Html.Raw($"postLogoutRedirectUri: '{redirectUri}',");
}

    },
cache: {
    cacheLocation: 'sessionStorage',
        storeAuthStateInCookie: false,
    }
});
```

### Client-side Authorization Code Redemption

Because this app is configured to make a simple web application using the `AddMicrosoftIdentityWebApp` this makes it possible to associate sessions with each login instance. The authorization code intended for redemption by the client side application is automatically passed into the client side through the `SpaAuthCode` session property as the server redeems an authorization code for itself.

This **authorization code** is extracted by the `_Layout.cshtml` razor pag and exchanged for an **authentication token** using the **MSAL.js** client
which is then cached in the application and the `SpaAuthCode` value is removed from the session.

After the either a token is redeemed for the user from Azure or a token is found withing the cache of the [PublicClientApplication](https://azuread.github.io/microsoft-authentication-library-for-js/ref/classes/_azure_msal_browser.publicclientapplication.html) an event is triggered named `AUTHENTICATED` which alerts other parts of the application a token is available to make requests with.

```JavaScript
(function () {
    const scopes = 
    @{
        var apiScopes = Configuration["DownstreamApi:Scopes"].Split(' ');
        @Html.Raw("[");

        foreach(var scope in apiScopes) {
            @Html.Raw($"'{scope}',")
        }

        @Html.Raw("];");

        Context.Session.TryGetValue(Constants.SpaAuthCode, out var spaCode);

        if (spaCode is not null && !string.IsNullOrEmpty(Encoding.Default.GetString(spaCode)))
        {
            @Html.Raw($"const code = '{Encoding.Default.GetString(spaCode)}';");
            Context.Session.Remove(Constants.SpaAuthCode);
        }
        else
        {
            @Html.Raw($"const code = '';");
        }
    }

    if (!!code) {
        msalInstance
            .handleRedirectPromise()
            .then(result => {
                if (result) {
                    console.log('MSAL: Returning from login');
                    document.dispatchEvent(new Event('AUTHENTICATED'));
                    return result;
                }

                const timeLabel = "Time for acquireTokenByCode";
                console.time(timeLabel);
                console.log('MSAL: acquireTokenByCode hybrid parameters present');

                return msalInstance.acquireTokenByCode({
                    code,
                    scopes,
                })
                    .then(result => {
                        console.timeEnd(timeLabel);
                        console.log('MSAL: acquireTokenByCode completed successfully', result);
                        document.dispatchEvent(new Event('AUTHENTICATED'));
                    })
                    .catch(error => {
                        console.timeEnd(timeLabel);
                        console.error('MSAL: acquireTokenByCode failed', error);
                        if (error instanceof msal.InteractionRequiredAuthError) {
                            console.log('MSAL: acquireTokenByCode failed, redirecting');

                            @{
                                if (User.Identity is not null)
                                {
                                    @Html.Raw($"const username = '{User.Identity.Name}';");
                                }
                                else
                                {
                                    @Html.Raw($"const username = '';");
                                }
                            }

                            const account = msalInstance.getAllAccounts()
                                .find(account => account.username === username);

                            return msalInstance.acquireTokenRedirect({
                                account,
                                scopes
                            });
                        }
                    });
            })
    }
    else {
        document.dispatchEvent(new Event('AUTHENTICATED'));
    }
})();
```

### Client-side Graph API requests

In order to make a successful call to Microsoft Graph you first need to get an access token from the `PublicClientApplication` cache and then use that access token within the `Authorization` header along with the `Bearer` key word.

The function `getTokenFromCache` defined within `_Layout.cshtml` does just that.

```JavaScript
function getTokenFromCache(scopes) {
    @if (User.Identity is not null)
    {
        @Html.Raw($"const username = '{User.Identity.Name}';");
    }
    else
    {
        @Html.Raw($"const username = '';");
    }

    const account = msalInstance.getAllAccounts()
        .find(account => account.username === username);

    return msalInstance.acquireTokenSilent({
        account,
        scopes
    })
        .catch(error => {
            if (error instanceof msal.InteractionRequiredAuthError) {
                return msalInstance.acquireTokenRedirect({
                    account,
                    scopes
                });
            }
        });
};
```

Actual requests made to the Graph API are handled by the `callMSGraph` function in the `_Layout.cshtml` file.

```JavaScript
function callMSGraph(path, token) {
    @{
        var graphEndpoint = Configuration["DownstreamApi:BaseUrl"];
        if (!string.IsNullOrEmpty(graphEndpoint))
        {
            @Html.Raw($"const graphEndpoint = '{graphEndpoint}';");
        }
        else
        {
            @Html.Raw($"const graphEndpoint = '';");
        }
    }

    const headers = new Headers();
    const bearer = `Bearer ${token}`;
    headers.append("Authorization", bearer);

    const options = {
        method: "GET",
        headers
    };

    console.log('request made to Graph API at: ' + new Date().toString());

    return fetch(`${graphEndpoint}${path}`, options)
        .catch(error => console.log(error))
}
```