# Web app using client certificates with Microsoft.Identity.Web

There are different approaches for [using certificates with Microsoft.Identity.Web](https://github.com/AzureAD/microsoft-identity-web/wiki/Certificates). In this Readme, we will explore use of certificate from a path on the disk.

## Use Self Signed Certificate from a path

To use certificates instead of an application secret you will need to make some changes to what you have done so far:

- generate a certificate and export it, if you don't have one already
- register the certificate with your application in the Azure AD portal
- Update appsettings.json

### (Optional) use the automation script

If you want to use the automation script:

1. On Windows run PowerShell and navigate to the root of the cloned directory.
1. Run the script to create your Azure AD application and configure the code of the sample application accordingly.

   ```PowerShell
   .\AppCreationScripts-WtihCert\Configure.ps1
   ```

   > Other ways of running the scripts are described in [App Creation Scripts](./AppCreationScripts-WithCert/AppCreationScripts.md)

### Generate a Self Signed certificate

Since we'd be using certificate instead of a client secret in this sample, we'd first create a new self signed certificate. If you have an actual valid certificate available, then skip the following step.

<details>
<summary>Click here to use Powershell</summary>

To generate a new self-signed certificate, we will use the [New-SelfSignedCertificate](https://docs.microsoft.com/powershell/module/pkiclient/new-selfsignedcertificate) Powershell command.

1. Open PowerShell and run `New-SelfSignedCertificate` command with the following parameters to create a new self-signed certificate that will be stored in the **current user** certificate store on your computer:

```PowerShell
$cert=New-SelfSignedCertificate -Subject "/CN=webapp" -CertStoreLocation "Cert:\CurrentUser\My"  -KeyExportPolicy Exportable -KeySpec Signature
```

1. Export this certificate using the "Manage User Certificate" MMC snap-in accessible from the Windows Control Panel. You can also add other options to generate the certificate in a different store such as the **Computer** or **service** store (See [How to: View Certificates with the MMC Snap-in](https://docs.microsoft.com/dotnet/framework/wcf/feature-details/how-to-view-certificates-with-the-mmc-snap-in)) for more details.

Export one with private key as webapp.pfx and another as webapp.cer without private key.
</details>

<details>
<summary>Click here to use OpenSSL</summary>

Type the following in a terminal.

```PowerShell
openssl req -x509 -newkey rsa:4096 -sha256 -days 365 -keyout webapp.key -out webapp.cer -nodes -batch

Generating a RSA private key
...........................................................................................................................................................................................................................................................++++
......................................................................................................++++
writing new private key to 'webapp.key'
----- 
```

Generate the webapp.pfx certificate with below command:

```console
openssl pkcs12 -export -out webapp.pfx -inkey webapp.key -in webapp.cer
```

Enter an export password when prompted and make a note of it.

The following files should be generated: `webapp.key`, `webapp.cer` and `webapp.pfx`.
</details>

### Add the certificate for the TodoListClient-aspnetcore-webapi application in the application's registration page

1. Navigate back to the [Azure portal](https://portal.azure.com).
In the left-hand navigation pane, select the **Azure Active Directory** service, and then select **App registrations**.
1. In the resultant screen, select the `TodoListClient-aspnetcore-webapi` application.
1. In the **Certificates & secrets** tab, go to **Certificates** section:
1. Select **Upload certificate** and, in select the browse button on the right to select the certificate you just exported, webapp.cer (or your existing certificate).
1. Select **Add**.

### Configure the Visual Studio project

To change the visual studio project to enable certificates you need to:

1. Open the `Client\appsettings.json` file
1. Find the app key `ClientCertificates` and add the keys as displayed below:

    ```json
    "ClientCertificates": [
     {
        "SourceType": "",
        "CertificateDiskPath": "",
        "CertificatePassword": ""
      }
    ]
    ```

    Update values of the keys:

    `SourceType` to `Path`.

    `CertificateDiskPath` to the path where certificate exported with private key (webapp.pfx) is stored. For example, `C:\\active-directory-aspnetcore-webapp-openidconnect-v2\\4-WebApp-your-API\\4-1-MyOrg\\AppCreationScripts-withCert\\webapp.pfx`

    `CertificatePassword` add the password used while exporting the certificate.
1. If you had set `ClientSecret` previously, set its value to empty string, `""`.

## (Alternate) Key Vault and Managed Service Identity (MSI)

[Azure Key Vault certificates](https://docs.microsoft.com/azure/key-vault/certificates/) enables Microsoft Azure applications and users to store and use certificates. Microsoft.Identity.Web leverages Managed Service Identity to retrieve these certificates. For details see [https://aka.ms/ms-id-web-certificates](https://aka.ms/ms-id-web-certificates).

To generate the certificate from KeyVault see [Get started with Key Vault certificates](https://docs.microsoft.com/azure/key-vault/certificates/certificate-scenarios) documentation.

## Run the sample

Follow [Step 4: Run the sample](Readme.md#step-4-run-the-sample) in [Readme.md](Readme.md).
