# GlucoDesk

**GlucoDesk** is a local-first desktop companion for glucose monitoring workflows.

It is designed for people who want a calm desktop presence while working: a focused dashboard, a menu bar indicator, local glucose history, data completeness checks, and readable PDF/Excel diary exports.

> GlucoDesk is not a medical device. It does not provide medical advice, treatment decisions, insulin dosing guidance, alarms, or emergency notifications. Always rely on your approved CGM/mobile app, pump system, and healthcare professional for medical decisions.

---

## Why GlucoDesk exists

When working at a computer, checking glucose data often means switching context to a phone, a web page, or another app.

GlucoDesk explores a different approach:

- a small desktop companion;
- local-first history;
- clear data freshness/status;
- readable diary exports;
- privacy-conscious desktop presence.

The goal is not to replace official diabetes tools. The goal is to make day-to-day glucose awareness and retrospective review more convenient on desktop.

---

## Current preview scope

The current preview focuses on a small, coherent product loop:

- live dashboard;
- menu bar / desktop presence;
- local glucose history;
- history continuity and gap awareness;
- PDF glycemic diary export;
- Excel glycemic diary export;
- glucose story;
- weekly review;
- local patterns;
- export metadata;
- data completeness reporting;
- safety notice in generated exports.

This preview is intentionally limited. Features such as medical recommendations, predictive alerts, insulin dosing suggestions, meal tagging, cloud sync, and mobile apps are outside the current release scope.

---

## Key features

### Desktop dashboard

GlucoDesk provides a desktop dashboard for quickly checking the latest glucose state, recent trend information, provider status, and local data health.

### Menu bar presence

On macOS, GlucoDesk can expose a compact menu bar presence so the current glucose context stays visible without keeping the full dashboard in focus.

The menu bar popover includes quick actions such as refresh, privacy mode, opening the main window, and quitting the application.

### Local-first history

GlucoDesk stores glucose readings locally so that statistics and exports can be generated from local history.

This makes the app useful when preparing diary exports, reviewing recent periods, or checking how complete the locally collected history is.

### PDF and Excel diary exports

GlucoDesk can generate readable diary exports intended for review and discussion.

The export flow currently includes:

- overview;
- glucose story;
- weekly review;
- local patterns;
- daily diary;
- time-block summaries;
- data completeness;
- export metadata;
- safety notice.

---

## Understanding data completeness

GlucoDesk reports data completeness to show how reliable the local history is for the selected period.

This is not a judgment about glucose quality.

For example, a low completeness percentage means that GlucoDesk does not have enough local readings for that selected period. This may happen if:

- the app was not running yet;
- the computer was off;
- provider sync was unavailable;
- the selected period starts before local history collection began;
- backfill was unavailable or incomplete.

A diary export should always be interpreted together with its data completeness section.

For more details, see:

- [`docs/safety/local-history-completeness.md`](docs/safety/local-history-completeness.md)

---

## Safety notice

GlucoDesk is a developer-built desktop companion and is not intended for medical use.

Do not use GlucoDesk to make emergency decisions, treatment decisions, or insulin dosing decisions.

Always use approved medical devices, official CGM applications, pump systems, and healthcare professional guidance for medical decisions.

Generated PDF/Excel exports are informational and depend on local data availability.

---

## Privacy model

GlucoDesk is designed around a local-first approach.

Current preview principles:

- glucose history is stored locally;
- exports are generated locally;
- generated files are under the user's control;
- credentials should be handled through platform-safe storage where available;
- logs should not contain secrets.

Before sharing logs, screenshots, or exports publicly, review them carefully because glucose data is personal health information.

---

## Supported platforms

The current release direction is:

- macOS-first preview;
- source-based development and testing through .NET;
- future packaging work for a macOS `.app` / `.dmg`.

Windows and Linux support may be possible through the underlying technology stack, but the current release-readiness work is focused on macOS.

---

## Requirements for development

- .NET 10 SDK
- macOS for the primary preview workflow
- Git
- An IDE such as JetBrains Rider or Visual Studio Code

---

## Build from source

Clone the repository and run:

```bash
dotnet restore
dotnet build -c Release
dotnet test -c Release
```

Run the desktop app:

```bash
dotnet run --project src/GlucoDesk.Desktop/GlucoDesk.Desktop.csproj
```

---

## Export workflow

From the application, generate a glycemic diary export for the selected period.

Before relying on an export, check:

- selected date range;
- preferred glucose unit;
- data completeness;
- export metadata;
- safety notice.

The export is only as complete as the local history available to GlucoDesk for that period.

---

## Project architecture

The solution is organized around a layered structure:

```text
src/
  GlucoDesk.Core
  GlucoDesk.Application
  GlucoDesk.Infrastructure
  GlucoDesk.Desktop

tests/
  GlucoDesk.Core.Tests
  GlucoDesk.Application.Tests
  GlucoDesk.Infrastructure.Tests
  GlucoDesk.Desktop.Tests
```

The application is designed to keep domain logic, application services, infrastructure integrations, and desktop UI concerns separated.

---

## Release readiness

The current path to a public preview release is:

1. stabilize dashboard and desktop presence;
2. protect PDF/Excel exports with regression tests;
3. polish README and safety documentation;
4. prepare macOS packaging;
5. create a GitHub preview release with clear limitations.

The release checklist is available here:

- [`docs/release/preview-release-checklist.md`](docs/release/preview-release-checklist.md)

---

## Known preview limitations

The current preview should be treated as early software.

Known limitations:

- not a medical device;
- no emergency alerts;
- no dosing recommendations;
- local history may be incomplete;
- export quality depends on available local readings;
- macOS packaging/signing/notarization may still require additional work;
- provider availability depends on configuration and external service behavior.

---

## License

This project is released under the MIT License.
