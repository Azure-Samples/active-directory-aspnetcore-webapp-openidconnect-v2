<#
 Use this script to create 200+ groups in your Azure AD tenant and assign a user to it.

 Use the companion script BulkRemoveGroups.ps1 to remove these security groups from your tenant.
#>

$ErrorActionPreference = "Stop"

 # ObjectId of the user to be assigned to these security groups. The ObjectId can be obtained via Graph Explorer or in the "Users" blade on the portal.
$usersobjectId = "5b6e08a5-7789-4ae0-a4cb-3d73b4097752"

Get-AzureADUser -ObjectId $usersobjectId

$newsg = New-AzureADGroup -Description "Test Group 001"  -DisplayName "Test Group 001" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup001"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 002"  -DisplayName "Test Group 002" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup002"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 003"  -DisplayName "Test Group 003" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup003"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 004"  -DisplayName "Test Group 004" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup004"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 005"  -DisplayName "Test Group 005" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup005"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 006"  -DisplayName "Test Group 006" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup006"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 007"  -DisplayName "Test Group 007" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup007"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 008"  -DisplayName "Test Group 008" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup008"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 009"  -DisplayName "Test Group 009" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup009"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 010"  -DisplayName "Test Group 010" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup010"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 011"  -DisplayName "Test Group 011" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup011"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 012"  -DisplayName "Test Group 012" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup012"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 013"  -DisplayName "Test Group 013" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup013"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 014"  -DisplayName "Test Group 014" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup014"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 015"  -DisplayName "Test Group 015" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup015"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 016"  -DisplayName "Test Group 016" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup016"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 017"  -DisplayName "Test Group 017" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup017"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 018"  -DisplayName "Test Group 018" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup018"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 019"  -DisplayName "Test Group 019" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup019"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 020"  -DisplayName "Test Group 020" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup020"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 021"  -DisplayName "Test Group 021" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup021"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 022"  -DisplayName "Test Group 022" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup022"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 023"  -DisplayName "Test Group 023" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup023"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 024"  -DisplayName "Test Group 024" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup024"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 025"  -DisplayName "Test Group 025" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup025"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 026"  -DisplayName "Test Group 026" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup026"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 027"  -DisplayName "Test Group 027" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup027"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 028"  -DisplayName "Test Group 028" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup028"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 029"  -DisplayName "Test Group 029" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup029"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 030"  -DisplayName "Test Group 030" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup030"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 031"  -DisplayName "Test Group 031" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup031"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 032"  -DisplayName "Test Group 032" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup032"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 033"  -DisplayName "Test Group 033" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup033"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 034"  -DisplayName "Test Group 034" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup034"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 035"  -DisplayName "Test Group 035" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup035"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 036"  -DisplayName "Test Group 036" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup036"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 037"  -DisplayName "Test Group 037" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup037"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 038"  -DisplayName "Test Group 038" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup038"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 039"  -DisplayName "Test Group 039" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup039"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 040"  -DisplayName "Test Group 040" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup040"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 041"  -DisplayName "Test Group 041" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup041"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 042"  -DisplayName "Test Group 042" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup042"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 043"  -DisplayName "Test Group 043" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup043"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 044"  -DisplayName "Test Group 044" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup044"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 045"  -DisplayName "Test Group 045" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup045"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 046"  -DisplayName "Test Group 046" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup046"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 047"  -DisplayName "Test Group 047" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup047"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 048"  -DisplayName "Test Group 048" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup048"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 049"  -DisplayName "Test Group 049" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup049"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 050"  -DisplayName "Test Group 050" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup050"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 051"  -DisplayName "Test Group 051" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup051"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 052"  -DisplayName "Test Group 052" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup052"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 053"  -DisplayName "Test Group 053" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup053"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 054"  -DisplayName "Test Group 054" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup054"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 055"  -DisplayName "Test Group 055" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup055"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 056"  -DisplayName "Test Group 056" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup056"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 057"  -DisplayName "Test Group 057" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup057"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 058"  -DisplayName "Test Group 058" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup058"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 059"  -DisplayName "Test Group 059" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup059"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 060"  -DisplayName "Test Group 060" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup060"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 061"  -DisplayName "Test Group 061" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup061"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 062"  -DisplayName "Test Group 062" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup062"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 063"  -DisplayName "Test Group 063" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup063"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 064"  -DisplayName "Test Group 064" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup064"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 065"  -DisplayName "Test Group 065" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup065"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 066"  -DisplayName "Test Group 066" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup066"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 067"  -DisplayName "Test Group 067" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup067"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 068"  -DisplayName "Test Group 068" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup068"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 069"  -DisplayName "Test Group 069" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup069"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 070"  -DisplayName "Test Group 070" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup070"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 071"  -DisplayName "Test Group 071" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup071"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 072"  -DisplayName "Test Group 072" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup072"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 073"  -DisplayName "Test Group 073" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup073"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 074"  -DisplayName "Test Group 074" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup074"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 075"  -DisplayName "Test Group 075" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup075"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 076"  -DisplayName "Test Group 076" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup076"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 077"  -DisplayName "Test Group 077" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup077"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 078"  -DisplayName "Test Group 078" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup078"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 079"  -DisplayName "Test Group 079" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup079"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 080"  -DisplayName "Test Group 080" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup080"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 081"  -DisplayName "Test Group 081" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup081"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 082"  -DisplayName "Test Group 082" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup082"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 083"  -DisplayName "Test Group 083" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup083"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 084"  -DisplayName "Test Group 084" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup084"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 085"  -DisplayName "Test Group 085" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup085"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 086"  -DisplayName "Test Group 086" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup086"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 087"  -DisplayName "Test Group 087" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup087"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 088"  -DisplayName "Test Group 088" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup088"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 089"  -DisplayName "Test Group 089" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup089"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 090"  -DisplayName "Test Group 090" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup090"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 091"  -DisplayName "Test Group 091" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup091"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 092"  -DisplayName "Test Group 092" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup092"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 093"  -DisplayName "Test Group 093" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup093"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 094"  -DisplayName "Test Group 094" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup094"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 095"  -DisplayName "Test Group 095" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup095"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 096"  -DisplayName "Test Group 096" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup096"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 097"  -DisplayName "Test Group 097" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup097"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 098"  -DisplayName "Test Group 098" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup098"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 099"  -DisplayName "Test Group 099" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup099"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 100"  -DisplayName "Test Group 100" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup100"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 101"  -DisplayName "Test Group 101" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup101"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 102"  -DisplayName "Test Group 102" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup102"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 103"  -DisplayName "Test Group 103" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup103"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 104"  -DisplayName "Test Group 104" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup104"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 105"  -DisplayName "Test Group 105" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup105"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 106"  -DisplayName "Test Group 106" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup106"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 107"  -DisplayName "Test Group 107" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup107"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 108"  -DisplayName "Test Group 108" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup108"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 109"  -DisplayName "Test Group 109" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup109"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 110"  -DisplayName "Test Group 110" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup110"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 111"  -DisplayName "Test Group 111" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup111"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 112"  -DisplayName "Test Group 112" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup112"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 113"  -DisplayName "Test Group 113" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup113"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 114"  -DisplayName "Test Group 114" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup114"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 115"  -DisplayName "Test Group 115" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup115"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 116"  -DisplayName "Test Group 116" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup116"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 117"  -DisplayName "Test Group 117" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup117"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 118"  -DisplayName "Test Group 118" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup118"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 119"  -DisplayName "Test Group 119" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup119"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 120"  -DisplayName "Test Group 120" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup120"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 121"  -DisplayName "Test Group 121" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup121"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 122"  -DisplayName "Test Group 122" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup122"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 123"  -DisplayName "Test Group 123" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup123"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 124"  -DisplayName "Test Group 124" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup124"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 125"  -DisplayName "Test Group 125" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup125"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 126"  -DisplayName "Test Group 126" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup126"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 127"  -DisplayName "Test Group 127" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup127"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 128"  -DisplayName "Test Group 128" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup128"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 129"  -DisplayName "Test Group 129" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup129"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 130"  -DisplayName "Test Group 130" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup130"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 131"  -DisplayName "Test Group 131" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup131"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 132"  -DisplayName "Test Group 132" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup132"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 133"  -DisplayName "Test Group 133" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup133"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 134"  -DisplayName "Test Group 134" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup134"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 135"  -DisplayName "Test Group 135" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup135"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 136"  -DisplayName "Test Group 136" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup136"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 137"  -DisplayName "Test Group 137" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup137"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 138"  -DisplayName "Test Group 138" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup138"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 139"  -DisplayName "Test Group 139" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup139"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 140"  -DisplayName "Test Group 140" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup140"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 141"  -DisplayName "Test Group 141" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup141"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 142"  -DisplayName "Test Group 142" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup142"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 143"  -DisplayName "Test Group 143" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup143"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 144"  -DisplayName "Test Group 144" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup144"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 145"  -DisplayName "Test Group 145" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup145"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 146"  -DisplayName "Test Group 146" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup146"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 147"  -DisplayName "Test Group 147" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup147"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 148"  -DisplayName "Test Group 148" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup148"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 149"  -DisplayName "Test Group 149" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup149"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 150"  -DisplayName "Test Group 150" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup150"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 151"  -DisplayName "Test Group 151" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup151"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 152"  -DisplayName "Test Group 152" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup152"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 153"  -DisplayName "Test Group 153" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup153"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 154"  -DisplayName "Test Group 154" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup154"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 155"  -DisplayName "Test Group 155" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup155"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 156"  -DisplayName "Test Group 156" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup156"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 157"  -DisplayName "Test Group 157" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup157"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 158"  -DisplayName "Test Group 158" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup158"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 159"  -DisplayName "Test Group 159" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup159"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 160"  -DisplayName "Test Group 160" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup160"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 161"  -DisplayName "Test Group 161" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup161"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 162"  -DisplayName "Test Group 162" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup162"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 163"  -DisplayName "Test Group 163" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup163"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 164"  -DisplayName "Test Group 164" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup164"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 165"  -DisplayName "Test Group 165" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup165"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 166"  -DisplayName "Test Group 166" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup166"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 167"  -DisplayName "Test Group 167" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup167"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 168"  -DisplayName "Test Group 168" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup168"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 169"  -DisplayName "Test Group 169" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup169"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 170"  -DisplayName "Test Group 170" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup170"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 171"  -DisplayName "Test Group 171" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup171"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 172"  -DisplayName "Test Group 172" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup172"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 173"  -DisplayName "Test Group 173" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup173"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 174"  -DisplayName "Test Group 174" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup174"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 175"  -DisplayName "Test Group 175" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup175"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 176"  -DisplayName "Test Group 176" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup176"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 177"  -DisplayName "Test Group 177" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup177"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 178"  -DisplayName "Test Group 178" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup178"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 179"  -DisplayName "Test Group 179" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup179"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 180"  -DisplayName "Test Group 180" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup180"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 181"  -DisplayName "Test Group 181" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup181"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 182"  -DisplayName "Test Group 182" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup182"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 183"  -DisplayName "Test Group 183" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup183"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 184"  -DisplayName "Test Group 184" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup184"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 185"  -DisplayName "Test Group 185" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup185"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 186"  -DisplayName "Test Group 186" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup186"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 187"  -DisplayName "Test Group 187" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup187"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 188"  -DisplayName "Test Group 188" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup188"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 189"  -DisplayName "Test Group 189" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup189"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 190"  -DisplayName "Test Group 190" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup190"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 191"  -DisplayName "Test Group 191" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup191"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 192"  -DisplayName "Test Group 192" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup192"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 193"  -DisplayName "Test Group 193" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup193"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 194"  -DisplayName "Test Group 194" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup194"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 195"  -DisplayName "Test Group 195" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup195"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 196"  -DisplayName "Test Group 196" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup196"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 197"  -DisplayName "Test Group 197" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup197"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 198"  -DisplayName "Test Group 198" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup198"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 199"  -DisplayName "Test Group 199" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup199"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 200"  -DisplayName "Test Group 200" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup200"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 201"  -DisplayName "Test Group 201" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup201"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 202"  -DisplayName "Test Group 202" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup202"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 203"  -DisplayName "Test Group 203" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup203"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 204"  -DisplayName "Test Group 204" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup204"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 205"  -DisplayName "Test Group 205" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup205"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 206"  -DisplayName "Test Group 206" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup206"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 207"  -DisplayName "Test Group 207" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup207"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 208"  -DisplayName "Test Group 208" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup208"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 209"  -DisplayName "Test Group 209" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup209"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 210"  -DisplayName "Test Group 210" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup210"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 211"  -DisplayName "Test Group 211" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup211"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 212"  -DisplayName "Test Group 212" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup212"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 213"  -DisplayName "Test Group 213" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup213"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 214"  -DisplayName "Test Group 214" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup214"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 215"  -DisplayName "Test Group 215" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup215"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 216"  -DisplayName "Test Group 216" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup216"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 217"  -DisplayName "Test Group 217" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup217"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 218"  -DisplayName "Test Group 218" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup218"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 219"  -DisplayName "Test Group 219" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup219"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 220"  -DisplayName "Test Group 220" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup220"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 221"  -DisplayName "Test Group 221" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup221"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
$newsg = New-AzureADGroup -Description "Test Group 222"  -DisplayName "Test Group 222" -MailEnabled $false -SecurityEnabled $true -MailNickName "TestGroup222"
Add-AzureADGroupMember -ObjectId $newsg.ObjectId -RefObjectId $usersobjectId
Write-Host "Successfully created $($newsg.DisplayName)"
