## Deployment

### Deploying web app to Azure App Services

There is one web app in this sample. To deploy it to **Azure App Services**, you'll need to:

- create an **Azure App Service**
- publish the projects to the **App Services**, and
- update its client(s) to call the website instead of the local environment.

#### Publish your files

##### Publish using Visual Studio

Follow the link to [Publish with Visual Studio](https://docs.microsoft.com/visualstudio/deployment/quickstart-deploy-to-azure).

##### Publish using Visual Studio Code

1. Open an instance of Visual Studio code set to the `WebApp-OpenIDConnect-DotNet-graph-v2` project folder.
1. Install the VS Code extension [Azure App Service](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azureappservice).
1. Using the extension you just installed, sign in to **Azure App Service** using your Azure AD account.
1. Choose `Terminal > New Terminal` from the VS Code menu to open a new terminal window in the project directory.
1. Run the following command

    ```console
    dotnet publish WebApp-OpenIDConnect-DotNet-graph.csproj --configuration Release
    ```

1. A `publish` folder is created within the following folder: `bin/Release/netcoreapp3.1/`.
1. From the VS Code file explorer, right-click on the **publish** folder and select **Deploy to Web App**.
1. Select **Create New Web App**.
1. Enter a unique name for the app, for example, `WebApp-OpenIDConnect-DotNet-graph-v2`. If you chose `example-domain` for your app name, your app's domain name will be `https://example-domain.azurewebsites.net`.
1. Select **Windows** as the OS. Press Enter.
1. Select **.NET Core 3.1 (LTS)** as runtime stack.
1. Select `Free` or any other option for your pricing tier.

#### Update the Azure AD app registration (WebApp-OpenIDConnect-DotNet-graph-v2)

1. Navigate back to to the [Azure portal](https://portal.azure.com).
1. Go to the **Azure Active Directory** section, and then select **App registrations**.
1. In the resulting screen, select the `WebApp-OpenIDConnect-DotNet-graph-v2` application.
1. In the app's registration screen, select **Authentication** in the menu.
   - In the **Redirect URIs** section, update both of the reply URLs to match the site URL of your Azure deployment. Using the following examples as a guide, **replace** the text `example-domain` with the app name you created while deploying, for example:
   - `https://example-domain.azurewebsites.net/`
   - `https://example-domain.azurewebsites.net/signin-oidc`
1. Update the **Front-channel logout URL** fields with the address of your service, for example `https://example-domain.azurewebsites.net`.

> :warning: If your app is using *in-memory* storage, **Azure App Services** will spin down your web site if it is inactive, and any records that your app was keeping will emptied. In addition, if you increase the instance count of your website, requests will be distributed among the instances. Your app's records, therefore, will not be the same on each instance.

### Enabling your code to get secrets from Key Vault using Managed Identity

One of the uber principals of security and **Zero Trust** is to place credentials out of your code and use in a manner that allows for credentials to be replaced or rotated without incurring a downtime.

To achieve this we'd place our application's credentials in [Azure Key Vault](https://azure.microsoft.com/services/key-vault/) and access it via [managed Identities for Azure resources](https://docs.microsoft.com/azure/active-directory/managed-identities-azure-resources/overview).

We will follow the steps broadly outlined in the guide: [Use Key Vault from App Service with Azure Managed Identity](https://github.com/Azure-Samples/app-service-msi-keyvault-dotnet/blob/master/README.md)

#### Set up your Managed Identity

1. Navigate to [Azure portal](https://portal.azure.com) and select the **Azure App Service**.
1. Find and select the App Service you've created previously.
1. On App Service portal, select **Identity**.
1. Within the **System assigned** tab, switch **Status** to **On**. Click **Save**.
1. Record the **Object Id** that will appear, as you will need it in the next step.

For more information, see [Add a system-assigned identity](https://docs.microsoft.com/azure/app-service/overview-managed-identity?tabs=dotnet#add-a-system-assigned-identity)

#### Set up your Key vault

Before starting here, make sure:

- You have an [Azure Subscription](https://azure.microsoft.com/free/).
- You have a working and deployed application as an Azure App Service following the steps listed at [Deploying web app to Azure App Services](#deploying-web-app-to-azure-app-services) above.
- Follow the guide to [create an Azure Key Vault](https://docs.microsoft.com/azure/key-vault/general/quick-create-portal).

##### Upload your secret to KeyVault

1. Navigate to your new key vault in the Azure portal.
1. On the Key Vault settings pages, select **Secrets**.
1. Click on **Generate/Import**.
1. On the **Create a secret** screen choose the following values:
    - **Upload options**: Manual.
    - **Name**: Type a name for the secret. The secret name must be unique within a Key Vault. For example, `myClientSecret`
    - **Value**: Copy and paste the value for the `ClientSecret` property (without quotes!) from your `appsettings.json` file.
    - Leave the other values to their defaults. Click **Create**.

##### Provide the managed identity access to Key Vault

1. Navigate to your Key Vault in the portal.
1. Select **Overview** > **Access policies**.
1. Click on **Add Access Policy**.
1. In the input box for **Secret permissions**, select **Get**.
1. Click on **Select Principal**, add the **system-assigned managed identity** that you have created in the [steps before](#set-up-your-managed-identity). You can use the **Object Id** you have recorded previously to search for it.
1. Click on **OK** to add the new Access Policy, then click **Save** to save the Access Policy.

#### Modify your code to connect to Key Vault

1. In the `appsettings.json` file, find and delete the `ClientSecret` property and its value.
1. In the `Properties\launchSettings.json` file, find the string `ENTER_YOUR_KEY_VAULT_URI` and replace it with the URI of your Key Vault, for example: `https://example-vault.vault.azure.net/`
1. Add the `Azure.Identity` NuGet package to the solution. This sample project has already added this package.
1. Add the following directives to your `startup.cs`.
    </p>
    :information_source: this has been already added in the sample project.

```CSharp
using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
```

5. In your `Startup.cs` file, you must create a `GetSecretFromKeyVault` method. This method sets up the Azure Key Vault client and returns the secret that is required.
    </p>
    :information_source: this has already been added in the sample project.

```CSharp
    private string GetSecretFromKeyVault(string tenantId, string secretName)
  {
      // this should point to your vault's URI, like https://<yourkeyvault>.vault.azure.net/
      string uri = Environment.GetEnvironmentVariable("KEY_VAULT_URI");
      DefaultAzureCredentialOptions options = new DefaultAzureCredentialOptions();

      // Specify the tenant ID to use the dev credentials when running the app locally
      options.VisualStudioTenantId = tenantId;
      options.SharedTokenCacheTenantId = tenantId;
      SecretClient client = new SecretClient(new Uri(uri), new DefaultAzureCredential(options));

      // The secret name, for example if the full url to the secret is https://<yourkeyvault>.vault.azure.net/secrets/Graph-App-Secret
        Response<KeyVaultSecret> secret = client.GetSecretAsync(secretName).Result;

      return secret.Value.Value;
  }
```

6. In your `Startup.cs` file, find the `ConfigureServices` method. Add the following code to call the GetSecretFromKeyVault method, right after `services.AddAuthentication`.
    </p>
    :information_source: In the sample project, this code is present but commented out by default. Uncomment it.
    </p>
    :warning: Replace the string `ENTER_YOUR_SECRET_NAME_HERE` with the name of the client secret you entered into Azure Key Vault, for example `myClientSecret`.

```CSharp
    // uncomment the following 3 lines to get ClientSecret from KeyVault
  string tenantId = Configuration.GetValue<string>("AzureAd:TenantId");
  services.Configure<MicrosoftIdentityOptions>(
        options => { options.ClientSecret = GetSecretFromKeyVault(tenantId, "ENTER_YOUR_SECRET_NAME_HERE"); });
```

7. Your `ConfigureServices` method should now look like the following snippet:

```CSharp
       public void ConfigureServices(IServiceCollection services)
        {
            string[] initialScopes = Configuration.GetValue<string>("DownstreamApi:Scopes")?.Split(' ');

            services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(Configuration)
                .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
                .AddMicrosoftGraph(Configuration.GetSection("DownstreamApi"))
                .AddInMemoryTokenCaches();

        // uncomment the following 3 lines to get ClientSecret from KeyVault
            string tenantId = Configuration.GetValue<string>("AzureAd:TenantId");
            services.Configure<MicrosoftIdentityOptions>(
            options => { options.ClientSecret = GetSecretFromKeyVault(tenantId, "myClientSecret"); });

      // ... more method code continues below
    }
```

8. Add an environment variable to your App Service so your web app can find its key vault.

    1. Go to the [Azure portal](https://portal.azure.com). Search for and select **App Service**, and then select your app.
    1. Select **Configuration** blade on the left, then select **New Application Settings**.
    1. Add a new variable, naming it **KEY_VAULT_URI**. Populate the value with the URI of your key vault, for example: `https://example-vault.vault.azure.net/`

1. Re-deploy your project to Azure App Service.

    1. Run the following command:

    ```console
    dotnet publish WebApp-OpenIDConnect-DotNet-graph.csproj --configuration Release
    ```

    1. Then, from the VS Code file explorer, right-click on the **bin/Release/netcoreapp3.1/publish** folder and select **Deploy to Web App**. If you are prompted to select an app, select one you created during this sample.

1. The deployment status is available from the output window. Within a few minutes you'll be able to visit your now-secure app and sign in.

## Optional - Handle Continuous Access Evaluation (CAE) challenge from Microsoft Graph

Continuous access evaluation (CAE) enables web APIs to do just-in time token validation, for instance enforcing user session revocation in the case of password change/reset but there are other benefits. For details, see [Continuous access evaluation](https://docs.microsoft.com/azure/active-directory/conditional-access/concept-continuous-access-evaluation).

Microsoft Graph is now CAE-enabled in Preview. This means that it can ask its clients for more claims when conditional access policies require it. Your can enable your application to be ready to consume CAE-enabled APIs by:

1. Declaring that the client app is capable of handling claims challenges from the web API.
2. Processing these challenges when thrown.

### Declare the CAE capability in the configuration

This sample declares that it's CAE-capable by adding a `ClientCapabilities` property in the configuration, whose value is `[ "cp1" ]`.

```Json
{
  "AzureAd": {
    // ...
    // the following is required to handle Continuous Access Evaluation challenges
    "ClientCapabilities": [ "cp1" ],
    // ...
  },
  // ...
}
```
