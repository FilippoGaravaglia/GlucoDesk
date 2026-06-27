<p align="center">
  <img src="https://img.shields.io/badge/.NET-10.0-512BD4" alt=".NET 10" />
  <img src="https://img.shields.io/badge/Avalonia-UI-0B8CE9" alt="Avalonia UI" />
  <img src="https://img.shields.io/badge/license-MIT-green" alt="MIT license" />
  <img src="https://img.shields.io/badge/status-v0.2.1--preview-blue" alt="v0.2.1 preview" />
  <img src="https://img.shields.io/badge/macOS-preview-00AEEF" alt="macOS preview" />
  <img src="https://img.shields.io/badge/Windows-portable--preview-0078D4" alt="Windows portable preview" />
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
  It brings glucose trend, recent history, local background sync, data completeness awareness, settings, account configuration and glycemic diary export into a clean desktop experience.
</p>

---

<p align="center">
  <img src="./docs/assets/glucodesk-social-preview.png" alt="GlucoDesk - A calm desktop companion for glucose awareness" width="100%" />
</p>

> [!IMPORTANT]
> **Safety notice**
>
> GlucoDesk is not a medical device and must not be used for treatment decisions, insulin dosing, emergency alerts, or as a replacement for approved diabetes applications.
>
> Always use approved medical devices and official medical apps for therapy decisions.

> [!WARNING]
> **Preview status**
>
> GlucoDesk is currently a preview project.
>
> The app is intended for awareness, personal review and desktop convenience only.
>
> The current preview supports macOS packages and a Windows x64 portable package. Windows support is available in preview and should still be considered early until validated across more real-world machines and runtime flows.

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
* privacy-conscious local storage.

The goal is simple:

> Make glucose awareness more comfortable during desktop work, without replacing official medical apps or devices.

GlucoDesk uses a provider-based architecture so the project can evolve beyond a single data source over time.

---

## Preview

GlucoDesk is currently in **v0.2.1-preview**.

This preview focuses on turning the app into a more complete desktop product loop:

```text
Connect an optional CGM data source
→ show glucose awareness on desktop
→ keep local history updated
→ reduce local history gaps
→ analyze recent glucose windows
→ export a readable glycemic diary
→ keep preferences consistent across app and exports
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
* clear safety messaging.

### Account

![GlucoDesk account](docs/assets/screenshots/account.png)

The Account page provides a cleaner place to configure provider-related account information and connection checks.

It is designed around a local-first workflow and keeps account configuration separate from the main dashboard experience.

The current preview supports secure local credential storage on:

* macOS, through macOS Keychain;
* Windows, through Windows Credential Manager.

Credentials are used locally by the desktop app to connect to the configured provider. GlucoDesk does not provide a custom backend for handling user credentials.

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

The Settings page controls provider routing, glucose preferences and dashboard behavior.

The current preview includes improved settings handling for:

* active live provider;
* historical provider;
* preferred glucose unit;
* target range;
* dashboard refresh interval;
* chart maximum;
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
* export a readable diary for personal review;
* avoid unnecessary backend services;
* keep the app focused, quiet and desktop-friendly.

GlucoDesk is not intended to replace official apps.

It is a companion experience for awareness, personal review and desktop convenience.

---

## Current release status

Current version:

```text
0.2.1-preview
```

The preview focuses on:

* redesigned desktop glucose dashboard;
* optional CGM provider integration;
* local glucose history;
* background synchronization;
* startup and resume history continuity;
* local data completeness awareness;
* glucose insights;
* preferred glucose unit support;
* Excel diary export;
* PDF diary export;
* updated app branding and screenshots;
* macOS preview packaging;
* Windows portable preview packaging;
* Windows Credential Manager support for Dexcom Share credentials;
* improved account connection flow on Windows.

Current runtime support:

| Platform            | Status                     | Notes                                 |
| ------------------- | -------------------------- | ------------------------------------- |
| macOS Apple Silicon | Preview supported          | Distributed as `osx-arm64` package    |
| macOS Intel         | Preview supported          | Distributed as `osx-x64` package      |
| Windows x64         | Portable preview available | Distributed as `win-x64` portable zip |
| Linux               | Not supported yet          | Planned for a future step             |

Windows support currently means a portable preview package, not a full installer.

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

This is especially important for future diary export and completeness reporting.

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

### Windows portable preview

GlucoDesk includes a Windows x64 portable preview build.

The Windows package is distributed as a zip archive.

The expected usage is:

```text
Extract the zip
→ open the extracted folder
→ run GlucoDesk.Desktop.exe
```

This is not a Windows installer yet.

---

## Privacy model

GlucoDesk is built with a local-first mindset.

By design:

* glucose history is stored locally on the user’s computer;
* app settings are stored locally;
* dashboard and widget-related state are stored locally;
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

Available package targets for this preview:

```text
GlucoDesk-0.2.1-preview-osx-arm64.zip
GlucoDesk-0.2.1-preview-osx-x64.zip
GlucoDesk-0.2.1-preview-win-x64-portable.zip
```

Choose the package for your operating system:

| System              | Download                                       |
| ------------------- | ---------------------------------------------- |
| macOS Apple Silicon | `GlucoDesk-0.2.1-preview-osx-arm64.zip`        |
| macOS Intel         | `GlucoDesk-0.2.1-preview-osx-x64.zip`          |
| Windows 64-bit      | `GlucoDesk-0.2.1-preview-win-x64-portable.zip` |

Not sure which macOS package to use?

* Choose `osx-arm64` for Apple Silicon Macs with M1, M2, M3, M4 or newer chips.
* Choose `osx-x64` for Intel Macs.

### Updating an existing installation

Replacing the application bundle or portable folder does not normally delete local data.

GlucoDesk stores app data and credentials outside the application files:

* macOS credentials are stored in macOS Keychain;
* Windows credentials are stored in Windows Credential Manager;
* local app data is stored in the operating-system application data location.

To update:

* close GlucoDesk;
* download the new package from the latest GitHub Release;
* replace the old app bundle or portable folder with the new one;
* open GlucoDesk again.

### macOS

Download the correct macOS zip from the release assets.

For Apple Silicon Macs such as M1, M2, M3 or newer, use:

```text
GlucoDesk-0.2.1-preview-osx-arm64.zip
```

For Intel Macs, use:

```text
GlucoDesk-0.2.1-preview-osx-x64.zip
```

After downloading:

1. unzip the package;
2. move `GlucoDesk.app` to the `Applications` folder;
3. if macOS asks whether to replace an existing copy, choose **Replace**;
4. open the app.

The preview app is currently not signed or notarized.

Because of this, macOS Gatekeeper may block the first launch.

First try:

```text
Right click GlucoDesk.app → Open → Open
```

If macOS shows a message such as:

```text
"GlucoDesk" is damaged and can't be opened.
```

this usually means the downloaded preview app is blocked by the quarantine attribute because it is not signed/notarized yet.

From Terminal, run:

```bash
xattr -dr com.apple.quarantine /Applications/GlucoDesk.app
```

Then open the app again:

```bash
open /Applications/GlucoDesk.app
```

If you are testing directly from the Downloads folder instead of Applications, use:

```bash
xattr -dr com.apple.quarantine "$HOME/Downloads/GlucoDesk.app"
open "$HOME/Downloads/GlucoDesk.app"
```

For normal usage, moving the app to `Applications` is recommended.

> [!NOTE]
> This manual macOS step is expected for the current unsigned preview build.
>
> A future release goal is to provide signed and notarized macOS packages so users do not need to run terminal commands.

### Windows

Download the Windows portable package from the release assets:

```text
GlucoDesk-0.2.1-preview-win-x64-portable.zip
```

Then:

1. extract the zip into a normal folder;
2. open the extracted folder;
3. run:

```text
GlucoDesk.Desktop.exe
```

Do not run the app directly from inside the compressed zip preview. Extract it first.

The Windows preview is portable.

It does not currently:

* install the app into the Start Menu;
* create a desktop shortcut;
* create a system installer entry;
* register an uninstaller.

To update the Windows portable preview:

1. close GlucoDesk;
2. download the new Windows zip;
3. extract it to a new folder or replace the previous portable folder;
4. run `GlucoDesk.Desktop.exe` again.

Dexcom Share credentials are stored locally using Windows Credential Manager.

> [!NOTE]
> The Windows package is self-contained and is intended to include the required .NET runtime files.
>
> Windows support is still considered preview-level until the main runtime flows are validated on more real Windows machines.

---

## Build from source

This section is intended for developers who want to inspect, modify or build the project locally.

If you only want to try the app, use the packages attached to the latest GitHub Release instead of cloning or downloading the source code.

### Requirements

* .NET 10 SDK;
* macOS for macOS app bundle packaging;
* Windows for validating the Windows portable package on the target platform.

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

### macOS Apple Silicon

```bash
./scripts/package-preview.sh osx-arm64
```

### macOS Intel

```bash
./scripts/package-preview.sh osx-x64
```

Generated macOS packages are written to:

```text
artifacts/releases/
```

### Windows x64 portable

From macOS, Linux or Windows, the Windows publish output can be produced with:

```bash
dotnet publish src/GlucoDesk.Desktop/GlucoDesk.Desktop.csproj \
  -c Release \
  -r win-x64 \
  --self-contained true \
  -p:PublishSingleFile=false \
  -o artifacts/publish/GlucoDesk-win-x64
