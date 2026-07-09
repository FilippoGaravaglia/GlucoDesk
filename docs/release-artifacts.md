# GlucoDesk Release Artifacts

Before publishing a GitHub Release, generate a manifest for the files attached to the release.

The manifest records:

- artifact file name
- artifact size in bytes
- SHA256 checksum
- release version, when provided

## Generate a manifest

Example:

    scripts/quality/release-artifacts-manifest.sh --artifacts-dir artifacts/release --release-version v0.4.0-preview

This creates:

    artifacts/release/release-artifacts-manifest.md
    artifacts/release/SHA256SUMS.txt

## Validate artifacts during release readiness

Example:

    scripts/quality/release-readiness-check.sh --strict --release-version v0.4.0-preview --artifacts-dir artifacts/release

## Why this matters

Release assets should be reproducible and verifiable.

A SHA256 manifest helps users and maintainers confirm that a downloaded package is exactly the file that was produced for the release.

## Notes

The script currently supports:

- .zip
- .dmg
- .pkg
- .tar.gz

GlucoDesk preview releases should attach ready-to-run app packages, not only source code archives.
