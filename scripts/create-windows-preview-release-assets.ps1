param(
    [string]$Version = "0.2.1-preview",
    [string]$RuntimeIdentifier = "win-x64",
    [switch]$SkipTests
)

$ErrorActionPreference = "Stop"

$AppName = "GlucoDesk"

$RootDir = Resolve-Path (Join-Path $PSScriptRoot "..")
$PackageScriptPath = Join-Path $RootDir "scripts/create-windows-preview-installer.ps1"
$VerifyScriptPath = Join-Path $RootDir "scripts/verify-windows-preview-artifacts.ps1"

$ArtifactRoot = Join-Path $RootDir "artifacts/windows/$Version/$RuntimeIdentifier"
$ManifestDir = Join-Path $RootDir "artifacts/windows/$Version"
$ManifestPath = Join-Path $ManifestDir "$AppName-$Version-windows-release-assets.txt"

$PortableZipName = "$AppName-$Version-$RuntimeIdentifier-portable.zip"
$SetupName = "$AppName-$Version-$RuntimeIdentifier-setup.exe"
$ChecksumsName = "$AppName-$Version-$RuntimeIdentifier-checksums.sha256"

$PortableZipPath = Join-Path $ArtifactRoot $PortableZipName
$SetupPath = Join-Path $ArtifactRoot $SetupName
$ChecksumsPath = Join-Path $ArtifactRoot $ChecksumsName

function Write-Step {
    param([string]$Message)

    Write-Host "==> $Message"
}

function Fail {
    param([string]$Message)

    throw "error: $Message"
}

function Assert-ScriptExists {
    param([string]$Path)

    if (-not (Test-Path $Path)) {
        Fail "required script not found: $Path"
    }
}

if ($env:OS -ne "Windows_NT") {
    Fail "Windows release asset creation must be run on Windows."
}

if ($RuntimeIdentifier -ne "win-x64") {
    Fail "unsupported runtime identifier '$RuntimeIdentifier'. This release flow currently supports win-x64 only."
}

Assert-ScriptExists -Path $PackageScriptPath
Assert-ScriptExists -Path $VerifyScriptPath

Write-Step "creating Windows release assets for $AppName $Version $RuntimeIdentifier"

$packageArgs = @(
    "-Version", $Version,
    "-RuntimeIdentifier", $RuntimeIdentifier
)

if ($SkipTests) {
    $packageArgs += "-SkipTests"
}

& $PackageScriptPath @packageArgs

Write-Step "verifying generated Windows release assets"

& $VerifyScriptPath -Version $Version -RuntimeIdentifier $RuntimeIdentifier

if (-not (Test-Path $PortableZipPath)) {
    Fail "portable zip not found while writing manifest: $PortableZipPath"
}

if (-not (Test-Path $SetupPath)) {
    Fail "setup executable not found while writing manifest: $SetupPath"
}

if (-not (Test-Path $ChecksumsPath)) {
    Fail "checksums file not found while writing manifest: $ChecksumsPath"
}

New-Item -ItemType Directory -Force -Path $ManifestDir | Out-Null

$generatedAt = [DateTime]::UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
$checksums = Get-Content $ChecksumsPath

$manifestLines = @(
    "GlucoDesk Windows release assets",
    "",
    "Version: $Version",
    "Runtime: $RuntimeIdentifier",
    "Generated at: $generatedAt",
    "",
    "Attach the generated portable ZIP, setup EXE and SHA256 file to the GitHub Release.",
    "",
    "Assets:",
    "",
    "- Runtime: $RuntimeIdentifier",
    "  Portable ZIP: $PortableZipName",
    "  Setup EXE: $SetupName",
    "  SHA256: $ChecksumsName",
    "  Checksums:"
)

foreach ($checksum in $checksums) {
    $manifestLines += "    $checksum"
}

$manifestLines += ""
$manifestLines += "Notes:"
$manifestLines += ""
$manifestLines += "- The setup installer is a per-user installer."
$manifestLines += "- Administrator privileges are not required."
$manifestLines += "- The installer may trigger Windows SmartScreen warnings until code signing is added."
$manifestLines += "- GlucoDesk is not a medical device."

$manifestLines | Set-Content -Path $ManifestPath -Encoding UTF8

Write-Step "Windows release assets created successfully"
Write-Host ""
Write-Host "Manifest:"
Write-Host "  $ManifestPath"
Write-Host ""
Get-Content $ManifestPath
