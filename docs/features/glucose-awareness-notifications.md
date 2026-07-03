# Glucose Awareness Notifications

GlucoDesk includes calm desktop awareness notifications for glucose readings that move outside the configured target range.

These notifications are designed as a lightweight desktop companion while working at the computer. They are not medical alerts and must not replace Dexcom, Omnipod, Nightscout, or any official diabetes device/app notification.

## Purpose

The feature helps users notice glucose conditions while using their desktop by combining:

- an in-app awareness banner
- optional native OS notifications
- configurable low and high alert conditions
- configurable repeat cooldown
- configurable stability gate
- snooze and dismiss actions
- privacy wording
- local privacy-safe event logging

## Safety boundaries

GlucoDesk is not a medical device.

Glucose awareness notifications must not be used for:

- urgent medical alerting
- insulin dosing decisions
- treatment recommendations
- replacing official diabetes apps or devices
- emergency situations

Users should keep official diabetes apps and devices active for therapy decisions and urgent alerts.

## Notification surfaces

### In-app banner

The in-app banner is the primary and most reliable notification surface.

It appears inside GlucoDesk when:

- glucose awareness notifications are enabled
- the current reading is below or above the configured target range
- the condition satisfies the configured consecutive reading requirement
- the alert has not been dismissed, snoozed, or blocked by cooldown rules

The banner supports:

- dismiss
- snooze
- privacy-safe wording
- calm non-medical language

### Native OS notifications

Native OS notifications are optional.

They can be delayed, blocked, hidden, or suppressed by the operating system. This can happen because of:

- macOS notification permissions
- Windows notification permissions
- Focus / Do Not Disturb modes
- development-mode host processes such as Terminal, Visual Studio Code, `dotnet`, or Avalonia Application
- packaged app identity limitations

For this reason, native OS notifications are best-effort and should be treated as a secondary surface. The in-app banner remains the reliable fallback.

## Settings

The Settings page exposes user-controlled notification behavior.

| Setting | Purpose |
| --- | --- |
| Enable in-app banner | Enables the primary in-app awareness banner. |
| Notify below target | Enables alerts when readings are below the configured target range. |
| Notify above target | Enables alerts when readings are above the configured target range. |
| Use privacy wording | Uses wording that avoids exposing raw glucose values. |
| Consecutive readings required | Requires the same out-of-range condition to be observed multiple times before alerting. |
| Enable native OS notifications | Enables optional native macOS/Windows notifications. |
| Send test notification | Requests a safe native OS test notification. |
| Repeat cooldown | Prevents repeated notification spam for the same condition. |

## Privacy wording

When privacy wording is enabled, notification text avoids showing raw glucose values.

This is useful for:

- screen sharing
- working in public spaces
- avoiding sensitive health information in OS notification previews
- reducing exposure in notification history

Privacy wording should be the default user-facing behavior.

## Stability gate

The stability gate prevents noisy alerts caused by a single transient reading.

For example:

| Consecutive readings required | Behavior |
| --- | --- |
| 1 | Faster alerts, more sensitive. |
| 2 | Balanced default. |
| 3-5 | Less noise, slower alerts. |

This helps reduce false positives and notification fatigue.

## Repeat cooldown

The repeat cooldown prevents repeated notifications for the same condition.

For example, if the cooldown is set to 30 minutes, GlucoDesk should not continuously request native notifications for the same high or low condition during every refresh cycle.

Cooldown protects users from notification spam and makes alerts feel calmer.

## Snooze and dismiss

### Dismiss

Dismiss hides the current visible banner for the current alert condition.

### Snooze

Snooze hides the current visible banner and suppresses immediate reappearance for the configured snooze behavior.

Snooze is useful when the user has already noticed the condition and does not want repeated desktop interruption.

## Local alert event log

GlucoDesk writes privacy-safe glucose alert events to a local JSON Lines log.

The log is used for debugging and QA. It is not telemetry and is not sent anywhere.

Event examples include:

- `Presented`
- `Dismissed`
- `Snoozed`
- `NativeNotificationRequested`

The event log must not include:

- raw glucose values
- provider credentials
- treatment notes
- insulin recommendations
- personally identifiable medical notes

The log uses bounded retention with rotation so that it cannot grow indefinitely.

## Native notification diagnostics

The Settings page includes a compact diagnostic hint for native OS notifications.

The goal is to avoid overpromising delivery.

For example, on macOS development mode, a native notification may be requested but not visibly displayed. The notification may be associated with Terminal, Visual Studio Code, `osascript`, `dotnet`, Avalonia Application, or the packaged app identity.

The app should explain this calmly and keep the text short.

## macOS behavior

In development mode, native notifications may not appear under the GlucoDesk app name.

Possible notification identities include:

- Terminal
- Visual Studio Code
- `osascript`
- Script Editor
- Avalonia Application
- `dotnet`
- GlucoDesk when packaged

Users should check:

- System Settings → Notifications
- Focus / Do Not Disturb
- notification settings for the host process or packaged app

## Windows behavior

On Windows, toast notifications may depend on:

- Windows notification permissions
- Focus assist / Do Not Disturb
- packaged app identity
- AppUserModelID or packaging configuration
- whether the app is running through `dotnet run`, Visual Studio, or as a packaged app

Users should check:

- Settings → System → Notifications
- Focus assist / Do Not Disturb
- notification settings for the host process or packaged app

## QA

Manual validation is documented in:

[`docs/qa/glucose-notifications-checklist.md`](../qa/glucose-notifications-checklist.md)

The QA checklist covers:

- macOS dev mode
- macOS packaged app
- Windows dev mode
- Windows packaged app
- privacy and safety
- local event logging
- regression checks

## Completion criteria

The notification feature can be considered production-ready only when:

- automated tests pass
- macOS dev-mode QA is complete
- macOS packaged-app QA is complete
- Windows dev-mode QA is complete
- Windows packaged-app QA is complete
- notification limitations are documented
- safety boundaries are documented
- known OS-specific limitations are either fixed or documented

## Packaged-app validation

Native OS notification delivery depends on operating-system permissions, Focus / Do Not Disturb modes, notification center behavior, and packaging identity.

Before publishing a release, validate packaged builds with:

- [`../qa/native-notification-packaged-app-checklist.md`](../qa/native-notification-packaged-app-checklist.md)

## Release preparation

Before tagging a release that includes glucose awareness notifications, use:

- [`../qa/release-readiness-checklist.md`](../qa/release-readiness-checklist.md)
- [`../release-notes/glucose-awareness-notifications-preview.md`](../release-notes/glucose-awareness-notifications-preview.md)
