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
AppCreationScripts-withCert/Cleanup.ps1
```
6. Run the configuration script to re-create the App Registration. The script will also create a [application name].pfx file that will be **manually** uploaded into KeyVault. When asked about a password, remember it - you will need the password when uploading the certificate.
```powershell
AppCreationScripts-withCert/Configuration.ps1
```
7. If you still don't have KeyVault, sign in to the [Azure portal](https://portal.azure.com) and [create an Azure KeyVault](https://docs.microsoft.com/en-us/azure/key-vault/general/quick-create-portal)
8. Inside Client/appsettings.json file - update "KeyVaultUrl" key to have URL of your KeyVault
9. [Upload](https://docs.microsoft.com/en-us/azure/key-vault/certificates/tutorial-import-certificate#import-a-certificate-to-key-vault) the generated AppCreationScripts-withCert\.PFX file into the KeyVault
10. Run the sample as indicated in [README.md](README.md)

## More information

### About Azure Key Vault

Cloud applications and services use cryptographic keys and secrets to help keep information secure. [Azure Key Vault](https://azure.microsoft.com/services/key-vault/) safeguards these keys and secrets. When you use Key Vault, you can encrypt authentication keys, storage account keys, data encryption keys, .pfx files, and passwords by using keys that are protected by hardware security modules (HSMs).

### About Managed Identities for Azure Resources

Azure Key Vault provides a way to securely store credentials, secrets, and other keys, but your code has to authenticate to Key Vault to retrieve them. The [managed identities for Azure resources](https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/overview) feature in Azure Active Directory (Azure AD) solves this problem. The feature provides Azure services with an automatically managed identity in Azure AD. You can use the identity to authenticate to any service that supports Azure AD authentication, including Key Vault, without any credentials in your code.

#### **Managed Identities for Azure Resources**

In a daemon application scenario, Managed Identity will work if you have it deployed it in an [Azure Virtual Machine](https://azure.microsoft.com/services/virtual-machines/) or [Azure Web Job](https://docs.microsoft.com/en-us/azure/app-service/webjobs-create). Please, read [this documentation](https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/overview) to understand how Managed Identity works with an Azure VM.

##### Configure Managed identity on Azure VM to access Key Vault

To authenticate to Key Vault using your Azure VM, you must first grant it permissions to Key Vault using the **Key Vault Access Policies**. To do that, follow the steps:

1. On Azure Portal, note the name of the Azure VM where you deployed the daemon application.
1. [Enable managed identity on the virtual machine](https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/qs-configure-portal-windows-vm).
1. On Azure Portal, navigate to **Key Vaults** and select the one that you want the daemon application's VM to access.
1. Then click on **Access policies** menu and click on **+Add Access Policy**.
1. Select an adequate template from the dropdown "Configure from template" (ie "Secret & Certificate Management") or set the permissions manually (this sample requires the permission **GET** for Secret and Certificate to be checked).
1. For **Select principal**, search for the Azure VM *name* or *ObjectId*, select it and click on **Select** button.
1. Click on **Add**.
1. Then, **Save**.

For more information about Key Vault, take a look at these links:

- [Key Vault documentation](https://docs.microsoft.com/en-us/azure/key-vault/)
- [Managed Identity Key Vault sample for dotnet](https://github.com/Azure-Samples/app-service-msi-keyvault-dotnet)

For more information about AzureVM and Managed Identities for Azure Resources, take a look at these links:

- [Managed Identity documentation](https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/overview)
- [AzureVM documentation](https://azure.microsoft.com/en-us/services/virtual-machines/)
