param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug",
    [switch]$IncludeIdeCaches,
    [switch]$PurgeManifestTaskCache
)

$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$cleanScript = Join-Path $scriptDir "clean.ps1"
$buildScript = Join-Path $scriptDir "build.ps1"

& $cleanScript -IncludeIdeCaches:$IncludeIdeCaches -PurgeManifestTaskCache:$PurgeManifestTaskCache
& $buildScript -Configuration $Configuration
