async function signOut() {
    msalInstance.logoutRedirect();

    fetch('/logout', {
        method: 'POST',
        mode: 'same-origin',
        credentials: 'same-origin',
    });
}
