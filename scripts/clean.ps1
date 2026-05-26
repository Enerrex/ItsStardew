param(
    [switch]$DryRun,
    [switch]$IncludeIdeCaches,
    [switch]$PurgeManifestTaskCache
)

$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptDir "..")).Path
$failedPaths = [System.Collections.Generic.List[string]]::new()

function To-RepoRelativePath([string]$absolutePath)
{
    if ($absolutePath.StartsWith($repoRoot, [System.StringComparison]::OrdinalIgnoreCase))
    {
        return $absolutePath.Substring($repoRoot.Length).TrimStart("\")
    }

    return $absolutePath
}

function Remove-RepoPath([string]$path)
{
    if (-not (Test-Path -LiteralPath $path))
    {
        return
    }

    $label = To-RepoRelativePath $path
    if ($DryRun)
    {
        Write-Host "[DRY RUN] Remove $label"
        return
    }

    try
    {
        Remove-Item -LiteralPath $path -Recurse -Force
        Write-Host "Removed $label"
    }
    catch
    {
        if (-not $DryRun)
        {
            $lockedProcesses = @(
                Get-CimInstance Win32_Process | Where-Object {
                    (
                        $_.Name -eq "dotnet.exe" -or
                        $_.Name -eq "MSBuild.exe"
                    ) -and
                    $_.CommandLine -and
                    $_.CommandLine -match "MSBuild\.dll"
                }
            )

            if ($lockedProcesses.Count -gt 0)
            {
                Write-Host "Stopping lingering MSBuild host(s) before retrying ${label}..."
                foreach ($process in $lockedProcesses)
                {
                    try
                    {
                        Stop-Process -Id $process.ProcessId -Force -ErrorAction Stop
                        Write-Host "Stopped dotnet MSBuild host $($process.ProcessId)"
                    }
                    catch
                    {
                        Write-Warning "Failed to stop process $($process.ProcessId): $($_.Exception.Message)"
                    }
                }

                Start-Sleep -Milliseconds 500

                try
                {
                    Remove-Item -LiteralPath $path -Recurse -Force
                    Write-Host "Removed $label"
                    return
                }
                catch
                {
                    # Fall through to the normal failure path.
                }
            }
        }

        [void]$failedPaths.Add($label)
        Write-Warning "Failed to remove ${label}: $($_.Exception.Message)"
    }
}

$artifactDirs = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
$artifactFiles = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)

# Top-level artifact output.
[void]$artifactDirs.Add((Join-Path $repoRoot "bin"))

# Project-local bin/obj artifacts.
Get-ChildItem -LiteralPath $repoRoot -Directory -Recurse -Force | Where-Object {
    ($_.Name -eq "bin" -or $_.Name -eq "obj") -and
    $_.FullName -notlike "$($repoRoot)\.git*"
} | ForEach-Object {
    [void]$artifactDirs.Add($_.FullName)
}

# Generated SDK package artifacts.
[void]$artifactDirs.Add((Join-Path $repoRoot "build\generated"))
[void]$artifactDirs.Add((Join-Path $repoRoot "build\sdk\SmapiManifestSdk"))
[void]$artifactFiles.Add((Join-Path $repoRoot "build\sdk\SmapiManifestSdk.zip"))

if ($PurgeManifestTaskCache)
{
    $manifestTaskCacheRoot = [System.IO.Path]::Combine(
        [System.Environment]::GetFolderPath([System.Environment+SpecialFolder]::LocalApplicationData),
        "ItsStardew",
        "ManifestTask"
    )
    [void]$artifactDirs.Add($manifestTaskCacheRoot)
}

if ($IncludeIdeCaches)
{
    [void]$artifactDirs.Add((Join-Path $repoRoot ".idea"))
}

$dirTargets = @($artifactDirs) | Sort-Object { $_.Length } -Descending
$fileTargets = @($artifactFiles) | Sort-Object

Write-Host "Cleaning repository artifacts..."
Write-Host "Repo root: $repoRoot"

if (-not $DryRun)
{
    Write-Host "Shutting down dotnet build servers..."
    dotnet build-server shutdown | Out-Host
}

foreach ($path in $dirTargets)
{
    Remove-RepoPath $path
}

foreach ($path in $fileTargets)
{
    Remove-RepoPath $path
}

if ($DryRun)
{
    Write-Host "Dry run complete."
}
else
{
    if ($failedPaths.Count -gt 0)
    {
        Write-Warning "Clean completed with failures:"
        foreach ($path in $failedPaths)
        {
            Write-Warning " - $path"
        }

        exit 1
    }

    Write-Host "Clean complete."
}
