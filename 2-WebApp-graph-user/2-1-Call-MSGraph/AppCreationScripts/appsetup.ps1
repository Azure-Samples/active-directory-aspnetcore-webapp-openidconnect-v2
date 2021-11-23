Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process -Force
$secpasswd = ConvertTo-SecureString "Abeyd@123" -AsPlainText -Force
$mycreds = New-Object System.Management.Automation.PSCredential ("usr@adstudies.onmicrosoft.com", $secpasswd)
$tenantId = "46f4b84b-469c-437b-a082-81d712b34232"

.\Cleanup.ps1 -Credential $mycreds -TenantId $tenantId
.\Configure.ps1 -Credential $mycreds -TenantId $tenantId