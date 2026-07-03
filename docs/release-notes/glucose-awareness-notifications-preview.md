# Glucose awareness notifications preview release notes

This document is the release-note draft for the GlucoDesk preview release that introduces glucose awareness notifications.

GlucoDesk notifications are non-medical awareness prompts. They do not replace Dexcom, Omnipod, Nightscout, clinical guidance, or emergency glucose alerts.

## Highlights

This release introduces a production-ready foundation for glucose awareness notifications:

- in-app glucose awareness banner;
- optional native OS notification requests;
- privacy-safe notification wording;
- user-configurable notification settings;
- cooldown / anti-spam behavior;
- snooze and dismiss behavior;
- stability gate before notifying;
- consecutive-reading threshold support;
- native test notification from Settings;
- native notification request result model;
- privacy-safe event logging for notification request outcomes;
- packaged-app QA checklist for macOS and Windows.

## Native notification behavior

Native OS notifications are best-effort.

A native notification request can be successfully handed to the operating system, but final display still depends on:

- OS notification permissions;
- Focus / Do Not Disturb;
- notification center behavior;
- app packaging identity;
- Windows toast restrictions;
- PowerShell availability and policy on Windows;
- macOS notification permissions for the packaged app or host process.

For this reason, GlucoDesk records notification request outcomes as request results, not guaranteed delivery confirmations.

## Event log behavior

When a real glucose alert requests a native notification, GlucoDesk writes a privacy-safe `NativeNotificationRequested` event.

Expected messages include:

- `Native notification requested.`;
- `Native notification requested. Delivery depends on OS notification settings.`;
- `Native notifications are disabled.`;
- `Native notification request failed.`;
- short diagnostic messages that do not include glucose values, credentials, tokens, URLs, or personal data.

## Safety and privacy

Notifications and logs must not include:

- insulin dosing recommendations;
- medical advice;
- raw glucose history dumps;
- access tokens;
- refresh tokens;
- Nightscout secrets;
- Dexcom credentials;
- personally identifying data.

## Known limitations

Native notification delivery is not guaranteed by GlucoDesk.

Known limitations:

- macOS may hide notifications when Focus / Do Not Disturb is active;
- macOS may associate notification permissions with the packaged app or host process;
- Windows toast behavior may depend on packaging identity and notification settings;
- Windows PowerShell restrictions can prevent toast creation;
- notification centers can delay or suppress notifications;
- development-mode behavior may differ from packaged-app behavior.

## QA required before tagging

Before tagging the release, complete:

- automated build and test validation;
- packaged macOS validation;
- packaged Windows validation;
- event log validation;
- privacy wording validation;
- release artifact smoke test.

Use:

- `docs/qa/glucose-notifications-checklist.md`;
- `docs/qa/native-notification-packaged-app-checklist.md`;
- `docs/qa/release-readiness-checklist.md`.

## Suggested release title

`GlucoDesk v0.x.x-preview — Glucose awareness notifications`

## Suggested release summary

This preview adds glucose awareness notifications to GlucoDesk, including an in-app alert banner, optional native OS notification requests, privacy-safe event logging, configurable notification behavior, cooldowns, snooze/dismiss support, and packaged-app QA guidance for macOS and Windows.

Native OS notifications are best-effort and depend on operating-system settings, notification permissions, Focus / Do Not Disturb modes, and packaging identity.
