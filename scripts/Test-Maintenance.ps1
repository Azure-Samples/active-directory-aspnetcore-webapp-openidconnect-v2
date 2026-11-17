[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'Maintenance.Common.ps1')

function dotnet { throw 'Offline tests prohibit builds/restores.' }
function Invoke-WebRequest { throw 'Offline tests prohibit HTTP downloads.' }
function Invoke-RestMethod { throw 'Offline tests prohibit HTTP calls.' }

function Assert-True {
    param([bool] $Condition, [string] $Message)
    if (!$Condition) { throw $Message }
}

function Assert-Throws {
    param([scriptblock] $Action, [string] $Pattern)
    $caught = $null
    try { & $Action | Out-Null } catch { $caught = $_.Exception.Message }
    Assert-True ($null -ne $caught -and $caught -like $Pattern) "Expected failure matching '$Pattern'; actual: $caught"
}

$catalog = Get-MaintenanceCatalog
Assert-True ($catalog.projects.Count -eq 13) 'Expected thirteen retained IdWeb applications.'
Assert-True ($catalog.supportProjects.Count -eq 4) 'Expected four retained UI-test projects.'
Assert-True ($catalog.excludedProjects.Count -eq 5) 'Expected five optional retired projects.'
foreach ($package in $catalog.packages) {
    $selected = @(Get-MaintenanceSelection -Package $package -TargetVersion '3.7.0')
    Assert-True ($selected.Count -gt 0) "No application consumes $package."
    Assert-True (@($selected | Where-Object { $_.project -in $catalog.excludedProjects }).Count -eq 0) 'An excluded project was selected.'
    Assert-True (@($selected | Where-Object { $_.status -notin @('current','newer','update') }).Count -eq 0) 'Invalid baseline classification.'
}
Assert-Throws { Get-MaintenanceSelection -Package Microsoft.Identity.Client -TargetVersion '4.91.1' } '*Only cataloged direct IdWeb*'
foreach ($version in @('latest','3.*','../3.7.0','3.7.0-preview','3.7.0;echo unsafe')) {
    Assert-Throws { Get-MaintenanceSelection -Package Microsoft.Identity.Web.UI -TargetVersion $version } '*exact three-part stable*'
}
$newer = @(Get-MaintenanceSelection -Package Microsoft.Identity.Web.UI -TargetVersion '0.0.1')
Assert-True (@($newer | Where-Object status -NE 'newer').Count -eq 0) 'Downgrades must be classified as newer.'
$updates = @(Get-MaintenanceSelection -Package Microsoft.Identity.Web.UI -TargetVersion '99.0.0')
Assert-True (@($updates | Where-Object status -NE 'update').Count -eq 0) 'New release classification mismatch.'
& (Join-Path $PSScriptRoot 'Validate-Maintenance.ps1')
Assert-Throws { & (Join-Path $PSScriptRoot 'Validate-Maintenance.ps1') -ExpectedVersion '3.7.0' } '*requires the original evidence*'
Assert-Throws { & (Join-Path $PSScriptRoot 'Get-IdWebSourceEvidence.ps1') -Package Microsoft.Identity.Client -TargetVersion '4.91.1' } '*Only cataloged direct IdWeb*'
$scratch = Join-Path ([IO.Path]::GetTempPath()) ('idweb-maintenance-' + [guid]::NewGuid())
New-Item -ItemType Directory -Path $scratch | Out-Null
try {
    Assert-Throws { & (Join-Path $PSScriptRoot 'Get-IdWebSourceEvidence.ps1') -Package Microsoft.Identity.Web.UI -TargetVersion '3.7.0' -OutputDirectory $scratch } '*Evidence already exists*'
    $root = Split-Path $PSScriptRoot -Parent
    New-Item -ItemType Directory -Path (Join-Path $scratch 'scripts') | Out-Null
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'maintenance-samples.json') -Destination (Join-Path $scratch 'scripts/maintenance-samples.json')
    foreach ($path in @($catalog.projects) + @($catalog.supportProjects) + @($catalog.excludedProjects)) {
        $source = Join-Path $root $path
        if (!(Test-Path -LiteralPath $source)) { continue }
        $destination = Join-Path $scratch $path
        New-Item -ItemType Directory -Path (Split-Path $destination -Parent) -Force | Out-Null
        Copy-Item -LiteralPath $source -Destination $destination
    }
    & git init --quiet $scratch
    if ($LASTEXITCODE -ne 0) { throw 'Cannot initialize offline catalog fixture.' }
    & git -C $scratch add .
    if ($LASTEXITCODE -ne 0) { throw 'Cannot stage offline catalog fixture.' }
    Get-MaintenanceCatalog -RepositoryRoot $scratch | Out-Null
    foreach ($path in $catalog.excludedProjects) {
        if (Test-Path -LiteralPath (Join-Path $scratch $path)) {
            & git -C $scratch rm --quiet --force -- $path
            if ($LASTEXITCODE -ne 0) { throw 'Cannot simulate retired-project removal.' }
        }
    }
    Get-MaintenanceCatalog -RepositoryRoot $scratch | Out-Null
    Remove-Item -LiteralPath (Join-Path $scratch $catalog.projects[0])
    Assert-Throws { Get-MaintenanceCatalog -RepositoryRoot $scratch } '*Required project missing*'
    Copy-Item -LiteralPath (Join-Path $root $catalog.projects[0]) -Destination (Join-Path $scratch $catalog.projects[0])
    Copy-Item -LiteralPath (Join-Path $root $catalog.projects[0]) -Destination (Join-Path $scratch 'Unexpected.csproj')
    & git -C $scratch add Unexpected.csproj
    if ($LASTEXITCODE -ne 0) { throw 'Cannot stage unknown-project fixture.' }
    Assert-Throws { Get-MaintenanceCatalog -RepositoryRoot $scratch } '*Uncataloged projects*'
    Write-Host 'PASS: original and cleanup-compatible layouts; required-project and uncataloged-project failures.'
    $sources = Join-Path $scratch 'synthetic-source'
    & git init --quiet $sources
    if ($LASTEXITCODE -ne 0) { throw 'Cannot initialize synthetic source fixture.' }
    & git -C $sources -c user.name=HarnessTest -c user.email=harness@example.invalid -c commit.gpgsign=false commit --quiet --allow-empty -m 'Synthetic fixture only; no SDK code'
    if ($LASTEXITCODE -ne 0) { throw 'Cannot commit synthetic source fixture.' }
    $commit = (& git -C $sources rev-parse HEAD).Trim()
    $originals = @(Get-MaintenanceSelection -Package Microsoft.Identity.Web -TargetVersion '99.0.0' | ForEach-Object originalVersion | Sort-Object { [version]$_ })
    $target = $originals[0]
    & git -C $sources tag $target
    if ($LASTEXITCODE -ne 0) { throw 'Cannot tag synthetic source fixture.' }
    $packagePath = Join-Path $scratch 'synthetic-package.txt'
    $diffPath = Join-Path $scratch 'synthetic-comparison.diff'
    Set-Content -LiteralPath $packagePath -Value 'Synthetic hash fixture; not a NuGet package.'
    Set-Content -LiteralPath $diffPath -Value '' -NoNewline
    $manifestPath = Join-Path $scratch 'synthetic-manifest.json'
    $manifest = [ordered]@{
        schemaVersion = 1
        repository = 'https://github.com/AzureAD/microsoft-identity-web.git'
        package = 'Microsoft.Identity.Web'
        targetVersion = $target
        samples = @(Get-MaintenanceSelection -Package Microsoft.Identity.Web -TargetVersion $target)
        releases = @([ordered]@{ version = $target; commit = $commit; sourcePath = $sources; packagePath = $packagePath; packageSha256 = (Get-FileHash -LiteralPath $packagePath).Hash })
        comparisons = @([ordered]@{ baselineVersion = $target; targetVersion = $target; path = $diffPath; sha256 = (Get-FileHash -LiteralPath $diffPath).Hash })
    }
    $manifest | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $manifestPath
    $validator = Join-Path $PSScriptRoot 'Validate-Maintenance.ps1'
    & $validator -EvidenceManifest $manifestPath -ExpectedVersion $target
    Assert-Throws { & $validator -EvidenceManifest $manifestPath -ExpectedVersion '0.0.1' } '*must match original setup evidence*'
    Set-Content -LiteralPath $packagePath -Value 'Changed fixture'
    Assert-Throws { & $validator -EvidenceManifest $manifestPath -ExpectedVersion $target } '*package evidence changed*'
    Set-Content -LiteralPath $packagePath -Value 'Synthetic hash fixture; not a NuGet package.'
    Set-Content -LiteralPath $diffPath -Value 'Changed comparison'
    Assert-Throws { & $validator -EvidenceManifest $manifestPath -ExpectedVersion $target } '*comparison changed or is missing*'
    Set-Content -LiteralPath $diffPath -Value '' -NoNewline
    Set-Content -LiteralPath (Join-Path $sources 'unexpected.txt') -Value 'Untracked source mutation'
    Assert-Throws { & $validator -EvidenceManifest $manifestPath -ExpectedVersion $target } '*source evidence is not immutable*'
    Assert-Throws { & $validator -Build } '*Offline tests prohibit builds/restores*'
    Write-Host 'PASS: synthetic evidence validation, mismatched target, package/diff/source tampering, and required build failure.'
} finally { Remove-Item -LiteralPath $scratch -Recurse -Force }
Write-Host 'PASS: IdWeb catalog, retired-project exclusions, package routing, exact versions, and no-downgrade classification. Builds/downloads/sign-in: NOT RUN.'