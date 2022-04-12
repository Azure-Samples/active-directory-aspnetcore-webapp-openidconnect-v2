[CmdletBinding()]
param(
    [Parameter(Mandatory = $False, HelpMessage = 'Tenant ID (This is a GUID which represents the "Directory ID" of the AzureAD tenant into which you want to create the apps')]
    [string] $tenantId,
    [Parameter(Mandatory = $False, HelpMessage = 'Azure environment to use while running the script. Default = Global')]
    [string] $azureEnvironmentName
)

<#
 This script creates the Azure AD applications needed for this sample and updates the configuration files
 for the visual Studio projects from the data in the Azure AD applications.

 In case you don't have Microsoft.Graph.Applications already installed, the script will automatically install it for the current user
 
 There are four ways to run this script. For more information, read the AppCreationScripts.md file in the same folder as this script.
#>

# Create an application key
# See https://www.sabin.io/blog/adding-an-azure-active-directory-application-and-key-using-powershell/
Function CreateAppKey([DateTime] $fromDate, [double] $durationInMonths) {
    $key = New-Object Microsoft.Graph.PowerShell.Models.MicrosoftGraphPasswordCredential

    $key.StartDateTime = $fromDate
    $key.EndDateTime = $fromDate.AddMonths($durationInMonths)
    $key.KeyId = (New-Guid).ToString()
    $key.DisplayName = "app secret"

    return $key
}

