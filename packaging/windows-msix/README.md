# GlucoDesk Microsoft Store MSIX

This directory contains the Microsoft Store MSIX manifest template and the
documentation required to build and locally test the Microsoft Store package.

## Store identity

- Package identity name: `FilippoGaravaglia.GlucoDesk`
- Publisher: `CN=2E604FFD-8484-4382-9A03-68FB9F2DFBF5`
- Publisher display name: `Filippo Garavaglia`
- Package family name: `FilippoGaravaglia.GlucoDesk_wwvep82rbc37p`
- Store ID: `9PD3JHJ49LRS`

The package identity values must exactly match the values assigned by
Microsoft Partner Center.

Do not change the package identity name or publisher without also updating
the product identity in Partner Center.

## Requirements

The Store MSIX package must be generated on Windows.

Required tools:

- .NET 10 SDK
- Windows 10 or Windows 11 SDK
- `MakeAppx.exe`
- `SignTool.exe`
- PowerShell

The Windows SDK tools are normally installed through Visual Studio Installer
or the standalone Windows SDK installer.

## Build the unsigned Store package

Run the following command from the repository root on Windows:

    powershell -ExecutionPolicy Bypass `
      -File scripts\create-windows-store-msix.ps1 `
      -PackageVersion 0.3.0.0

The unsigned package is intended for upload to Microsoft Partner Center.

Expected output:

    artifacts/store/0.3.0.0/win-x64/GlucoDesk-0.3.0.0-win-x64-store-unsigned.msix

Do not try to install the unsigned Store package directly on Windows.

## Build and install a local development package

To create a temporary local signing certificate, trust it for the current
Windows user, sign the development package and install it, run:

    powershell -ExecutionPolicy Bypass `
      -File scripts\create-windows-store-msix.ps1 `
      -PackageVersion 0.3.0.0 `
      -CreateDevelopmentCertificate `
      -InstallDevelopmentCertificate `
      -InstallPackage

Expected local development package:

    artifacts/store/0.3.0.0/win-x64/GlucoDesk-0.3.0.0-win-x64-store-dev-signed.msix

## Local validation checklist

After installing the development package:

1. Launch GlucoDesk from the Windows Start menu.
2. Confirm the displayed application name is `GlucoDesk`.
3. Verify the dashboard opens correctly.
4. Verify local settings and history persistence.
5. Verify Nightscout configuration and data loading.
6. Verify Dexcom configuration where available.
7. Verify the Windows tray icon.
8. Verify native Windows notifications.
9. Verify PDF and Excel export.
10. Close and reopen the application.
11. Uninstall GlucoDesk from Windows Settings.
12. Confirm the application files are removed correctly.

## Versioning

Microsoft Store package versions use four numeric components:

    Major.Minor.Build.Revision

Examples:

    0.3.0.0
    0.3.1.0
    0.4.0.0

Every package uploaded to Partner Center must have a version greater than the
previously submitted package version.

## Security

The generated development certificate and PFX are local testing artifacts.

They must never be committed to Git or uploaded to GitHub.

The following paths and extensions are ignored by the repository:

    artifacts/store/
    *.pfx

The unsigned Store MSIX does not need to be signed locally before being
uploaded to Partner Center. Microsoft signs the certified package for Store
distribution.
