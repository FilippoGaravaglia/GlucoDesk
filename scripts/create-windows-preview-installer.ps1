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

$ReleaseReadmePath = Join-Path $PublishDir "README-WINDOWS-PREVIEW.txt"

@"
GlucoDesk $Version ($RuntimeIdentifier)

This is a Windows preview build of GlucoDesk.

Safety notice:
GlucoDesk is not a medical device. It does not provide medical advice, treatment decisions, insulin dosing guidance, alarms, or emergency notifications. Always rely on approved CGM apps, pump systems, and healthcare professionals for medical decisions.

Preview limitations:
- local history may be incomplete;
- export quality depends on available local readings;
- provider behavior depends on configuration and external service availability;
- Windows support is currently preview-level.
"@ | Set-Content -Path $ReleaseReadmePath -Encoding UTF8

$LicensePath = Join-Path $RootDir "LICENSE"

if (Test-Path $LicensePath) {
    Copy-Item $LicensePath (Join-Path $PublishDir "LICENSE.txt") -Force
}

Write-Step "creating portable zip"
Remove-Item -Force $PortableZipPath -ErrorAction SilentlyContinue
Compress-Archive -Path (Join-Path $PublishDir "*") -DestinationPath $PortableZipPath -Force

Write-Step "building Inno Setup installer"
& $InnoCompilerPath `
    "/DMyAppVersion=$Version" `
    "/DRuntimeIdentifier=$RuntimeIdentifier" `
    "/DSourceDir=$PublishDir" `
    "/DOutputDir=$InstallerOutputDir" `
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
Write-Host ""
Write-Host "Manual smoke test:"
Write-Host "  1. Run the setup EXE."
Write-Host "  2. Complete the wizard."
Write-Host "  3. Launch GlucoDesk."
Write-Host "  4. Verify dashboard, account/settings, export flow and uninstall."
