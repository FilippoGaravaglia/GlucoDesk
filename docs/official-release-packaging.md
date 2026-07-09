# GlucoDesk Official Release Packaging Workflow

This document describes the packaging workflow used for GlucoDesk preview releases.

The project already contains platform-specific packaging and verification scripts. The release process should use those scripts instead of adding parallel packaging workflows.

## Existing packaging and verification scripts

Current release-related scripts include:

- scripts/publish-windows.ps1
- scripts/verify-macos-preview-artifacts.sh
- scripts/verify-windows-preview-artifacts.ps1
- scripts/quality/release-artifacts-manifest.sh
- scripts/quality/release-readiness-check.sh
- scripts/quality/prepare-preview-release.sh
- scripts/quality/create-release-notes.sh
- scripts/quality/create-smoke-test-report.sh

## Recommended release flow

1. Prepare release notes and smoke test report.

    scripts/quality/prepare-preview-release.sh --version v0.4.0-preview

2. Complete release notes.

    docs/release-notes/v0.4.0-preview.md

3. Complete smoke test report.

    docs/smoke-tests/v0.4.0-preview.md

4. Build the platform packages using the existing platform-specific scripts.

    Use the existing macOS packaging script for Apple Silicon and Intel packages.
    Use scripts/publish-windows.ps1 for the Windows portable package or installer flow.

5. Verify macOS artifacts.

    scripts/verify-macos-preview-artifacts.sh 0.4.0-preview osx-arm64
    scripts/verify-macos-preview-artifacts.sh 0.4.0-preview osx-x64

6. Verify Windows artifacts on Windows.

    powershell -ExecutionPolicy Bypass -File scripts/verify-windows-preview-artifacts.ps1 -Version 0.4.0-preview -RuntimeIdentifier win-x64

7. Generate release artifact checksums.

    scripts/quality/release-artifacts-manifest.sh --artifacts-dir artifacts/release/v0.4.0-preview --release-version v0.4.0-preview

8. Run final readiness check on main.

    scripts/quality/release-readiness-check.sh --strict --release-version v0.4.0-preview --require-smoke-test --artifacts-dir artifacts/release/v0.4.0-preview

## Why this workflow avoids duplication

Packaging is platform-specific and already handled by existing scripts.

The quality scripts are responsible for:

- checking that packaging scripts exist
- validating release notes
- validating smoke test reports
- generating checksums
- running build and tests
- checking release readiness

The quality scripts should not duplicate the actual packaging logic unless the existing platform-specific packaging scripts are intentionally retired.

## Artifact expectations

A preview release should attach ready-to-run packages, not only source code archives.

Expected artifact categories:

- macOS Apple Silicon package
- macOS Intel package
- Windows package or setup artifact
- SHA256SUMS.txt
- release-artifacts-manifest.md

## Safety note

GlucoDesk is an awareness companion. It is not a medical device and must not be used for treatment or insulin dosing decisions.
