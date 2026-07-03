# Release artifacts smoke-test checklist

Use this checklist after generating release artifacts with:

    scripts/build-release-artifacts.sh

GlucoDesk notifications are non-medical awareness prompts. They must never replace Dexcom, Omnipod, Nightscout, clinical guidance, or emergency glucose alerts.

## Artifact build validation

| Check | Expected result | Result |
| --- | --- | --- |
| Build script runs | `scripts/build-release-artifacts.sh` completes successfully | Pass / Fail |
| Release build passes | `dotnet build -c Release` succeeds | Pass / Fail |
| Release tests pass | `dotnet test -c Release` succeeds | Pass / Fail |
| Whitespace check passes | `git diff --check` has no output | Pass / Fail |
| macOS Intel artifact exists | `GlucoDesk-osx-x64*.zip` exists | Pass / Fail |
| macOS Apple Silicon artifact exists | `GlucoDesk-osx-arm64*.zip` exists | Pass / Fail |
| Windows x64 artifact exists | `GlucoDesk-win-x64*.zip` exists | Pass / Fail |
| Checksum file exists | `SHA256SUMS.txt` exists | Pass / Fail |
| Manifest file exists | `artifacts-manifest.json` exists | Pass / Fail |
| Checksums are populated | Each zip has one SHA256 entry | Pass / Fail |
| Manifest is populated | Each zip appears with name, path, size, and SHA256 | Pass / Fail |

## Checksum validation

After artifact generation, inspect:

    artifacts/release/SHA256SUMS.txt

Each release zip should have a SHA256 entry.

The GitHub release should include:

- the platform zip archives;
- `SHA256SUMS.txt`;
- `artifacts-manifest.json`.

## Manifest validation

After artifact generation, inspect:

    artifacts/release/artifacts-manifest.json

Each artifact entry should include:

- `name`;
- `path`;
- `sizeBytes`;
- `sha256`.

## macOS artifact smoke test

Run this on the target macOS machine whenever possible.

| Check | Expected result | Result |
| --- | --- | --- |
| Extract artifact | Archive extracts without errors | Pass / Fail |
| Launch app | App starts without crash | Pass / Fail |
| Open Settings | Settings screen is usable | Pass / Fail |
| Provider settings | Existing provider configuration flow still works | Pass / Fail |
| Notification settings | Glucose notification settings are visible | Pass / Fail |
| Test native notification | Request does not crash the app | Pass / Fail |
| Demo low/high alert | In-app banner appears | Pass / Fail |
| Event log | `NativeNotificationRequested` can be recorded | Pass / Fail |
| Privacy wording | No insulin dosing or medical advice appears | Pass / Fail |

## Windows artifact smoke test

Run this on a Windows x64 machine.

| Check | Expected result | Result |
| --- | --- | --- |
| Extract artifact | Archive extracts without errors | Pass / Fail |
| Launch app | App starts without crash | Pass / Fail |
| Open Settings | Settings screen is usable | Pass / Fail |
| Provider settings | Existing provider configuration flow still works | Pass / Fail |
| Notification settings | Glucose notification settings are visible | Pass / Fail |
| Test native notification | Toast request does not crash the app | Pass / Fail |
| Demo low/high alert | In-app banner appears | Pass / Fail |
| Event log | `NativeNotificationRequested` can be recorded | Pass / Fail |
| Privacy wording | No insulin dosing or medical advice appears | Pass / Fail |

## Release artifact acceptance criteria

A release artifact is acceptable only if:

- the app starts on the target platform;
- dashboard refresh still works;
- settings remain usable;
- in-app glucose alert banner works;
- native notification request failures do not crash the dashboard;
- privacy-sensitive data does not appear in notifications or logs;
- known OS limitations are documented in release notes;
- checksums and artifact manifest are generated.

## Notes

Generated artifacts are build outputs and should not be committed to Git.

The release artifact script writes archives under:

    artifacts/release/

Release archives, `SHA256SUMS.txt`, and `artifacts-manifest.json` should be attached to the GitHub release after final QA.
