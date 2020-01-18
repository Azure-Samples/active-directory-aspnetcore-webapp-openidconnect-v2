[CmdletBinding()]
param(
    [PSCredential] $Credential,
    [Parameter(Mandatory=$False, HelpMessage='Tenant ID (This is a GUID which represents the "Directory ID" of the AzureAD tenant into which you want to create the apps')]
    [string] $tenantId
)

<#
 This script creates the following artefacts in the Azure AD tenant.
 1) A number of App roles
 2) A set of users and assigns them to the app roles.

 Before running this script you need to install the AzureAD cmdlets as an administrator. 
 For this:
 1) Run Powershell as an administrator
 2) in the PowerShell window, type: Install-Module AzureAD

 There are four ways to run this script. For more information, read the AppCreationScripts.md file in the same folder as this script.
#>

$ErrorActionPreference = "Stop"

# Create an application role of given name and description
Function CreateAppRole([string] $Name, [string] $Description)
{
    $appRole = New-Object Microsoft.Open.AzureAD.Model.AppRole
    $appRole.AllowedMemberTypes = New-Object System.Collections.Generic.List[string]
    $appRole.AllowedMemberTypes.Add("User");
    $appRole.DisplayName = $Name
    $appRole.Id = New-Guid
    $appRole.IsEnabled = $true
    $appRole.Description = $Description
    $appRole.Value = $Name;
    return $appRole
}

Function CreateUserRepresentingAppRole([string]$appName, $role, [string]$tenantName)
{
    $password = "test123456789."
    $displayName = $appName +"-" + $role.Value
    $userEmail = $displayName + "@" + $tenantName
    $nickName = $role.Value

    CreateUser -displayName $displayName -nickName $nickName -tenantName $tenantName
}

Function CreateUser([string]$displayName, [string]$nickName, [string]$tenantName)
{
    $password = "test123456789."
    $userEmail = $displayName + "@" + $tenantName
    $passwordProfile = New-Object Microsoft.Open.AzureAD.Model.PasswordProfile($password, $false, $false)

    New-AzureADUser -DisplayName $displayName -PasswordProfile $passwordProfile -AccountEnabled $true -MailNickName $nickName -UserPrincipalName $userEmail
}

Function CreateRolesUsersAndRoleAssignments
{
<#.Description
   This function creates the 
#> 

    # $tenantId is the Active Directory Tenant. This is a GUID which represents the "Directory ID" of the AzureAD tenant
    # into which you want to create the apps. Look it up in the Azure portal in the "Properties" of the Azure AD.

    # Login to Azure PowerShell (interactive if credentials are not already provided:
    # you'll need to sign-in with creds enabling your to create apps in the tenant)
    if (!$Credential -and $TenantId)
    {
        $creds = Connect-AzureAD -TenantId $tenantId
    }
    else
    {
        if (!$TenantId)
        {
            $creds = Connect-AzureAD -Credential $Credential
        }
        else
        {
            $creds = Connect-AzureAD -TenantId $tenantId -Credential $Credential
        }
    }

    if (!$tenantId)
    {
        $tenantId = $creds.Tenant.Id
    }

    $tenant = Get-AzureADTenantDetail
    $tenantName =  ($tenant.VerifiedDomains | Where { $_._Default -eq $True }).Name

    # Get the user running the script
    $user = Get-AzureADUser -ObjectId $creds.Account.Id
    
    # Add the roles
    Write-Host "Adding app roles to to the app 'WebApp-RolesClaims' in tenant '$tenantName'"

    $app=Get-AzureADApplication -Filter "DisplayName eq 'WebApp-RolesClaims'" 
    
    if ($app)
    {
        $servicePrincipal = Get-AzureADServicePrincipal -Filter "AppId eq '$($app.AppId)'"
        
        $directoryViewerRole = $servicePrincipal.AppRoles | Where-Object { $_.DisplayName -eq "DirectoryViewers" }
        $userreaderRole = $servicePrincipal.AppRoles | Where-Object { $_.DisplayName -eq "UserReaders" }
        
        $appName = $app.DisplayName
        
        Write-Host "Creating users and assigning them to roles."

        # Create users
        # ------
        # Make sure that the user who is running this script is assigned to the Directory viewer role
        Write-Host "Adding '$($user.DisplayName)' as a member of the '$($directoryViewerRole.DisplayName)' role"
        $userAssignment = New-AzureADUserAppRoleAssignment -ObjectId $user.ObjectId -PrincipalId $user.ObjectId -ResourceId $servicePrincipal.ObjectId -Id $directoryViewerRole.Id

        # Creating a directory viewer
        Write-Host "Creating a new user and assigning to '$($directoryViewerRole.DisplayName)' role"
        $aDirectoryViewer = CreateUserRepresentingAppRole $appName $directoryViewerRole $tenantName
        $userAssignment = New-AzureADUserAppRoleAssignment -ObjectId $aDirectoryViewer.ObjectId -PrincipalId $aDirectoryViewer.ObjectId -ResourceId $servicePrincipal.ObjectId -Id $directoryViewerRole.Id
        Write-Host "Created user "($aDirectoryViewer.UserPrincipalName)" with password 'test123456789.'"

        # Creating a users reader
        Write-Host "Creating a user and assigning to '$($userreaderRole.DisplayName)' role"
        $auserreader = CreateUserRepresentingAppRole $appName $userreaderRole $tenantName
        $userAssignment = New-AzureADUserAppRoleAssignment -ObjectId $auserreader.ObjectId -PrincipalId $auserreader.ObjectId -ResourceId $servicePrincipal.ObjectId -Id $userreaderRole.Id
        Write-Host "Created user "($auserreader.UserPrincipalName)" with password 'test123456789.'"
    }
    else {
        Write-Host -ForegroundColor Red "Failed to add app roles to the app 'WebApp-RolesClaims'."
    }

    Write-Host -ForegroundColor Green "Run the ..\CleanupUsersAndRoles.ps1 command to remove users created for this sample's application ."
}

# Pre-requisites
if ((Get-Module -ListAvailable -Name "AzureAD") -eq $null) { 
    Install-Module "AzureAD" -Scope CurrentUser 
} 
Import-Module AzureAD
$ErrorActionPreference = 'Stop'

CreateRolesUsersAndRoleAssignments -Credential $Credential -tenantId $TenantId
