# .NET MVC Web application playground to test the migration to AAD

> This sample assumes that you already have an ADFS environment.

## Scenario

You have an ADFS Web application using SAML protocol and would like to migrate this application to AAD.

## About the sample

This sample is a .NET MVC Web application, configured to use SAML single-sign-on on ADFS, that you could use as a playground to test the migration to AAD.

### Pre-requisites

- [Visual Studio](https://aka.ms/vsdownload)
- .NET Framework 4.7.2
- An Internet connection
- An ADFS environment
- An Azure Active Directory (Azure AD) tenant. For more information on how to get an Azure AD tenant, see [How to get an Azure AD tenant](https://azure.microsoft.com/en-us/documentation/articles/active-directory-howto-tenant/)

## Setup Azure AD Connect

On your ADFS machine, [download and install Azure AD Connect](https://www.microsoft.com/download/details.aspx?id=47594).

Configure the [Azure AD Connect](https://docs.microsoft.com/azure/active-directory/hybrid/how-to-connect-sync-whatis) to sync with the tenant that you want to migrate to, using the wizard provided in the tool.

## Configuring the sample on your ADFS

### Step 1: Adding a Relying Party Trust

1. Log into the server where AD is installed
1. Open the Server Manager Dashboard. Under Tools choose ADFS Management
1. Select Add Relying Party Trust
1. Click Start
1. Choose the option **Enter data about relying party manually** and click **Next**
1. Add a display name, for instance `AD FS Migration Playground`, and click **Next**
1. Choose the **AD FS profile** option and click **Next**
1. Click **Next** on the Configure Certificate step
1. Click **Next** on the Configure URL step
1. Under Relying party trust identifier add your application’s website 
1. Click **Add** and then click **Next**
    - >Note: The website needs to have an SSL certificate and https as the navigation path. The AD FS will only communicate through https context. For demo purposes, we have an IIS Express development certificate.
1. Click **Next** on the Configure Multi-factor Authentication step
1. Click **Next** on the Choose Issuance Authorization Rules step
1. Click **Next** on the Ready to Add Trust step
1. Close the wizard

### Step 2: Adding Claim Policy

1. On ADFS Management window, select *Relying Party Trust*
1. Select the name that you chose on the previous step and click **Edit Claim Issuance Policy** on the right menu
1. In the new popup window, click **Add Rule**
1. Choose the Claim rule template as **Send LDAP Attributes as Claims** and click **Next**
1. Add a Claim rule name, for instance `Basic Claims`
1. Select **Active Directory** as the Attribute Store
1. Configure the LDAP attributes to outgoing claim types. Add the above four LDAP attributes and their corresponding outing claim type:
    - E-Mail-Addresses -> Email Address
    - User-Principal-Name -> UPN
    - Given-Name -> Given Name
    - Display-Name -> Name

3. Click **Finish**

### Step 3: Configuring the .NET MVC application

This sample is using the NuGet package **Microsoft.Owin.Security.WsFederation** to configure the authentication with ADFS.

1. Open the **WebApp_SAML** application
1. Open the `Web.config` file and replace the key `ida:ADFSMetadata` value with your ADFS FederationMetadata.xml URI. For instance: 
    ```c#
    <add key="ida:ADFSMetadata" value="https://sts.contoso.com/FederationMetadata/2007-06/FederationMetadata.xml" />
    ```
1. The key `ida:Wtrealm` is the current website URL. Since this sample is running on *localhost*, there is no need to update it. 

Run the **WebApp_SAML** application and sign-in using a ADFS user.

## How to migrate this sample to AAD?

As a first step you'll need to:

1. Sign in to the [Azure portal](https://portal.azure.com) using either a work or school account or a personal Microsoft account.
1. If your account is present in more than one Azure AD tenant, select your profile at the top right corner in the menu on top of the page, and then **switch directory**.
   Change your portal session to the desired Azure AD tenant where you want to migrate the SAML application.

### Registering the application

1. Navigate to the Microsoft identity platform for developers [Enterprise applications](https://portal.azure.com/#blade/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade/EnterpriseApps) page.
1. Select **New registration**.
1. Select **Non gallery app**. 
1. Choose a name for the application, for instance `WebApp_SAML` and select **Add** on the bottom.
1. Under the Manage section, select **Single sign-on**.
1. Select **SAML**. The **Set up Single Sign-On with SAML** page appears. 

#### Basic SAML configuration

For detailed information about the configuration, please follow [this doc](https://docs.microsoft.com/azure/active-directory/manage-apps/configure-single-sign-on-non-gallery-applications)

1. To edit the basic SAML configuration options, select the **Edit** icon (a pencil) in the upper-right corner of the Basic SAML Configuration section.
1. Set **Identifier (Entity ID)** with an unique URL that follows the pattern, `http://.contoso.com`. For instance: `http://webappsaml.contoso.com`.
1. Set **Reply URL** with the URL that AAD will reply after the authentication. In our sample we are using `https://localhost:44347/`.
1. [Optional] Set the other parameters with values according to your scenario. 

#### [Optional] User Attributes and Claims

1. In the **User Attributes and Claims** section, select the **Edit** icon (a pencil) in the upper-right corner.
1. Add additional claims that you would like to use in the WebApp project.
1. To add a claim, select **Add new claim** at the top of the page. Enter the Name and select the appropriate source.
1. Select **Save**. The new claim appears in the table.

#### SAML Signing Certificate

1. In the **SAML Signing Certificate** section, copy the value for `App Federation Metadata Url`. We will use it on the MVC project.

## Configuring the MVC project

Open the project `Web.config` file and:

1. Replace the value for `ida:ADFSMetadata` with the link that you copied from **App Federation Metadata Url** field.
2. Replace the value for `ida:Wtrealm` with the value that you set for **Identifier (Entity ID)**. For instance, `http://webappsaml.contoso.com`.

Run the **WebApp_SAML** application and sign-in using a ADFS user or a user from your AAD tenant.

# Chapter 2: Sync Directory Extensions

While groups and claims will get sync to Azure AD automatically using Azure AD Connect tool, your ADFS directory extensions require a configuration to have it done.

## Configuring Azure AD Connect for Directory Extensions

1. Open the Azure AD Connect tool and select **Configure**
2. Select the option **Customize synchronization options** and click **Next**
3. Sign-in with your AAD global administrator user
4. Select **Next** on the *Connect your directories* tab
5. Select **Next** on the *Domain and OU filtering* tab
6. Check the box **Directory extension sync** and click **Next**
7. The following screen will show all the available attributes in your ADFS. Move to the right box all the attributes that you would like to send to AAD
8. Click **Next** and the synchronization will be executed.

### Testing the directory extensions migration

To test the migration, you can access [Graph Explorer](https://aka.ms/ge), sign-in using a ADFS user that has those attributes set. 

Once signed-in and accepted the consent screen, run the query for the URL: `https://graph.microsoft.com/beta/me`, and the attributes will be listed in the result.

>Note: When you sync directory attributes via Azure AD Connect tool, their name on Microsoft Graph will have an auto-generated prefix that cannot be edited. For instance, if you have an attribute called `genderAttribute` and use the tool so sync it with Azure AD, its name will be transformed to something like: `extension_90f5761cbd854b259d47fde20b522087_genderAttribute`

### Step 1: 

- Azure AD Sync
- Register Application
- Config SAML
- Update code