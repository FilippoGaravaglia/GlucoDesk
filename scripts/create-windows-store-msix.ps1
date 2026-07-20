param(
    [string]$PackageVersion = "0.3.0.0",
    [string]$RuntimeIdentifier = "win-x64",
    [switch]$SkipTests,
    [switch]$CreateDevelopmentCertificate,
    [switch]$InstallDevelopmentCertificate,
    [switch]$InstallPackage
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$AppName = "GlucoDesk"
$ExecutableName = "GlucoDesk.Desktop.exe"
$ExpectedPublisher = "CN=2E604FFD-8484-4382-9A03-68FB9F2DFBF5"

$RootDir = Resolve-Path (Join-Path $PSScriptRoot "..")
$ProjectPath = Join-Path $RootDir "src/GlucoDesk.Desktop/GlucoDesk.Desktop.csproj"
$ManifestTemplatePath = Join-Path $RootDir "packaging/windows-msix/AppxManifest.template.xml"
$SourceIconPath = Join-Path $RootDir "src/GlucoDesk.Desktop/Assets/AppIcon/glucodesk-app-icon.png"

$ArtifactRoot = Join-Path $RootDir "artifacts/store/$PackageVersion/$RuntimeIdentifier"
$PublishDir = Join-Path $ArtifactRoot "publish"
$PackageRoot = Join-Path $ArtifactRoot "package-root"
$PackageAssetsDir = Join-Path $PackageRoot "Assets"
$UnsignedPackagePath = Join-Path $ArtifactRoot "$AppName-$PackageVersion-$RuntimeIdentifier-store-unsigned.msix"
$SignedPackagePath = Join-Path $ArtifactRoot "$AppName-$PackageVersion-$RuntimeIdentifier-store-dev-signed.msix"
$CertificatePath = Join-Path $ArtifactRoot "$AppName-store-development.cer"
$PfxPath = Join-Path $ArtifactRoot "$AppName-store-development.pfx"
$PfxPassword = "GlucoDesk-Local-Development-Only"

function Write-Step {
    param([string]$Message)
    Write-Host ""
    Write-Host "==> $Message"
}

function Fail {
    param([string]$Message)
    throw "error: $Message"
}

function Require-Path {
    param(
        [string]$Path,
        [string]$Description
    )

    if (-not (Test-Path $Path)) {
        Fail "$Description not found: $Path"
    }
}

function Find-WindowsSdkTool {
    param([string]$ExecutableName)

    $candidateRoots = @(
        "${env:ProgramFiles(x86)}\Windows Kits\10\bin",
        "$env:ProgramFiles\Windows Kits\10\bin"
    ) | Where-Object { $_ -and (Test-Path $_) }

    foreach ($root in $candidateRoots) {
        $candidate = Get-ChildItem `
            -Path $root `
            -Filter $ExecutableName `
            -Recurse `
            -File `
            -ErrorAction SilentlyContinue |
            Where-Object { $_.FullName -match '\\x64\\' } |
            Sort-Object FullName -Descending |
            Select-Object -First 1

        if ($candidate) {
            return $candidate.FullName
        }
    }

    Fail "$ExecutableName was not found. Install the Windows 10/11 SDK."
}

function Assert-PackageVersion {
    param([string]$Version)

    if ($Version -notmatch '^\d+\.\d+\.\d+\.\d+$') {
        Fail "PackageVersion must contain four numeric components, for example 0.3.0.0."
    }

    foreach ($component in ($Version -split '\.')) {
        $value = [int]$component

        if ($value -lt 0 -or $value -gt 65535) {
            Fail "Each package version component must be between 0 and 65535."
        }
    }
}

function New-ResizedPng {
    param(
        [string]$InputPath,
        [string]$OutputPath,
        [int]$Width,
        [int]$Height,
        [switch]$WideCanvas
    )

    Add-Type -AssemblyName System.Drawing

    $source = [System.Drawing.Image]::FromFile($InputPath)

    try {
        $bitmap = New-Object System.Drawing.Bitmap($Width, $Height)
        $graphics = [System.Drawing.Graphics]::FromImage($bitmap)

        try {
            $graphics.Clear([System.Drawing.Color]::Transparent)
            $graphics.CompositingQuality =
                [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
            $graphics.InterpolationMode =
                [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
            $graphics.SmoothingMode =
                [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
            $graphics.PixelOffsetMode =
                [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality

            $paddingRatio = if ($WideCanvas) { 0.25 } else { 0.12 }
            $availableWidth = [int]($Width * (1 - (2 * $paddingRatio)))
            $availableHeight = [int]($Height * (1 - (2 * $paddingRatio)))

            $scale = [Math]::Min(
                $availableWidth / $source.Width,
                $availableHeight / $source.Height
            )

            $drawWidth = [int]($source.Width * $scale)
            $drawHeight = [int]($source.Height * $scale)
            $x = [int](($Width - $drawWidth) / 2)
            $y = [int](($Height - $drawHeight) / 2)

            $graphics.DrawImage(
                $source,
                (New-Object System.Drawing.Rectangle($x, $y, $drawWidth, $drawHeight))
            )

            $bitmap.Save(
                $OutputPath,
                [System.Drawing.Imaging.ImageFormat]::Png
            )
        }
        finally {
            $graphics.Dispose()
            $bitmap.Dispose()
        }
    }
    finally {
        $source.Dispose()
    }
}

if ($env:OS -ne "Windows_NT") {
    Fail "MSIX creation must run on Windows."
}

if ($RuntimeIdentifier -ne "win-x64") {
    Fail "Only win-x64 is currently supported."
}

Assert-PackageVersion -Version $PackageVersion

Require-Path -Path $ProjectPath -Description "Desktop project"
Require-Path -Path $ManifestTemplatePath -Description "MSIX manifest template"
Require-Path -Path $SourceIconPath -Description "Source application icon"

$MakeAppxPath = Find-WindowsSdkTool -ExecutableName "makeappx.exe"
$SignToolPath = Find-WindowsSdkTool -ExecutableName "signtool.exe"

Write-Step "Windows SDK tools"
Write-Host "MakeAppx: $MakeAppxPath"
Write-Host "SignTool: $SignToolPath"

Write-Step "cleaning output directory"
Remove-Item -Recurse -Force $ArtifactRoot -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path $PublishDir | Out-Null
New-Item -ItemType Directory -Force -Path $PackageRoot | Out-Null
New-Item -ItemType Directory -Force -Path $PackageAssetsDir | Out-Null

Push-Location $RootDir

try {
    Write-Step "restoring solution"
    dotnet restore
    if ($LASTEXITCODE -ne 0) {
        Fail "dotnet restore failed with exit code $LASTEXITCODE"
    }

    Write-Step "building solution"
    dotnet build -c Release --no-restore
    if ($LASTEXITCODE -ne 0) {
        Fail "dotnet build failed with exit code $LASTEXITCODE"
    }

    if (-not $SkipTests) {
        Write-Step "running tests"
        dotnet test -c Release --no-build
        if ($LASTEXITCODE -ne 0) {
            Fail "dotnet test failed with exit code $LASTEXITCODE"
        }
    }
    else {
        Write-Step "skipping tests"
    }

    Write-Step "publishing Windows desktop application"
    dotnet publish $ProjectPath `
        -c Release `
        -r $RuntimeIdentifier `
        --self-contained true `
        -p:PublishSingleFile=false `
        -p:DebugType=None `
        -p:DebugSymbols=false `
        -o $PublishDir

    if ($LASTEXITCODE -ne 0) {
        Fail "dotnet publish failed with exit code $LASTEXITCODE"
    }
}
finally {
    Pop-Location
}

Require-Path `
    -Path (Join-Path $PublishDir $ExecutableName) `
    -Description "Published executable"

Write-Step "copying application payload"
Copy-Item `
    -Path (Join-Path $PublishDir "*") `
    -Destination $PackageRoot `
    -Recurse `
    -Force

Write-Step "generating Store visual assets"
New-ResizedPng `
    -InputPath $SourceIconPath `
    -OutputPath (Join-Path $PackageAssetsDir "StoreLogo.png") `
    -Width 50 `
    -Height 50

New-ResizedPng `
    -InputPath $SourceIconPath `
    -OutputPath (Join-Path $PackageAssetsDir "Square44x44Logo.png") `
    -Width 44 `
    -Height 44

New-ResizedPng `
    -InputPath $SourceIconPath `
    -OutputPath (Join-Path $PackageAssetsDir "Square150x150Logo.png") `
    -Width 150 `
    -Height 150

New-ResizedPng `
    -InputPath $SourceIconPath `
    -OutputPath (Join-Path $PackageAssetsDir "Wide310x150Logo.png") `
    -Width 310 `
    -Height 150 `
    -WideCanvas

New-ResizedPng `
    -InputPath $SourceIconPath `
    -OutputPath (Join-Path $PackageAssetsDir "Square310x310Logo.png") `
    -Width 310 `
    -Height 310

New-ResizedPng `
    -InputPath $SourceIconPath `
    -OutputPath (Join-Path $PackageAssetsDir "SplashScreen.png") `
    -Width 620 `
    -Height 300 `
    -WideCanvas

Write-Step "creating AppxManifest.xml"
$manifest = Get-Content -Raw $ManifestTemplatePath
$manifest = $manifest.Replace("__PACKAGE_VERSION__", $PackageVersion)
$manifest | Set-Content `
    -Path (Join-Path $PackageRoot "AppxManifest.xml") `
    -Encoding UTF8

Write-Step "creating unsigned Store MSIX"
Remove-Item -Force $UnsignedPackagePath -ErrorAction SilentlyContinue

& $MakeAppxPath pack `
    /d $PackageRoot `
    /p $UnsignedPackagePath `
    /o

if ($LASTEXITCODE -ne 0) {
    Fail "MakeAppx failed with exit code $LASTEXITCODE"
}

Require-Path -Path $UnsignedPackagePath -Description "Unsigned MSIX package"

Write-Step "validating package contents"
& $MakeAppxPath validate `
    /p $UnsignedPackagePath

if ($LASTEXITCODE -ne 0) {
    Fail "MakeAppx validation failed with exit code $LASTEXITCODE"
}

if ($CreateDevelopmentCertificate) {
    Write-Step "creating local development signing certificate"

    $certificate = New-SelfSignedCertificate `
        -Type Custom `
        -Subject $ExpectedPublisher `
        -KeyUsage DigitalSignature `
        -FriendlyName "GlucoDesk Store Local Development" `
        -CertStoreLocation "Cert:\CurrentUser\My" `
        -TextExtension @(
            "2.5.29.37={text}1.3.6.1.5.5.7.3.3",
            "2.5.29.19={text}"
        )

    $securePassword = ConvertTo-SecureString `
        -String $PfxPassword `
        -Force `
        -AsPlainText

    Export-PfxCertificate `
        -Cert $certificate `
        -FilePath $PfxPath `
        -Password $securePassword |
        Out-Null

    Export-Certificate `
        -Cert $certificate `
        -FilePath $CertificatePath |
        Out-Null

    Copy-Item $UnsignedPackagePath $SignedPackagePath -Force

    Write-Step "signing local development package"
    & $SignToolPath sign `
        /fd SHA256 `
        /a `
        /f $PfxPath `
        /p $PfxPassword `
        $SignedPackagePath

    if ($LASTEXITCODE -ne 0) {
        Fail "SignTool failed with exit code $LASTEXITCODE"
    }

    Write-Step "verifying development signature"
    & $SignToolPath verify `
        /pa `
        /v `
        $SignedPackagePath

    if ($LASTEXITCODE -ne 0) {
        Fail "Signature verification failed with exit code $LASTEXITCODE"
    }

    if ($InstallDevelopmentCertificate) {
        Write-Step "installing development certificate for current user"
        Import-Certificate `
            -FilePath $CertificatePath `
            -CertStoreLocation "Cert:\CurrentUser\TrustedPeople" |
            Out-Null
    }

    if ($InstallPackage) {
        if (-not $InstallDevelopmentCertificate) {
            Fail "-InstallPackage also requires -InstallDevelopmentCertificate."
        }

        Write-Step "installing development MSIX"
        Add-AppxPackage -Path $SignedPackagePath
    }
}

Write-Step "MSIX build completed"
Write-Host ""
Write-Host "Store upload package:"
Write-Host "  $UnsignedPackagePath"

if ($CreateDevelopmentCertificate) {
    Write-Host ""
    Write-Host "Local test package:"
    Write-Host "  $SignedPackagePath"
    Write-Host ""
    Write-Host "Development certificate:"
    Write-Host "  $CertificatePath"
}

Write-Host ""
Write-Host "Package identity:"
Write-Host "  Name: FilippoGaravaglia.GlucoDesk"
Write-Host "  Publisher: $ExpectedPublisher"
Write-Host "  Version: $PackageVersion"
