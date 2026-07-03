# Glucose Awareness Notifications QA Checklist

This checklist verifies GlucoDesk glucose awareness notifications across macOS and Windows.

GlucoDesk notifications are non-medical awareness prompts. They are not urgent medical alerts and must not replace Dexcom, Omnipod, Nightscout, or any official diabetes device/app notifications.

## Scope

This checklist covers:

- in-app glucose awareness banner
- optional native OS notifications
- privacy wording
- below-target and above-target alert conditions
- repeat cooldown
- snooze
- dismiss
- consecutive reading stability gate
- native notification test action
- local alert event log
- native notification diagnostics
- Settings responsive layout

## General prerequisites

Before testing:

- build the app in Release mode
- use a local test profile or non-production data source when possible
- keep official diabetes apps/devices active for real therapy decisions
- do not use GlucoDesk as the only alerting surface
- confirm the app starts without crashes

Commands:

```bash
dotnet build -c Release
dotnet test -c Release
dotnet run --project src/GlucoDesk.Desktop/GlucoDesk.Desktop.csproj
```

## Test settings baseline

Use this baseline unless a test case says otherwise:

| Setting | Value |
| --- | --- |
| Enable in-app banner | Enabled |
| Notify below target | Enabled |
| Notify above target | Enabled |
| Use privacy wording | Enabled |
| Enable native OS notifications | Enabled when testing OS notifications |
| Consecutive readings required | 2 |
| Repeat cooldown | 30 minutes |
| Target low | 70 mg/dL |
| Target high | 180 mg/dL |

## macOS dev-mode QA

Use this section when running the app through `dotnet run`, Terminal, or Visual Studio Code.

### Settings page

- [ ] Settings page opens without layout glitches.
- [ ] Wide window uses two-column layout with vertical separators.
- [ ] Narrow window switches to compact stacked layout.
- [ ] Target low and target high accept numeric input only.
- [ ] Consecutive readings required accepts the configured supported range.
- [ ] Native notification diagnostic text is short and user-friendly.
- [ ] Test notification button is disabled when native OS notifications are disabled.
- [ ] Test notification button is enabled when native OS notifications are enabled.

### Native notification test action

- [ ] Click "Send test notification".
- [ ] App does not crash.
- [ ] Status text confirms that a safe test notification was requested.
- [ ] If no macOS notification appears, the Settings diagnostic text remains accurate and does not overpromise delivery.
- [ ] Check macOS System Settings → Notifications for possible host process entries such as Terminal, Visual Studio Code, osascript, Script Editor, Avalonia Application, or GlucoDesk.
- [ ] Check Focus / Do Not Disturb mode.

Expected result in dev mode:

- The app may request a native notification without macOS visibly displaying it.
- This is acceptable as long as the app does not crash and the UI explains that native notifications depend on macOS permissions.

### In-app banner

Temporarily set:

| Setting | Value |
| --- | --- |
| Target high | 80 mg/dL |
| Consecutive readings required | 1 |

Then verify:

- [ ] Dashboard shows an above-target awareness banner when the current reading is above target.
- [ ] Banner text uses calm non-medical wording.
- [ ] Banner does not include glucose values when privacy wording is enabled.
- [ ] Dismiss hides the current banner.
- [ ] Snooze hides the current banner and suppresses immediate reappearance.
- [ ] Returning to in-range clears the current alert state.
- [ ] Changing from above-target to below-target allows a new alert condition.

Restore baseline settings after the test.

### Repeat cooldown

Temporarily set:

| Setting | Value |
| --- | --- |
| Target high | 80 mg/dL |
| Consecutive readings required | 1 |
| Repeat cooldown | 30 minutes |

Then verify:

- [ ] First stable above-target condition can show a banner.
- [ ] Repeated refreshes do not continuously spam native notifications.
- [ ] Repeated refreshes do not continuously spam duplicate Presented events in the local event log.
- [ ] Cooldown prevents repeated native notification requests for the same condition.

Restore baseline settings after the test.

### Local alert event log

Check the log path:

```bash
find ~/Library/Application\ Support/GlucoDesk -type f -name "glucose-alert-events.jsonl" -print
cat ~/Library/Application\ Support/GlucoDesk/logs/glucose-alert-events.jsonl | tail -n 20
```

Verify:

- [ ] Log file is created locally.
- [ ] Log file uses JSON Lines format.
- [ ] Events are privacy-safe.
- [ ] Events do not include raw glucose values.
- [ ] Presented events are not duplicated on every refresh for the same visible alert.
- [ ] Dismissed event is written when the banner is dismissed.
- [ ] Snoozed event is written when the banner is snoozed.
- [ ] NativeNotificationRequested event is written when native notification sending is requested.
- [ ] Log rotation keeps bounded retention when the configured max file size is exceeded.

