
[CmdletBinding()]
param(
    [Parameter(Mandatory=$False, HelpMessage='Tenant ID (This is a GUID which represents the "Directory ID" of the AzureAD tenant into which you want to create the apps')]
    [string] $tenantId,
    [Parameter(Mandatory=$False, HelpMessage='Azure environment to use while running the script. Default = Global')]
    [string] $azureEnvironmentName
)

<#
 This script creates the Azure AD applications needed for this sample and updates the configuration files
 for the visual Studio projects from the data in the Azure AD applications.

 In case you don't have Microsoft.Graph.Applications already installed, the script will automatically install it for the current user
 
 There are four ways to run this script. For more information, read the AppCreationScripts.md file in the same folder as this script.
#>

# Adds the requiredAccesses (expressed as a pipe separated string) to the requiredAccess structure
# The exposed permissions are in the $exposedPermissions collection, and the type of permission (Scope | Role) is 
# described in $permissionType
Function AddResourcePermission($requiredAccess, `
                               $exposedPermissions, [string]$requiredAccesses, [string]$permissionType)
{
    foreach($permission in $requiredAccesses.Trim().Split("|"))
    {
        foreach($exposedPermission in $exposedPermissions)
        {
            if ($exposedPermission.Value -eq $permission)
                {
                $resourceAccess = New-Object Microsoft.Graph.PowerShell.Models.MicrosoftGraphResourceAccess
                $resourceAccess.Type = $permissionType # Scope = Delegated permissions | Role = Application permissions
                $resourceAccess.Id = $exposedPermission.Id # Read directory data
                $requiredAccess.ResourceAccess += $resourceAccess
                }
        }
    }
}

#
# Example: GetRequiredPermissions "Microsoft Graph"  "Graph.Read|User.Read"
# See also: http://stackoverflow.com/questions/42164581/how-to-configure-a-new-azure-ad-application-through-powershell
Function GetRequiredPermissions([string] $applicationDisplayName, [string] $requiredDelegatedPermissions, [string]$requiredApplicationPermissions, $servicePrincipal)
{
    # If we are passed the service principal we use it directly, otherwise we find it from the display name (which might not be unique)
    if ($servicePrincipal)
    {
        $sp = $servicePrincipal
    }
    else
    {
        $sp = Get-MgServicePrincipal -Filter "DisplayName eq '$applicationDisplayName'"
    }
    $appid = $sp.AppId
    $requiredAccess = New-Object Microsoft.Graph.PowerShell.Models.MicrosoftGraphRequiredResourceAccess
    $requiredAccess.ResourceAppId = $appid 
    $requiredAccess.ResourceAccess = New-Object System.Collections.Generic.List[Microsoft.Graph.PowerShell.Models.MicrosoftGraphResourceAccess]

    # $sp.Oauth2Permissions | Select Id,AdminConsentDisplayName,Value: To see the list of all the Delegated permissions for the application:
    if ($requiredDelegatedPermissions)
    {
        AddResourcePermission $requiredAccess -exposedPermissions $sp.Oauth2PermissionScopes -requiredAccesses $requiredDelegatedPermissions -permissionType "Scope"
    }
    
    # $sp.AppRoles | Select Id,AdminConsentDisplayName,Value: To see the list of all the Application permissions for the application
    if ($requiredApplicationPermissions)
    {
        AddResourcePermission $requiredAccess -exposedPermissions $sp.AppRoles -requiredAccesses $requiredApplicationPermissions -permissionType "Role"
    }
    return $requiredAccess
}


Function UpdateLine([string] $line, [string] $value)
{
    $index = $line.IndexOf(':')
    $lineEnd = ''

    if($line[$line.Length - 1] -eq ','){   $lineEnd = ',' }
    
    if ($index -ige 0)
    {
        $line = $line.Substring(0, $index+1) + " " + '"' + $value+ '"' + $lineEnd
    }
    return $line
}

Function UpdateTextFile([string] $configFilePath, [System.Collections.HashTable] $dictionary)
{
    $lines = Get-Content $configFilePath
    $index = 0
    while($index -lt $lines.Length)
    {
        $line = $lines[$index]
        foreach($key in $dictionary.Keys)
        {
            if ($line.Contains($key))
            {
                $lines[$index] = UpdateLine $line $dictionary[$key]
            }
        }
        $index++
    }

    Set-Content -Path $configFilePath -Value $lines -Force
}


Function ConfigureApplications
{
    <#.Description
       This function creates the Azure AD applications for the sample in the provided Azure AD tenant and updates the
       configuration files in the client and service project  of the visual studio solution (App.Config and Web.Config)
       so that they are consistent with the Applications parameters
    #> 
    
    if (!$azureEnvironmentName)
    {
        $azureEnvironmentName = "Global"
    }

    # Connect to the Microsoft Graph API, non-interactive is not supported for the moment (Oct 2021)
    Write-Host "Connecting to Microsoft Graph"
    if ($tenantId -eq "") {
        Connect-MgGraph -Scopes "Application.ReadWrite.All" -Environment $azureEnvironmentName
        $tenantId = (Get-MgContext).TenantId
    }
    else {
        Connect-MgGraph -TenantId $tenantId -Scopes "Application.ReadWrite.All" -Environment $azureEnvironmentName
    }
    

   # Create the HybridFlowAspNetCore AAD application
   Write-Host "Creating the AAD application (HybridFlowAspNetCore)"
   
   # create the application 
   $HybridFlowAspNetCoreAadApplication = New-MgApplication -DisplayName "HybridFlowAspNetCore" `
                                                                    -Web `
                                                                    @{ `
                                                                        RedirectUris = "https://localhost:7089/signin-oidc"; `
                                                                        ImplicitGrantSettings = @{ `
                                                                            EnableIdTokenIssuance=$true; `
                                                                        } `
                                                                      } `
                                                                    -Spa `
                                                                    @{ `
                                                                        RedirectUris = "https://localhost:7089/"; `
                                                                      } `
                                                                     -SignInAudience AzureADMyOrg `
                                                                    #end of command
    $tenantName = (Get-MgApplication -ApplicationId $HybridFlowAspNetCoreAadApplication.Id).PublisherDomain
    Update-MgApplication -ApplicationId $HybridFlowAspNetCoreAadApplication.Id -IdentifierUris @("https://$tenantName/HybridFlowAspNetCore")
    
    # Generate a certificate
    Write-Host "Creating the HybridFlowAspNetCore application (HybridFlowAspNetCore)"

    $certificateName = 'HybridFlowAspNetCore'

    # temporarily disable the option and procees to certificate creation
    #$isOpenSSL = Read-Host ' By default certificate is generated using New-SelfSignedCertificate. Do you want to generate cert using OpenSSL(Y/N)?'
    $isOpenSSl = 'N'
    if($isOpenSSL -eq 'Y')
    {
        $certificate=openssl req -x509 -newkey rsa:4096 -sha256 -days 365 -keyout "$certificateName.key" -out "$certificateName.cer" -nodes -batch
        openssl pkcs12 -export -out "$certificateName.pfx" -inkey $certificateName.key -in "$certificateName.cer"
    }
    else
    {
        $certificate=New-SelfSignedCertificate -Subject $certificateName `
                                                -CertStoreLocation "Cert:\CurrentUser\My" `
                                                -KeyExportPolicy Exportable `
                                                -KeySpec Signature

        $thumbprint = $certificate.Thumbprint
        $certificatePassword = Read-Host -Prompt "Enter password for your certificate (Please remember the password, you will need it when uploading to KeyVault): " -AsSecureString
        Write-Host "Exporting certificate as a PFX file"
        Export-PfxCertificate -Cert "Cert:\Currentuser\My\$thumbprint" -FilePath "$pwd\$certificateName.pfx" -ChainOption EndEntityCertOnly -NoProperties -Password $certificatePassword
        Write-Host "PFX written to:"
        Write-Host "$pwd\$certificateName.pfx"

        # Add a Azure Key Credentials from the certificate for the application
        $HybridFlowAspNetCoreKeyCredentials = Update-MgApplication -ApplicationId $HybridFlowAspNetCoreAadApplication.Id `
            -KeyCredentials @(@{Type = "AsymmetricX509Cert"; Usage = "Verify"; Key= $certificate.RawData; StartDateTime = $certificate.NotBefore; EndDateTime = $certificate.NotAfter;})       
       
    }
  
    
    # create the service principal of the newly created application 
    $currentAppId = $HybridFlowAspNetCoreAadApplication.AppId
    $HybridFlowAspNetCoreServicePrincipal = New-MgServicePrincipal -AppId $currentAppId -Tags {WindowsAzureActiveDirectoryIntegratedApp}

    # add the user running the script as an app owner if needed
    $owner = Get-MgApplicationOwner -ApplicationId $HybridFlowAspNetCoreAadApplication.Id
    if ($owner -eq $null)
    { 
        New-MgApplicationOwnerByRef -ApplicationId $HybridFlowAspNetCoreAadApplication.Id  -BodyParameter = @{"@odata.id" = "htps://graph.microsoft.com/v1.0/directoryObjects/$user.ObjectId"}
        Write-Host "'$($user.UserPrincipalName)' added as an application owner to app '$($HybridFlowAspNetCoreServicePrincipal.DisplayName)'"
    }
    Write-Host "Done creating the HybridFlowAspNetCore application (HybridFlowAspNetCore)"

    # URL of the AAD application in the Azure portal
    # Future? $HybridFlowAspNetCorePortalUrl = "https://portal.azure.com/#@"+$tenantName+"/blade/Microsoft_AAD_RegisteredApps/ApplicationMenuBlade/Overview/appId/"+$HybridFlowAspNetCoreAadApplication.AppId+"/objectId/"+$HybridFlowAspNetCoreAadApplication.Id+"/isMSAApp/"
    $HybridFlowAspNetCorePortalUrl = "https://portal.azure.com/#blade/Microsoft_AAD_RegisteredApps/ApplicationMenuBlade/CallAnAPI/appId/"+$HybridFlowAspNetCoreAadApplication.AppId+"/objectId/"+$HybridFlowAspNetCoreAadApplication.Id+"/isMSAApp/"
    Add-Content -Value "<tr><td>HybridFlowAspNetCore</td><td>$currentAppId</td><td><a href='$HybridFlowAspNetCorePortalUrl'>HybridFlowAspNetCore</a></td></tr>" -Path createdApps.html
    $requiredResourcesAccess = New-Object System.Collections.Generic.List[Microsoft.Graph.PowerShell.Models.MicrosoftGraphRequiredResourceAccess]

    
    # Add Required Resources Access (from 'HybridFlowAspNetCore' to 'Microsoft Graph')
    Write-Host "Getting access from 'HybridFlowAspNetCore' to 'Microsoft Graph'"
    $requiredPermissions = GetRequiredPermissions -applicationDisplayName "Microsoft Graph" `
        -requiredDelegatedPermissions "User.Read|Contacts.Read" `
    

    $requiredResourcesAccess.Add($requiredPermissions)
    Update-MgApplication -ApplicationId $HybridFlowAspNetCoreAadApplication.Id -RequiredResourceAccess $requiredResourcesAccess
    Write-Host "Granted permissions."
    
    # Update config file for 'HybridFlowAspNetCore'
    $configFile = $pwd.Path + "\..\appsettings.json"
    $dictionary = @{  };

    Write-Host "Updating the sample code ($configFile)"

    UpdateTextFile -configFilePath $configFile -dictionary $dictionary
        
    $appSettingsObject = (Get-Content ..\appsettings.json | ConvertFrom-Json)

	# JSON is auto-generated.
    $appSettingsObject.AzureAd = ConvertFrom-Json '{"Instance":"https://login.microsoftonline.com/","Domain":"Auto","TenantId":"Auto","ClientId":"Auto","CallbackPath":"/signin-oidc","WithSpaAuthCode":true,"ClientCertificates":[{"SourceType":"StoreWithDistinguishedName","CertificateStorePath":"CurrentUser/My","CertificateDistinguishedName":"CN=HybridFlowAspNetCore"}]}';

    $appSettingsObject.AzureAd.TenantId = $tenantId;
    $appSettingsObject.AzureAd.ClientId = $currentAppId;
    $appSettingsObject.AzureAd.Domain = $tenantName;

	# JSON is auto-generated.
    $appSettingsObject.DownStreamApi = ConvertFrom-Json '{"BaseUrl":"https://graph.microsoft.com/v1.0","Scopes":"User.Read Contacts.Read"}';

    Write-Host "Updating the appsetings.json file at '..\appsettings.json'"
    $appSettingsObject | ConvertTo-Json -Depth 3 | Out-File ..\appsettings.json

    if($isOpenSSL -eq 'Y')
    {
        Write-Host -ForegroundColor Green "------------------------------------------------------------------------------------------------" 
        Write-Host "You have generated certificate using OpenSSL so follow below steps: "
        Write-Host "Install the certificate on your system from current folder."
        Write-Host -ForegroundColor Green "------------------------------------------------------------------------------------------------" 
    }
    Add-Content -Value "</tbody></table></body></html>" -Path createdApps.html  
}

# Pre-requisites
if ($null -eq (Get-Module -ListAvailable -Name "Microsoft.Graph.Applications")) {
    Install-Module "Microsoft.Graph.Applications" -Scope CurrentUser 
}

Import-Module Microsoft.Graph.Applications

Set-Content -Value "<html><body><table>" -Path createdApps.html
Add-Content -Value "<thead><tr><th>Application</th><th>AppId</th><th>Url in the Azure portal</th></tr></thead><tbody>" -Path createdApps.html

$ErrorActionPreference = "Stop"

# Run interactively (will ask you for the tenant ID)
ConfigureApplications -tenantId $tenantId -environment $azureEnvironmentName

Write-Host "Disconnecting from tenant"
Disconnect-MgGraph