[CmdletBinding()]
param(
    [Parameter(Mandatory)][string] $Package,
    [Parameter(Mandatory)][string] $TargetVersion,
    [string] $OutputDirectory = (Join-Path (Split-Path $PSScriptRoot -Parent) '.maintenance-evidence')
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'Maintenance.Common.ps1')
$samples = @(Get-MaintenanceSelection -Package $Package -TargetVersion $TargetVersion)
if (!$samples.Count) { throw 'No retained project directly references this package.' }
if (Test-Path -LiteralPath $OutputDirectory) { throw 'Evidence already exists; preserve the original setup evidence.' }
$evidencePath = [IO.Path]::GetFullPath($OutputDirectory)
$repositoryUrl = 'https://github.com/AzureAD/microsoft-identity-web.git'
$versions = @(@($TargetVersion) + @($samples | Where-Object status -NE 'newer' | ForEach-Object originalVersion) | Select-Object -Unique)
New-Item -ItemType Directory -Path $evidencePath | Out-Null
$sourcePath = Join-Path $evidencePath 'repository'
$oldPrompt = $env:GIT_TERMINAL_PROMPT
try {
    $env:GIT_TERMINAL_PROMPT = '0'
    & git -c credential.helper= -c http.extraHeader= init --quiet $sourcePath
    if ($LASTEXITCODE -ne 0) { throw 'Cannot initialize source evidence repository.' }
    $releases = @(foreach ($version in $versions) {
        $packageUrl = "https://www.nuget.org/api/v2/package/$Package/$version"
        $packagePath = Join-Path $evidencePath "$version.nupkg"
        Invoke-WebRequest -Uri $packageUrl -OutFile $packagePath -TimeoutSec 60
        $archive = [IO.Compression.ZipFile]::OpenRead($packagePath)
        try {
            $entries = @($archive.Entries | Where-Object FullName -CEQ "$Package.nuspec")
            if ($entries.Count -ne 1) { throw 'Expected exactly one package nuspec.' }
            $reader = [IO.StreamReader]::new($entries[0].Open())
            try { [xml] $nuspec = $reader.ReadToEnd() } finally { $reader.Dispose() }
        } finally { $archive.Dispose() }
        $metadata = $nuspec.SelectSingleNode('/*[local-name()="package"]/*[local-name()="metadata"]')
        $identity = $metadata.SelectSingleNode('*[local-name()="id"]')
        $packageVersion = $metadata.SelectSingleNode('*[local-name()="version"]')
        $repository = $metadata.SelectSingleNode('*[local-name()="repository"]')
        if (!$identity -or !$packageVersion -or !$repository -or $identity.InnerText -cne $Package -or $packageVersion.InnerText -cne $version) { throw 'Package identity/version or source metadata mismatch.' }
        if ($repository.GetAttribute('url').TrimEnd('/').Replace('.git', '') -ine $repositoryUrl.Replace('.git', '')) { throw 'Package source is not the official IdWeb repository.' }
        $commit = $repository.GetAttribute('commit')
        if ($commit -notmatch '^[0-9a-fA-F]{40}$') { throw 'Package source commit is missing or incomplete.' }
        & git -c credential.helper= -c http.extraHeader= -C $sourcePath fetch --quiet --depth=1 $repositoryUrl "refs/tags/$version`:refs/tags/$version"
        if ($LASTEXITCODE -ne 0) { throw "SETUP_BLOCKED: cannot fetch exact IdWeb tag $version." }
        $tagCommit = (& git -C $sourcePath rev-parse "$version^{commit}").Trim()
        if ($LASTEXITCODE -ne 0 -or $tagCommit -ine $commit) { throw "SETUP_BLOCKED: NuGet/tag provenance mismatch for $Package $version; do not substitute another revision." }
        $checkout = Join-Path $evidencePath $version
        & git -C $sourcePath worktree add --quiet --detach $checkout $commit
        if ($LASTEXITCODE -ne 0) { throw 'Cannot materialize exact-release source.' }
        [ordered]@{
            version = $version
            commit = $commit
            sourcePath = $checkout
            packagePath = $packagePath
            packageUrl = $packageUrl
            packageSha256 = (Get-FileHash -LiteralPath $packagePath -Algorithm SHA256).Hash
        }
    })
    $target = $releases | Where-Object version -CEQ $TargetVersion
    $comparisons = @(foreach ($version in @($samples | Where-Object status -NE 'newer' | ForEach-Object originalVersion | Select-Object -Unique)) {
        $baseline = $releases | Where-Object version -CEQ $version
        $diffPath = Join-Path $evidencePath "$version-to-$TargetVersion.diff"
        $content = @(& git -C $sourcePath diff --no-ext-diff --no-textconv --unified=3 $baseline.commit $target.commit -- src tests)
        if ($LASTEXITCODE -ne 0) { throw 'Cannot compare verified IdWeb source/tests.' }
        [string]::Join([Environment]::NewLine, $content) | Set-Content -LiteralPath $diffPath -Encoding utf8
        [ordered]@{ baselineVersion = $version; targetVersion = $TargetVersion; path = $diffPath; sha256 = (Get-FileHash -LiteralPath $diffPath -Algorithm SHA256).Hash }
    })
    $manifestPath = Join-Path $evidencePath 'manifest.json'
    [ordered]@{
        schemaVersion = 1
        repository = $repositoryUrl
        package = $Package
        targetVersion = $TargetVersion
        samples = $samples
        releases = $releases
        comparisons = $comparisons
    } | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $manifestPath -Encoding utf8
    Write-Output $manifestPath
} finally { $env:GIT_TERMINAL_PROMPT = $oldPrompt }