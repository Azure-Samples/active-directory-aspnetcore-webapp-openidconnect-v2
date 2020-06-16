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

### Step 3: Configuring 

## How to migrate to AAD?

### Step 1: 

- Azure AD Sync
- Register Application
- Config SAML
- Update code