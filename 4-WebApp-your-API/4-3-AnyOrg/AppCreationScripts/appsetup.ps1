Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process -Force
$secpasswd = ConvertTo-SecureString "Abeyd@123" -AsPlainText -Force
$mycreds = New-Object System.Management.Automation.PSCredential ("alexbeyd@alexbdev.onmicrosoft.com", $secpasswd)
$tenantId = "859c0fc9-4300-47be-b473-597fa2fc2104"

.\Cleanup.ps1 -Credential $mycreds -TenantId $tenantId
.\Configure.ps1 -Credential $mycreds -TenantId $tenantId