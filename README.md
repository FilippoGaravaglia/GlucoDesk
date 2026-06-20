# GlucoDesk

**A calm desktop companion for glucose awareness.**

GlucoDesk is a local-first desktop app designed to help people keep an eye on their CGM glucose data while working on a computer.

It brings your glucose trend, recent history, background sync status, history continuity and diary export into a clean desktop experience.

> **Safety notice**
>
> GlucoDesk is not a medical device and must not be used for treatment decisions.
> Always use official Dexcom, Omnipod or other approved medical apps and devices for therapy decisions.

---

## Why GlucoDesk exists

Many people spend hours at their computer every day.

GlucoDesk was created to make glucose awareness more comfortable during that time:

- no need to constantly check the phone;
- a calm dashboard always close to your work;
- local-first history and exports;
- clear status when data is fresh, stale or unavailable;
- privacy-conscious design.

---

## Preview status

GlucoDesk is currently in **v0.1.0-preview**.

This means the app is usable for early testing, but still evolving.

The preview focuses on:

- Dexcom Share connectivity;
- desktop glucose dashboard;
- local glucose history;
- background sync;
- history continuity;
- diary export;
- app branding and first release packaging.

---

## Screenshots

### Dashboard

![GlucoDesk dashboard](docs/assets/screenshots/dashboard.png)

### Account and secure credential storage

![GlucoDesk account](docs/assets/screenshots/account.png)

### Diary export

![GlucoDesk diary](docs/assets/screenshots/diary.png)

### Settings

![GlucoDesk settings](docs/assets/screenshots/settings.png)

---

## Main features

### Desktop glucose dashboard

GlucoDesk shows your current glucose value, trend, data freshness and recent glucose chart in a desktop-friendly layout.

### Dexcom Share support

The preview release supports Dexcom Share as the main CGM provider.

You can configure:

- Dexcom email;
- Dexcom password;
- region;
- connection test;
- secure local credential storage.

### Secure local credential storage

GlucoDesk is designed so credentials stay on your computer.

On macOS, Dexcom Share credentials are stored using the macOS Keychain through the configured secure credential store.

Credentials are not stored in local JSON settings and must never be committed to Git.

### Automatic reconnect

After saving your Dexcom Share account, GlucoDesk selects Dexcom Share as the active live and historical provider.

This allows the app to reconnect after restart without entering credentials again.

### Connection diagnostics

The Account page clearly shows whether the current credentials are:

- not tested;
- not verified;
- verified;
- failed.

If email, password or region changes, the connection status becomes stale and GlucoDesk asks you to test again.

### Background sync status

The sidebar shows whether background sync is active and when the last successful update happened.

### History continuity

GlucoDesk keeps a local glucose history and includes a history continuity system to reduce local gaps.

The sidebar includes:

- startup/resume sync status;
- last successful history sync;
- fetched/added/duplicate/stored readings;
- manual “Sync history now” action.

### Glycemic diary export

GlucoDesk can export a glycemic diary in:

- Excel format;
- PDF format.

The diary is designed to be readable and focused on useful summaries instead of overwhelming the user with every single CGM point.

---

## Privacy

GlucoDesk is built with a local-first mindset.

The preview app stores data locally on your computer and avoids unnecessary external services.

Sensitive credentials are handled through the configured secure credential store.

---

## Installation preview

Download the latest preview package from GitHub Releases.

For macOS Apple Silicon, the package name is expected to look like:

GlucoDesk-0.1.0-preview-osx-arm64.zip

Unzip it and open:

GlucoDesk.app

The preview app is not signed or notarized yet.

On macOS, the first launch may require:

Right click > Open

---

## Build from source

Requirements:

- .NET 10 SDK;
- macOS, Windows or Linux for development;
- macOS required for the current .app preview packaging script.

Run:

- dotnet restore
- dotnet build -c Release
- dotnet test -c Release
- dotnet run --project src/GlucoDesk.Desktop/GlucoDesk.Desktop.csproj

---

## Create a local preview package

On macOS:

./scripts/package-preview.sh osx-arm64

Optional Intel macOS package:

./scripts/package-preview.sh osx-x64

Generated packages are written to:

artifacts/releases/

The artifacts/ directory is ignored by Git.

---

## Known limitations

GlucoDesk is still a preview.

Current limitations:

- macOS packages are not signed or notarized yet;
- app icon and brand assets may still evolve;
- packaging is currently focused on macOS;
- Windows and Linux installers are not finalized yet;
- the app is not intended for treatment decisions;
- Dexcom Share availability may depend on region and account configuration.

---

## Roadmap

Planned next improvements include:

- polished public release packaging;
- better onboarding;
- improved dashboard empty and error states;
- stronger release automation;
- macOS widget exploration;
- more complete diary and data completeness reporting;
- Windows and Linux packaging improvements.

---

## Disclaimer

GlucoDesk is an independent software project.

It is not affiliated with, endorsed by or approved by Dexcom, Insulet, Omnipod or any other medical device manufacturer.

GlucoDesk is not a medical device.

Do not use GlucoDesk for treatment decisions.
