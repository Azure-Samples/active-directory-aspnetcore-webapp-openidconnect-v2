
[CmdletBinding()]
param(    
    [Parameter(Mandatory=$False, HelpMessage='Tenant ID (This is a GUID which represents the "Directory ID" of the AzureAD tenant into which you want to create the apps')]
    [string] $tenantId,
    [Parameter(Mandatory=$False, HelpMessage='Azure environment to use while running the script. Default = Global')]
    [string] $azureEnvironmentName
)

if ($null -eq (Get-Module -ListAvailable -Name "Microsoft.Graph.Groups")) {
    Install-Module "Microsoft.Graph.Groups" -Scope CurrentUser 
}

Import-Module Microsoft.Graph.Groups

<#.Description
   This function creates a new Azure AD Security Group with provided values
#>  
Function CreateSecurityGroup([string] $name, [string] $description)
{
    Write-Host "Creating a security group by the name '$name'."
    $newGroup = New-MgGroup -Description $description -DisplayName $name -MailEnabled:$false -SecurityEnabled:$true -MailNickName $name
    return Get-MgGroup -Filter "DisplayName eq '$name'" 
}

<#.Description
   This function first checks and then creates a new Azure AD Security Group with provided values, if required
#>  
Function CreateIfNotExistsSecurityGroup([string] $name, [string] $description,  [switch] $promptBeforeCreate)
{

    # check if Group exists
    $group = Get-MgGroup -Filter "DisplayName eq '$name'"    
    
    if( $group -eq $null)
    {
        if ($promptBeforeCreate) 
        {
            $confirmation = Read-Host "Proceed to create a new security group named '$name' in the tenant ? (Y/N)"

            if($confirmation -eq 'y')
            {
                $group = CreateSecurityGroup -name $name -description $description
            }
        }
        else
        {
            Write-Host "No Security Group created!"
        }     
    }
    
    return $group    
}

<#.Description
   This function first checks and then deletes an existing Azure AD Security Group, if required
#>  
Function RemoveSecurityGroup([string] $name, [switch] $promptBeforeDelete)
{

    # check if Group exists
    $group = Get-MgGroup -Filter "DisplayName eq '$name'"
    
    if( $group -ne $null)
    {
        if ($promptBeforeDelete) 
        {
            $confirmation = Read-Host "Proceed to delete an existing group named '$name' in the tenant ?(Y/N)"

            if($confirmation -eq 'y')
            {
               Remove-MgGroup -GroupId $group.Id
               Write-Host "Security group '$name' successfully deleted"
            }
        }
        else
        {
            Write-Host "No Security group by name '$name' exists in the tenant, no deletion needed."
        }     
    }
    
    return $group.Id    
}

<#.Description
   This function assigns a provided user to a security group
#>  
Function AssignUserToGroup([Microsoft.Graph.PowerShell.Models.MicrosoftGraphUser]$userToAssign, [Microsoft.Graph.PowerShell.Models.MicrosoftGraphGroup]$groupToAssign)
{
    $owneruserId = $userToAssign.Id
    $params = @{
        "@odata.id" = "https://graph.microsoft.com/v1.0/directoryObjects/{$owneruserId}"
    }

    New-MgGroupMemberByRef -GroupId $groupToAssign.Id -BodyParameter $params
    Write-Host "Successfully assigned user '$($userToAssign.UserPrincipalName)' to group '$($groupToAssign.DisplayName)'"
}

