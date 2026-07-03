# Native notification packaged-app QA checklist

This checklist validates glucose awareness notifications in packaged macOS and Windows builds.

GlucoDesk notifications are non-medical awareness prompts. They must never replace Dexcom, Omnipod, Nightscout, or clinical guidance.

## Scope

Validate that packaged builds can:

- show the in-app glucose awareness banner;
- request native OS notifications;
- record privacy-safe notification request events;
- keep the dashboard stable if native notifications are blocked, delayed, unsupported, or fail.

## Preconditions

Before testing:

- build a packaged app, not only a `dotnet run` session;
- use a non-production/demo glucose source when simulating alert values;
- disable Focus / Do Not Disturb during the positive notification test;
- keep the real CGM app and diabetes devices as the primary safety source;
- confirm GlucoDesk settings allow glucose awareness notifications;
- confirm native glucose notifications are enabled in GlucoDesk settings;
- confirm a reasonable repeat/cooldown interval is configured.

## macOS packaged-app QA

| Check | Expected result | Evidence |
| --- | --- | --- |
| Launch packaged app | App starts without crash | Screenshot or note |
| Open notification settings | GlucoDesk/app host is allowed to send notifications | Screenshot or note |
| Trigger test notification from Settings | Native notification is requested | Screenshot or notification center note |
| Trigger a low glucose demo alert | In-app banner appears | Screenshot |
| Trigger a high glucose demo alert | In-app banner appears | Screenshot |
| Trigger native notification for alert | Notification appears or is visible in Notification Center | Screenshot or note |
| Enable Focus / Do Not Disturb and retrigger | App remains stable even if notification is hidden | Note |
| Disable OS notification permission and retrigger | App remains stable and event log records request result | Note |
| Check privacy wording | Notification does not include medical dosing instructions | Screenshot |
| Check event log | `NativeNotificationRequested` event is written | Log excerpt |

## Windows packaged-app QA

| Check | Expected result | Evidence |
| --- | --- | --- |
| Launch packaged app | App starts without crash | Screenshot or note |
| Check Windows notification settings | Notifications are enabled for the app/app host | Screenshot or note |
| Trigger test notification from Settings | Native notification is requested | Screenshot or notification center note |
| Trigger a low glucose demo alert | In-app banner appears | Screenshot |
| Trigger a high glucose demo alert | In-app banner appears | Screenshot |
| Trigger native notification for alert | Toast appears or is visible in notification center | Screenshot or note |
| Enable Focus Assist / Do Not Disturb and retrigger | App remains stable even if toast is hidden | Note |
| Disable/block notifications and retrigger | App remains stable and event log records request result | Note |
| Check PowerShell restrictions | Failure is handled without dashboard crash | Note |
| Check privacy wording | Notification does not include medical dosing instructions | Screenshot |
| Check event log | `NativeNotificationRequested` event is written | Log excerpt |

## Event log acceptance criteria

For real glucose alerts that request native notifications, the event log should contain:

- `Presented` when the in-app banner is presented;
- `NativeNotificationRequested` when a native OS notification request is attempted;
- a privacy-safe message such as:
  - `Native notification requested.`;
  - `Native notification requested. Delivery depends on OS notification settings.`;
  - `Native notifications are disabled.`;
  - `Native notification request failed.`;
  - a short diagnostic message that does not include glucose values, credentials, tokens, URLs, or personal data.

The event log must not contain:

- access tokens;
- refresh tokens;
- Nightscout secrets;
- Dexcom credentials;
- raw glucose history dumps;
- insulin dosing recommendations;
- medical advice.

## Failure-mode QA

Validate that the dashboard keeps working when:

- native notifications are unsupported;
- the native OS command fails;
- notification permissions are denied;
- Focus / Do Not Disturb hides notifications;
- the notification center delays delivery;
- the event log write fails.

Expected behavior:

- no dashboard crash;
- no blocking UI freeze;
- in-app banner still works;
- settings remain usable;
- next glucose refresh still works.

## Release evidence template

Use this table before tagging a release.

| Platform | Build artifact | Tester | Date | Result | Notes |
| --- | --- | --- | --- | --- | --- |
| macOS Intel |  |  |  | Pass / Fail |  |
| macOS Apple Silicon |  |  |  | Pass / Fail |  |
| Windows x64 |  |  |  | Pass / Fail |  |

## Release decision

A release can proceed only if:

- build succeeds;
- all automated tests pass;
- in-app alert banner works;
- native notification request does not crash the app;
- event log records `NativeNotificationRequested`;
- known OS limitations are documented in release notes;
- no privacy-sensitive data appears in notifications or logs.
