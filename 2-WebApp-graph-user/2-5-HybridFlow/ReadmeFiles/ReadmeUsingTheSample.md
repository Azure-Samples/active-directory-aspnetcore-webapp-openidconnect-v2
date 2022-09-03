 Open your web browser and navigate to `https://localhost:7089`. You should see the application home page with a link for **Home**, **Privacy** and **Sign in**. Click the **Sign in** link and you'll be redirected to the Microsoft login page. Sign in using an user account in your tenant. After you sign in the client side `MSAL.js` client will receive an **auth** code from the server that will be exchanged for an authorization token and cached immediately in the browser.

1. Click on `Profile` tab to see the signed in user's information and contacts. This information is retrieved using the `MSAL.js` library in the browser.

Did the sample not work for you as expected? Did you encounter issues trying this sample? Then please reach out to us using the [GitHub Issues](../../../../issues) page.

[Consider taking a moment to share your experience with us.](https://forms.office.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbRz0h_jLR5HNJlvkZAewyoWxUNEFCQ0FSMFlPQTJURkJZMTRZWVJRNkdRMC4u)

