<p align="center">
  <img src="https://img.shields.io/badge/.NET-10.0-512BD4" alt=".NET 10" />
  <img src="https://img.shields.io/badge/Avalonia-UI-0B8CE9" alt="Avalonia UI" />
  <img src="https://img.shields.io/badge/license-MIT-green" alt="MIT license" />
  <img src="https://img.shields.io/badge/status-v0.3.0--preview-blue" alt="v0.3.0 preview" />
  <img src="https://img.shields.io/badge/macOS-DMG--preview-00AEEF" alt="macOS DMG preview" />
  <img src="https://img.shields.io/badge/Windows-installer--preview-0078D4" alt="Windows installer preview" />
  <img src="https://img.shields.io/badge/local--first-yes-00AEEF" alt="Local-first" />
</p>

<h1 align="center">GlucoDesk</h1>

<p align="center">
  <strong>A calm desktop companion for glucose awareness.</strong>
</p>

<p align="center">
  GlucoDesk is a local-first desktop app that helps people keep an eye on CGM glucose data while working on a computer.
</p>

<p align="center">
  It brings glucose trends, recent history, local background sync, data completeness awareness, settings, account configuration, glucose awareness notifications and glycemic diary export into a clean desktop experience.
</p>

---

<p align="center">
  <img src="./docs/assets/glucodesk-social-preview.png" alt="GlucoDesk - A calm desktop companion for glucose awareness" width="100%" />
</p>

> [!IMPORTANT]
> **Safety notice**
>
> GlucoDesk is not a medical device and must not be used for treatment decisions, insulin dosing, emergency alerts, alarms, diagnosis, or as a replacement for approved diabetes applications or medical devices.
>
> Always use approved CGM apps, insulin pump systems, glucose meters, medical devices and healthcare professionals for therapy decisions.

> [!WARNING]
> **Preview status**
>
> GlucoDesk is currently a preview project.
>
> The app is intended for awareness, personal review and desktop convenience only.
>
> The current preview supports macOS Apple Silicon, macOS Intel and Windows x64 packages. macOS builds are not notarized yet and Windows builds are not code-signed yet, so first-launch approval may be required on both platforms.
>
> Linux is not supported in the current preview.

---

## Table of contents