# Adds the requiredAccesses (expressed as a pipe separated string) to the requiredAccess structure
# The exposed permissions are in the $exposedPermissions collection, and the type of permission (Scope | Role) is 
# described in $permissionType
Function AddResourcePermission($requiredAccess, `
        $exposedPermissions, [string]$requiredAccesses, [string]$permissionType) {
    foreach ($permission in $requiredAccesses.Trim().Split("|")) {
        foreach ($exposedPermission in $exposedPermissions) {
            if ($exposedPermission.Value -eq $permission) {
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
Function GetRequiredPermissions([string] $applicationDisplayName, [string] $requiredDelegatedPermissions, [string]$requiredApplicationPermissions, $servicePrincipal) {
    # If we are passed the service principal we use it directly, otherwise we find it from the display name (which might not be unique)
    if ($servicePrincipal) {
        $sp = $servicePrincipal
    }
    else {
        $sp = Get-MgServicePrincipal -Filter "DisplayName eq '$applicationDisplayName'"
    }
    $appid = $sp.AppId
    $requiredAccess = New-Object Microsoft.Graph.PowerShell.Models.MicrosoftGraphRequiredResourceAccess
    $requiredAccess.ResourceAppId = $appid 
    $requiredAccess.ResourceAccess = New-Object System.Collections.Generic.List[Microsoft.Graph.PowerShell.Models.MicrosoftGraphResourceAccess]

    # $sp.Oauth2Permissions | Select Id,AdminConsentDisplayName,Value: To see the list of all the Delegated permissions for the application:
    if ($requiredDelegatedPermissions) {
        AddResourcePermission $requiredAccess -exposedPermissions $sp.Oauth2PermissionScopes -requiredAccesses $requiredDelegatedPermissions -permissionType "Scope"
    }
    
    # $sp.AppRoles | Select Id,AdminConsentDisplayName,Value: To see the list of all the Application permissions for the application
    if ($requiredApplicationPermissions) {
        AddResourcePermission $requiredAccess -exposedPermissions $sp.AppRoles -requiredAccesses $requiredApplicationPermissions -permissionType "Role"
    }
    return $requiredAccess
}

<#.Description
   This function creates a new Azure AD scope (OAuth2Permission) with default and provided values
#>  
Function CreateScope( [string] $value, [string] $userConsentDisplayName, [string] $userConsentDescription, [string] $adminConsentDisplayName, [string] $adminConsentDescription) {
    $scope = New-Object Microsoft.Graph.PowerShell.Models.MicrosoftGraphPermissionScope
    $scope.Id = New-Guid
    $scope.Value = $value
    $scope.UserConsentDisplayName = $userConsentDisplayName
    $scope.UserConsentDescription = $userConsentDescription
    $scope.AdminConsentDisplayName = $adminConsentDisplayName
    $scope.AdminConsentDescription = $adminConsentDescription
    $scope.IsEnabled = $true
    $scope.Type = "User"
    return $scope
}

<#.Description
   This function creates a new Azure AD AppRole with default and provided values
#>  
Function CreateAppRole([string] $types, [string] $name, [string] $description) {
    $appRole = New-Object Microsoft.Graph.PowerShell.Models.MicrosoftGraphAppRole
    $appRole.AllowedMemberTypes = New-Object System.Collections.Generic.List[string]
    $typesArr = $types.Split(',')
    foreach ($type in $typesArr) {
        $appRole.AllowedMemberTypes += $type;
    }
    $appRole.DisplayName = $name
    $appRole.Id = New-Guid
    $appRole.IsEnabled = $true
    $appRole.Description = $description
    $appRole.Value = $name;
    return $appRole
}

Function ConfigureApplications {
    <#.Description
       This function creates the Azure AD applications for the sample in the provided Azure AD tenant and updates the
       configuration files in the client and service project  of the visual studio solution (App.Config and Web.Config)
       so that they are consistent with the Applications parameters
    #> 
    
    if (!$azureEnvironmentName) {
        $azureEnvironmentName = "Global"
    }

    # Connect to the Microsoft Graph API, non-interactive is not supported for the moment (Oct 2021)
    Write-Host "Connecting to Microsoft Graph"
    if ($tenantId -eq "") {
        Connect-MgGraph -Scopes "User.Read Contacts.Read" -Environment $azureEnvironmentName
        $tenantId = (Get-MgContext).TenantId
    }
    else {
        Connect-MgGraph -TenantId $tenantId -Scopes "User.Read Contacts.Read" -Environment $azureEnvironmentName
    }
    

    # Create the service AAD application
    Write-Host "Creating the AAD application (HybridFlow-aspnetcore)"
   
    # create the application 
    $hybridAadApplication = New-MgApplication -DisplayName "HybridFlow-aspnetcore" `
        -Web `
    @{ `
            RedirectUris      = "https://localhost:7089/signin/"
        ImplicitGrantSettings =
        @{
            EnableIdTokenIssuance     = $true
            EnableAccessTokenIssuance = $true
        }
    } `
        -Spa `
    @{
        RedirectUris = "https://localhost:7089/"
    } `
        -SignInAudience AzureADMyOrg `
        #end of command
    
    # create the service principal of the newly created application 
    $currentAppId = $hybridAadApplication.AppId
    $serviceServicePrincipal = New-MgServicePrincipal -AppId $currentAppId -Tags { WindowsAzureActiveDirectoryIntegratedApp }

    # add the user running the script as an app owner if needed
    $owner = Get-MgApplicationOwner -ApplicationId $hybridAadApplication.Id
    if ($owner -eq $null) { 
        New-MgApplicationOwnerByRef -ApplicationId $hybridAadApplication.Id  -BodyParameter = @{"@odata.id" = "htps://graph.microsoft.com/v1.0/directoryObjects/$user.ObjectId" }
        Write-Host "'$($user.UserPrincipalName)' added as an application owner to app '$($hybridAadApplication.DisplayName)'"
    }

    Write-Host "Done creating the service application (HybridFlow-aspnetcore)"

    # Generate a certificate
    Write-Host "Creating the client application (HybridFlow-aspnetcore)"
    $certificate = New-SelfSignedCertificate -Subject CN=HybridFlowCert `
        -CertStoreLocation "Cert:\CurrentUser\My" `
        -KeyExportPolicy Exportable `
        -KeySpec Signature

    $thumbprint = $certificate.Thumbprint
    $certificatePassword = Read-Host -Prompt "Enter password for your certificate: " -AsSecureString
    Write-Host "Exporting certificate as a PFX file"
    Export-PfxCertificate -Cert "Cert:\Currentuser\My\$thumbprint" -FilePath "$pwd\HybridFlowCert.pfx" -ChainOption EndEntityCertOnly -NoProperties -Password $certificatePassword
    Write-Host "PFX written to:"
    Write-Host "$pwd\HybridFlowCert.pfx"

    Write-Host "Exporting certificate as a CER file"
    Export-Certificate -Cert $certificate -FilePath "$pwd\HybridFlowCert.cer"
    Write-Host "CER written to:"
    Write-Host "$pwd\HybridFlowCert.cer"

    # URL of the AAD application in the Azure portal
    # Future? $clientPortalUrl = "https://portal.azure.com/#@"+$tenantName+"/blade/Microsoft_AAD_RegisteredApps/ApplicationMenuBlade/Overview/appId/"+$clientAadApplication.AppId+"/objectId/"+$clientAadApplication.ObjectId+"/isMSAApp/"
    $clientPortalUrl = "https://portal.azure.com/#blade/Microsoft_AAD_RegisteredApps/ApplicationMenuBlade/CallAnAPI/appId/" + $currentAppId + "/isMSAApp/"
    Add-Content -Value "<tr><td>client</td><td>$currentAppId</td><td><a href='$clientPortalUrl'>TodoListClient-aspnetcore-webapi</a></td></tr>" -Path createdApps.html
    $requiredResourcesAccess = New-Object System.Collections.Generic.List[Microsoft.Graph.PowerShell.Models.MicrosoftGraphRequiredResourceAccess]
    # Add Required Resources Access (from 'HybridFlow-aspnetcore' to 'Microsoft Graph')
    Write-Host "Getting access from 'HybridFlow-aspnetcore' to 'Microsoft Graph'"
    $requiredPermissions = GetRequiredPermissions -applicationDisplayName "Microsoft Graph" `
        -requiredDelegatedPermissions "User.Read|Contacts.Read";

    $requiredResourcesAccess.Add($requiredPermissions)


    Update-MgApplication -ApplicationId $hybridAadApplication.Id -RequiredResourceAccess $requiredResourcesAccess
    Write-Host "Granted permissions."

    $tenantName = (Get-MgApplication -ApplicationId $hybridAadApplication.Id).PublisherDomain

    # Update config file for 'client'
    $configFile = $pwd.Path + "\..\appsettings.json"
    Write-Host "Updating the sample code ($configFile)"
    $certificateDescriptor = [ordered]@{"SourceType" = "StoreWithDistinguishedName"; "CertificateStorePath" = "CurrentUser/My"; "CertificateDistinguishedName" = "CN=HybridFlowCert" };
    $azureAdSettings = [ordered]@{ "Instance" = "https://login.microsoftonline.com/"; "ClientId" = $hybridAadApplication.AppId; "Domain" = $tenantName; "TenantId" = $tenantId; "CallbackPath" = "/signin/"; "Certificate" = $certificateDescriptor; };
    $downstreamApiSettings = [ordered]@{ "BaseUrl" = "https://graph.microsoft.com/v1.0"; "Scopes" = "user.read contacts.read"; };
    $loggingSettings = @{ "LogLevel" = @{ "Default" = "Warning" } };
    $dictionary = [ordered]@{ "AzureAd" = $azureAdSettings; "Logging" = $loggingSettings; "AllowedHosts" = "*"; "DownstreamApi" = $downstreamApiSettings; "SpaRedirectUri" = "https://localhost:7089/";  };
    $dictionary | ConvertTo-Json | Out-File $configFile
    Write-Host ""
    Write-Host -ForegroundColor Green "------------------------------------------------------------------------------------------------" 
     
    Add-Content -Value "</tbody></table></body></html>" -Path createdApps.html  

    if ($isOpenSSL -eq 'Y') {
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