## macOS packaged-app QA

Use this section for a packaged `.app`.

- [ ] GlucoDesk.app starts successfully.
- [ ] App name appears correctly in the menu bar.
- [ ] App name appears correctly in the Dock.
- [ ] Settings page uses responsive layout correctly.
- [ ] Native notification test action does not crash.
- [ ] macOS notification permissions can be found under the expected app identity when available.
- [ ] Native notification appears when macOS permissions allow it.
- [ ] If native notification does not appear, diagnostic text remains accurate.
- [ ] In-app banner still works even when native notifications are blocked.
- [ ] Local event log is written under the user application data path.
- [ ] Closing the app does not leave background notification work running unexpectedly.

## Windows dev-mode QA

Use this section when running from `dotnet run`, Visual Studio, or Visual Studio Code.

### Settings page

- [ ] Settings page opens without layout glitches.
- [ ] Wide window uses two-column layout with vertical separators.
- [ ] Narrow window switches to compact stacked layout.
- [ ] Target low and target high accept numeric input only.
- [ ] Native notification diagnostic text mentions Windows notification permissions.
- [ ] Test notification button is disabled when native OS notifications are disabled.
- [ ] Test notification button is enabled when native OS notifications are enabled.

### Native notification test action

- [ ] Click "Send test notification".
- [ ] App does not crash.
- [ ] Status text confirms that a safe test notification was requested.
- [ ] If no toast appears, check Windows Settings → System → Notifications.
- [ ] Check Focus assist / Do not disturb.
- [ ] Confirm whether the notification appears under GlucoDesk, dotnet, Visual Studio, or another host process.

Expected result in dev mode:

- Toast delivery may depend on the host process and Windows notification permissions.
- The app must not crash if Windows does not show the notification.

### In-app banner

Temporarily set:

| Setting | Value |
| --- | --- |
| Target high | 80 mg/dL |
| Consecutive readings required | 1 |

Then verify:

- [ ] Dashboard shows an above-target awareness banner.
- [ ] Banner text uses calm non-medical wording.
- [ ] Privacy wording hides glucose values.
- [ ] Dismiss hides the current banner.
- [ ] Snooze hides the current banner and suppresses immediate reappearance.
- [ ] Cooldown prevents repeated native notification requests.

Restore baseline settings after the test.

### Local alert event log

Verify:

- [ ] Log file is created under the Windows user application data path.
- [ ] Log file uses JSON Lines format.
- [ ] Events are privacy-safe.
- [ ] Events do not include raw glucose values.
- [ ] Presented events are deduplicated for the same visible alert.
- [ ] Log rotation keeps bounded retention.

## Windows packaged-app QA

Use this section for the packaged Windows app.

- [ ] App starts successfully.
- [ ] Settings page uses responsive layout correctly.
- [ ] Native toast test action does not crash.
- [ ] Windows notification permissions are available for the expected app identity when applicable.
- [ ] Toast notification appears when Windows permissions allow it.
- [ ] If toast notification does not appear, diagnostic text remains accurate.
- [ ] In-app banner works even when native notifications are blocked.
- [ ] Local event log is written under the expected user application data path.
- [ ] App can be closed and reopened without losing notification settings.

## Privacy and safety QA

Verify on both macOS and Windows:

- [ ] Notification text does not claim medical urgency.
- [ ] Notification text does not tell the user how much insulin to take.
- [ ] Notification text does not recommend treatment decisions.
- [ ] Privacy wording avoids raw glucose values.
- [ ] Event log avoids raw glucose values.
- [ ] Event log avoids provider credentials.
- [ ] Event log avoids personally identifiable medical notes.
- [ ] UI clearly states that GlucoDesk is not a medical device.

## Regression checklist

Before considering the feature complete:

- [ ] `dotnet build -c Release` passes.
- [ ] `dotnet test -c Release` passes.
- [ ] Dashboard still renders current glucose state.
- [ ] Diary page still opens.
- [ ] Account page still opens.
- [ ] Settings page still opens.
- [ ] Settings save still works.
- [ ] App can be resized without layout breakage.
- [ ] No `.bak`, temporary, or local log files are committed.

## Completion criteria

The notification feature can be considered production-ready when:

- [ ] macOS dev-mode QA is complete.
- [ ] macOS packaged-app QA is complete.
- [ ] Windows dev-mode QA is complete.
- [ ] Windows packaged-app QA is complete.
- [ ] README documents notification limitations and safety boundaries.
- [ ] All automated tests pass.
- [ ] Manual QA issues are either fixed or explicitly documented.

## Packaged native notification QA

Before releasing packaged builds, also run the dedicated packaged-app checklist:

- [`native-notification-packaged-app-checklist.md`](native-notification-packaged-app-checklist.md)