* [What is GlucoDesk?](#what-is-glucodesk)
* [Preview](#preview)
* [Why GlucoDesk?](#why-glucodesk)
* [Current release status](#current-release-status)
* [Key features](#key-features)
* [Privacy model](#privacy-model)
* [Installation preview](#installation-preview)
* [Build from source](#build-from-source)
* [Create local preview packages](#create-local-preview-packages)
* [Architecture overview](#architecture-overview)
* [Quality and release engineering](#quality-and-release-engineering)
* [Known limitations](#known-limitations)
* [Roadmap](#roadmap)
* [Disclaimer](#disclaimer)
* [License](#license)

---

## What is GlucoDesk?

GlucoDesk is a desktop companion for glucose awareness.

It is designed for people who spend many hours at a computer and want a calmer way to keep glucose information close to their work without constantly reaching for their phone.

GlucoDesk focuses on:

* current glucose value and trend;
* recent glucose chart;
* data freshness and provider status;
* local glucose history;
* background synchronization;
* history continuity and gap reduction;
* glucose insights over selectable time windows;
* readable glycemic diary export;
* configurable display preferences;
* glucose awareness notifications;
* secure local credential storage where supported by the operating system;
* privacy-conscious local storage;
* a quiet desktop presence through macOS menu bar and Windows tray companion flows.

The goal is simple:

> Make glucose awareness more comfortable during desktop work, without replacing official medical apps or devices.

GlucoDesk uses a provider-based architecture so the project can evolve beyond a single data source over time.

---

## Preview

GlucoDesk is currently in **v0.3.0-preview**.

This preview focuses on turning the app into a more complete desktop product loop:

```text
Connect an optional CGM data source
→ show glucose awareness on desktop
→ keep local history updated
→ reduce local history gaps
→ notify calmly when glucose is outside the configured range
→ analyze recent glucose windows
→ export a readable glycemic diary
→ keep preferences consistent across app and exports
→ package the app for real macOS and Windows installation flows
```

### Dashboard

![GlucoDesk dashboard](docs/assets/screenshots/dashboard.png)

The dashboard shows the current glucose value, trend, data freshness, provider status, recent glucose chart and glucose insights in a desktop-friendly layout.

The current preview includes:

* a redesigned dashboard hierarchy;
* recent glucose trend visualization;
* target range indicators;
* selectable insight windows;
* time-in-range summary;
* average glucose;
* below-range and above-range exposure;
* local history status;
* in-app glucose awareness banner;
* clear safety messaging;
* a calm desktop-first UI intended to stay readable during work.

### Account

![GlucoDesk account](docs/assets/screenshots/account.png)

The Account page provides a cleaner place to configure provider-related account information and connection checks.

It is designed around a local-first workflow and keeps account configuration separate from the main dashboard experience.

The current preview supports secure local credential storage on:

* macOS, through macOS Keychain;
* Windows, through Windows Credential Manager.

Credentials are used locally by the desktop app to connect to the configured provider.

GlucoDesk does not provide a custom backend for handling user credentials.

### Glycemic diary export

![GlucoDesk diary](docs/assets/screenshots/diary.png)

The diary export is designed to generate readable Excel and PDF summaries from local glucose history.

The export flow focuses on useful daily summaries instead of overwhelming the user with every single CGM data point.

The current preview supports:

* Excel diary export;
* PDF diary export;
* daily summaries;
* time-block summaries;
* data completeness reporting;
* clear incomplete-data awareness;
* selected display unit support.

### Settings

![GlucoDesk settings](docs/assets/screenshots/settings.png)

The Settings page controls provider routing, glucose preferences, dashboard behavior and glucose awareness notifications.

The current preview includes improved settings handling for:

* active live provider;
* historical provider;
* preferred glucose unit;
* target range;
* dashboard refresh interval;
* chart maximum;
* in-app glucose awareness alerts;
* native notification opt-in;
* notification cooldown;
* required consecutive out-of-range readings;
* privacy-conscious notification wording;
* consistent unit conversion across the app and exported files.

---

## Why GlucoDesk?

Many people spend hours at their desk every day.

When glucose information is only available through a phone, checking it repeatedly can become distracting.

GlucoDesk was created to make that experience calmer:

* keep glucose awareness visible while working;
* avoid constantly switching context to the phone;
* understand whether data is fresh, stale or unavailable;
* keep a local glucose history;
* reduce missing local history where possible;
* receive calm, non-medical awareness prompts when glucose is outside the configured range;
* export a readable diary for personal review;
* avoid unnecessary backend services;
* keep the app focused, quiet and desktop-friendly;
* provide a small desktop companion through the macOS menu bar or Windows tray.

GlucoDesk is not intended to replace official apps.

It is a companion experience for awareness, personal review and desktop convenience.

---

## Current release status

Current version:

```text
0.3.0-preview
```

Latest preview release:

```text
https://github.com/FilippoGaravaglia/GlucoDesk/releases/tag/v0.3.0-preview
```

The current preview includes:

* redesigned desktop glucose dashboard;
* optional CGM provider integration;
* local glucose history;
* background synchronization;
* startup and resume history continuity;
* local data completeness awareness;
* glucose insights;
* preferred glucose unit support;
* in-app glucose awareness banner;
* native macOS glucose awareness notifications;
* bundled macOS notification helper inside the app package;
* notification cooldown and anti-spam behavior;
* privacy-conscious notification wording;
* snooze and dismiss behavior;
* native notification test flow from Settings;
* notification diagnostics and event logging;
* Excel diary export;
* PDF diary export;
* updated app branding and screenshots;
* macOS Apple Silicon DMG package;
* macOS Intel DMG package;
* Windows x64 installer with setup wizard;
* macOS menu bar companion;
* Windows tray companion;
* Windows Credential Manager support for Dexcom Share credentials;
* improved account connection flow on Windows;
* documented macOS Gatekeeper first-launch flow;
* documented Windows SmartScreen first-launch flow.

Current runtime support:

| Platform            | Status            | Package type                                       | Notes                                           |
| ------------------- | ----------------- | -------------------------------------------------- | ----------------------------------------------- |
| macOS Apple Silicon | Preview supported | `macos-arm64-installable.zip` containing DMG       | Tested on Apple Silicon                         |
| macOS Intel         | Preview supported | `macos-x64-installable.zip` containing DMG         | Built by CI, physical Intel validation may vary |
| Windows x64         | Preview supported | `windows-x64-installable.zip` with setup installer | Windows package available for testing           |
| Linux               | Not supported yet | Not available                                      | Planned for a future step                       |

Linux remains part of the cross-platform roadmap but is not a supported runtime target in this preview.

---

## Key features

### Desktop glucose dashboard

GlucoDesk shows:

* current glucose value;
* trend direction;
* data freshness;
* provider status;
* recent glucose chart;
* target range indicators;
* glucose insights;
* safety notice.

The UI is designed to stay calm, readable and useful during desktop work.

### Glucose awareness notifications

GlucoDesk includes calm, non-medical glucose awareness notifications.

The current preview includes:

* in-app glucose awareness banner;
* automatic above-target and below-target awareness states;
* optional native macOS notifications;
* notification cooldown;
* anti-spam behavior;
* configurable required consecutive out-of-range readings;
* privacy-conscious notification wording;
* snooze and dismiss behavior;
* manual native notification test flow from Settings;
* notification request result model;
* event logging for native notification request outcomes.

On macOS, native notifications are delivered through a bundled helper app inside the main app package:

```text
GlucoDesk.app/Contents/Helpers/GlucoDeskNotificationHelper.app
```

On first use, macOS may ask permission for **GlucoDesk Notifications**.

Native notifications can be delayed, blocked, or hidden by operating-system notification permissions, Focus / Do Not Disturb modes, or other platform settings.

This feature is intended for desktop awareness only.

It is not an alarm system and must not be used for emergency or safety-critical notifications.

### macOS menu bar and Windows tray companion

GlucoDesk includes a small desktop presence outside the main window:

* on macOS, GlucoDesk appears in the menu bar;
* on Windows, GlucoDesk appears in the system tray / hidden icons area.

The companion icon provides quick access to the desktop popup and keeps the app close without requiring the main window to stay in focus.

This feature is intended for desktop convenience only.

It is not an alarm system and must not be used for emergency or safety-critical notifications.

### Glucose insights

The dashboard includes glucose insight windows based on local history.

Current insight areas include:

* time in range;
* average glucose;
* below-range exposure;
* above-range exposure;
* analyzed reading count;
* selected time window.

These insights are intended for awareness and personal review only.

### Preferred glucose unit

GlucoDesk supports display preferences for:

* `mg/dL`;
* `mmol/L`.

The selected unit is applied consistently across:

* dashboard value presentation;
* chart labels;
* target range display;
* settings fields;
* chart maximum selection;
* Excel diary export;
* PDF diary export.

Internally, glucose data remains normalized so the app can keep storage and calculations consistent while presenting values in the preferred unit.

### CGM provider routing

GlucoDesk follows a provider-based architecture.

The desktop app can route live and historical glucose data through configured CGM providers.

The current preview focuses on practical desktop usage while keeping the architecture open to future provider extensions.

### Account configuration and connection diagnostics

The Account page clearly separates provider account configuration from the dashboard.

The connection flow is designed to show whether the configured connection is:

* not tested;
* not verified;
* verified;
* failed;
* stale after configuration changes.

Credential persistence is platform-aware:

* on macOS, credentials are stored through macOS Keychain;
* on Windows, credentials are stored through Windows Credential Manager.

### Local history

GlucoDesk stores glucose history locally on the user’s computer.

Local history powers:

* recent glucose chart;
* dashboard insights;
* background sync status;
* diary export;
* data completeness reporting.

### Background sync status

The sidebar shows whether local history is up to date and when the last successful update happened.

This makes it easier to understand whether the local view is fresh or outdated.

### History continuity

GlucoDesk includes a history continuity workflow to reduce missing local glucose history where possible.

The app can run startup or resume synchronization and store fetched readings locally.

This is especially important for diary export and completeness reporting.

### Glycemic diary export

GlucoDesk can export a glycemic diary in:

* Excel workbook format;
* PDF format.

The diary is designed to be readable and focused on useful summaries instead of overwhelming the user with every single CGM data point.

The current diary direction focuses on:

* daily summaries;
* key time blocks;
* time-in-range information;
* data coverage indicators;
* incomplete-data awareness;
* structured data suitable for personal review.

### macOS package

GlucoDesk includes installable macOS preview packages for Apple Silicon and Intel.

The macOS package contains:

* a DMG installer flow;
* `GlucoDesk.app`;
* a bundled native notification helper;
* optimized app icon;
* macOS menu bar presence;
* local app data and credential storage outside the app bundle.

The macOS application menu name is configured as `GlucoDesk`.

### Windows installer

GlucoDesk includes a Windows x64 installer preview.

The Windows installable package contains:

* a setup wizard;
* per-user installation;
* Start Menu shortcut support;
* optional desktop shortcut support;
* license page;
* safety notice page;
* uninstall support.

---

## Privacy model

GlucoDesk is built with a local-first mindset.

By design:

* glucose history is stored locally on the user’s computer;
* app settings are stored locally;
* dashboard and tray/menu-bar state are stored locally;
* credentials are handled through the configured operating-system credential store where supported;
* credentials must not be committed to Git;
* GlucoDesk does not require a custom backend to handle user credentials or glucose history.

Local-first does not mean that no sensitive data exists.

Glucose readings are personal health-related data and are stored locally when history features are enabled.

The privacy goal is:

> Keep user data on the user’s machine and avoid unnecessary external services.

Users should still protect their computer account, disk, backups and operating-system credential store.

---

## Installation preview

Download the latest ready-to-run preview package from the [GitHub Releases page](https://github.com/FilippoGaravaglia/GlucoDesk/releases).

> [!IMPORTANT]
> The green **Code → Download ZIP** button downloads the source code, not the ready-to-run app.
>
> To install or try GlucoDesk, download one of the packages attached to the latest GitHub Release under **Assets**.

Latest recommended preview:

```text
v0.3.0-preview
```

Release page:

```text
https://github.com/FilippoGaravaglia/GlucoDesk/releases/tag/v0.3.0-preview
```

Available package targets for this preview:

```text
GlucoDesk-0.3.0-preview-macos-arm64-installable.zip
GlucoDesk-0.3.0-preview-macos-x64-installable.zip
GlucoDesk-0.3.0-preview-windows-x64-installable.zip
```

Choose the package for your operating system:

| System              | Download                                              |
| ------------------- | ----------------------------------------------------- |
| macOS Apple Silicon | `GlucoDesk-0.3.0-preview-macos-arm64-installable.zip` |
| macOS Intel         | `GlucoDesk-0.3.0-preview-macos-x64-installable.zip`   |
| Windows 64-bit      | `GlucoDesk-0.3.0-preview-windows-x64-installable.zip` |

Not sure which macOS package to use?

* Choose `macos-arm64` for Apple Silicon Macs with M1, M2, M3, M4 or newer chips.
* Choose `macos-x64` for Intel Macs.

### Package contents

The macOS installable ZIP packages contain:

* a platform-specific DMG;
* SHA256 checksum file;
* installation instructions.

The Windows installable ZIP package contains:

* Windows setup installer;
* SHA256 checksum file;
* installation instructions.

### Updating an existing installation

Replacing the application bundle or installer version does not normally delete local data.

GlucoDesk stores app data and credentials outside the application files:

* macOS credentials are stored in macOS Keychain;
* Windows credentials are stored in Windows Credential Manager;
* local app data is stored in the operating-system application data location.

To update:

* close GlucoDesk;
* download the new package from the latest GitHub Release;
* replace the old macOS app bundle or reinstall with the new Windows setup;
* open GlucoDesk again.

### macOS Apple Silicon and Intel

Download the correct macOS package from the release assets.

For Apple Silicon Macs such as M1, M2, M3, M4 or newer, use:

```text
GlucoDesk-0.3.0-preview-macos-arm64-installable.zip
```

For Intel Macs, use:

```text
GlucoDesk-0.3.0-preview-macos-x64-installable.zip
```

After downloading:

1. extract the installable ZIP;
2. open the included `.dmg` file;
3. drag `GlucoDesk.app` to the `Applications` folder;
4. if macOS asks whether to replace an existing copy, choose **Replace**;
5. open GlucoDesk from `Applications`.

The preview app is currently not signed with Apple Developer ID and is not notarized.

Because of this, macOS Gatekeeper may block the first launch with a message saying that Apple cannot verify whether GlucoDesk contains malware.

If that happens:

1. click **Done** or close the warning dialog;
2. open **System Settings**;
3. go to **Privacy & Security**;
4. scroll to the **Security** section;
5. find the GlucoDesk warning;
6. click **Open Anyway**;
7. confirm with password or Touch ID;
8. launch GlucoDesk again from `Applications`.

This approval is normally required only once.

On first use of native notifications, macOS may ask permission for:

```text
GlucoDesk Notifications
```

Allow notifications if you want desktop glucose awareness prompts.

> [!NOTE]
> The recommended preview flow is:
>
> ```text
> Download ZIP → extract → open DMG → drag to Applications → approve from Privacy & Security if required
> ```
>
> Terminal commands such as `xattr` should not be needed as the primary user-facing installation path. A future release goal is to provide signed and notarized macOS packages.

### Windows x64

Download the Windows package from the release assets:

```text
GlucoDesk-0.3.0-preview-windows-x64-installable.zip
```

After downloading:

1. extract the installable ZIP;
2. run `GlucoDesk-0.3.0-preview-win-x64-setup.exe`;
3. follow the setup wizard;
4. read the safety notice page;
5. optionally create a desktop shortcut;
6. launch GlucoDesk from the Start Menu.

The Windows installer:

* installs GlucoDesk for the current Windows user;
* does not require administrator privileges;
* adds Start Menu shortcuts;
* can optionally create a desktop shortcut;
* includes the MIT license page;
* includes a safety notice page before installation;
* supports standard Windows uninstall.

The Windows preview build is currently not code-signed.

Because of this, Microsoft Defender SmartScreen may show a warning such as:

```text
Windows protected your PC
```

If this happens:

1. click **More info**;
2. verify that the app name is the GlucoDesk installer downloaded from the official GitHub Release;
3. click **Run anyway**.

On Italian Windows, the buttons may appear as:

```text
Ulteriori informazioni
Esegui comunque
```

Only continue if you downloaded GlucoDesk from the official GitHub Releases page.

### Windows notification settings

If you do not see desktop notifications on Windows, check that notifications are allowed for GlucoDesk.

On Windows 11:

1. open **Settings**;
2. go to **System**;
3. open **Notifications**;
4. make sure notifications are enabled globally;
5. find GlucoDesk in the app list, if available;
6. enable notifications for GlucoDesk.

Windows notifications can also be hidden or delayed by Focus Assist, Do Not Disturb, notification rules, or system-level privacy settings.

GlucoDesk notifications are intended for calm glucose awareness only. They are not medical alarms and must not be used for emergency or treatment decisions.

> [!NOTE]
> The Windows package is self-contained and is intended to include the required .NET runtime files.

### Linux

Linux is not supported in the current preview.

The project is built with cross-platform technologies, but Linux runtime packaging and validation have not been completed yet.

### Verify release checksums

Each preview release includes SHA256 checksum files.

The current installable release bundle checksum file is:

```text
SHA256SUMS-installable.txt
```

To verify the top-level installable bundles on macOS or Linux:

```bash
shasum -a 256 -c SHA256SUMS-installable.txt
```

On Windows PowerShell, you can calculate hashes manually:

```powershell
Get-FileHash .\GlucoDesk-0.3.0-preview-windows-x64-installable.zip -Algorithm SHA256
```

Then compare the value with the checksum file.

---

## Build from source

This section is intended for developers who want to inspect, modify or build the project locally.

If you only want to try the app, use the packages attached to the latest GitHub Release instead of cloning or downloading the source code.

### Requirements

* .NET 10 SDK;
* macOS for macOS app bundle and DMG packaging;
* Windows for validating the Windows installer and tray behavior on the target platform.

### Restore, build, test and run

From the repository root:

```bash
dotnet restore
dotnet build -c Release
dotnet test -c Release
dotnet run --project src/GlucoDesk.Desktop/GlucoDesk.Desktop.csproj
```

### Full local verification

```bash
dotnet clean
dotnet restore
dotnet build -c Release
dotnet test -c Release
```

or:

```bash
./scripts/verify.sh
```

---

## Create local preview packages

This section is intended for developers and maintainers who want to generate release packages locally.

Regular users should download ready-to-run packages from the GitHub Releases page.

### macOS release assets

On macOS:

```bash
./scripts/create-macos-preview-release-assets.sh 0.3.0-preview all
```

This generates macOS preview assets for:

* `osx-arm64`;
* `osx-x64`.

Generated artifacts are written under:

```text
artifacts/macos/
```

### Windows release assets

On Windows PowerShell:

```powershell
.\scripts\create-windows-preview-release-assets.ps1 -Version "0.3.0-preview"
```

This generates:

* Windows setup installer;
* Windows checksum file;
* Windows release manifest.

Generated artifacts are written under:

```text
artifacts/windows/
```

### GitHub Actions release artifacts

Maintainers can generate release artifacts through the manual GitHub Actions workflow:

```text
Preview release artifacts
```

The workflow builds, tests, packages and uploads macOS and Windows preview artifacts.

After downloading workflow artifacts, maintainers can create final installable ZIP bundles containing the macOS DMGs and Windows installer.

Generated release bundles are written under:

```text
artifacts/release-candidate/
```

The `artifacts/` directory is ignored by Git.

---

## Architecture overview

GlucoDesk follows a layered .NET architecture.

```text
GlucoDesk.slnx
├── src
│   ├── GlucoDesk.Core
│   ├── GlucoDesk.Application
│   ├── GlucoDesk.Infrastructure
│   └── GlucoDesk.Desktop
├── tests
│   ├── GlucoDesk.Core.Tests
│   ├── GlucoDesk.Application.Tests
│   ├── GlucoDesk.Infrastructure.Tests
│   └── GlucoDesk.Desktop.Tests
├── docs
├── scripts
└── Directory.Build.props
```

### GlucoDesk.Core

Contains core domain concepts and shared glucose models.

This layer is independent from infrastructure, UI, file system and provider implementation details.

### GlucoDesk.Application

Contains application behavior, orchestration and abstractions.

This layer coordinates use cases such as:

* reading current glucose data;
* handling provider metadata;
* managing local history workflows;
* coordinating history continuity;
* preparing diary export data;
* exposing application-level results to the desktop UI.

### GlucoDesk.Infrastructure

Contains technical implementations.

This layer handles:

* CGM provider integrations;
* local storage;
* platform-aware local data paths;
* secure credential storage integration points;
* background sync infrastructure;
* history persistence;
* diary export generation;
* platform-specific integrations.

### GlucoDesk.Desktop

Contains the Avalonia desktop application.

This layer handles:

* desktop UI;
* view models;
* dependency injection composition;
* dashboard rendering;
* account configuration;
* settings screens;
* glucose awareness notifications;
* diary export user flow;
* macOS menu bar integration;
* Windows tray integration;
* desktop file save dialogs.

The desktop layer should remain focused on presentation and composition, while application and infrastructure behavior stay in dedicated layers.

---

## Quality and release engineering

GlucoDesk is developed as a production-oriented portfolio project.

Current quality practices include:

* layered architecture;
* provider-based design;
* local-first data model;
* platform-aware local storage paths;
* automated tests across core, application, infrastructure and desktop layers;
* shared build configuration through `Directory.Build.props`;
* nullable reference types enabled;
* warnings treated as errors;
* repository-level `.editorconfig`;
* GitHub Actions continuous integration;
* CI build and test on Ubuntu, macOS and Windows;
* manual GitHub Actions workflow for preview release artifacts;
* macOS Apple Silicon and Intel packaging;
* Windows setup installer packaging;
* release artifact verification scripts;
* checksum generation;
* release smoke-test checklist;
* documented macOS Gatekeeper flow;
* documented Windows SmartScreen flow.

Run the full local validation with:

```bash
dotnet clean
dotnet restore
dotnet build -c Release
dotnet test -c Release
```

The current test suite covers core, application, infrastructure and desktop behavior.

The v0.3.0-preview release was validated with:

```text
906 tests, 0 failures
```

---

## Known limitations

GlucoDesk is still a preview.

Current limitations:

* the app is not a medical device;
* the app must not be used for treatment decisions;
* glucose awareness notifications are not alarms;
* native notifications can be delayed, blocked or hidden by the operating system;
* macOS packages are not signed with Apple Developer ID and are not notarized yet;
* macOS may require approval from Privacy & Security on first launch;
* Windows packages are not code-signed yet;
* Windows may show a Microsoft Defender SmartScreen warning on first launch;
* Linux runtime support is not available yet;
* auto-update is not available yet;
* provider runtime behavior may depend on platform, region and account configuration;
* local history completeness depends on sync availability and app runtime;
* data completeness reporting can only describe the available local history;
* diary exports depend on locally available readings;
* app icon and brand assets may still evolve.

---

## Roadmap

Planned improvements include:

* signed and notarized macOS packages;
* Windows code signing;
* stronger release automation;
* improved first-run onboarding;
* improved dashboard empty states;
* improved dashboard error states;
* richer diary and data-completeness reporting;
* additional statistics views;
* macOS widget exploration;
* Linux packaging and runtime support;
* platform-specific secure credential storage hardening;
* additional provider abstraction hardening;
* improved local history continuity and backfill behavior;
* auto-update exploration;
* README and release asset polish.

---

## Disclaimer

GlucoDesk is an independent software project.

It is not affiliated with, endorsed by, approved by, or sponsored by Dexcom, Insulet, Omnipod, or any other medical device manufacturer.

GlucoDesk is not a medical device.

Do not use GlucoDesk for treatment decisions, insulin dosing, emergency alerts, alarms, diagnosis, or as a replacement for approved diabetes applications.

For therapy decisions, always use approved medical devices and official medical apps.

---

## License

This project is licensed under the MIT License.

See [LICENSE](LICENSE) for details.

---

## Additional documentation

Learn more about glucose awareness notifications in:

```text
docs/features/glucose-awareness-notifications.md
```

For manual validation before releases, see:

```text
docs/qa/glucose-notifications-checklist.md
docs/qa/native-notification-packaged-app-checklist.md
docs/qa/release-readiness-checklist.md
```

For release notes, see:

```text
docs/release-notes/glucose-awareness-notifications-preview.md
```