# How to use certificates instead of secrets in your client applications

We recommend you familiarize yourself with [Using certificates with Microsoft\.Identity\.Web](https://github.com/AzureAD/microsoft-identity-web/wiki/Certificates#getting-certificates-from-key-vault) as it provides various ways for a developer to use a certificate instead of a client secret to authenticate their apps with Microsoft Entra ID.
> Note: Please carefully go through [Getting certificates from Key Vault](https://github.com/AzureAD/microsoft-identity-web/wiki/Certificates#getting-certificates-from-key-vault) when deploying your app to production.

- [Using a Client certificate with KeyVault](#using-a-client-certificate-with-keyvault)
  - [To be able to use a certificate, please make the following changes:](#to-be-able-to-use-a-certificate-please-make-the-following-changes)
- [Using a local Client certificate](#using-a-local-client-certificate)
  - [Generate a self-signed certificate](#generate-a-self-signed-certificate)
  - [Add a certificate in the application's registration page](#add-a-certificate-in-the-applications-registration-page)
- [More information](#more-information)
  - [Using Azure KeyVault](#using-azure-keyvault)
  - [About Managed Identities for Azure Resources](#about-managed-identities-for-azure-resources)

## Using a Client certificate with KeyVault

This sample was configured to use a client secret, but have an option to use a certificate instead.

### To be able to use a certificate, please make the following changes:

1. Open Client/appsettings.json file
1. **Comment out** the next line:

```json
"ClientSecret": "[Copy the client secret added to the app from the Microsoft Entra admin center]"
```

1. **Un-comment** the following lines:

```json
"ClientCertificates": [
  {
    "SourceType": "KeyVault",
    "KeyVaultUrl": "[Enter URL for you KeyVault]",
    "KeyVaultCertificateName": "the name of the certificate, for ex.TodoListClient-aspnetcore-webapi"
  }
]
```

1. While inside the sample folder, open a PowerShell terminal

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

1. To use KeyVault, sign in to the [Microsoft Entra admin center](https://entra.microsoft.com) and [create an Azure Key Vault](https://docs.microsoft.com/azure/key-vault/general/quick-create-portal)
1. Inside Client/appsettings.json file - update "KeyVaultUrl" key to have URL of your Key Vault, like https://[your Key Vault name here].vault.azure.net
1. [Upload](https://docs.microsoft.com/azure/key-vault/certificates/tutorial-import-certificate#import-a-certificate-to-key-vault) the generated AppCreationScripts-withCert\.PFX file into the Key Vault
1. Run the sample as indicated in [README.md](README.md)
1. Use the account you used to upload the certificate to key vault to sign-into the web app.
1. In production environments, you'd give access to your deployed web app or Virtual machine to read this certificate's Key Vault entry.  

## Using a local Client certificate

1. Open Client/appsettings.json file
2. **Comment out** the next line:

```json
"ClientSecret": "[Copy the client secret added to the app from the Microsoft Entra admin center]"
```

1. **Un-comment** the following lines:

```json
"ClientCertificates": [
  {
   "SourceType": "StoreWithDistinguishedName",
   "CertificateStorePath": "CurrentUser/My",
   "CertificateDistinguishedName": "<the name of the certificate, for ex.CN=TodoListClient-aspnetcore-webapi>"
  }
]
```

1. While inside the sample folder, open a Powershell terminal

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

### Generate a self-signed certificate

If you wish to generate a new self-signed certificate yourself, you can follow the steps below. If you have an actual valid certificate available, you can skip the following step.

<details>
<summary>Click here to use Powershell</summary>

To generate a new self-signed certificate, we will use the [New-SelfSignedCertificate](https://docs.microsoft.com/powershell/module/pkiclient/new-selfsignedcertificate) Powershell command.

1. Open PowerShell and run the command with the following parameters to create a new self-signed certificate that will be stored in the **current user** certificate store on your computer:

```PowerShell
$cert=New-SelfSignedCertificate -Subject "/CN=the name will be assigned automatically by PowerShell script and it will be equal to the Application name" -CertStoreLocation "Cert:\CurrentUser\My"  -KeyExportPolicy Exportable -KeySpec Signature
```

1. Export this certificate using the "Manage User Certificate" MMC snap-in accessible from the Windows Control Panel. You can also add other options to generate the certificate in a different store such as the **Computer** or **service** store (see [How to: View Certificates with the MMC Snap-in](https://docs.microsoft.com/dotnet/framework/wcf/feature-details/how-to-view-certificates-with-the-mmc-snap-in) for more details).

Export one with private key as *the name will be assigned automatically by PowerShell script and it will be equal to the Application name.pfx* and another as *the name will be assigned automatically by PowerShell script and it will be equal to the Application name.cer* without private key.

</details>

<details>
<summary>Click here to use OpenSSL</summary>

Type the following in a terminal.

```PowerShell
openssl req -x509 -newkey rsa:2048 -sha256 -days 365 -keyout the name will be assigned automatically by PowerShell script and it will be equal to the Application name.key -out the name will be assigned automatically by PowerShell script and it will be equal to the Application name.cer -nodes -batch

Generating a RSA private key
.........................................................
.........................................................
writing new private key to 'the name will be assigned automatically by PowerShell script and it will be equal to the Application name.key'
```

The following files should be generated: *the name will be assigned automatically by PowerShell script and it will be equal to the Application name.key*, *the name will be assigned automatically by PowerShell script and it will be equal to the Application name.cer*
You can generate the the name will be assigned automatically by PowerShell script and it will be equal to the Application name.pfx certificate + private key combination with the command below:

```console
openssl pkcs12 -export -out the name will be assigned automatically by PowerShell script and it will be equal to the Application name.pfx -inkey the name will be assigned automatically by PowerShell script and it will be equal to the Application name.key -in the name will be assigned automatically by PowerShell script and it will be equal to the Application name.cer
```

Enter an export password when prompted and make a note of it.

The following file should be generated: *the name will be assigned automatically by PowerShell script and it will be equal to the Application name.pfx*.

</details>

### Add a certificate in the application's registration page

1. Select **Upload certificate** and, in select the browse button on the right to select the certificate you just exported.
1. Select **Add**

## More information

### Using Azure KeyVault

Cloud applications and services use cryptographic keys and secrets to help keep information secure. [Azure KeyVault](https://azure.microsoft.com/services/key-vault/) safeguards these keys and secrets. When you use Key Vault, you can encrypt authentication keys, storage account keys, data encryption keys, .pfx files, and passwords by using keys that are protected by hardware security modules (HSMs).

### About Managed Identities for Azure Resources

[Azure KeyVault](https://azure.microsoft.com/services/key-vault/#product-overview)

[Managed Identities for Azure Resources](https://docs.microsoft.com/azure/active-directory/managed-identities-azure-resources/)

[Managed Identities for Azure App Services](https://docs.microsoft.com/azure/app-service/overview-managed-identity?tabs=dotnet)
