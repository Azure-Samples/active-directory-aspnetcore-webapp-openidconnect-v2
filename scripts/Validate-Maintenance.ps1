[CmdletBinding()]
param(
    [string] $EvidenceManifest,
    [string] $ExpectedVersion,
    [switch] $Build
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'Maintenance.Common.ps1')
$root = Split-Path $PSScriptRoot -Parent
$catalog = Get-MaintenanceCatalog -RepositoryRoot $root
if ($ExpectedVersion -and !$EvidenceManifest) { throw 'Exact-version maintenance validation requires the original evidence manifest.' }
if ($EvidenceManifest) {
    $manifest = Get-Content -LiteralPath $EvidenceManifest -Raw | ConvertFrom-Json
    if ($manifest.schemaVersion -ne 1 -or $manifest.repository -cne 'https://github.com/AzureAD/microsoft-identity-web.git') { throw 'Invalid IdWeb source manifest.' }
    if (!$ExpectedVersion -or $manifest.targetVersion -cne $ExpectedVersion) { throw 'Expected version must match original setup evidence.' }
    $selected = @(Get-MaintenanceSelection -Package $manifest.package -TargetVersion $ExpectedVersion)
    if (Compare-Object @($selected.project | Sort-Object) @($manifest.samples.project | Sort-Object)) { throw 'Manifest project scope does not match the catalog selection.' }
    $versions = @(@($ExpectedVersion) + @($manifest.samples | Where-Object status -NE 'newer' | ForEach-Object originalVersion) | Select-Object -Unique)
    if (Compare-Object @($versions | Sort-Object) @($manifest.releases.version | Sort-Object)) { throw 'Missing or duplicate verified source releases.' }
    foreach ($release in $manifest.releases) {
        if ((Get-FileHash -LiteralPath $release.packagePath -Algorithm SHA256).Hash -cne $release.packageSha256) { throw 'Original package evidence changed.' }
        $commit = & git -C $release.sourcePath rev-parse HEAD
        if ($LASTEXITCODE -ne 0 -or $commit -cne $release.commit) { throw 'Source checkout commit changed.' }
        $tagCommit = & git -C $release.sourcePath rev-parse "$($release.version)^{commit}"
        if ($LASTEXITCODE -ne 0 -or $tagCommit -cne $release.commit) { throw 'Exact source tag changed.' }
        $dirty = @(& git -C $release.sourcePath status --porcelain --untracked-files=normal)
        if ($LASTEXITCODE -ne 0 -or $dirty.Count) { throw 'Original source evidence is not immutable.' }
    }
    foreach ($sample in $manifest.samples) {
        if ($sample.originalVersion -notmatch '^\d+\.\d+\.\d+$') { throw 'Invalid original version.' }
        $status = if ([version]$sample.originalVersion -gt [version]$ExpectedVersion) { 'newer' } elseif ($sample.originalVersion -ceq $ExpectedVersion) { 'current' } else { 'update' }
        if ($sample.status -cne $status) { throw 'Invalid sample update disposition.' }
        [xml] $project = Get-Content -LiteralPath (Join-Path $root $sample.project) -Raw
        $frameworks = @($project.SelectNodes('//TargetFramework | //TargetFrameworks') | ForEach-Object InnerText)
        if (Compare-Object @($sample.frameworks) $frameworks) { throw 'Target frameworks changed.' }
        $actualReferences = @($project.SelectNodes('//PackageReference') | ForEach-Object { [ordered]@{ package = $_.Include; version = $_.Version } } | Sort-Object { $_.package })
        $expectedReferences = @($sample.references | ForEach-Object {
            $version = if ($_.package -ceq $manifest.package -and $status -ne 'newer') { $ExpectedVersion } else { $_.version }
            [ordered]@{ package = $_.package; version = $version }
        } | Sort-Object { $_.package })
        if (($actualReferences | ConvertTo-Json -Compress) -cne ($expectedReferences | ConvertTo-Json -Compress)) { throw 'Package changes are outside the verified update scope or attempt a downgrade.' }
        if ($status -ne 'newer') {
            $comparisons = @($manifest.comparisons | Where-Object { $_.baselineVersion -ceq $sample.originalVersion -and $_.targetVersion -ceq $ExpectedVersion })
            if ($comparisons.Count -ne 1 -or (Get-FileHash -LiteralPath $comparisons[0].path -Algorithm SHA256).Hash -cne $comparisons[0].sha256) { throw 'Prepared baseline-to-target source comparison changed or is missing.' }
        }
    }
}
if ($Build) {
    foreach ($path in $catalog.projects) {
        & dotnet build (Join-Path $root $path) --configuration Release --verbosity minimal
        if ($LASTEXITCODE -ne 0) { throw "Build failed: $path" }
    }
} else { Write-Host 'Builds: NOT RUN. Use -Build only on an approved build host.' }
Write-Host 'PASS: retained IdWeb catalog and requested evidence checks. Live sign-in/downstream calls: NOT RUN.'