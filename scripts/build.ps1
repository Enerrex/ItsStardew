param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptDir "..")).Path
$manifestTaskProject = Join-Path $repoRoot "Orchestration\ManifestTask\ManifestTask.csproj"
$projectVarGeneratorProject = Join-Path $repoRoot "Orchestration\ProjectVarGenerator\ProjectVarGenerator.csproj"
$productBundlerProject = Join-Path $repoRoot "Orchestration\ProductBundler\ProductBundler.csproj"
$solutionPath = Join-Path $repoRoot "ItsStardew.slnx"
$manifestTaskCacheRoot = [System.IO.Path]::Combine(
    [System.Environment]::GetFolderPath([System.Environment+SpecialFolder]::LocalApplicationData),
    "ItsStardew",
    "ManifestTask"
)
$manifestTaskCacheStamp = [System.Guid]::NewGuid().ToString("N")
$manifestTaskCacheAssembly = Join-Path (Join-Path $manifestTaskCacheRoot $manifestTaskCacheStamp) "ManifestTask.dll"

Write-Host "Publishing ManifestTask to $manifestTaskCacheAssembly ..."
& dotnet build $manifestTaskProject --configuration $Configuration `
    /p:ManifestTaskCacheRoot=$manifestTaskCacheRoot `
    /p:ManifestTaskCacheStamp=$manifestTaskCacheStamp `
    /p:ManifestTaskPublishCache=true
if ($LASTEXITCODE -ne 0)
{
    throw "ManifestTask publish failed with exit code $LASTEXITCODE."
}

Write-Host "Generating manifest variables..."
& dotnet run --project $projectVarGeneratorProject --configuration $Configuration
if ($LASTEXITCODE -ne 0)
{
    throw "Project variable generation failed with exit code $LASTEXITCODE."
}

Write-Host "Building $solutionPath ($Configuration)..."
& dotnet build $solutionPath --configuration $Configuration `
    /p:ManifestTaskAssembly=$manifestTaskCacheAssembly `
    /p:ManifestTaskCacheRoot=$manifestTaskCacheRoot `
    /p:ManifestTaskCacheStamp=$manifestTaskCacheStamp `
    /p:ManifestTaskPublishCache=false
if ($LASTEXITCODE -ne 0)
{
    throw "Solution build failed with exit code $LASTEXITCODE."
}

Write-Host "Building product zips..."
& dotnet run --project $productBundlerProject --configuration $Configuration -- $Configuration
if ($LASTEXITCODE -ne 0)
{
    throw "Product bundling failed with exit code $LASTEXITCODE."
}
