[CmdletBinding()]
param(
    [PSCredential] $Credential,
    [Parameter(Mandatory=$False, HelpMessage='Tenant ID (This is a GUID which represents the "Directory ID" of the AzureAD tenant into which you want to create the apps')]
    [string] $tenantId
)

<#
 This script creates the Azure AD applications needed for this sample and updates the configuration files
 for the visual Studio projects from the data in the Azure AD applications.

 Before running this script you need to install the AzureAD cmdlets as an administrator. 
 For this:
 1) Run Powershell as an administrator
 2) in the PowerShell window, type: Install-Module AzureAD

 There are four ways to run this script. For more information, read the AppCreationScripts.md file in the same folder as this script.
#>

Function UpdateLine([string] $line, [string] $value)
{
    $index = $line.IndexOf('=')
    $delimiter = ';'
    if ($index -eq -1)
    {
        $index = $line.IndexOf(':')
        $delimiter = ','
    }
    if ($index -ige 0)
    {
        $line = $line.Substring(0, $index+1) + " "+'"'+$value+'"'+$delimiter
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

Set-Content -Value "<html><body><table>" -Path createdApps.html
Add-Content -Value "<thead><tr><th>Application</th><th>AppId</th><th>Url in the Azure portal</th></tr></thead><tbody>" -Path createdApps.html

$ErrorActionPreference = "Stop"

Function ConfigureApplications
{
<#.Description
   This function creates the Azure AD applications for the sample in the provided Azure AD tenant and updates the
   configuration files in the client and service project  of the visual studio solution (App.Config and Web.Config)
   so that they are consistent with the Applications parameters
#> 
    $commonendpoint = "common"

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

    # Get the user running the script to add the user as the app owner
    $user = Get-AzureADUser -ObjectId $creds.Account.Id

   # Create the webApp AAD application
   Write-Host "Creating the AAD application (WebApp)"
   # create the application 
   $webAppAadApplication = New-AzureADApplication -DisplayName "WebApp" `
                                                  -HomePage "https://localhost:44321/" `
                                                  -LogoutUrl "https://localhost:44321/signout-oidc" `
                                                  -ReplyUrls "https://localhost:44321/", "https://localhost:44321/signin-oidc" `
                                                  -IdentifierUris "https://$tenantName/WebApp" `
                                                  -AvailableToOtherTenants $True `
                                                  -Oauth2AllowImplicitFlow $true `
                                                  -PublicClient $False

   # create the service principal of the newly created application 
   $currentAppId = $webAppAadApplication.AppId
   $webAppServicePrincipal = New-AzureADServicePrincipal -AppId $currentAppId -Tags {WindowsAzureActiveDirectoryIntegratedApp}

   # add the user running the script as an app owner if needed
   $owner = Get-AzureADApplicationOwner -ObjectId $webAppAadApplication.ObjectId
   if ($owner -eq $null)
   { 
        Add-AzureADApplicationOwner -ObjectId $webAppAadApplication.ObjectId -RefObjectId $user.ObjectId
        Write-Host "'$($user.UserPrincipalName)' added as an application owner to app '$($webAppServicePrincipal.DisplayName)'"
   }


   Write-Host "Done creating the webApp application (WebApp)"

   # URL of the AAD application in the Azure portal
   # Future? $webAppPortalUrl = "https://portal.azure.com/#@"+$tenantName+"/blade/Microsoft_AAD_RegisteredApps/ApplicationMenuBlade/Overview/appId/"+$webAppAadApplication.AppId+"/objectId/"+$webAppAadApplication.ObjectId+"/isMSAApp/"
   $webAppPortalUrl = "https://portal.azure.com/#blade/Microsoft_AAD_RegisteredApps/ApplicationMenuBlade/CallAnAPI/appId/"+$webAppAadApplication.AppId+"/objectId/"+$webAppAadApplication.ObjectId+"/isMSAApp/"
   Add-Content -Value "<tr><td>webApp</td><td>$currentAppId</td><td><a href='$webAppPortalUrl'>WebApp</a></td></tr>" -Path createdApps.html


   # Update config file for 'webApp'
   $configFile = $pwd.Path + "\..\appsettings.json"
   Write-Host "Updating the sample code ($configFile)"
   $dictionary = @{ "ClientId" = $webAppAadApplication.AppId;"TenantId" = "organizations";"Domain" = $tenantName };
   UpdateTextFile -configFilePath $configFile -dictionary $dictionary
  
   Add-Content -Value "</tbody></table></body></html>" -Path createdApps.html  
}

# Pre-requisites
if ((Get-Module -ListAvailable -Name "AzureAD") -eq $null) { 
    Install-Module "AzureAD" -Scope CurrentUser 
}

Import-Module AzureAD

# Run interactively (will ask you for the tenant ID)
ConfigureApplications -Credential $Credential -tenantId $TenantId