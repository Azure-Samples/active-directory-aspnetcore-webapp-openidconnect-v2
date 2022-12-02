## Explore the sample

<details>
 <summary>Expand the section</summary>

1. Open your web browser and make a request to the app. The app immediately attempts to authenticate you via the Microsoft identity platform endpoint. Sign in using an user account in that tenant.

![First time Consent](ReadmeFiles/Sign-in-Consent.png)

2. On the home page, the app lists the various claims it obtained from your [ID token](https://docs.microsoft.com/azure/active-directory/develop/id-tokens). You'd notice a claim named `roles`. There will be one `roles` claim for each app role the signed-in use is assigned to.
3. There also are two links provided on the home page under the **Try one of the following Azure App Role driven operations** heading. These links will result in an access denied error if the signed-in user is not present in the expected role. Sign-out and sign-in with a user account with the correct role assignment to view the contents of these pages. When you click on the page that fetches the signed-in user's roles and group assignments, the sample will attempt to obtain consent from you for the **User.Read** permission using [incremental consent](https://docs.microsoft.com/azure/active-directory/develop/azure-ad-endpoint-comparison#incremental-and-dynamic-consent).
4. When a user was not added to any of the required groups (**UserReader** and/or **DirectoryViewers**), clicking on the links will result in `Access Denied` message on the page. Also looking into `Claims from signed-in user's token` don't have **roles** claim.
5. Add an user to at lease one of the groups. You will be able to get corresponding information when clicking links on main page. Also you can see a **roles** claim printed as part of claims on main page.

> You can use `AppCreationScripts/CreateUsersAndAssignRoles.ps1` and `AppCreationScripts/CleanupUsersAndAssignRoles.ps1` to create and remove 2 users correspondingly. The scripts also will assign roles required by the sample.

> Did the sample not work for you as expected? Did you encounter issues trying this sample? Then please reach out to us using the [GitHub Issues](../../../../issues) page.

> [Consider taking a moment to share your experience with us.](https://forms.office.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbRz0h_jLR5HNJlvkZAewyoWxUNEFCQ0FSMFlPQTJURkJZMTRZWVJRNkdRMC4u)

</details>
