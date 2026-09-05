param(
    [string]$ProjectRoot = (Join-Path $PSScriptRoot "JUtilityPalette")
)

$ErrorActionPreference = "Stop"

function Require([bool]$Condition, [string]$Message) {
    if (-not $Condition) {
        throw $Message
    }
}

$manifestPath = Join-Path $ProjectRoot "Package.appxmanifest"
$projectPath = Join-Path $ProjectRoot "JUtilityPalette.csproj"
$extensionPath = Join-Path $ProjectRoot "JUtilityPaletteExtension.cs"
$bridgePath = Join-Path $ProjectRoot "Utilities\PowerToysBridge.cs"
$launchSettingsPath = Join-Path $ProjectRoot "Properties\launchSettings.json"
$x64ProfilePath = Join-Path $ProjectRoot "Properties\PublishProfiles\win-x64.pubxml"
$arm64ProfilePath = Join-Path $ProjectRoot "Properties\PublishProfiles\win-arm64.pubxml"
$repoRoot = Split-Path $PSScriptRoot -Parent
$powerToysSharedConstantsPath = Join-Path $repoRoot "src\common\interop\shared_constants.h"

foreach ($path in @($manifestPath, $projectPath, $extensionPath, $bridgePath, $launchSettingsPath, $x64ProfilePath, $arm64ProfilePath, $powerToysSharedConstantsPath)) {
    Require (Test-Path $path) "Required extension/deployment file is missing: $path"
}

[xml]$manifest = Get-Content $manifestPath -Raw
$ns = New-Object System.Xml.XmlNamespaceManager($manifest.NameTable)
$ns.AddNamespace("f", "http://schemas.microsoft.com/appx/manifest/foundation/windows10")
$ns.AddNamespace("com", "http://schemas.microsoft.com/appx/manifest/com/windows10")
$ns.AddNamespace("uap3", "http://schemas.microsoft.com/appx/manifest/uap/windows10/3")

$identity = $manifest.SelectSingleNode("/f:Package/f:Identity", $ns)
$comClass = $manifest.SelectSingleNode("//com:Class", $ns)
$appExtension = $manifest.SelectSingleNode("//uap3:AppExtension", $ns)
$createInstance = $manifest.SelectSingleNode("//uap3:AppExtension/uap3:Properties/f:CmdPalProvider/f:Activation/f:CreateInstance", $ns)
$commands = $manifest.SelectSingleNode("//uap3:AppExtension/uap3:Properties/f:CmdPalProvider/f:SupportedInterfaces/f:Commands", $ns)

Require ($null -ne $identity) "Package Identity is missing from Package.appxmanifest."
Require (-not [string]::IsNullOrWhiteSpace($identity.Name)) "Package Identity Name is empty."
Require (-not [string]::IsNullOrWhiteSpace($identity.Publisher)) "Package Identity Publisher is empty."
Require ($null -ne $comClass) "windows.comServer class registration is missing."
Require ($null -ne $appExtension) "windows.appExtension registration is missing."
Require ($appExtension.Name -eq "com.microsoft.commandpalette") "AppExtension Name must be 'com.microsoft.commandpalette'."
Require ($null -ne $createInstance) "CmdPalProvider Activation/CreateInstance is missing."
Require ($null -ne $commands) "CmdPalProvider must advertise the Commands interface."

$source = Get-Content $extensionPath -Raw
$guidMatch = [regex]::Match($source, '\[Guid\("(?<guid>[0-9A-Fa-f-]{36})"\)\]')
Require $guidMatch.Success "JUtilityPaletteExtension.cs is missing its [Guid(...)] attribute."

$sourceGuid = $guidMatch.Groups["guid"].Value.ToUpperInvariant()
$comGuid = ([string]$comClass.Id).ToUpperInvariant()
$activationGuid = ([string]$createInstance.ClassId).ToUpperInvariant()
Require ($sourceGuid -eq $comGuid) "Extension [Guid] ($sourceGuid) does not match COM Class Id ($comGuid)."
Require ($sourceGuid -eq $activationGuid) "Extension [Guid] ($sourceGuid) does not match CmdPal CreateInstance ClassId ($activationGuid)."

