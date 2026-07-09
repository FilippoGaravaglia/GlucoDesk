# GlucoDesk Release Readiness Checklist

This checklist is used before publishing a preview release.

## Automated checks

Run from the repository root:

    scripts/quality/release-readiness-check.sh

For final verification on main before tagging:

    scripts/quality/release-readiness-check.sh --strict

The script checks:

- Git repository and branch state
- Solution discovery
- Required source and test directories
- README and license presence
- Critical menu bar assets
- Restore/build/test in Release configuration
- git diff --check

## Manual smoke test

Before publishing a release, verify:

1. App starts on macOS.
2. Dashboard loads without crashing.
3. Menu bar indicator appears.
4. Presence panel opens from menu bar.
5. Privacy mode Off:
   - glucose value is visible
   - glycemic menu bar icon is shown
6. Privacy mode On:
   - glucose value is hidden
   - neutral blue privacy icon is shown
7. Privacy mode persists after app restart.
8. Mock provider works.
9. Nightscout provider error states are understandable.
10. App can quit cleanly from the presence panel.

## Release notes checklist

Release notes should include:

- Version
- Supported platform
- New features
- Known limitations
- Safety disclaimer
- Installation instructions
- Feedback/contact link

## Safety disclaimer

GlucoDesk is an awareness companion. It is not a medical device and must not be used for treatment or insulin dosing decisions.


## Versioned release notes check

For a final release candidate, provide the release notes version explicitly:

    scripts/quality/release-readiness-check.sh --strict --release-version v0.4.0-preview

This requires a matching file:

    docs/release-notes/v0.4.0-preview.md

The file must contain the required release note sections.


## Release artifacts manifest

Before publishing a GitHub Release, generate artifact checksums:

    scripts/quality/release-artifacts-manifest.sh --artifacts-dir artifacts/release --release-version v0.4.0-preview

For final readiness with artifacts:

    scripts/quality/release-readiness-check.sh --strict --release-version v0.4.0-preview --artifacts-dir artifacts/release

This validates the attached packages and writes SHA256 checksums.

## Release notes creation

Create versioned release notes from the preview template:

    scripts/quality/create-release-notes.sh --version v0.4.0-preview

Then edit the generated file:

    docs/release-notes/v0.4.0-preview.md

Finally validate it:

    scripts/quality/release-readiness-check.sh --release-version v0.4.0-preview

## Manual smoke test report

Create a versioned smoke test report:

    scripts/quality/create-smoke-test-report.sh --version v0.4.0-preview

Then complete:

    docs/smoke-tests/v0.4.0-preview.md

For final readiness with a required smoke test report:

    scripts/quality/release-readiness-check.sh --strict --release-version v0.4.0-preview --require-smoke-test