```

On Windows, the portable zip can be generated with:

```powershell
.\scripts\publish-windows.ps1
```

Generated artifacts are written under:

```text
artifacts/
```

The `artifacts/` directory is ignored by Git.

---

<!-- GLUCODESK_PREVIEW_INSTALL_START -->
## Preview installation

GlucoDesk is currently distributed as a preview desktop application.

> Safety notice: GlucoDesk is not a medical device. It must not be used to make insulin dosing, treatment, diagnosis, emergency, or safety-critical decisions. Always rely on your official CGM system, insulin pump, glucose meter, and clinical guidance for medical decisions.

### macOS preview

Recommended asset:

```text
GlucoDesk-<version>-osx-arm64.dmg
```

Installation flow:

1. Download the macOS DMG from the GitHub Release.
2. Open the `.dmg` file.
3. Drag `GlucoDesk.app` to `Applications`, if the DMG layout provides that shortcut.
4. Open GlucoDesk from `Applications`.

If macOS Gatekeeper warns that the app cannot be opened because it is not notarized yet, this is expected for the preview. Notarization is planned as a production-readiness step.

Alternative macOS asset:

```text
GlucoDesk-<version>-osx-arm64.zip
```

The ZIP is useful for testing and manual extraction, while the DMG is the preferred user-facing macOS preview package.

### Windows preview

Recommended asset:

```text
GlucoDesk-<version>-win-x64-setup.exe
```

The Windows setup installer:

- installs GlucoDesk for the current Windows user;
- does not require administrator privileges;
- adds Start Menu shortcuts;
- can optionally create a desktop shortcut;
- includes the MIT license page;
- includes a safety notice page before installation;
- supports standard Windows uninstall.

Windows may show a SmartScreen warning because the preview installer is not code-signed yet. This is expected until Windows code signing is added.

Alternative Windows asset:

```text
GlucoDesk-<version>-win-x64-portable.zip
```

Portable ZIP usage:

1. Download the ZIP from the GitHub Release.
2. Extract it to a local folder.
3. Run `GlucoDesk.Desktop.exe`.

The portable ZIP does not create Start Menu shortcuts and does not register an uninstall entry.

### Verify release checksums

Each preview release includes SHA256 checksum files.

On macOS or Linux:

```bash
shasum -a 256 -c GlucoDesk-<version>-osx-arm64-checksums.sha256
```

On Windows PowerShell:

```powershell
Get-FileHash .\GlucoDesk-<version>-win-x64-setup.exe -Algorithm SHA256
Get-FileHash .\GlucoDesk-<version>-win-x64-portable.zip -Algorithm SHA256
```

Compare the output with:

```text
GlucoDesk-<version>-win-x64-checksums.sha256
```

### Current preview limitations

The preview is intentionally limited:

- macOS notarization is not available yet;
- Windows code signing is not available yet;
- auto-update is not available yet;
- Windows support currently targets `win-x64`;
- macOS preview packaging currently targets Apple Silicon;
- data completeness depends on local history availability and provider connectivity;
- exports are intended for personal review and discussion with healthcare professionals, not urgent or automated medical decisions.
<!-- GLUCODESK_PREVIEW_INSTALL_END -->

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
* diary export user flow;
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
* Windows portable publish workflow;
* macOS preview packaging script;
* release-readiness documentation.

Run the full local validation with:

```bash
dotnet clean
dotnet restore
dotnet build -c Release
dotnet test -c Release
```

The current test suite covers core, application, infrastructure and desktop behavior.

---

## Known limitations

GlucoDesk is still a preview.

Current limitations:

* the app is not a medical device;
* the app must not be used for treatment decisions;
* macOS packages are not signed or notarized yet;
* macOS may require a manual Gatekeeper quarantine workaround on first launch;
* Windows support is currently distributed as a portable preview package, not a full installer;
* Windows packages are not signed yet;
* Linux runtime support is not available yet;
* provider runtime behavior may depend on platform, region and account configuration;
* local history completeness depends on sync availability and app runtime;
* data completeness reporting can only describe the available local history;
* app icon and brand assets may still evolve.

---

## Roadmap

Planned improvements include:

* polished public release packaging;
* signed and notarized macOS packages;
* stronger release automation;
* Windows installer support;
* Windows runtime validation and hardening;
* Linux packaging and runtime support;
* platform-specific secure credential storage hardening;
* improved first-run onboarding;
* improved dashboard empty states;
* improved dashboard error states;
* richer diary and data-completeness reporting;
* additional statistics views;
* macOS widget exploration;
* additional provider abstraction hardening;
* improved local history continuity and backfill behavior;
* README and release asset polish.

---

## Disclaimer

GlucoDesk is an independent software project.

It is not affiliated with, endorsed by, approved by, or sponsored by Dexcom, Insulet, Omnipod, or any other medical device manufacturer.

GlucoDesk is not a medical device.

Do not use GlucoDesk for treatment decisions, insulin dosing, emergency alerts, or as a replacement for approved diabetes applications.

For therapy decisions, always use approved medical devices and official medical apps.

---

## License

This project is licensed under the MIT License.

See [LICENSE](LICENSE) for details.

## macOS first launch note

GlucoDesk preview builds may be unsigned or not notarized.

The macOS DMG supports the standard drag-to-Applications installation flow, but macOS Gatekeeper may still block the app the first time it is opened because Apple cannot verify non-notarized builds.

If macOS blocks GlucoDesk on first launch:

1. Click **Done** or close the warning dialog.
2. Open **System Settings**.
3. Go to **Privacy & Security**.
4. Scroll to the **Security** section.
5. Find the GlucoDesk warning.
6. Click **Open Anyway**.
7. Confirm with password or Touch ID.
8. Launch GlucoDesk again from **Applications**.

This approval is normally required only once.

GlucoDesk is not a medical device and must not be used for insulin dosing, treatment, diagnosis, emergency, or safety-critical decisions.
