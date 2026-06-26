param(
    [string]$Version = "0.2.1-preview",
    [string]$RuntimeIdentifier = "win-x64",
    [switch]$SkipTests
)

$ErrorActionPreference = "Stop"

$AppName = "GlucoDesk"
$ExecutableName = "GlucoDesk.Desktop.exe"

$RootDir = Resolve-Path (Join-Path $PSScriptRoot "..")
$ProjectPath = Join-Path $RootDir "src/GlucoDesk.Desktop/GlucoDesk.Desktop.csproj"
$InstallerScriptPath = Join-Path $RootDir "installer/windows/GlucoDesk.iss"

$ArtifactRoot = Join-Path $RootDir "artifacts/windows/$Version/$RuntimeIdentifier"
$PublishDir = Join-Path $ArtifactRoot "publish"
$InstallerOutputDir = Join-Path $ArtifactRoot "installer"

$PortableZipPath = Join-Path $ArtifactRoot "$AppName-$Version-$RuntimeIdentifier-portable.zip"
$SetupPath = Join-Path $InstallerOutputDir "$AppName-$Version-$RuntimeIdentifier-setup.exe"
$SetupReleasePath = Join-Path $ArtifactRoot "$AppName-$Version-$RuntimeIdentifier-setup.exe"
$ChecksumsPath = Join-Path $ArtifactRoot "$AppName-$Version-$RuntimeIdentifier-checksums.sha256"

$InstallerLicensePath = Join-Path $ArtifactRoot "LICENSE.txt"
$InstallerSafetyNoticePath = Join-Path $ArtifactRoot "WINDOWS-INSTALLER-SAFETY-NOTICE.txt"
$InstallerAfterInstallPath = Join-Path $ArtifactRoot "WINDOWS-INSTALLER-AFTER-INSTALL.txt"

function Write-Step {
    param([string]$Message)

    Write-Host "==> $Message"
}

function Fail {
    param([string]$Message)

    throw "error: $Message"
}

function Require-Command {
    param([string]$CommandName)

    if (-not (Get-Command $CommandName -ErrorAction SilentlyContinue)) {
        Fail "required command '$CommandName' was not found"
    }
}

function Resolve-InnoSetupCompiler {
    $fromPath = Get-Command "ISCC.exe" -ErrorAction SilentlyContinue

    if ($fromPath) {
        return $fromPath.Source
    }

    $candidates = @(
        "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
        "$env:ProgramFiles\Inno Setup 6\ISCC.exe",
        "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe"
    )

    foreach ($candidate in $candidates) {
        if ($candidate -and (Test-Path $candidate)) {
            return $candidate
        }
    }

    Fail "Inno Setup compiler was not found. Install Inno Setup 6 and try again."
}

function New-ChecksumLine {
    param([string]$Path)

    $hash = (Get-FileHash -Algorithm SHA256 -Path $Path).Hash.ToLowerInvariant()
    $fileName = Split-Path -Leaf $Path

    return "$hash  $fileName"
}

function Test-ChecksumFile {
    param([string]$ChecksumFilePath)

    $checksumDirectory = Split-Path -Parent $ChecksumFilePath
    $lines = Get-Content $ChecksumFilePath | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }

    foreach ($line in $lines) {
        $parts = $line -split "\s+", 2

        if ($parts.Count -ne 2) {
            Fail "invalid checksum line: $line"
        }

        $expectedHash = $parts[0].Trim().ToLowerInvariant()
        $fileName = $parts[1].Trim()
        $targetPath = Join-Path $checksumDirectory $fileName

        if (-not (Test-Path $targetPath)) {
            Fail "checksum target not found: $targetPath"
        }

        $actualHash = (Get-FileHash -Algorithm SHA256 -Path $targetPath).Hash.ToLowerInvariant()

        if ($actualHash -ne $expectedHash) {
            Fail "checksum mismatch for $fileName"
        }

        Write-Host "${fileName}: OK"
    }
}

