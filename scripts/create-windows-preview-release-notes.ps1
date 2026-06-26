param(
    [string]$Version = "0.2.1-preview",
    [string]$RuntimeIdentifier = "win-x64",
    [string]$RepositoryUrl = "https://github.com/FilippoGaravaglia/GlucoDesk"
)

$ErrorActionPreference = "Stop"

$AppName = "GlucoDesk"
$SupportedRuntimeIdentifier = "win-x64"

function Write-Step {
    param([string]$Message)

    Write-Host "==> $Message"
}

function Fail {
    param([string]$Message)

    throw "error: $Message"
}

function Assert-FileExists {
    param(
        [string]$Path,
        [string]$Description
    )

    if (-not (Test-Path $Path -PathType Leaf)) {
        Fail "$Description not found: $Path"
    }
}

function Read-ChecksumForFile {
    param(
        [string]$ChecksumFilePath,
        [string]$FileName
    )

    $line = Get-Content $ChecksumFilePath |
        Where-Object { $_ -match [regex]::Escape($FileName) } |
        Select-Object -First 1

    if ([string]::IsNullOrWhiteSpace($line)) {
        Fail "checksum entry not found for $FileName"
    }

    return ($line -split "\s+")[0]
}

if ($RuntimeIdentifier -ne $SupportedRuntimeIdentifier) {
    Fail "unsupported runtime identifier '$RuntimeIdentifier'. This preview release notes generator currently supports $SupportedRuntimeIdentifier only."
}

$root = Split-Path -Parent $PSScriptRoot
$artifactRoot = Join-Path $root "artifacts/windows/$Version"
$runtimeArtifactRoot = Join-Path $artifactRoot $RuntimeIdentifier

$portableZipFileName = "$AppName-$Version-$RuntimeIdentifier-portable.zip"
$setupExeFileName = "$AppName-$Version-$RuntimeIdentifier-setup.exe"
$checksumsFileName = "$AppName-$Version-$RuntimeIdentifier-checksums.sha256"
$releaseNotesFileName = "$AppName-$Version-windows-release-notes.md"

$portableZipPath = Join-Path $runtimeArtifactRoot $portableZipFileName
$setupExePath = Join-Path $runtimeArtifactRoot $setupExeFileName
$checksumsPath = Join-Path $runtimeArtifactRoot $checksumsFileName
$releaseNotesPath = Join-Path $artifactRoot $releaseNotesFileName

Write-Step "creating Windows release notes for $AppName $Version $RuntimeIdentifier"

Assert-FileExists -Path $portableZipPath -Description "portable zip"
Assert-FileExists -Path $setupExePath -Description "setup executable"
Assert-FileExists -Path $checksumsPath -Description "checksums file"

$portableZipChecksum = Read-ChecksumForFile -ChecksumFilePath $checksumsPath -FileName $portableZipFileName
$setupExeChecksum = Read-ChecksumForFile -ChecksumFilePath $checksumsPath -FileName $setupExeFileName

$releaseNotesLines = @(
    "# $AppName $Version - Windows preview",
    "",
    "This is a Windows preview release of $AppName.",
    "",
    "$AppName is a local-first desktop glucose companion designed for personal glucose awareness while working at a computer.",
    "",
    "Safety notice: $AppName is not a medical device and must not be used to make insulin dosing, treatment, diagnosis or emergency decisions. Always rely on your official CGM system, insulin pump, glucose meter and clinical guidance for medical decisions.",
    "",
    "---",
    "",
    "## Windows assets",
    "",
    "Attach these files to the GitHub Release:",
    "",
    "| Asset | File |",
    "| --- | --- |",
    "| Setup installer | $setupExeFileName |",
    "| Portable ZIP | $portableZipFileName |",
    "| SHA256 checksums | $checksumsFileName |",
    "",
    "---",
    "",
    "## Recommended installation",
    "",
    "Most users should download and run:",
    "",
    "$setupExeFileName",
    "",
    "The setup installer:",
    "",
    "- installs $AppName for the current Windows user;",
    "- does not require administrator privileges;",
    "- adds Start Menu shortcuts;",
    "- can optionally create a desktop shortcut;",
    "- includes the MIT license page;",
    "- includes a safety notice page before installation;",
    "- supports standard Windows uninstall.",
    "",
    "---",
    "",
    "## Portable ZIP alternative",
    "",
    "Advanced users can download:",
    "",
    "$portableZipFileName",
    "",
    "Then:",
    "",
    "1. Extract the ZIP to a local folder.",
    "2. Open the extracted folder.",
    "3. Run GlucoDesk.Desktop.exe.",
    "",
    "The portable ZIP does not create Start Menu shortcuts and does not register an uninstall entry.",
    "",
    "---",
    "",
    "## Verify SHA256 checksums",
    "",
    "From the folder containing the downloaded files, run:",
    "",
    "Get-FileHash .\$setupExeFileName -Algorithm SHA256",
    "Get-FileHash .\$portableZipFileName -Algorithm SHA256",
    "",
    "Expected checksums:",
    "",
    "$setupExeChecksum  $setupExeFileName",
    "$portableZipChecksum  $portableZipFileName",
    "",
    "You can also compare against:",
    "",
    "$checksumsFileName",
    "",
    "---",
    "",
    "## Windows SmartScreen notice",
    "",
    "The Windows installer is currently not code-signed.",
    "",
    "Because of this, Windows SmartScreen may show a warning when opening the setup executable.",
    "",
    "This is expected for the preview until Windows code signing is added.",
    "",
    "---",
    "",
    "## Preview limitations",
    "",
    "This preview is intentionally limited:",
    "",
    "- Windows support currently targets $RuntimeIdentifier.",
    "- The installer is not code-signed yet.",
    "- Auto-update is not available yet.",
    "- This is not a medical-device-grade product.",
    "- Data completeness depends on local history availability and provider connectivity.",
    "- Glucose exports are intended for personal review and discussion with healthcare professionals, not for urgent or automated medical decisions.",
    "",
    "---",
    "",
    "## Source",
    "",
    "Repository:",
    "",
    "$RepositoryUrl",
    ""
)

$releaseNotes = $releaseNotesLines -join [Environment]::NewLine

if (-not (Test-Path $artifactRoot -PathType Container)) {
    New-Item -ItemType Directory -Path $artifactRoot | Out-Null
}

$utf8NoBom = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllText($releaseNotesPath, $releaseNotes, $utf8NoBom)

Write-Step "Windows release notes created successfully"
Write-Host ""
Write-Host "Release notes:"
Write-Host "  $releaseNotesPath"
Write-Host ""
Get-Content $releaseNotesPath