# Web app using client certificates with Microsoft.Identity.Web

There are different approaches for [using certificates with Microsoft.Identity.Web](https://github.com/AzureAD/microsoft-identity-web/wiki/Certificates).
This document will talk about the simplest option - use of Azure KeyVault and a certificate.

## Using Client certificate with KeyVault
This sample was configured to use a client secret, but have an option to use a certificate instead. Currently this is AAD's most secure and recommended practice.

### To be able to use the option you will need to make a number of changes:
1. Open Client/appsettings.json file
2. **Comment out** the next line:
```json
"ClientSecret": "[Copy the client secret added to the app from the Azure portal]"
```
3. **Un-comment** next lines:
```json
"ClientCertificates": [
  {
    "SourceType": "KeyVault",
    "KeyVaultUrl": "[Enter URL for you KeyVault]",
    "KeyVaultCertificateName": "[Enter name of the certificate]"
  }
]
```
4. Set exection policy
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process -Force
```
5. Run the cleanup script to delete old App Registration for the sample
```powershell
AppCreationScripts-withCert/Cleanup.ps1 -TenantId "your tenant id"
```
6. Run the configuration script to re-create the App Registration. The script will also create a [application name].pfx file that will be **manually** uploaded into KeyVault. When asked about a password, remember it - you will need the password when uploading the certificate.
```powershell
AppCreationScripts-withCert/Configuration.ps1 -TenantId "your tenant id"
```
7. If you still don't have KeyVault, sign in to the [Azure portal](https://portal.azure.com) and [create an Azure KeyVault](https://docs.microsoft.com/en-us/azure/key-vault/general/quick-create-portal)
8. Inside Client/appsettings.json file - update "KeyVaultUrl" key to have URL of your KeyVault
9. [Upload](https://docs.microsoft.com/en-us/azure/key-vault/certificates/tutorial-import-certificate#import-a-certificate-to-key-vault) the generated AppCreationScripts-withCert\.PFX file into the KeyVault
10. Run the sample as indicated in [README.md](README.md)
