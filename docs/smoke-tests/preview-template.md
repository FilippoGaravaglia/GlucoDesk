# GlucoDesk VERSION Manual Smoke Test Report

## Result

Overall result: TODO PASS / FAIL

Tester: TODO
Date: TODO
Machine: TODO
macOS version: TODO
Build artifact tested: TODO

## Scope

This smoke test verifies that the preview build starts, shows glucose awareness data, respects privacy mode, and exits cleanly.

## Automated checks completed

- TODO PASS / FAIL - scripts/quality/release-readiness-check.sh
- TODO PASS / FAIL - dotnet build Release
- TODO PASS / FAIL - dotnet test Release
- TODO PASS / FAIL - git diff --check

## Manual checks

| Check | Result | Notes |
|---|---|---|
| App starts on macOS | TODO | TODO |
| Dashboard loads without crashing | TODO | TODO |
| Menu bar indicator appears | TODO | TODO |
| Presence panel opens from menu bar | TODO | TODO |
| Refresh button works from presence panel | TODO | TODO |
| Privacy mode Off shows glucose value | TODO | TODO |
| Privacy mode Off shows glycemic menu bar icon | TODO | TODO |
| Privacy mode On hides glucose value | TODO | TODO |
| Privacy mode On shows neutral blue privacy icon | TODO | TODO |
| Privacy mode persists after restart | TODO | TODO |
| Mock provider works | TODO | TODO |
| Nightscout provider errors are understandable | TODO | TODO |
| App quits cleanly from presence panel | TODO | TODO |

## Safety checks

| Check | Result | Notes |
|---|---|---|
| Safety disclaimer is visible in app or release notes | TODO | TODO |
| Release notes state that GlucoDesk is not a medical device | TODO | TODO |
| Release notes state not to use GlucoDesk for treatment or insulin dosing decisions | TODO | TODO |

## Known issues found during smoke test

- TODO or None.

## Decision

Release decision: TODO GO / NO-GO

Reason:

TODO