function Test-ZipContainsExecutable {
    param(
        [string]$ZipPath,
        [string]$ExecutableFileName
    )

    Add-Type -AssemblyName System.IO.Compression.FileSystem

    $zip = [System.IO.Compression.ZipFile]::OpenRead($ZipPath)

    try {
        $entry = $zip.Entries | Where-Object { $_.FullName -eq $ExecutableFileName } | Select-Object -First 1

        if (-not $entry) {
            Fail "portable zip does not contain $ExecutableFileName"
        }
    }
    finally {
        $zip.Dispose()
    }
}

function Write-InstallerTextFiles {
    $licenseSourcePath = Join-Path $RootDir "LICENSE"

    if (-not (Test-Path $licenseSourcePath)) {
        Fail "LICENSE file not found: $licenseSourcePath"
    }

    Copy-Item $licenseSourcePath $InstallerLicensePath -Force

    @"
GlucoDesk Windows Preview Safety Notice

GlucoDesk is not a medical device.

GlucoDesk does not provide medical advice, treatment decisions, insulin dosing guidance, alarms, or emergency notifications.

Always rely on approved CGM/mobile apps, pump systems, and healthcare professionals for medical decisions.

This Windows build is a preview release intended for awareness, personal review, and desktop convenience only.

Preview limitations:
- local history may be incomplete;
- export quality depends on available local readings;
- provider behavior depends on account configuration and external service availability;
- Windows support is currently preview-level;
- this installer is not code-signed yet and Windows SmartScreen may show warnings.

Do not use GlucoDesk as a replacement for approved diabetes applications or medical devices.
"@ | Set-Content -Path $InstallerSafetyNoticePath -Encoding UTF8

    @"
GlucoDesk Windows Preview Installed

Thank you for installing GlucoDesk.

Installed application:
GlucoDesk.Desktop.exe

This is a per-user installation. It does not require administrator privileges and is installed under the current user's local application data folder.

Useful notes:
- GlucoDesk stores local app data outside the installation folder.
- Uninstalling the application removes the installed program files.
- Local app data and operating-system credential storage may remain outside the installation folder.
- Review exported PDF/Excel files carefully before sharing them.

Safety reminder:
GlucoDesk is not a medical device and must not be used for treatment decisions, insulin dosing, emergency alerts, or as a replacement for approved diabetes applications.
"@ | Set-Content -Path $InstallerAfterInstallPath -Encoding UTF8
}

function Write-PortableDocumentation {
    $releaseReadmePath = Join-Path $PublishDir "README-WINDOWS-PREVIEW.txt"
    $portableSafetyPath = Join-Path $PublishDir "SAFETY-NOTICE.txt"
    $licenseSourcePath = Join-Path $RootDir "LICENSE"

    @"
GlucoDesk $Version ($RuntimeIdentifier)

This is a Windows preview build of GlucoDesk.

How to use the portable package:
1. Extract the zip into a normal folder.
2. Do not run GlucoDesk directly from inside the compressed zip.
3. Run GlucoDesk.Desktop.exe from the extracted folder.

Safety notice:
GlucoDesk is not a medical device. It does not provide medical advice, treatment decisions, insulin dosing guidance, alarms, or emergency notifications. Always rely on approved CGM apps, pump systems, and healthcare professionals for medical decisions.

Preview limitations:
- local history may be incomplete;
- export quality depends on available local readings;
- provider behavior depends on configuration and external service availability;
- Windows support is currently preview-level;
- the Windows installer is not code-signed yet and Windows SmartScreen may show warnings.
"@ | Set-Content -Path $releaseReadmePath -Encoding UTF8

    @"
GlucoDesk Safety Notice

GlucoDesk is not a medical device.

Do not use GlucoDesk for treatment decisions, insulin dosing decisions, emergency alerts, or as a replacement for approved diabetes applications.

Generated PDF/Excel exports are informational and depend on local data availability.

Always use approved medical devices, official CGM/mobile apps, pump systems, and healthcare professional guidance for medical decisions.
"@ | Set-Content -Path $portableSafetyPath -Encoding UTF8

    Copy-Item $licenseSourcePath (Join-Path $PublishDir "LICENSE.txt") -Force
}

if ($env:OS -ne "Windows_NT") {
    Fail "Windows installer creation must be run on Windows."
}

if ($RuntimeIdentifier -ne "win-x64") {
    Fail "unsupported runtime identifier '$RuntimeIdentifier'. This preview installer currently supports win-x64 only."
}

