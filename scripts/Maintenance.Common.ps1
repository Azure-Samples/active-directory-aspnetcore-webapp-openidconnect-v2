Set-StrictMode -Version Latest

function Get-MaintenanceCatalog {
    param([string] $RepositoryRoot = (Split-Path $PSScriptRoot -Parent))

    $catalog = Get-Content -LiteralPath (Join-Path $RepositoryRoot 'scripts/maintenance-samples.json') -Raw | ConvertFrom-Json
    if ($catalog.schemaVersion -ne 1) { throw 'Unsupported maintenance catalog schema.' }
    $known = @($catalog.projects) + @($catalog.supportProjects) + @($catalog.excludedProjects)
    if (@($known | Sort-Object -Unique).Count -ne $known.Count) { throw 'Catalog paths must be unique.' }
    foreach ($path in $known) {
        if ($path -match '(^/|\\|(^|/)\.\.(/|$)|:)' -or $path -notlike '*.csproj') { throw 'Invalid catalog path.' }
    }
    $actual = @(& git -C $RepositoryRoot ls-files '*.csproj')
    if ($LASTEXITCODE -ne 0) { throw 'Cannot inspect tracked project inventory.' }
    if (@($actual | Where-Object { $_ -notin $known }).Count) { throw 'Uncataloged projects require maintainer review.' }
    foreach ($path in @($catalog.projects) + @($catalog.supportProjects)) {
        if (!(Test-Path -LiteralPath (Join-Path $RepositoryRoot $path))) { throw "Required project missing: $path" }
    }
    foreach ($path in $catalog.projects) {
        [xml] $project = Get-Content -LiteralPath (Join-Path $RepositoryRoot $path) -Raw
        if ($project.SelectNodes('//PackageReference[@Include="Microsoft.Identity.Client"]').Count) { throw 'Do not introduce a direct MSAL dependency into IdWeb samples.' }
        $references = @($project.SelectNodes('//PackageReference[starts-with(@Include,"Microsoft.Identity.Web")]'))
        if (!$references.Count) { throw "IdWeb dependency missing: $path" }
        foreach ($reference in $references) {
            if ($reference.Include -cnotin $catalog.packages -or $reference.Version -notmatch '^\d+\.\d+\.\d+$') { throw "Unsupported IdWeb package/reference: $path" }
        }
    }
    return $catalog
}

function Get-MaintenanceSelection {
    param(
        [Parameter(Mandatory)][string] $Package,
        [Parameter(Mandatory)][string] $TargetVersion,
        [string] $RepositoryRoot = (Split-Path $PSScriptRoot -Parent)
    )

    $catalog = Get-MaintenanceCatalog -RepositoryRoot $RepositoryRoot
    if ($Package -cnotin $catalog.packages) { throw 'Only cataloged direct IdWeb packages are supported; MSAL releases are not IdWeb releases.' }
    if ($TargetVersion -notmatch '^\d+\.\d+\.\d+$') { throw 'An exact three-part stable version is required.' }
    foreach ($path in $catalog.projects) {
        [xml] $project = Get-Content -LiteralPath (Join-Path $RepositoryRoot $path) -Raw
        $matches = @($project.SelectNodes('//PackageReference') | Where-Object { $_.Include -ceq $Package })
        if ($matches.Count -gt 1) { throw "Duplicate package reference: $path" }
        if (!$matches.Count) { continue }
        $original = $matches[0].Version
        $status = if ([version]$original -gt [version]$TargetVersion) { 'newer' } elseif ($original -ceq $TargetVersion) { 'current' } else { 'update' }
        [ordered]@{
            project = $path
            originalVersion = $original
            targetVersion = $TargetVersion
            status = $status
            frameworks = @($project.SelectNodes('//TargetFramework | //TargetFrameworks') | ForEach-Object InnerText)
            references = @($project.SelectNodes('//PackageReference') | ForEach-Object { [ordered]@{ package = $_.Include; version = $_.Version } })
        }
    }
}