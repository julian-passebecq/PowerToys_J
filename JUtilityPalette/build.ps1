param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",

    [ValidateSet("x64", "ARM64")]
    [string]$Platform = "x64"
)

$ErrorActionPreference = "Stop"

$solution = Join-Path $PSScriptRoot "JUtilityPalette.sln"
$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"

if (-not (Test-Path $vswhere)) {
    throw "vswhere.exe was not found. Install Visual Studio 2026 (or a compatible Visual Studio) with MSBuild and Windows app development tooling."
}

$msbuild = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
if ([string]::IsNullOrWhiteSpace($msbuild)) {
    throw "MSBuild was not found by vswhere."
}

Write-Host "Building J Utility Palette" -ForegroundColor Cyan
Write-Host "MSBuild: $msbuild"
Write-Host "Configuration: $Configuration"
Write-Host "Platform: $Platform"

& $msbuild $solution `
    /m `
    /restore `
    /t:Build `
    /p:Configuration=$Configuration `
    /p:Platform=$Platform `
    /p:AppxPackageSigningEnabled=false `
    /p:GenerateAppxPackageOnBuild=false

if ($LASTEXITCODE -ne 0) {
    throw "J Utility Palette build failed with exit code $LASTEXITCODE."
}

Write-Host "J Utility Palette build succeeded." -ForegroundColor Green
