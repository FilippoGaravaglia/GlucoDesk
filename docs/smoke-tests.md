# GlucoDesk Smoke Test Workflow

Before publishing a preview release, create and complete a manual smoke test report.

## Create a smoke test report

Example:

    scripts/quality/create-smoke-test-report.sh --version v0.4.0-preview

This creates:

    docs/smoke-tests/v0.4.0-preview.md

## Complete the report

Open the generated file and replace TODO values.

Every manual check should be marked as:

- PASS
- FAIL
- N/A

## Validate during readiness

To require a smoke test report during release readiness:

    scripts/quality/release-readiness-check.sh --release-version v0.4.0-preview --require-smoke-test

For final verification on main:

    scripts/quality/release-readiness-check.sh --strict --release-version v0.4.0-preview --require-smoke-test
