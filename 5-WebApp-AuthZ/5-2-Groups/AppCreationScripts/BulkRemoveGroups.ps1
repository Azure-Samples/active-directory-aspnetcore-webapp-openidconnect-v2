<#
 Use this script to remove the 200+ groups created in your Azure AD tenant by BulkCreateGroups.ps1
#>

$ErrorActionPreference = "Stop"

$groupNamePrefix = "TestGroup"
$numberOfGroupsToDelete = 222;

for($i = 1; $i -le $numberOfGroupsToDelete; $i++)
{
  $groupName = $groupNamePrefix + $i
  $group = Get-AzureADGroup -SearchString $groupName
  Remove-AzureADGroup -ObjectId $group.ObjectId
  Write-Host "Successfully deleted $($group.DisplayName)"
}
