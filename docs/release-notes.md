# GlucoDesk Release Notes Workflow

GlucoDesk preview releases should have versioned release notes before publishing a GitHub Release.

## Create release notes

Example:

    scripts/quality/create-release-notes.sh --version v0.4.0-preview

This creates:

    docs/release-notes/v0.4.0-preview.md

The generated file is based on:

    docs/release-notes/preview-template.md

## Validate release notes

Before publishing, run:

    scripts/quality/release-readiness-check.sh --release-version v0.4.0-preview

For final verification on main:

    scripts/quality/release-readiness-check.sh --strict --release-version v0.4.0-preview

## Required sections

The readiness check requires these sections:

- Supported platform
- Installation
- What's new
- Known limitations
- Safety disclaimer
- Manual smoke test

## Guidance

Release notes should be written for users, not only developers.

Mention:

- what changed
- which platform is supported
- how to install the ready-to-run package
- known limitations
- the safety disclaimer
- what was manually smoke-tested
