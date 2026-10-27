param(
    [switch]$Run,
    [switch]$SkipTest
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$csc = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
$wpf = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\WPF"
$dist = Join-Path $root "dist"

if (-not (Test-Path $csc)) {
    throw "Cannot find the .NET Framework compiler: $csc"
}
if (-not (Test-Path (Join-Path $wpf "PresentationFramework.dll"))) {
    throw "Cannot find WPF reference assemblies: $wpf"
}

New-Item -ItemType Directory -Force -Path $dist | Out-Null

$sources = Get-ChildItem (Join-Path $root "src") -Filter "*.cs" | Sort-Object Name | ForEach-Object { $_.FullName }
if ($sources.Count -eq 0) { throw "No .cs files found under src" }
$profileResources = Get-ChildItem (Join-Path $root "profiles") -Filter "*.json" -File |
    Sort-Object Name | ForEach-Object { "/resource:$($_.FullName),profiles/$($_.Name)" }

$arguments = @(
    "/nologo",
    "/target:winexe",
    "/platform:anycpu",
    "/langversion:5",
    "/out:$dist\GearPower.exe",
    "/resource:$root\ui.xaml,ui.xaml",
    "/resource:$root\assets\logo.png,assets/logo.png",
    "/win32icon:$root\assets\app.ico",
    "/win32manifest:$root\app.manifest",
    "/r:$wpf\PresentationFramework.dll",
    "/r:$wpf\PresentationCore.dll",
    "/r:$wpf\WindowsBase.dll",
    "/r:C:\Windows\Microsoft.NET\Framework64\v4.0.30319\System.Xaml.dll",
    "/r:C:\Windows\Microsoft.NET\Framework64\v4.0.30319\System.Windows.Forms.dll",
    "/r:C:\Windows\Microsoft.NET\Framework64\v4.0.30319\System.Drawing.dll",
    "/r:System.dll",
    "/r:System.Core.dll",
    "/r:System.Web.Extensions.dll"
) + $profileResources + $sources

Write-Host "Compiling GearPower..." -ForegroundColor Cyan
& $csc $arguments
if ($LASTEXITCODE -ne 0) { throw "Compilation failed (exit $LASTEXITCODE)" }

Copy-Item (Join-Path $root "README.md") $dist -Force
Copy-Item (Join-Path $root "THIRD_PARTY_NOTICES.md") $dist -Force
Copy-Item (Join-Path $root "LICENSE") $dist -Force
Copy-Item (Join-Path $root "CONTRIBUTING.md") $dist -Force
Copy-Item (Join-Path $root "SECURITY.md") $dist -Force
Copy-Item (Join-Path $root "CHANGELOG.md") $dist -Force
Copy-Item (Join-Path $root "docs\DEVICE_PROFILES.md") $dist -Force
Copy-Item (Join-Path $root "docs\DEVICE_ADAPTERS.md") $dist -Force
Copy-Item (Join-Path $root "docs\QIJU_UI.md") $dist -Force
Copy-Item (Join-Path $root "docs\PATCH_NOTES.md") $dist -Force
Get-ChildItem $dist -Filter "RELEASE_NOTES_*.md" -File | Remove-Item -Force -ErrorAction SilentlyContinue
Get-ChildItem (Join-Path $root "docs\releases") -Filter "RELEASE_NOTES_*.md" -File |
    Copy-Item -Destination $dist -Force
$assetTarget = Join-Path $dist "assets"
New-Item -ItemType Directory -Force -Path $assetTarget | Out-Null
Get-ChildItem (Join-Path $root "assets") -Filter "*.png" | Copy-Item -Destination $assetTarget -Force
$profileTarget = Join-Path $dist "profiles"
New-Item -ItemType Directory -Force -Path $profileTarget | Out-Null
Get-ChildItem (Join-Path $root "profiles") -Filter "*.json" -File | Copy-Item -Destination $profileTarget -Force

if (-not $SkipTest) {
    Write-Host "Running self-test..." -ForegroundColor Cyan
    $process = Start-Process -FilePath (Join-Path $dist "GearPower.exe") -ArgumentList "--selftest" -Wait -PassThru
    $localAppData = [Environment]::GetFolderPath("LocalApplicationData")
    $selftestLog = Join-Path (Join-Path $localAppData "GearPower\logs") "selftest.log"
    if ($process.ExitCode -ne 0) {
        throw "Self-test failed. See $selftestLog"
    }
    Write-Host "Self-test passed. Log: $selftestLog" -ForegroundColor Green
}

Write-Host "Output: $dist\GearPower.exe" -ForegroundColor Green
if ($Run) {
    Start-Process -FilePath (Join-Path $dist "GearPower.exe")
}
