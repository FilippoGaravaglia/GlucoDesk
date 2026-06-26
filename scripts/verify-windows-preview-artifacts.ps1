param(
    [string]$Version = "0.2.1-preview",
    [string]$RuntimeIdentifier = "win-x64"
)

$ErrorActionPreference = "Stop"

$AppName = "GlucoDesk"
$ExecutableName = "GlucoDesk.Desktop.exe"

$RootDir = Resolve-Path (Join-Path $PSScriptRoot "..")
$ArtifactRoot = Join-Path $RootDir "artifacts/windows/$Version/$RuntimeIdentifier"

$PortableZipPath = Join-Path $ArtifactRoot "$AppName-$Version-$RuntimeIdentifier-portable.zip"
$SetupPath = Join-Path $ArtifactRoot "$AppName-$Version-$RuntimeIdentifier-setup.exe"
$ChecksumsPath = Join-Path $ArtifactRoot "$AppName-$Version-$RuntimeIdentifier-checksums.sha256"

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
        [string]$Label
    )

    if (-not (Test-Path $Path)) {
        Fail "$Label not found: $Path"
    }

    if ((Get-Item $Path).Length -le 0) {
        Fail "$Label is empty: $Path"
    }

    Write-Step "$Label found"
}

function Test-ChecksumFile {
    param([string]$ChecksumFilePath)

    $checksumDirectory = Split-Path -Parent $ChecksumFilePath
    $lines = Get-Content $ChecksumFilePath | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }

    if ($lines.Count -eq 0) {
        Fail "checksum file is empty: $ChecksumFilePath"
    }

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

function Test-ZipEntries {
    param(
        [string]$ZipPath,
        [string[]]$RequiredEntries
    )

    Add-Type -AssemblyName System.IO.Compression.FileSystem

    $zip = [System.IO.Compression.ZipFile]::OpenRead($ZipPath)

    try {
        foreach ($requiredEntry in $RequiredEntries) {
            $entry = $zip.Entries |
                Where-Object { $_.FullName -eq $requiredEntry } |
                Select-Object -First 1

            if (-not $entry) {
                Fail "portable zip does not contain required entry: $requiredEntry"
            }
        }
    }
    finally {
        $zip.Dispose()
    }
}

if ($env:OS -ne "Windows_NT") {
    Fail "Windows artifact verification must be run on Windows."
}

if ($RuntimeIdentifier -ne "win-x64") {
    Fail "unsupported runtime identifier '$RuntimeIdentifier'. This verifier currently supports win-x64 only."
}

Write-Step "verifying Windows preview artifacts for $AppName $Version $RuntimeIdentifier"

if (-not (Test-Path $ArtifactRoot)) {
    Fail "artifact directory not found: $ArtifactRoot"
}

Assert-FileExists -Path $PortableZipPath -Label "portable zip"
Assert-FileExists -Path $SetupPath -Label "setup executable"
Assert-FileExists -Path $ChecksumsPath -Label "checksums file"

$portableZipName = Split-Path -Leaf $PortableZipPath
$setupName = Split-Path -Leaf $SetupPath

$checksumsContent = Get-Content $ChecksumsPath -Raw

if (-not $checksumsContent.Contains($portableZipName)) {
    Fail "checksums file does not reference portable zip: $portableZipName"
}

if (-not $checksumsContent.Contains($setupName)) {
    Fail "checksums file does not reference setup executable: $setupName"
}

Write-Step "verifying portable zip required entries"
Test-ZipEntries -ZipPath $PortableZipPath -RequiredEntries @(
    $ExecutableName,
    "README-WINDOWS-PREVIEW.txt",
    "SAFETY-NOTICE.txt",
    "LICENSE.txt"
)

Write-Step "verifying SHA256 checksums"
Test-ChecksumFile -ChecksumFilePath $ChecksumsPath

if (Get-Command Get-AuthenticodeSignature -ErrorAction SilentlyContinue) {
    $signature = Get-AuthenticodeSignature -FilePath $SetupPath
    Write-Host "Setup signature status: $($signature.Status)"
}

Write-Step "Windows preview artifacts verified successfully"
