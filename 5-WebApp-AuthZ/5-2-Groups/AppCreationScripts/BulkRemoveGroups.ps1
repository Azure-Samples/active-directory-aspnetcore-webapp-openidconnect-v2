
[CmdletBinding()]
param(
    [PSCredential] $Credential,
    [Parameter(Mandatory = $False, HelpMessage = 'Tenant ID (This is a GUID which represents the "Directory ID" of the AzureAD tenant into which you want to create the apps')]
    [string] $tenantId,
    [Parameter(Mandatory = $False, HelpMessage = 'Azure environment to use while running the script (it defaults to AzureCloud)')]
    [string] $azureEnvironmentName
)

<#.Description
    This function generates groups names.
#> 
Function GetGroupName([int] $val) 
{

    if ($val -lt 10) 
    {
        $groupName = "Test Group 00" + $val;
    }
    elseif ($val -lt 100) 
    { 
        $groupName = "Test Group 0" + $val;
    }
    else 
    {
        $groupName = "Test Group " + $val;
    }

    return $groupName;

}

<#.Description
   This function removes security groups from tenant
#> 
Function RemoveGroups
{
    $val = 1;
    while ($val -ne 223) 
    {
       
        $groupName = GetGroupName -val $val
       
        $group = Get-MgGroup -Filter "DisplayName eq '$groupName'"
        if ($group) 
        {
            try
            {
                Remove-MgGroup -GroupId $group.Id
                Write-Host "Successfully deleted group named '$($group.DisplayName)'"
            }
            catch 
            {
                $_.Exception.ToString() | out-host
                $message = $_
                Write-Warning $Error[0]
                Write-Host "Unable to remove group '$($newsg.DisplayName)'. Error is $message." -ForegroundColor White -BackgroundColor Red
            }
        }
        else 
        {
            Write-Host "Couldn't find group with name '$($groupName)' "
        }
       
       
        $val += 1;
    }
}


<#.Description
    This function signs in the user to the tenant using Graph SDK.
#>
Function ConfigureApplications 
{
    if (!$azureEnvironmentName) 
    {
        $azureEnvironmentName = "Global"
    }

    Write-Host "Connecting to Microsoft Graph"

    if ($tenantId -eq "") 
    {
        Connect-MgGraph -Scopes "User.Read.All Organization.Read.All Group.ReadWrite.All" -Environment $azureEnvironmentName
    }
    else 
    {
        Connect-MgGraph -TenantId $tenantId -Scopes "Organization.Read.All Group.ReadWrite.All" -Environment $azureEnvironmentName
    }
            
    $context = Get-MgContext
    $tenantId = $context.TenantId

    # Get the user running the script
    $currentUserPrincipalName = $context.Account
    $user = Get-MgUser -Filter "UserPrincipalName eq '$currentUserPrincipalName'"

    # get the tenant we signed in to
    $Tenant = Get-MgOrganization
    $tenantName = $Tenant.DisplayName
    
    $verifiedDomain = $Tenant.VerifiedDomains | where {$_.Isdefault -eq $true}
    $verifiedDomainName = $verifiedDomain.Name
    $tenantId = $Tenant.Id

    Write-Host ("Connected to Tenant {0} ({1}) as account '{2}'. Domain is '{3}'" -f  $Tenant.DisplayName, $Tenant.Id, $currentUserPrincipalName, $verifiedDomainName)

    # now remove groups
    RemoveGroups

}


$ErrorActionPreference = "Stop"

if ($null -eq (Get-Module -ListAvailable -Name "Microsoft.Graph.Authentication")) 
{
    Install-Module "Microsoft.Graph.Authentication" -Scope CurrentUser 
}

Import-Module Microsoft.Graph.Authentication

if ($null -eq (Get-Module -ListAvailable -Name "Microsoft.Graph.Groups")) 
{
    Install-Module "Microsoft.Graph.Groups" -Scope CurrentUser 
}

Import-Module Microsoft.Graph.Groups

if ($null -eq (Get-Module -ListAvailable -Name "Microsoft.Graph.Users")) 
{
    Install-Module "Microsoft.Graph.Users" -Scope CurrentUser 
}

Import-Module Microsoft.Graph.Users

try 
{
    ConfigureApplications -tenantId $tenantId -environment $azureEnvironmentName
}
catch 
{
    $_.Exception.ToString() | out-host
    $message = $_
    Write-Warning $Error[0]
    Write-Host "Unable to register apps. Error is $message." -ForegroundColor White -BackgroundColor Red
}

Write-Host "Disconnecting from tenant"
Disconnect-MgGraph