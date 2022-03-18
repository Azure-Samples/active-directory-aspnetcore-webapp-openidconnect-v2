const msalInstance = new msal.PublicClientApplication({
    auth: {
        clientId: '[YOUR CLIENT ID]',
        redirectUri: 'https://localhost:7089/',
        authority: 'https://login.microsoftonline.com/[YOUR DOMAIN OR TENANT ID HERE]',
        postLogoutRedirectUri: 'https://localhost:7089/',
    },
    cache: {
        cacheLocation: 'sessionStorage',
        storeAuthStateInCookie: false,
    }
});

async function signOut() {
    msalInstance.logoutRedirect({
        onRedirectNavigate: _ => undefined
    });

    fetch('/logout', {
        method: 'POST',
        mode: 'same-origin',
        credentials: 'same-origin',
    });
}
