[CmdletBinding()]
param(    
    [PSCredential] $Credential,
    [Parameter(Mandatory=$False, HelpMessage='Tenant ID (This is a GUID which represents the "Directory ID" of the AzureAD tenant into which you want to create the apps')]
    [string] $tenantId
)

Import-Module AzureAD
$ErrorActionPreference = 'Stop'

Function RemoveUser([string]$userPrincipal)
{
    $user = Get-AzureADUser -Filter "UserPrincipalName eq '$userPrincipal'"
    if ($user)
    {
        Write-Host "Removing User '($userPrincipal)'"
        Remove-AzureADUser -ObjectId $user.ObjectId
    }
    else {
        Write-Host "Failed to remove user '($userPrincipal)'"
    }
}

Function CleanupUsers
{
    <#
    .Description
    This function removes the users created in the Azure AD tenant by the CreateUsersAndRoles.ps1 script.
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

    $appName = "WebApp-RolesClaims"

    # Removes the users created for the application
    Write-Host "Removing Users"
    RemoveUser -userPrincipal "$appName-DirectoryViewers@$tenantName"
    RemoveUser -userPrincipal "$appName-UserReaders@$tenantName"

    Write-Host "finished removing  users created for this app." 
}

# Pre-requisites
if ((Get-Module -ListAvailable -Name "AzureAD") -eq $null) { 
    Install-Module "AzureAD" -Scope CurrentUser 
} 
Import-Module AzureAD
$ErrorActionPreference = 'Stop'

CleanupUsers -Credential $Credential -tenantId $TenantId
