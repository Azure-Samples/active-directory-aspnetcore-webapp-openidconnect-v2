<#
 Use this script to create 200+ groups in your Azure AD tenant and assign a user to it.

 Use the companion script BulkRemoveGroups.ps1 to remove these security groups from your tenant.
#>

$ErrorActionPreference = "Stop"

 # ObjectId of the user to be assigned to these security groups. The ObjectId can be obtained via Graph Explorer or in the "Users" blade on the portal.
$usersobjectId = "695a3e1d-2e9f-4d24-aa3c-ac795c16f25c"

Get-AzureADUser -ObjectId $usersobjectId

$groupNamePrefix = "TestGroup"
$numberOfGroupsToCreate = 222;

for($i = 1; $i -le $numberOfGroupsToCreate; $i++)
{
  $groupName = $groupNamePrefix + $i

  # create a new group
  $newsg = New-AzureADGroup -Description $groupName -DisplayName $groupName -MailEnabled $false -SecurityEnabled $true -MailNickName $groupName
  Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
  Write-Host "Successfully created $($newsg.DisplayName)"
}
