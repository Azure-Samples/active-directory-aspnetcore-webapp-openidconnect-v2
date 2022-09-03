---
page_type: sample
services: ms-identity
client: ASP.NET Core Web App
service: Azure REST Api
level: 200
languages:
 - aspnetcore
 - charp
products:
 - azure-active-directory
 - microsoft-identity-web
 - azure-resource-manager
 - azure-resource-graph
 - ms-graph
platform: aspnetcore
endpoint: Microsoft identity platform
urlFragment: active-directory-aspnetcore-webapp-openidconnect-v2
name: An ASP.NET Core Web App which sign-in users with work and school or Microsoft personal accounts and calls Azure REST API and Azure Storage
description: An ASP.NET Core Web App which sign-in users with work and school or Microsoft personal accounts and calls Azure REST API and Azure Storage
---

# An ASP.NET Core Web App which sign-in users with work and school or Microsoft personal accounts and calls Azure REST API and Azure Storage

[![Build status](https://identitydivision.visualstudio.com/IDDP/_apis/build/status/AAD%20Samples/.NET%20client%20samples/ASP.NET%20Core%20Web%20App%20tutorial)](https://identitydivision.visualstudio.com/IDDP/_build/latest?definitionId=819)

## Scenario

This sample shows how to update your ASP.NET Core Web API so that it now calls other Microsoft APIs than Microsoft Graph. The sample now calls the Azure Resource Manager API
as well as the Azure Storage

![Sign in and call ARM and Azure Storage](ReadmeFiles/graph-arm-storage.svg)

## How to run this sample

To run this sample:

> Pre-requisites:
>
> This is the third phase of the tutorial. It's recommended that you have gone through the previous phases of the tutorial, in particular how the [WebApp signs-in users with Microsoft Identity (OIDC) / with work and school or personal accounts](../1-WebApp-OIDC/1-3-AnyOrgOrPersonal) and  [Web app calls the Microsoft Graph API on behalf of a user signing-in](../2-WebApp-graph-user/2-1-Call-MSGraph).
>
> This chapter shows the incremental changes required to call two Microsoft APIs other than Microsoft Graph (Azure Resource Management and Azure Storage).

### Step 1: Register the sample with your Azure AD tenant

You first need to [register](../1-WebApp-OIDC/1-3-AnyOrgOrPersonal/README.md#step-1-register-the-sample-with-your-azure-ad-tenant/README.md) your app as described in [the first phase of the tutorial](../1-WebApp-OIDC)

Then, the follow the following extra set of steps:

1. In the **API permissions** section for the application, notice that a delegated permission is set by default to Microsoft Graph for the scope **User.Read**
1. Select **Add a permission**
   - In the **Microsoft APIs** tab, select **Azure Service Management**
   - Check **user_impersonation**
   - Select **Add permissions**
1. Select **Add a permission**
   - In the **Microsoft APIs** tab, select **Azure Storage**. If you cannot find it in this category, try in the **APIs my organization uses** tab
   - Check **user_impersonation**
   - Select **Add permissions**

For the Azure Storage preparation see [Authenticate with Azure Active Directory from an application for access to blobs and queues](https://docs.microsoft.com/en-us/azure/storage/common/storage-auth-aad-app)

### Step 2: Download/Clone/Go to the folder containing the sample code and build the application

If you have not already,  clone this sample from your shell or command line:

```shell
git clone https://github.com/Azure-Samples/microsoft-identity-platform-aspnetcore-webapp-tutorial webapp
cd webapp
cd "3-WebApp-multi-APIs"
```

In the `appsettings.json` file, replace, if you have not already:

- the `ClientID` value with the *Application ID* from the application you registered in Application Registration portal,
- the `TenantId` by `common`, as here you chose to sign-in users with their work or school or personal account. In case you want to sign-in different audiences, refer back to the first phase of the tutorial
- and the `ClientSecret` by the client secret you generated in Step 1.

In the `HomeController.cs` file, replace the blob container `Uri` (line 77) with the `Uri` of a blob container in your storage account. Also make sure that your user has enough permissions to create blobs in this container (see comment above).

### Step 3: Run the sample

1. Build the solution and run it.

2. Open your web browser and make a request to the app. The app immediately attempts to authenticate you via the Microsoft identity platform endpoint. Sign in with your personal account or with a work or school account.

3. Go to the Contacts page, you should now see all kind of information about yourself (a call was made to the Microsoft Graph *me* endpoint)

> Did the sample not work for you as expected? Did you encounter issues trying this sample? Then please reach out to us using the [GitHub Issues](../../../../issues) page.

> [Consider taking a moment to share your experience with us.](https://forms.office.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbRz0h_jLR5HNJlvkZAewyoWxUNEFCQ0FSMFlPQTJURkJZMTRZWVJRNkdRMC4u)

## About The code

Starting from the [previous phase of the tutorial](../2-WebApp-graph-user/2-1-Call-MSGraph), the code was incrementally updated with the following steps:

### Update the `Startup.cs` file to enable TokenAcquisition by a MSAL.NET based service

After the following lines in the ConfigureServices(IServiceCollection services) method, after `services.AddMicrosoftWebAppAuthentication(Configuration);`, add `services.AddHttpClient<IArmOperations, ArmApiOperationService>();`:

```CSharp
 public void ConfigureServices(IServiceCollection services)
{
    . . .
    services.AddMicrosoftIdentityWebAppAuthentication(Configuration)
                    .EnableTokenAcquisitionToCallDownstreamApi( new string[] { Constants.ScopeUserRead })
                    .AddInMemoryTokenCaches();
    services.AddHttpClient<IArmOperations, ArmApiOperationService>();
```

This enables to add the Azure Resource manager micro-service to use the HttpClient by dependency injection.

### Add the `Services\ARM` sub folder

The `Services\ARM` sub folder which is a simple wrapper against the ARM REST API.

### Add the `Tenants.cshtml` view

In the `Views\Home` folder add a view named `Tenants.cshtml`

```html
@using System
@using System.Collections.Generic
@{
    ViewData["Title"] = "Tenants";
    IDictionary<string, string> tenants = ViewData["tenants"] as IDictionary<string, string>;
    if (tenants == null)
    {
        tenants = (ViewData["tenants"] as IEnumerable<string>).ToDictionary(name => name);
    }
}
<h2>@ViewData["Title"]</h2>
<h3>@ViewData["Message"]</h3>

<table class="table table-striped table-condensed" style="font-family: monospace">
    <tr>
        <th>Tenant ID</th>
        <th>Tenant name</th>
    </tr>

    @foreach(var tenant in tenants)
    {
    <tr>
        <td>@tenant.Key</td>
        <td>@tenant.Value</td>
    </tr>
    }
</table>
```

### Add methods in the HomeController to call ARM, and Azure storage

```CSharp
  // Requires that the app has added the Azure Service Management / user_impersonation scope, and that
  // the admin tenant does not require admin consent for ARM.
  [AuthorizeForScopes(Scopes = new[] { "https://management.core.windows.net/user_impersonation"})]
  public async Task<IActionResult> Tenants()
  {
      var accessToken =
          await tokenAcquisition.GetAccessTokenOnBehalfOfUser(HttpContext,
             new[] { $"{ArmApiOperationService.ArmResource}user_impersonation" });

      var tenantIds = await armOperations.EnumerateTenantsIdsAccessibleByUser(accessToken);
      ViewData["tenants"] = tenantIds;

      return View();
  }


  [AuthorizeForScopes(Scopes = new[] { "https://storage.azure.com/user_impersonation" })]
  public async Task<IActionResult> Blob()
  {
      var scopes = new string[] { "https://storage.azure.com/user_impersonation" };

      var accessToken =
          await tokenAcquisition.GetAccessTokenOnBehalfOfUser(HttpContext, scopes);

      // create a blob on behalf of the user
      TokenCredential tokenCredential = new TokenCredential(accessToken);
      StorageCredentials storageCredentials = new StorageCredentials(tokenCredential);

      // replace the URL below with your storage account URL
      Uri blobUri = new Uri("https://blobstorageazuread.blob.core.windows.net/sample-container/Blob1.txt");
      CloudBlockBlob blob = new CloudBlockBlob(blobUri, storageCredentials);
      await blob.UploadTextAsync("Blob created by Azure AD authenticated user.");

      ViewData["Message"] = "Blob successfully created";
      return View();
  }
```

### Add new buttons in the menu bar to call the new actions

In the `Views\Shared\_Layout.cshtml` file

after:

```html
 <li><a asp-area="" asp-controller="Home" asp-action="Profile">Profile</a></li>
```

insert
```html
  <li><a asp-area="" asp-controller="Home" asp-action="Tenants">Tenants</a></li>
  <li><a asp-area="" asp-controller="Home" asp-action="Blob">Blob</a></li>
```

### Using implicit authentication (ArmApiOperationsServiceWithImplicitAuth)
When calling an API, instead of explicitly giving the token in each call you can use a delegating handler on your HTTP client to automatically inject the access token.

In `Startup.cs` after `services.AddHttpClient<IArmOperations, ArmApiOperationService>();` add `.services.AddHttpClient<IArmOperationsWithImplicitAuth, ArmApiOperationServiceWithImplicitAuth>()...`:

```CSharp
public void ConfigureServices(IServiceCollection services)
{
    . . .
    services.AddMicrosoftIdentityWebAppAuthentication(Configuration)
        .EnableTokenAcquisitionToCallDownstreamApi( new string[] { Constants.ScopeUserRead })
        .AddInMemoryTokenCaches();
    services.AddHttpClient<IArmOperations, ArmApiOperationService>();
    services.AddHttpClient<IArmOperationsWithImplicitAuth, ArmApiOperationServiceWithImplicitAuth>()
        .AddMicrosoftIdentityUserAuthenticationHandler(
            "arm", 
            options => options.Scopes = $"{ArmApiOperationService.ArmResource}user_impersonation");
```

In `HomeController.cs` add `TenantsWithImplicitAuth`:
```CSharp
    // Requires that the app has added the Azure Service Management / user_impersonation scope, and that
    // the admin tenant does not require admin consent for ARM.
    [AuthorizeForScopes(Scopes = new[] { "https://management.core.windows.net/user_impersonation" })]
    public async Task<IActionResult> TenantsWithImplicitAuth()
    {
        var tenantIds = await armOperationsWithImplicitAuth.EnumerateTenantsIds();
        ViewData["tenants"] = tenantIds;

        return View(nameof(Tenants));
    }
```

## Troubleshooting

To access Azure Resource Management (ARM), you'll need a work or school account (AAD account) and an Azure subscription. If your Azure subscription is for a Microsoft personal account, just create a new user in your directory, and use this user to run the sample

OpenIdConnectProtocolException: Message contains error: 'invalid_client', error_description: 'AADSTS650052: The app needs access to a service (\"https://*.blob.core.windows.net\") that your organization \"*tenantname*.onmicrosoft.com\" has not subscribed to or enabled. Contact your IT Admin to review the configuration of your service subscriptions.
this is because the AzureStorage API was not registered as an API used by your Web App

## Learn more

You can learn more about the tokens by looking at the following articles in MSAL.NET's conceptual documentation:

- The [Authorization code flow](https://aka.ms/msal-net-authorization-code), which is used, after the user signed-in with Open ID Connect, in order to get a token and cache it for a later use. See [TokenAcquisition L 107](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/blob/f99e913cc032e16c59b748241111e97108e87918/Extensions/TokenAcquisition.cs#L107) for details of this code
- [AcquireTokenSilent](https://aka.ms/msal-net-acquiretokensilent ), which is used by the controller to get an access token for the downstream API. See [TokenAcquisition L 168](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/blob/f99e913cc032e16c59b748241111e97108e87918/Extensions/TokenAcquisition.cs#L168) for details of this code
- [Token cache serialization](msal-net-token-cache-serialization)