Function Cleanup
{
    if (!$azureEnvironmentName)
    {
        $azureEnvironmentName = "Global"
    }

    <#
    .Description
    This function removes the Azure AD applications for the sample. These applications were created by the Configure.ps1 script
    #>

    # $tenantId is the Active Directory Tenant. This is a GUID which represents the "Directory ID" of the AzureAD tenant 
    # into which you want to create the apps. Look it up in the Azure portal in the "Properties" of the Azure AD. 

    # Connect to the Microsoft Graph API
    Write-Host "Connecting to Microsoft Graph"


    if ($tenantId -eq "") 
    {
        Connect-MgGraph -Scopes "User.Read.All Organization.Read.All Application.ReadWrite.All Group.ReadWrite.All" -Environment $azureEnvironmentName
    }
    else 
    {
        Connect-MgGraph -TenantId $tenantId -Scopes "User.Read.All Organization.Read.All Application.ReadWrite.All Group.ReadWrite.All" -Environment $azureEnvironmentName
    }
    
    $context = Get-MgContext
    $tenantId = $context.TenantId

    # Get the user running the script
    $currentUserPrincipalName = $context.Account
    $user = Get-MgUser -Filter "UserPrincipalName eq '$($context.Account)'"

    # get the tenant we signed in to
    $Tenant = Get-MgOrganization
    $tenantName = $Tenant.DisplayName
    
    $verifiedDomain = $Tenant.VerifiedDomains | where {$_.Isdefault -eq $true}
    $verifiedDomainName = $verifiedDomain.Name
    $tenantId = $Tenant.Id

    Write-Host ("Connected to Tenant {0} ({1}) as account '{2}'. Domain is '{3}'" -f  $Tenant.DisplayName, $Tenant.Id, $currentUserPrincipalName, $verifiedDomainName)

    # Removes the applications
    Write-Host "Cleaning-up applications from tenant '$tenantId'"

    Write-Host "Removing 'webApp' (WebApp-GroupClaims) if needed"
    try
    {
        Get-MgApplication -Filter "DisplayName eq 'WebApp-GroupClaims'" | ForEach-Object {Remove-MgApplication -ApplicationId $_.Id }
    }
    catch
    {
        $message = $_
        Write-Warning $Error[0]
        Write-Host "Unable to remove the application 'WebApp-GroupClaims'. Error is $message. Try deleting manually." -ForegroundColor White -BackgroundColor Red
    }

    Write-Host "Making sure there are no more (WebApp-GroupClaims) applications found, will remove if needed..."
    $apps = Get-MgApplication -Filter "DisplayName eq 'WebApp-GroupClaims'" | Format-List Id, DisplayName, AppId, SignInAudience, PublisherDomain
    
    if ($apps)
    {
        Remove-MgApplication -ApplicationId $apps.Id
    }

    foreach ($app in $apps) 
    {
        Remove-MgApplication -ApplicationId $app.Id
        Write-Host "Removed WebApp-GroupClaims.."
    }

    # also remove service principals of this app
    try
    {
        Get-MgServicePrincipal -filter "DisplayName eq 'WebApp-GroupClaims'" | ForEach-Object {Remove-MgServicePrincipal -ServicePrincipalId $_.Id -Confirm:$false}
    }
    catch
    {
        $message = $_
        Write-Warning $Error[0]
        Write-Host "Unable to remove ServicePrincipal 'WebApp-GroupClaims'. Error is $message. Try deleting manually from Enterprise applications." -ForegroundColor White -BackgroundColor Red
    }

    # remove security groups, if relevant to the sample
    RemoveSecurityGroup -name 'GroupAdmin' -promptBeforeDelete 'Y'
    RemoveSecurityGroup -name 'GroupMember' -promptBeforeDelete 'Y'
}

# Pre-requisites
if ($null -eq (Get-Module -ListAvailable -Name "Microsoft.Graph")) {
    Install-Module "Microsoft.Graph" -Scope CurrentUser 
}

#Import-Module Microsoft.Graph

if ($null -eq (Get-Module -ListAvailable -Name "Microsoft.Graph.Authentication")) {
    Install-Module "Microsoft.Graph.Authentication" -Scope CurrentUser 
}

Import-Module Microsoft.Graph.Authentication

if ($null -eq (Get-Module -ListAvailable -Name "Microsoft.Graph.Identity.DirectoryManagement")) {
    Install-Module "Microsoft.Graph.Identity.DirectoryManagement" -Scope CurrentUser 
}

Import-Module Microsoft.Graph.Identity.DirectoryManagement

if ($null -eq (Get-Module -ListAvailable -Name "Microsoft.Graph.Applications")) {
    Install-Module "Microsoft.Graph.Applications" -Scope CurrentUser 
}

Import-Module Microsoft.Graph.Applications

if ($null -eq (Get-Module -ListAvailable -Name "Microsoft.Graph.Groups")) {
    Install-Module "Microsoft.Graph.Groups" -Scope CurrentUser 
}

Import-Module Microsoft.Graph.Groups

if ($null -eq (Get-Module -ListAvailable -Name "Microsoft.Graph.Users")) {
    Install-Module "Microsoft.Graph.Users" -Scope CurrentUser 
}

Import-Module Microsoft.Graph.Users

$ErrorActionPreference = "Stop"


try
{
    Cleanup -tenantId $tenantId -environment $azureEnvironmentName
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
