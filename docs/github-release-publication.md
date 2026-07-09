# GlucoDesk GitHub Release Publication

This document describes the final publication step for a GlucoDesk preview release.

## Goal

Publish a GitHub Release with:

- clear release notes
- ready-to-run app packages
- SHA256 checksums
- known limitations
- safety disclaimer
- manual smoke test evidence

## Final release sequence

1. Update the version.

    scripts/quality/set-preview-version.sh --version v0.4.0-preview

2. Prepare release notes and smoke test report.

    scripts/quality/prepare-preview-release.sh --version v0.4.0-preview

3. Complete the generated files.

    docs/release-notes/v0.4.0-preview.md
    docs/smoke-tests/v0.4.0-preview.md

4. Build packages using the official platform-specific packaging scripts.

5. Verify platform artifacts.

    scripts/verify-macos-preview-artifacts.sh 0.4.0-preview osx-arm64
    scripts/verify-macos-preview-artifacts.sh 0.4.0-preview osx-x64

    Run the Windows verifier on Windows when publishing Windows artifacts.

6. Generate artifact manifest and checksums.

    scripts/quality/release-artifacts-manifest.sh --artifacts-dir artifacts/release/v0.4.0-preview --release-version v0.4.0-preview

7. Run final readiness on main.

    scripts/quality/release-readiness-check.sh --strict --release-version v0.4.0-preview --require-smoke-test --artifacts-dir artifacts/release/v0.4.0-preview

8. Create and push the Git tag.

    git tag v0.4.0-preview
    git push origin v0.4.0-preview

9. Create the GitHub Release.

Attach:

- macOS Apple Silicon package
- macOS Intel package
- Windows package or installer, only if supported for that release
- SHA256SUMS.txt
- release-artifacts-manifest.md

## GitHub Release description

Use the content from:

    docs/release-notes/v0.4.0-preview.md

The release description must clearly state:

- supported platform
- installation instructions
- what changed
- known limitations
- safety disclaimer
- whether Windows is supported or experimental

## Safety wording

GlucoDesk is an awareness companion. It is not a medical device and must not be used for treatment or insulin dosing decisions.
