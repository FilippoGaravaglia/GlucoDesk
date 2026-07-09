# GlucoDesk Preview Release Closure Workflow

This document describes the final steps used to close a GlucoDesk preview release.

## Prepare a preview release

Create versioned release notes and a manual smoke test report:

    scripts/quality/prepare-preview-release.sh --version v0.4.0-preview

This creates:

    docs/release-notes/v0.4.0-preview.md
    docs/smoke-tests/v0.4.0-preview.md

## Complete release notes

Edit the generated release notes and replace placeholders with real release information.

Required sections:

- Supported platform
- Installation
- What's new
- Known limitations
- Safety disclaimer
- Manual smoke test

## Complete smoke test report

Edit the generated smoke test report and replace TODO values.

Each manual check should be marked as:

- PASS
- FAIL
- N/A

The release decision should be:

- GO
- NO-GO

## Final readiness check

Before tagging on main:

    scripts/quality/release-readiness-check.sh --strict --release-version v0.4.0-preview --require-smoke-test

## Release artifacts

After building ready-to-run packages, generate artifact checksums:

    scripts/quality/release-artifacts-manifest.sh --artifacts-dir artifacts/release --release-version v0.4.0-preview

Then run readiness with artifacts:

    scripts/quality/release-readiness-check.sh --strict --release-version v0.4.0-preview --require-smoke-test --artifacts-dir artifacts/release

## Publishing

Only publish the GitHub Release when:

- readiness check is green
- release notes are complete
- smoke test result is GO
- release artifacts have checksums
- supported platform and known limitations are clear
- safety disclaimer is present
