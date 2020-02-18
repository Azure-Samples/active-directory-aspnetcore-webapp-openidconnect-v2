[CmdletBinding()]
param(    
    [PSCredential] $Credential,
    [Parameter(Mandatory=$False, HelpMessage='Tenant ID (This is a GUID which represents the "Directory ID" of the AzureAD tenant into which you want to create the apps')]
    [string] $tenantId
)

if ($null -eq (Get-Module -ListAvailable -Name "AzureAD")) { 
    Install-Module "AzureAD" -Scope CurrentUser 
} 
Import-Module AzureAD
$ErrorActionPreference = "Stop"

Function Cleanup
{
<#
.Description
This function removes the Azure AD applications for the sample. These applications were created by the Configure.ps1 script
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
    $tenantName =  ($tenant.VerifiedDomains | Where-Object { $_._Default -eq $True }).Name
    
    # Removes the applications
    Write-Host "Cleaning-up applications from tenant '$tenantName'"

    Write-Host "Removing 'service' (TodoListService-aspnetcore-webapi) if needed"
    Get-AzureADApplication -Filter "DisplayName eq 'TodoListService-aspnetcore-webapi'"  | ForEach-Object {Remove-AzureADApplication -ObjectId $_.ObjectId }
    $apps = Get-AzureADApplication -Filter "DisplayName eq 'TodoListService-aspnetcore-webapi'"
    if ($apps)
    {
        Remove-AzureADApplication -ObjectId $apps.ObjectId
    }

    foreach ($app in $apps) 
    {
        Remove-AzureADApplication -ObjectId $app.ObjectId
        Write-Host "Removed TodoListService-aspnetcore-webapi.."
    }
    # also remove service principals of this app
    Get-AzureADServicePrincipal -filter "DisplayName eq 'TodoListService-aspnetcore-webapi'" | ForEach-Object {Remove-AzureADServicePrincipal -ObjectId $_.Id -Confirm:$false}
    
    Write-Host "Removing 'client' (TodoListClient-aspnetcore-webapi) if needed"
    Get-AzureADApplication -Filter "DisplayName eq 'TodoListClient-aspnetcore-webapi'"  | ForEach-Object {Remove-AzureADApplication -ObjectId $_.ObjectId }
    $apps = Get-AzureADApplication -Filter "DisplayName eq 'TodoListClient-aspnetcore-webapi'"
    if ($apps)
    {
        Remove-AzureADApplication -ObjectId $apps.ObjectId
    }

    foreach ($app in $apps) 
    {
        Remove-AzureADApplication -ObjectId $app.ObjectId
        Write-Host "Removed TodoListClient-aspnetcore-webapi.."
    }
    # also remove service principals of this app
    Get-AzureADServicePrincipal -filter "DisplayName eq 'TodoListClient-aspnetcore-webapi'" | ForEach-Object {Remove-AzureADServicePrincipal -ObjectId $_.Id -Confirm:$false}
    
}

Cleanup -Credential $Credential -tenantId $TenantId