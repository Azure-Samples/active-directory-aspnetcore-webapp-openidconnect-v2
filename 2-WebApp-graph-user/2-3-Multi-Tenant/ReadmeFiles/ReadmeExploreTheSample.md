## Explore the sample

<details>
 <summary>Expand the section</summary>

Clean the solution, rebuild the solution, and run it.
The sample implements two distinct tasks: the onboarding of a new tenant and a basic ToDo List CRUD operation.

Ideally, you would want to have two Azure AD tenants so you can test the multi-tenant aspect of this sample. For more information on how to get an Azure AD tenant, see [How to get an Azure AD tenant](https://azure.microsoft.com/documentation/articles/active-directory-howto-tenant/).

#### Signing-in

Users can only sign-in if their tenant had been "onboarded" first. The sample will guide them how to do so, but it requires a **tenant admin account** to complete the onboarding process. Once the admin has consented and provisioned the app in their tenant, all users from their tenant will be able to sign-in.

If you try to onboard without an admin account, you will be presented with the following screen. Please switch to an admin account to complete this step:

![Admin Consent](ReadmeFiles/admin-approval.png)

If you try to sign-in with a tenant that hasn't been "onboarded" yet, you will land on the following page. Please click on **Take me to the onboarding process** button and follow the instructions to get your tenant registered in the sample database:

![Unauthorized Tenant](ReadmeFiles/unauthorized-tenant.png)

> :warning: If you had onboarded your tenant using this sample in the past and now getting the **AADSTS650051** error when onboarding again, please refer to the [Error AADSTS650051](#error-aadsts650051) section below to mitigate this error.

#### ToDo List

Users from one tenant can't see the **ToDo** items of users from other tenants. They will be able to perform basic CRUD operations on ToDo items assigned to them. When editing a ToDo item, users can assign it to any other user from their tenant. The list of users in a tenant is fetched from Microsoft Graph, using the [Graph SDK](https://github.com/microsoftgraph/msgraph-sdk-dotnet).

The list of users will be presented in the **Assigned To** dropdown:

![todo Edit](ReadmeFiles/todo-edit.png)

> Did the sample not work for you as expected? Did you encounter issues trying this sample? Then please reach out to us using the [GitHub Issues](../../../../issues) page.

> [Consider taking a moment to share your experience with us.](https://forms.office.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbRz0h_jLR5HNJlvkZAewyoWxUNEFCQ0FSMFlPQTJURkJZMTRZWVJRNkdRMC4u)

</details>
