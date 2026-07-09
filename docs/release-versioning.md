# GlucoDesk Release Versioning

GlucoDesk uses a plain VERSION file as the source of truth for package version metadata.

The VERSION file should not include the leading v.

Example VERSION value:

    0.4.0-preview

The GitHub Release and Git tag can use the leading v.

Example GitHub Release version:

    v0.4.0-preview

## Set preview version

Use:

    scripts/quality/set-preview-version.sh --version v0.4.0-preview

This writes:

    VERSION = 0.4.0-preview

## Validate version consistency

Use:

    scripts/quality/release-readiness-check.sh --release-version v0.4.0-preview

The readiness check verifies that:

- VERSION exists
- VERSION is not empty
- VERSION matches the release version without the leading v

## Why this matters

Packaging scripts read VERSION when producing release artifacts.

If VERSION and release notes use different versions, the GitHub Release can contain misleading package names or metadata.

## Recommended release sequence

1. Set the version.

    scripts/quality/set-preview-version.sh --version v0.4.0-preview

2. Prepare release notes and smoke test report.

    scripts/quality/prepare-preview-release.sh --version v0.4.0-preview

3. Complete release notes and smoke test report.

4. Run final readiness.

    scripts/quality/release-readiness-check.sh --strict --release-version v0.4.0-preview --require-smoke-test