Require-Command "dotnet"

$InnoCompilerPath = Resolve-InnoSetupCompiler

if (-not (Test-Path $InstallerScriptPath)) {
    Fail "installer script not found: $InstallerScriptPath"
}

Write-Step "creating Windows preview installer for $AppName $Version $RuntimeIdentifier"

Remove-Item -Recurse -Force $ArtifactRoot -ErrorAction SilentlyContinue

New-Item -ItemType Directory -Force -Path $PublishDir | Out-Null
New-Item -ItemType Directory -Force -Path $InstallerOutputDir | Out-Null

Write-InstallerTextFiles

Push-Location $RootDir

try {
    Write-Step "restoring solution"
    dotnet restore

    Write-Step "building solution"
    dotnet build -c Release --no-restore

    if (-not $SkipTests) {
        Write-Step "running tests"
        dotnet test -c Release --no-build
    }
    else {
        Write-Step "skipping tests because -SkipTests was provided"
    }

    Write-Step "publishing desktop project"
    dotnet publish $ProjectPath `
        -c Release `
        -r $RuntimeIdentifier `
        --self-contained true `
        -o $PublishDir `
        -p:PublishSingleFile=false `
        -p:DebugType=None `
        -p:DebugSymbols=false
}
finally {
    Pop-Location
}

$PublishedExePath = Join-Path $PublishDir $ExecutableName

if (-not (Test-Path $PublishedExePath)) {
    Fail "expected executable not found: $PublishedExePath"
}

Write-PortableDocumentation

Write-Step "creating portable zip"
Remove-Item -Force $PortableZipPath -ErrorAction SilentlyContinue
Compress-Archive -Path (Join-Path $PublishDir "*") -DestinationPath $PortableZipPath -Force

Write-Step "building Inno Setup installer"
& $InnoCompilerPath `
    "/DMyAppVersion=$Version" `
    "/DRuntimeIdentifier=$RuntimeIdentifier" `
    "/DSourceDir=$PublishDir" `
    "/DOutputDir=$InstallerOutputDir" `
    "/DLicenseFilePath=$InstallerLicensePath" `
    "/DInfoBeforeFilePath=$InstallerSafetyNoticePath" `
    "/DInfoAfterFilePath=$InstallerAfterInstallPath" `
    $InstallerScriptPath

if (-not (Test-Path $SetupPath)) {
    Fail "expected setup executable not found: $SetupPath"
}

Copy-Item $SetupPath $SetupReleasePath -Force

if (-not (Test-Path $SetupReleasePath)) {
    Fail "expected release setup executable not found: $SetupReleasePath"
}

if ((Get-Item $SetupReleasePath).Length -le 0) {
    Fail "setup executable is empty: $SetupReleasePath"
}

if ((Get-Item $PortableZipPath).Length -le 0) {
    Fail "portable zip is empty: $PortableZipPath"
}

Write-Step "verifying portable zip content"
Test-ZipContainsExecutable -ZipPath $PortableZipPath -ExecutableFileName $ExecutableName

Write-Step "creating SHA256 checksums"
@(
    New-ChecksumLine -Path $PortableZipPath
    New-ChecksumLine -Path $SetupReleasePath
) | Set-Content -Path $ChecksumsPath -Encoding ASCII

Write-Step "verifying SHA256 checksums"
Test-ChecksumFile -ChecksumFilePath $ChecksumsPath

Write-Step "Windows preview installer assets created successfully"
Write-Host ""
Write-Host "Artifacts:"
Write-Host "  Portable ZIP: $PortableZipPath"
Write-Host "  Setup EXE: $SetupReleasePath"
Write-Host "  Checksums: $ChecksumsPath"
Write-Host "  Installer safety notice: $InstallerSafetyNoticePath"
Write-Host "  Installer after-install notes: $InstallerAfterInstallPath"
Write-Host ""
Write-Host "Manual smoke test:"
Write-Host "  1. Run the setup EXE."
Write-Host "  2. Confirm the MIT license page appears."
Write-Host "  3. Confirm the safety notice page appears."
Write-Host "  4. Complete the wizard."
Write-Host "  5. Launch GlucoDesk."
Write-Host "  6. Verify dashboard, account/settings, export flow and uninstall."
