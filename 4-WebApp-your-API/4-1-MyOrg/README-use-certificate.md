# Web app using client certificates with Microsoft.Identity.Web

There are different approaches to [use certificates with Microsoft.Identity.Web](https://github.com/AzureAD/microsoft-identity-web/wiki/Certificates). In this Readme, we will explore use of certificate from a path on the disk.

## Use Self Signed Certificate from a path

To use certificates instead of an application secret you will need to do some changes to what you have done so far:

- generate a certificate and export it, if you don't have one already
- register the certificate with your application in the Azure AD portal
- Update appsettings.json

### Generate a Self Signed certificate

#### with Powershell

To complete this step, you will use the `New-SelfSignedCertificate` Powershell command. You can find more information about the New-SelfSignedCertificate command [here](https://docs.microsoft.com/powershell/module/pkiclient/new-selfsignedcertificate).

1. Open PowerShell and run New-SelfSignedCertificate with the following parameters to create a self-signed certificate in the user certificate store on your computer:

    ```PowerShell
    $cert=New-SelfSignedCertificate -Subject "CN=WebAppCert" -CertStoreLocation "Cert:\CurrentUser\My"  -KeyExportPolicy Exportable -KeySpec Signature
    ```

1. Export this certificate using the "Manage User Certificate" MMC snap-in accessible from the Windows Control Panel. You can also add other options to generate the certificate in a different
store such as the Computer or service store (See [How to: View Certificates with the MMC Snap-in](https://docs.microsoft.com/dotnet/framework/wcf/feature-details/how-to-view-certificates-with-the-mmc-snap-in)).

Alternatively you can use an existing certificate if you have one (just be sure to record its name for the next steps).

Export one with private key as WebAppCert.pfx and another as WebAppCert.cer without private key.

#### with OpenSSL

Type the following in a terminal. You will be prompted to enter some identifying information and a *passphrase*:

```console
    openssl req -x509 -newkey rsa:4096 -sha256 -days 365 -keyout webapp.key -out webapp.cer -subj "/CN=webapp.com" -addext "subjectAltName=DNS:webapp.com"

    Generating a RSA private key
    ...........................................................................................................................................................................................................................................................++++
    ......................................................................................................++++
    writing new private key to 'webapp.key'
    Enter PEM pass phrase:
    Verifying - Enter PEM pass phrase:
    ----- 
```

Generate the webapp.pfx certificate with below command:

```console
  openssl pkcs12 -export -out webapp.pfx -inkey webapp.key -in webapp.cer
```

After that, the following files should be generated: `webapp.key`, `webapp.cer` and `webapp.pfx`.

### Add the certificate for the web application in Azure AD

1. Navigate back to the [Azure portal](https://portal.azure.com).
In the left-hand navigation pane, select the **Azure Active Directory** service, and then select **App registrations**.
1. In the resultant screen, select the `TodoListClient-aspnetcore-webapi` application.
1. In the **Certificates & secrets** tab, go to **Certificates** section:
1. Click on **Upload certificate** and, in click the browse button on the right to select the certificate you just exported, WebAppCert.cer (or your existing certificate).
1. Click **Add**.

### Configure the Visual Studio project

To change the visual studio project to enable certificates you need to:

1. Open the `Client\appsettings.json` file
1. Find the app key `ClientCertificates` and add the keys as displayed below:

    "ClientCertificates": [
      {
        "SourceType": "",
        "CertificateDiskPath": "",
        "CertificatePassword": ""
      }
    ]

    Update values of the keys:

    `SourceType` to `Path`.

    `CertificateDiskPath` to the path where certificate exported with private key (WebAppCert.pfx) is stored.

    `CertificatePassword` add the password used while exporting the certificate.
1. If you had set `ClientSecret` previously, set its value to empty string, `""`.

## (Alternate) Key Vault and Managed Service Identity (MSI)

[Azure Key Vault certificates](https://docs.microsoft.com/azure/key-vault/certificates/) enables Microsoft Azure applications and users to store and use certificates. Microsoft.Identity.Web leverages Managed Service Identity to retrieve these certificates. For details see [https://aka.ms/ms-id-web-certificates](https://aka.ms/ms-id-web-certificates).

## Run the sample

Follow [Step 4: Run the sample](Readme.md#step-4-run-the-sample) in [Readme.md](Readme.md).
