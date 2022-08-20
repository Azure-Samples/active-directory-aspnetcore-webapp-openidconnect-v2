# How to use certificates instead of secrets in your client applications

We recommend you familiarize yourself with [Using certificates with Microsoft\.Identity\.Web](https://github.com/AzureAD/microsoft-identity-web/wiki/Certificates#getting-certificates-from-key-vault) as it provides various ways for a developer to use a certificate instead of a client secret to authenticate their apps with Azure AD.
> Note: Please carefully go through [Getting certificates from Key Vault](https://github.com/AzureAD/microsoft-identity-web/wiki/Certificates#getting-certificates-from-key-vault) when deploying your app to production.

## Using Client certificate with KeyVault

This sample was configured to use a client secret, but have an option to use a certificate instead.

### To be able to use a certificate, please make the following changes:

1. Open Client/appsettings.json file
1. **Comment out** the next line:

```json
"ClientSecret": "[Copy the client secret added to the app from the Azure portal]"
```

1. **Un-comment** the following lines:

```json
"ClientCertificates": [
  {
    "SourceType": "KeyVault",
    "KeyVaultUrl": "[Enter URL for you KeyVault]",
    "KeyVaultCertificateName": "TodoListClient-aspnetcore-webapi"
  }
]
```

1. While inside '4-1-MyOrg' folder, open a Powershell terminal

1. Set next execution policy

```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process -Force
```

1. Run the Cleanup.ps1 script to delete any existing old App Registration for the sample

```powershell
AppCreationScripts-withCert/Cleanup.ps1
```

1. Run the AppCreationScripts-withCert/Configure.ps1 script to re-create the App Registration. The script will also create a [application name].pfx file that will be **manually** uploaded into Key Vault. When asked about a password, remember it - you will need the password when uploading the certificate.

```powershell
AppCreationScripts-withCert/Configure.ps1
```

1. To use KeyVault, sign in to the [Azure portal](https://portal.azure.com) and [create an Azure Key Vault](https://docs.microsoft.com/azure/key-vault/general/quick-create-portal)
1. Inside Client/appsettings.json file - update "KeyVaultUrl" key to have URL of your Key Vault, like https://[your Key Vault name here].vault.azure.net
1. [Upload](https://docs.microsoft.com/azure/key-vault/certificates/tutorial-import-certificate#import-a-certificate-to-key-vault) the generated AppCreationScripts-withCert\.PFX file into the Key Vault
1. Run the sample as indicated in [README.md](README.md)
1. Use the account you used to upload the certificate to key vault to sign-into the web app.
1. In production environments, you'd give access to your deployed web app or Virtual machine to read this certificate's Key Vault entry.  

## Using a local Client certificate

1. Open Client/appsettings.json file
2. **Comment out** the next line:

```json
"ClientSecret": "[Copy the client secret added to the app from the Azure portal]"
```

1. **Un-comment** the following lines:

```json
"ClientCertificates": [
  {
   "SourceType": "StoreWithDistinguishedName",
   "CertificateStorePath": "CurrentUser/My",
   "CertificateDistinguishedName": "CN=TodoListClient-aspnetcore-webapi"
  }
]
```

1. While inside '4-1-MyOrg' folder, open a Powershell terminal

1. Set next execution policy

```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process -Force
```

1. Run the *Cleanup.ps1* script to delete any existing old App Registration for the sample

```powershell
AppCreationScripts-withCert/Cleanup.ps1 -TenantId "[Optional] - your tenant id" -AzureEnvironmentName "[Optional] - Azure environment, defaults to 'Global'"
```

1. Run the AppCreationScripts-withCert/Configure.ps1 script to re-create the App Registration. The script will also create a [application name].pfx file that will be **manually** uploaded into Key Vault. When asked about a password, remember it - you will need the password when uploading the certificate.

```powershell
AppCreationScripts-withCert/Configure.ps1 -TenantId "[Optional] - your tenant id" -AzureEnvironmentName "[Optional] - Azure environment, defaults to 'Global'"
```

1. Run the sample as indicated in [README.md](README.md)

## More information

### Using Azure KeyVault

Cloud applications and services use cryptographic keys and secrets to help keep information secure. [Azure KeyVault](https://azure.microsoft.com/services/key-vault/) safeguards these keys and secrets. When you use Key Vault, you can encrypt authentication keys, storage account keys, data encryption keys, .pfx files, and passwords by using keys that are protected by hardware security modules (HSMs).

### About Managed Identities for Azure Resources

[Azure KeyVault](https://azure.microsoft.com/services/key-vault/#product-overview)

[Managed Identities for Azure Resources](https://docs.microsoft.com/azure/active-directory/managed-identities-azure-resources/)

[Managed Identities for Azure App Services](https://docs.microsoft.com/azure/app-service/overview-managed-identity?tabs=dotnet)
