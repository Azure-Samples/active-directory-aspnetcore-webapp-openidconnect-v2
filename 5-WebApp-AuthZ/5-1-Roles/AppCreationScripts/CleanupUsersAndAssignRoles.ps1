
[CmdletBinding()]
param(
    [Parameter(Mandatory=$False, HelpMessage='Tenant ID (This is a GUID which represents the "Directory ID" of the AzureAD tenant into which you want to create the apps')]
    [string] $tenantId,
    [Parameter(Mandatory=$False, HelpMessage='Azure environment to use while running the script. Default = Global')]
    [string] $azureEnvironmentName
)

Function RemoveUser([string]$userPrincipal)
{
    Remove-MgUser -UserId $userPrincipal
}

Function CleanupRolesUsersAndRoleAssignments
{
    if (!$azureEnvironmentName)
    {
        $azureEnvironmentName = "Global"
    }

    Write-Host "Connecting to Microsoft Graph"

    if ($tenantId -eq "") 
    {
        Connect-MgGraph -Scopes "Organization.Read.All Application.Read.All AppRoleAssignment.ReadWrite.All User.ReadWrite.All" -Environment $azureEnvironmentName
    }
    else 
    {
        Connect-MgGraph -TenantId $tenantId -Scopes "Organization.Read.All Application.Read.All AppRoleAssignment.ReadWrite.All User.ReadWrite.All" -Environment $azureEnvironmentName
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

    #$split = $currentUserPrincipalName.Split("@")
    $tenantName = $verifiedDomainName
    Write-Host "get the AAD application (WebApp-RolesClaims)"
    $app = Get-MgApplication -Filter "DisplayName eq 'WebApp-RolesClaims'" 
    if ($app)
    {
        $appName = $app.DisplayName
        $userEmail =  $appName +"-" + "UserReaders" + "@" + $tenantName
        RemoveUser -userPrincipal $userEmail
        Write-Host "user name ($userEmail)"
        $userEmail =  $appName +"-" + "DirectoryViewers" + "@" + $tenantName
        RemoveUser -userPrincipal $userEmail
        Write-Host "user name ($userEmail)"
    }
    else
    {
        Write-Host "Couldn't find application (WebApp-RolesClaims)"  -BackgroundColor Red
    }
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

if ($null -eq (Get-Module -ListAvailable -Name "Microsoft.Graph.Authentication")) {
    Install-Module "Microsoft.Graph.Authentication" -Scope CurrentUser 
}

Import-Module Microsoft.Graph.Authentication

if ($null -eq (Get-Module -ListAvailable -Name "Microsoft.Graph.Applications")) {
    Install-Module "Microsoft.Graph.Applications" -Scope CurrentUser 
}

Import-Module Microsoft.Graph.Applications

if ($null -eq (Get-Module -ListAvailable -Name "Microsoft.Graph.Users")) {
    Install-Module "Microsoft.Graph.Users" -Scope CurrentUser 
}

Import-Module Microsoft.Graph.Users

try
{
    # Run interactively (will ask you for the tenant ID)
    CleanupRolesUsersAndRoleAssignments -tenantId $tenantId -environment $azureEnvironmentName
}
catch
{
    $_.Exception.ToString() | out-host
    $message = $_
    Write-Warning $Error[0]    
    Write-Host "Unable to cleanup app roles and assignments. Error is $message." -ForegroundColor White -BackgroundColor Red
}

Write-Host "Disconnecting from tenant"
Disconnect-MgGraph