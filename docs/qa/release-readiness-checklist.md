# Release readiness checklist

Use this checklist before tagging a GlucoDesk preview release.

## Automated validation

| Check | Command | Result |
| --- | --- | --- |
| Restore/build release configuration | `dotnet build -c Release` | Pass / Fail |
| Run all tests | `dotnet test -c Release` | Pass / Fail |
| Check whitespace | `git diff --check` | Pass / Fail |
| Check working tree | `git status -sb` | Clean / Dirty |

## Documentation validation

| Check | Expected result | Result |
| --- | --- | --- |
| README links are valid | Key feature and QA docs are reachable | Pass / Fail |
| Feature docs are updated | Notification behavior is documented | Pass / Fail |
| QA checklist exists | General notification QA exists | Pass / Fail |
| Packaged-app QA checklist exists | macOS and Windows packaged validation exists | Pass / Fail |
| Release notes draft exists | Release notes can be copied into GitHub release | Pass / Fail |
| Safety notice is present | No medical-replacement wording | Pass / Fail |

## Notification validation

| Check | Expected result | Result |
| --- | --- | --- |
| In-app banner | Low/high alert banner appears | Pass / Fail |
| Snooze | Banner can be snoozed without breaking refresh | Pass / Fail |
| Dismiss | Banner can be dismissed without breaking refresh | Pass / Fail |
| Cooldown | Repeated notifications are limited | Pass / Fail |
| Stability gate | Notifications require stable configured readings | Pass / Fail |
| Native test notification | Settings can request a native notification | Pass / Fail |
| Real native notification request | Real alert can request native notification | Pass / Fail |
| Event log | `NativeNotificationRequested` is written | Pass / Fail |
| Failure handling | Native failure does not crash dashboard | Pass / Fail |

## Privacy validation

| Check | Expected result | Result |
| --- | --- | --- |
| Notification text | No insulin dosing or medical advice | Pass / Fail |
| Event log text | No tokens, secrets, credentials, or raw glucose dumps | Pass / Fail |
| Diagnostics | Short and privacy-safe | Pass / Fail |
| Settings UI | Describes native notifications as OS-dependent | Pass / Fail |
| README | Contains clear non-medical safety wording | Pass / Fail |

## Packaging validation

| Platform | Artifact | Required validation |
| --- | --- | --- |
| macOS Intel | `.app` / archive | Launch, settings, banner, native notification request, event log |
| macOS Apple Silicon | `.app` / archive | Launch, settings, banner, native notification request, event log |
| Windows x64 | archive / installer | Launch, settings, banner, toast request, event log |

## Release gate

Do not tag the release unless:

- automated build succeeds;
- all tests pass;
- documentation is updated;
- packaged-app QA has been completed or explicitly documented as pending;
- known native notification limitations are listed in release notes;
- no safety or privacy regressions are found;
- Git working tree is clean.

## Automated verification script

Before tagging, run the release verification script from the repository root:

    scripts/verify-release-readiness.sh

When developing the checklist/script itself on a feature branch, use:

    scripts/verify-release-readiness.sh --allow-dirty

The default mode requires a clean working tree and is intended for the final release gate.

## Final release commands

Use these only after all checks pass.

    git checkout main
    git pull --ff-only

    dotnet build -c Release
    dotnet test -c Release
    git diff --check
    git status -sb

Then create the tag according to the chosen version:

    git tag v0.x.x-preview
    git push origin v0.x.x-preview

## Release artifact build

After automated verification passes, generate release archives with:

    scripts/build-release-artifacts.sh

Then smoke-test the generated archives with:

- [`release-artifacts-smoke-test.md`](release-artifacts-smoke-test.md)

Generated release archives are build outputs and must not be committed.