[xml]$project = Get-Content $projectPath -Raw
$enableMsix = @($project.Project.PropertyGroup.EnableMsixTooling) | Where-Object { $_ -ne $null } | Select-Object -First 1
$outputType = @($project.Project.PropertyGroup.OutputType) | Where-Object { $_ -ne $null } | Select-Object -First 1
$publishProfile = @($project.Project.PropertyGroup.PublishProfile) | Where-Object { $_ -ne $null } | Select-Object -First 1
Require ([string]$enableMsix -eq "true") "EnableMsixTooling must be true in JUtilityPalette.csproj."
Require ([string]$outputType -eq "WinExe") "OutputType must remain WinExe for the packaged COM server."
Require ([string]$publishProfile -eq 'win-$(Platform).pubxml') "PublishProfile must remain bound to win-$(Platform).pubxml."

$launchSettings = Get-Content $launchSettingsPath -Raw | ConvertFrom-Json
$packageProfile = $launchSettings.profiles.PSObject.Properties | Where-Object { $_.Value.commandName -eq "MsixPackage" } | Select-Object -First 1
Require ($null -ne $packageProfile) "launchSettings.json must contain an MsixPackage profile."

[xml]$x64Profile = Get-Content $x64ProfilePath -Raw
[xml]$arm64Profile = Get-Content $arm64ProfilePath -Raw
Require ([string]$x64Profile.Project.PropertyGroup.Platform -eq "x64") "win-x64.pubxml must target x64."
Require ([string]$x64Profile.Project.PropertyGroup.RuntimeIdentifier -eq "win-x64") "win-x64.pubxml must use win-x64."
Require ([string]$arm64Profile.Project.PropertyGroup.Platform -eq "ARM64") "win-arm64.pubxml must target ARM64."
Require ([string]$arm64Profile.Project.PropertyGroup.RuntimeIdentifier -eq "win-arm64") "win-arm64.pubxml must use win-arm64."

# J System signals the same named events used by the PowerToys runner. Keep these
# synchronized with the fork so a future PowerToys rebase cannot silently break them.
$bridgeSource = Get-Content $bridgePath -Raw
$sharedConstants = Get-Content $powerToysSharedConstantsPath -Raw

$bridgeHosts = [regex]::Match($bridgeSource, 'HostsAdminEvent\s*=\s*@"(?<value>[^"]+)"')
$bridgeEnv = [regex]::Match($bridgeSource, 'EnvironmentVariablesAdminEvent\s*=\s*@"(?<value>[^"]+)"')
$powerToysHosts = [regex]::Match($sharedConstants, 'SHOW_HOSTS_ADMIN_EVENT\[\]\s*=\s*L"(?<value>[^"]+)"')
$powerToysEnv = [regex]::Match($sharedConstants, 'SHOW_ENVIRONMENT_VARIABLES_ADMIN_EVENT\[\]\s*=\s*L"(?<value>[^"]+)"')

Require $bridgeHosts.Success "PowerToysBridge.cs is missing HostsAdminEvent."
Require $bridgeEnv.Success "PowerToysBridge.cs is missing EnvironmentVariablesAdminEvent."
Require $powerToysHosts.Success "PowerToys SHOW_HOSTS_ADMIN_EVENT was not found in shared_constants.h."
Require $powerToysEnv.Success "PowerToys SHOW_ENVIRONMENT_VARIABLES_ADMIN_EVENT was not found in shared_constants.h."

$powerToysHostsValue = $powerToysHosts.Groups["value"].Value.Replace('\\', '\')
$powerToysEnvValue = $powerToysEnv.Groups["value"].Value.Replace('\\', '\')
Require ($bridgeHosts.Groups["value"].Value -eq $powerToysHostsValue) "Hosts bridge event drifted from PowerToys shared_constants.h."
Require ($bridgeEnv.Groups["value"].Value -eq $powerToysEnvValue) "Environment Variables bridge event drifted from PowerToys shared_constants.h."

Write-Host "J Utility Palette extension registration verified." -ForegroundColor Green
Write-Host "Package: $($identity.Name)"
Write-Host "Publisher: $($identity.Publisher)"
Write-Host "Extension CLSID: $sourceGuid"
Write-Host "PowerToys Hosts/Environment Variables bridge events: synchronized"
