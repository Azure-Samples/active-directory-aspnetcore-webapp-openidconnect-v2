// IIFE to acquire token from auth code sent through server.
(function () {
    const code = document.cookie.split(';').reduce((cookies, cookie) => {
        const [name, value] = cookie.split('=').map(c => c.trim());
        cookies[name] = value;
        return cookies;
    }, {})['Microsoft.Identity.Hybrid.Authentication.Cookie'];

    const scopes = ["user.read"];

    if (!!code) {
        msalInstance.acquireTokenByCode({
            code,
            scopes
        });

        // Remove the authcode from cookies
        document.cookie = document.cookie.split(';').map(cookie => {
            const [name, _] = cookie.split('=').map(c => c.trim());
            if (name === 'Microsoft.Identity.Hybrid.Authentication.Cookie') {
                return name + '=; Max-Age=-99999999999';
            }
            return cookie;
        }).join(';');
    }
})();

