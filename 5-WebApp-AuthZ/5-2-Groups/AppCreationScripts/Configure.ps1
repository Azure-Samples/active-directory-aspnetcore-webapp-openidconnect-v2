 
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
 
 There are two ways to run this script. For more information, read the AppCreationScripts.md file in the same folder as this script.
#>

# Create an application key
# See https://www.sabin.io/blog/adding-an-azure-active-directory-application-and-key-using-powershell/
Function CreateAppKey([DateTime] $fromDate, [double] $durationInMonths)
{
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


<#.Description
   This function takes a string input as a single line, matches a key value and replaces with the replacement value
#> 
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

<#.Description
   This function takes a dictionary of keys to search and their replacements and replaces the placeholders in a text file
#> 
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

<#.Description
   This function takes a string as input and creates an instance of an Optional claim object
#> 
Function CreateOptionalClaim([string] $name)
{
    <#.Description
    This function creates a new Azure AD optional claims  with default and provided values
    #>  

    $appClaim = New-Object Microsoft.Graph.PowerShell.Models.MicrosoftGraphOptionalClaim
    $appClaim.AdditionalProperties =  New-Object System.Collections.Generic.List[string]
    $appClaim.Source =  $null
    $appClaim.Essential = $false
    $appClaim.Name = $name
    return $appClaim
}

<#.Description
   Primary entry method to create and configure app registrations
#> 
Function ConfigureApplications
{
    $isOpenSSl = 'N' #temporary disable open certificate creation 

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
        Connect-MgGraph -Scopes "User.Read.All Organization.Read.All Application.ReadWrite.All Group.ReadWrite.All" -Environment $azureEnvironmentName
    }
    else {
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

   # Create the webApp AAD application
   Write-Host "Creating the AAD application (WebApp-GroupClaims)"
   # Get a 6 months application key for the webApp Application
   $fromDate = [DateTime]::Now;
   $key = CreateAppKey -fromDate $fromDate -durationInMonths 6
   
   # create the application 
   $webAppAadApplication = New-MgApplication -DisplayName "WebApp-GroupClaims" `
                                                      -Web `
                                                      @{ `
                                                          RedirectUris = "https://localhost:44321/", "https://localhost:44321/signin-oidc"; `
                                                          HomePageUrl = "https://localhost:44321/"; `
                                                          LogoutUrl = "https://localhost:44321/signout-oidc"; `
                                                          ImplicitGrantSettings = @{ `
                                                              EnableIdTokenIssuance=$true; `
                                                          } `
                                                        } `
                                                       -SignInAudience AzureADMyOrg `
                                                      -GroupMembershipClaims "SecurityGroup" `
                                                      #end of command

    #add a secret to the application
    $pwdCredential = Add-MgApplicationPassword -ApplicationId $webAppAadApplication.Id -PasswordCredential $key
    $webAppAppKey = $pwdCredential.SecretText

    $currentAppId = $webAppAadApplication.AppId
    $currentAppObjectId = $webAppAadApplication.Id

    $tenantName = (Get-MgApplication -ApplicationId $currentAppObjectId).PublisherDomain
    #Update-MgApplication -ApplicationId $currentAppObjectId -IdentifierUris @("https://$tenantName/WebApp-GroupClaims")
    
    # create the service principal of the newly created application     
    $webAppServicePrincipal = New-MgServicePrincipal -AppId $currentAppId -Tags {WindowsAzureActiveDirectoryIntegratedApp}

    # add the user running the script as an app owner if needed
    $owner = Get-MgApplicationOwner -ApplicationId $currentAppObjectId
    if ($owner -eq $null)
    { 
        New-MgApplicationOwnerByRef -ApplicationId $currentAppObjectId  -BodyParameter = @{"@odata.id" = "htps://graph.microsoft.com/v1.0/directoryObjects/$user.ObjectId"}
        Write-Host "'$($user.UserPrincipalName)' added as an application owner to app '$($webAppServicePrincipal.DisplayName)'"
    }

    # Add Claims

    $optionalClaims = New-Object Microsoft.Graph.PowerShell.Models.MicrosoftGraphOptionalClaims
    $optionalClaims.AccessToken = New-Object System.Collections.Generic.List[Microsoft.Graph.PowerShell.Models.MicrosoftGraphOptionalClaim]
    $optionalClaims.IdToken = New-Object System.Collections.Generic.List[Microsoft.Graph.PowerShell.Models.MicrosoftGraphOptionalClaim]
    $optionalClaims.Saml2Token = New-Object System.Collections.Generic.List[Microsoft.Graph.PowerShell.Models.MicrosoftGraphOptionalClaim]

    # Add Groups Claims

    $newClaim =  CreateOptionalClaim  -name "groups" 
    $optionalClaims.IdToken += ($newClaim)
    $newClaim =  CreateOptionalClaim  -name "groups" 
    $optionalClaims.AccessToken += ($newClaim)
    # $newClaim =  CreateOptionalClaim  -name "groups" 
    # $optionalClaims.Saml2Token += ($newClaim)

    # Add Optional Claims

    $newClaim =  CreateOptionalClaim  -name "acct" 
    $optionalClaims.IdToken += ($newClaim)
    Update-MgApplication -ApplicationId $currentAppObjectId -OptionalClaims $optionalClaims
    Write-Host "Done creating the webApp application (WebApp-GroupClaims)"

    # URL of the AAD application in the Azure portal
    # Future? $webAppPortalUrl = "https://portal.azure.com/#@"+$tenantName+"/blade/Microsoft_AAD_RegisteredApps/ApplicationMenuBlade/Overview/appId/"+$currentAppId+"/objectId/"+$currentAppObjectId+"/isMSAApp/"
    $webAppPortalUrl = "https://portal.azure.com/#blade/Microsoft_AAD_RegisteredApps/ApplicationMenuBlade/CallAnAPI/appId/"+$currentAppId+"/objectId/"+$currentAppObjectId+"/isMSAApp/"

    Add-Content -Value "<tr><td>webApp</td><td>$currentAppId</td><td><a href='$webAppPortalUrl'>WebApp-GroupClaims</a></td></tr>" -Path createdApps.html
    # Declare a list to hold RRA items    
    $requiredResourcesAccess = New-Object System.Collections.Generic.List[Microsoft.Graph.PowerShell.Models.MicrosoftGraphRequiredResourceAccess]

    # Add Required Resources Access (from 'webApp' to 'Microsoft Graph')
    Write-Host "Getting access from 'webApp' to 'Microsoft Graph'"
    $requiredPermission = GetRequiredPermissions -applicationDisplayName "Microsoft Graph"`
        -requiredDelegatedPermissions "User.Read|GroupMember.Read.All"

    $requiredResourcesAccess.Add($requiredPermission)
    Write-Host "Added 'Microsoft Graph' to the RRA list."
    # Useful for RRA additions troubleshooting
    # $requiredResourcesAccess.Count
    # $requiredResourcesAccess
    
    Update-MgApplication -ApplicationId $currentAppObjectId -RequiredResourceAccess $requiredResourcesAccess
    Write-Host "Granted permissions."
    
    # we assign the currently signed-in user to the first security group. The following flag tracks that
    [bool] $ownerAssigned = $false

    # Create any security groups that this app requires.

    $GroupAdmin = CreateIfNotExistsSecurityGroup -name 'GroupAdmin' -description 'Admin Security Group' -promptBeforeCreate 'Y'
    Write-Host "group id of 'GroupAdmin'" -> $GroupAdmin.Id -ForegroundColor Green 

    if ($ownerAssigned -eq $false)
    {
        AssignUserToGroup -userToAssign $user -groupToAssign $GroupAdmin
        $ownerAssigned = $true
    }

    $GroupMember = CreateIfNotExistsSecurityGroup -name 'GroupMember' -description 'User Security Group' -promptBeforeCreate 'Y'
    Write-Host "group id of 'GroupMember'" -> $GroupMember.Id -ForegroundColor Green 

    if ($ownerAssigned -eq $false)
    {
        AssignUserToGroup -userToAssign $user -groupToAssign $GroupMember
        $ownerAssigned = $true
    }
    Write-Host "Don't forget to assign the users you wish to work with to the newly created security groups !" -ForegroundColor Red 

    # print the registered app portal URL for any further navigation
    Write-Host "Successfully registered and configured that app registration for 'WebApp-GroupClaims' at `n $webAppPortalUrl" -ForegroundColor Green 
    
    # Update config file for 'webApp'
    # $configFile = $pwd.Path + "\..\appsettings.json"
    $configFile = $(Resolve-Path ($pwd.Path + "\..\appsettings.json"))
    
    $dictionary = @{ "ClientId" = $webAppAadApplication.AppId;"TenantId" = $tenantId;"ClientSecret" = $webAppAppKey;"Enter the objectID for GroupAdmin group copied from Azure Portal" = $GroupAdmin.Id;"Enter the objectID for GroupMember group copied from Azure Portal" = $GroupMember.Id };

    Write-Host "Updating the sample config '$configFile' with the following config values:" -ForegroundColor Yellow 
    $dictionary
    Write-Host "-----------------"

    UpdateTextFile -configFilePath $configFile -dictionary $dictionary
    Write-Host -ForegroundColor Green "------------------------------------------------------------------------------------------------" 
    Write-Host "IMPORTANT: Please follow the instructions below to complete a few manual step(s) in the Azure portal":
    Write-Host "- For webApp"
    Write-Host "  - Navigate to $webAppPortalUrl"
    Write-Host "  - Navigate to the API Permissions page and select 'Grant admin consent for (your tenant)'" -ForegroundColor Red 
    Write-Host "  - Security groups matching the names you provided have been created in this tenant (if not present already). On Azure portal, assign some users to it, and configure ID & Access tokens to emit Group IDs" -ForegroundColor Red 
    Write-Host -ForegroundColor Green "------------------------------------------------------------------------------------------------" 
   
if($isOpenSSL -eq 'Y')
{
    Write-Host -ForegroundColor Green "------------------------------------------------------------------------------------------------" 
    Write-Host "You have generated certificate using OpenSSL so follow below steps: "
    Write-Host "Install the certificate on your system from current folder."
    Write-Host -ForegroundColor Green "------------------------------------------------------------------------------------------------" 
}
Add-Content -Value "</tbody></table></body></html>" -Path createdApps.html  
} # end of ConfigureApplications function

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

Set-Content -Value "<html><body><table>" -Path createdApps.html
Add-Content -Value "<thead><tr><th>Application</th><th>AppId</th><th>Url in the Azure portal</th></tr></thead><tbody>" -Path createdApps.html

$ErrorActionPreference = "Stop"

# Run interactively (will ask you for the tenant ID)

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