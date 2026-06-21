<p align="center">
  <img src="./docs/assets/glucodesk-social-preview.png" alt="GlucoDesk - A calm desktop companion for glucose awareness" width="100%" />
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-10.0-512BD4" alt=".NET 10" />
  <img src="https://img.shields.io/badge/Avalonia-UI-0B8CE9" alt="Avalonia UI" />
  <img src="https://img.shields.io/badge/license-MIT-green" alt="MIT license" />
  <img src="https://img.shields.io/badge/status-v0.1.0--preview-blue" alt="v0.1.0 preview" />
  <img src="https://img.shields.io/badge/preview-macOS--only-00AEEF" alt="macOS-only preview" />
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
  It brings glucose trend, recent history, background sync status, history continuity and diary export into a clean desktop experience.
</p>

---

> [!IMPORTANT]
> **Safety notice**
>
> GlucoDesk is not a medical device and must not be used for treatment decisions.
>
> Always use approved medical devices and official medical apps for therapy decisions.

> [!WARNING]
> **Platform support**
>
> GlucoDesk **v0.1.0-preview is currently intended for macOS only**.
>
> The available preview packages target:
>
> * macOS Apple Silicon (`osx-arm64`);
> * macOS Intel (`osx-x64`).
>
> Windows and Linux are part of the long-term cross-platform direction, but they are **not supported in the current preview**.
>
> The app may build or start on other platforms, but some runtime features, especially account configuration and secure credential storage, are currently validated only on macOS.

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
* [Create a local preview package](#create-a-local-preview-package)
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
* history continuity;
* readable diary export;
* privacy-conscious local storage.

The goal is simple:

> Make glucose awareness more comfortable during desktop work, without replacing official medical apps or devices.

GlucoDesk is designed with a provider-based architecture so the project can evolve beyond a single data source over time.

---

## Preview

GlucoDesk is currently in **v0.1.0-preview**.

The preview already includes the first complete product loop:

```text
Connect an optional CGM data source
→ show glucose awareness on desktop
→ keep local history updated
→ reduce local history gaps
→ export a readable glycemic diary
```

> [!NOTE]
> The current public preview is validated on macOS.
>
> Windows and Linux are future runtime targets, but they are not supported in v0.1.0-preview.

### Dashboard

![GlucoDesk dashboard](docs/assets/screenshots/dashboard.png)

The dashboard shows the current glucose value, trend, data freshness, provider status and recent glucose chart in a desktop-friendly layout.

### Account and secure credential storage

![GlucoDesk account](docs/assets/screenshots/account.png)

The Account page allows local account configuration, connection testing and secure credential handling.

> [!NOTE]
> In the current preview, account configuration and secure credential storage are validated on macOS only.

### Glycemic diary export

![GlucoDesk diary](docs/assets/screenshots/diary.png)

The diary export is designed to generate readable Excel and PDF summaries from local glucose history.

### Settings

![GlucoDesk settings](docs/assets/screenshots/settings.png)

The Settings page controls provider routing, glucose preferences and dashboard behavior.

---

## Why GlucoDesk?

Many people spend hours at their desk every day.

When glucose information is only available through a phone, checking it repeatedly can become distracting.

GlucoDesk was created to make that experience calmer:

* keep glucose awareness visible while working;
* avoid constantly switching context to the phone;
* understand whether data is fresh, stale or unavailable;
* keep a local glucose history;
* export a readable diary for personal review;
* avoid unnecessary backend services;
* keep the app focused, quiet and desktop-friendly.

GlucoDesk is not intended to replace official apps.

It is a companion experience for awareness, personal review and desktop convenience.

---

## Current release status

Current version:

```text
0.1.0-preview
```

The preview focuses on:

* desktop glucose dashboard;
* optional CGM provider integration;
* local glucose history;
* background synchronization;
* startup and resume history continuity;
* secure local credential handling;
* Excel diary export;
* PDF diary export;
* app branding;
* first macOS preview packaging.

Current runtime support:

| Platform            | Status            |
| ------------------- | ----------------- |
| macOS Apple Silicon | Preview supported |
| macOS Intel         | Preview supported |
| Windows             | Not supported yet |
| Linux               | Not supported yet |

The app is usable for early testing on macOS, but still evolving.

Windows and Linux remain part of the cross-platform roadmap, but the current preview release should be considered **macOS-only**.

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
* basic glucose insights.

The UI is designed to stay calm, readable and useful during desktop work.

### CGM provider routing

GlucoDesk follows a provider-based architecture.

The desktop app can route live and historical glucose data through configured CGM providers.

The current preview focuses on a first practical provider flow, while the architecture keeps the project open to future provider extensions.

### Secure local credential handling

GlucoDesk is designed so sensitive credentials stay on the user’s computer.

On macOS, account credentials are handled through the configured secure credential store, using the macOS Keychain for the current preview flow.

Credentials are not stored in local JSON settings files and must never be committed to Git.

> [!WARNING]
> Secure credential storage is currently validated only on macOS.
>
> Windows and Linux credential-store implementations are not supported in v0.1.0-preview.

### Automatic reconnect

After account configuration, GlucoDesk can reconnect after app restart without requiring the user to re-enter credentials.

This allows the desktop app to restore the selected provider configuration and continue the local-first workflow more smoothly.

### Connection diagnostics

The Account page clearly shows whether the configured connection is:

* not tested;
* not verified;
* verified;
* failed.

If email, password or region changes, the connection state becomes stale and the app asks the user to test again.

### Background sync status

The sidebar shows whether background synchronization is active and when the last successful update happened.

This makes it easier to understand whether the local view is fresh or outdated.

### History continuity

GlucoDesk keeps local glucose history and includes a continuity system to reduce local gaps.

The sidebar includes:

* startup or resume sync status;
* last successful history sync;
* fetched readings;
* added readings;
* duplicate readings;
* stored readings;
* manual “Sync history now” action.

### Glycemic diary export

GlucoDesk can export a glycemic diary in:

* Excel workbook format;
* PDF format.

The diary is designed to be readable and focused on useful summaries instead of overwhelming the user with every single CGM data point.

The current diary direction focuses on:

* daily summaries;
* time-in-range information;
* data coverage indicators;
* incomplete-data awareness;
* structured data suitable for personal review.

---

## Privacy model

GlucoDesk is built with a local-first mindset.

By design:

* glucose history is stored locally on the user’s computer;
* app settings are stored locally;
* widget and dashboard state are stored locally;
* credentials are handled through the configured secure credential store;
* credentials are not stored in local JSON settings files;
* GlucoDesk does not require a custom backend to handle user credentials or glucose history.

Local-first does not mean that no sensitive data exists.

Glucose readings are personal health-related data and are stored locally when history features are enabled.

The privacy goal is:

> Keep user data on the user’s machine and avoid unnecessary external services.

Users should still protect their computer account, disk, backups and operating-system credential store.

---

## Installation preview

Download the latest preview package from GitHub Releases.

GlucoDesk v0.1.0-preview is currently distributed as a **macOS preview build**.

Available package targets:

```text
GlucoDesk-0.1.0-preview-osx-arm64.zip
GlucoDesk-0.1.0-preview-osx-x64.zip
```

Use:

* `osx-arm64` for Apple Silicon Macs;
* `osx-x64` for Intel Macs.

Unzip the package and open:

```text
GlucoDesk.app
```

The preview app is not signed or notarized yet.

On macOS, the first launch may require:

```text
Right click → Open
```

This is expected for an unsigned preview build.

> [!NOTE]
> Windows and Linux builds are not available in the current preview.
>
> Even if the project can be built from source on other platforms, the runtime experience is currently validated only on macOS.

---

## Build from source

### Requirements

* .NET 10 SDK;
* macOS for the supported preview runtime;
* macOS required for the current `.app` preview packaging script;
* Windows or Linux can be used for source-level experimentation, but they are not supported runtime targets in v0.1.0-preview.

### Restore, build, test and run

From the repository root:

```bash
dotnet restore
dotnet build -c Release
dotnet test -c Release
dotnet run --project src/GlucoDesk.Desktop/GlucoDesk.Desktop.csproj
```

> [!WARNING]
> Running the desktop app from source on Windows or Linux is not supported in the current preview.
>
> Some UI paths may open, but account configuration, secure credential storage and provider runtime behavior are currently validated only on macOS.

---

## Create a local preview package

On macOS Apple Silicon:

```bash
./scripts/package-preview.sh osx-arm64
```

Optional Intel macOS package:

```bash
./scripts/package-preview.sh osx-x64
```

Generated packages are written to:

```text
artifacts/releases/
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
* secure credential storage;
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
* diary export user flow.

The desktop layer should remain focused on presentation and composition, while application and infrastructure behavior stay in dedicated layers.

---

## Quality and release engineering

GlucoDesk is developed as a production-oriented portfolio project.

Current quality practices include:

* layered architecture;
* provider-based design;
* local-first data model;
* secure credential-store abstraction;
* automated tests across core, application, infrastructure and desktop layers;
* shared build configuration through `Directory.Build.props`;
* nullable reference types enabled;
* warnings treated as errors;
* repository-level `.editorconfig`;
* GitHub Actions continuous integration;
* preview packaging script;
* release-readiness documentation.

Run the full local validation with:

```bash
dotnet clean
dotnet restore
dotnet build -c Release
dotnet test -c Release
```

---

## Known limitations

GlucoDesk is still a preview.

Current limitations:

* v0.1.0-preview is currently supported and validated only on macOS;
* available preview packages target macOS Apple Silicon and macOS Intel;
* Windows and Linux runtime support is not available yet;
* Windows and Linux installers are not finalized yet;
* account configuration and secure credential storage are currently validated only on macOS;
* provider runtime behavior may depend on platform, region and account configuration;
* macOS packages are not signed or notarized yet;
* app icon and brand assets may still evolve;
* local history completeness depends on sync availability and app runtime;
* the app is not intended for treatment decisions;
* the app is not a medical device.

---

## Roadmap

Planned improvements include:

* polished public release packaging;
* better first-run onboarding;
* improved dashboard empty states;
* improved dashboard error states;
* stronger release automation;
* Windows packaging improvements;
* Linux packaging improvements;
* platform-specific secure credential storage hardening;
* richer diary and data-completeness reporting;
* macOS widget exploration;
* additional provider abstraction hardening;
* improved local history continuity and backfill behavior.

---

## Disclaimer

GlucoDesk is an independent software project.

It is not affiliated with, endorsed by, approved by, or sponsored by Dexcom, Insulet, Omnipod, or any other medical device manufacturer.

GlucoDesk is not a medical device.

Do not use GlucoDesk for treatment decisions.

For therapy decisions, always use approved medical devices and official medical apps.

---

## License

This project is licensed under the MIT License.

See [LICENSE](LICENSE) for details.
