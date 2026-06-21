param(
    [string]$RuntimeIdentifier = "win-x64",
    [string]$Configuration = "Release",
    [switch]$SkipTests
)

$ErrorActionPreference = "Stop"

$ProjectRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$VersionFile = Join-Path $ProjectRoot "VERSION"
$SolutionFile = Join-Path $ProjectRoot "GlucoDesk.slnx"
$DesktopProject = Join-Path $ProjectRoot "src/GlucoDesk.Desktop/GlucoDesk.Desktop.csproj"

if (-not (Test-Path $VersionFile)) {
    throw "VERSION file not found at $VersionFile"
}

$Version = (Get-Content $VersionFile -Raw).Trim()

if ([string]::IsNullOrWhiteSpace($Version)) {
    throw "VERSION file is empty"
}

$BaseVersion = ($Version -split "-")[0]
$ArtifactsRoot = Join-Path $ProjectRoot "artifacts"
$PublishDir = Join-Path $ArtifactsRoot "publish/GlucoDesk-$Version-$RuntimeIdentifier"
$ReleasesDir = Join-Path $ArtifactsRoot "releases"
$ArchivePath = Join-Path $ReleasesDir "GlucoDesk-$Version-$RuntimeIdentifier-portable.zip"

Write-Host "Publishing GlucoDesk $Version for $RuntimeIdentifier"
Write-Host "Project root: $ProjectRoot"

if (Test-Path $PublishDir) {
    Remove-Item $PublishDir -Recurse -Force
}

New-Item -ItemType Directory -Path $PublishDir -Force | Out-Null
New-Item -ItemType Directory -Path $ReleasesDir -Force | Out-Null

dotnet clean $SolutionFile
dotnet restore $SolutionFile
dotnet build $SolutionFile -c $Configuration --no-restore

if (-not $SkipTests) {
    dotnet test $SolutionFile -c $Configuration --no-build
}

dotnet publish $DesktopProject `
    -c $Configuration `
    -r $RuntimeIdentifier `
    --self-contained true `
    -p:PublishSingleFile=false `
    -p:Version="$Version" `
    -p:AssemblyVersion="$BaseVersion.0" `
    -p:FileVersion="$BaseVersion.0" `
    -o $PublishDir

if (-not (Test-Path (Join-Path $PublishDir "GlucoDesk.Desktop.exe"))) {
    throw "Expected executable not found: $(Join-Path $PublishDir "GlucoDesk.Desktop.exe")"
}

if (Test-Path $ArchivePath) {
    Remove-Item $ArchivePath -Force
}

Compress-Archive `
    -Path (Join-Path $PublishDir "*") `
    -DestinationPath $ArchivePath `
    -Force

Write-Host ""
Write-Host "Windows portable package created:"
Write-Host $ArchivePath
Write-Host ""
Write-Host "To test locally on Windows:"
Write-Host "1. Extract the zip"
Write-Host "2. Run GlucoDesk.Desktop.exe"