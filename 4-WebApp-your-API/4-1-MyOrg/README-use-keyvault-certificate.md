# How to use certificates instead of secrets in your client applications

[Microsoft.Identity.Web](https://github.com/AzureAD/microsoft-identity-web/wiki/Certificates) provides various ways for a developer to use a certificate instead of a client secret to authenticate their apps with Azure AD.

## Using Client certificate with KeyVault

This sample was configured to use a client secret, but have an option to use a certificate instead.

### To be able to use a certificate, please make the following changes:

1. Open Client/appsettings.json file
2. **Comment out** the next line:

```json
"ClientSecret": "[Copy the client secret added to the app from the Azure portal]"
```

3. **Un-comment** the following lines:

```json
"ClientCertificates": [
  {
    "SourceType": "KeyVault",
    "KeyVaultUrl": "[Enter URL for you KeyVault]",
    "KeyVaultCertificateName": "TodoListClient-aspnetcore-webapi"
  }
]
```

4. While inside '4-1-MyOrg' folder, open a Powershell terminal

5. Set next execution policy

```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process -Force
```

6. Run the cleanup script to delete old App Registration for the sample

```powershell
AppCreationScripts-withCert/Cleanup.ps1
```

7. Run the AppCreationScripts-withCert/Configure.ps1 script to re-create the App Registration. The script will also create a [application name].pfx file that will be **manually** uploaded into Key Vault. When asked about a password, remember it - you will need the password when uploading the certificate.

```powershell
AppCreationScripts-withCert/Configuration.ps1
```
8. To use KeyVault, sign in to the [Azure portal](https://portal.azure.com) and [create an Azure Key Vault](https://docs.microsoft.com/azure/key-vault/general/quick-create-portal)
9. Inside Client/appsettings.json file - update "KeyVaultUrl" key to have URL of your Key Vault, like https://[your Key Vault name here].vault.azure.net
10. [Upload](https://docs.microsoft.com/azure/key-vault/certificates/tutorial-import-certificate#import-a-certificate-to-key-vault) the generated AppCreationScripts-withCert\.PFX file into the Key Vault
11.  Run the sample as indicated in [README.md](README.md)

## Using local Client certificate

1. Open Client/appsettings.json file
2. **Comment out** the next line:

```json
"ClientSecret": "[Copy the client secret added to the app from the Azure portal]"
```

3. **Un-comment** the following lines:

```json
"ClientCertificates": [
  {
   "SourceType": "StoreWithDistinguishedName",
   "CertificateStorePath": "CurrentUser/My",
   "CertificateDistinguishedName": "CN=TodoListClient-aspnetcore-webapi"
  }
]
```

4. Go through steps 4-6 of **"Using client certificate with KeyVault"** instructions
5. Run the AppCreationScripts-withCert/Configure.ps1 script to re-create the App Registration.
6. Run the sample as indicated in [README.md](README.md)

## More information

### About Azure KeyVault

Cloud applications and services use cryptographic keys and secrets to help keep information secure. [Azure KeyVault](https://azure.microsoft.com/services/key-vault/) safeguards these keys and secrets. When you use Key Vault, you can encrypt authentication keys, storage account keys, data encryption keys, .pfx files, and passwords by using keys that are protected by hardware security modules (HSMs).

### About Managed Identities for Azure Resources

[Azure KeyVault](https://azure.microsoft.com/services/key-vault/#product-overview)

[Managed Identities for Azure Resources](https://docs.microsoft.com/azure/active-directory/managed-identities-azure-resources/)

[Managed Identities for Azure App Services](https://docs.microsoft.com/azure/app-service/overview-managed-identity?tabs=dotnet)
