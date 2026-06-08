# GlucoDesk

GlucoDesk is a local-first, cross-platform desktop companion for visualizing CGM glucose data while working at your computer.

The project is designed around a provider-based architecture:

* Mock provider for local development, tests, demos and future demo mode.
* Nightscout provider for near real-time CGM visualization.
* Dexcom Official API provider for delayed official historical data and metadata.

> GlucoDesk is not a medical device. It must not be used for treatment decisions, insulin dosing, emergency alerts, or as a replacement for Dexcom, Omnipod, Nightscout, or any approved medical application.

## Goals

* Cross-platform desktop application for Windows, macOS and Linux.
* Local-first and privacy-first architecture.
* Provider-based CGM data access.
* Clean .NET architecture.
* Useful desktop dashboard and compact widget.
* Open-source friendly design.
* Clear separation between domain, application, infrastructure and UI concerns.
* Support for fake demo data without exposing real personal glucose data.
* Future support for local history, diabetes diary exports and reporting.

## Tech stack

* .NET 10
* Avalonia UI
* CommunityToolkit.Mvvm
* xUnit
* Microsoft.Extensions.DependencyInjection
* System.Text.Json

## Repository structure

```text
src/
  GlucoDesk.Core/
    Glucose/
      Enums/
      Readings/
      ValueObjects/

  GlucoDesk.Application/
    Cgm/
      Dashboard/
        Requests/
        Results/
      Providers/
        Abstractions/
        Metadata/
      Readings/
        Requests/
        Results/
      Services/
        Abstractions/
    Common/
      DependencyInjection/
      Errors/
      Results/
    Settings/
      Abstractions/
      Models/
      Services/

  GlucoDesk.Infrastructure/
    Cgm/
      Mock/
        DependencyInjection/
        Generators/
        Options/
        Providers/
    Settings/
      DependencyInjection/
      Options/
      Stores/

  GlucoDesk.Desktop/
    Bootstrap/
    ViewModels/
      Common/
      Dashboard/
        Chart/
        Options/
      Main/
    Views/
      Dashboard/
        Controls/
      Main/

tests/
  GlucoDesk.Core.Tests/
    Glucose/

  GlucoDesk.Application.Tests/
    Cgm/
      Dashboard/
        Requests/
        Results/
      Providers/
        Metadata/
      Readings/
        Requests/
        Results/
      Services/
    Common/
      DependencyInjection/
      Errors/
      Results/
    Settings/
      Models/
      Services/

  GlucoDesk.Infrastructure.Tests/
    Cgm/
      Mock/
        DependencyInjection/
        Options/
        Providers/
    Settings/
      DependencyInjection/
      Options/
      Stores/

  GlucoDesk.Desktop.Tests/
    ViewModels/
      Dashboard/
        Chart/
        Options/

docs/
  architecture-decisions/

build/
```

## Current status

The project is currently in the early foundation phase.

Implemented:

* Solution structure.
* Core project.
* Application project.
* Infrastructure project.
* Desktop project.
* Core test project.
* Application test project.
* Infrastructure test project.
* Desktop test project.
* Initial glucose domain model.
* Unit tests for glucose value, range and reading behavior.
* Application-level result/error model.
* CGM provider abstractions.
* CGM readings request and result contracts.
* Provider metadata contract.
* Deterministic mock CGM provider.
* Mock provider dependency injection registration.
* Application-level glucose data service.
* Dashboard snapshot request/result model.
* Dependency injection registration for application services.
* Avalonia desktop shell.
* Desktop dependency injection bootstrap.
* Initial dashboard shell connected to the mock CGM provider.
* Dashboard auto-refresh timer.
* Dashboard refresh options.
* Lightweight glucose trend chart.
* Dashboard chart point model.
* Application settings model.
* Application settings service abstraction and implementation.
* Local JSON settings store.
* Dependency injection registration for local settings infrastructure.
* Unit tests for application contracts, glucose data service, mock provider options, provider behavior and DI registration.
* Unit tests for dashboard refresh options and dashboard view model behavior.
* Unit tests for dashboard chart point validation.
* Unit tests for application settings, settings service, local settings options, JSON store and DI registration.

## Architecture

GlucoDesk follows a layered architecture:

```text
GlucoDesk.Core
  Pure domain model.
  No dependency on UI, external APIs, storage or infrastructure concerns.

GlucoDesk.Application
  Application contracts, provider abstractions, request/result models, dashboard models,
  settings models, application-level errors and application services.
  No dependency on concrete CGM providers or storage implementations.

GlucoDesk.Infrastructure
  Implementation layer for concrete providers, local storage, HTTP clients and platform integrations.
  Currently includes the deterministic mock CGM provider and local JSON settings storage.

GlucoDesk.Desktop
  Avalonia desktop application.
  Currently includes the initial desktop shell, dashboard view model,
  auto-refresh behavior, lightweight glucose trend chart and a mock-powered dashboard preview.
```

The goal is to keep the domain and application layers independent from concrete providers, storage implementations and UI frameworks.

The current application flow is:

```text
GlucoDesk.Desktop
  -> DashboardView / DashboardViewModel
    -> IGlucoseDataService
      -> ICgmLiveProvider / ICgmHistoricalProvider / ICgmMetadataProvider
        -> Mock / Nightscout / Dexcom
```

The current settings flow is:

```text
GlucoDesk.Desktop
  -> IApplicationSettingsService
    -> IApplicationSettingsStore
      -> JsonApplicationSettingsStore
        -> Local settings.json
```

## Domain model

The current core glucose domain includes:

* `GlucoseValue`
* `GlucoseRange`
* `GlucoseReading`
* `GlucoseUnit`
* `TrendDirection`
* `GlucoseStatus`
* `GlucoseDataFreshness`
* `CgmProviderKind`

The domain is intentionally independent from UI frameworks, external APIs, storage technologies and platform-specific integrations.

## Application model

The current application layer includes:

* `Error`
* `Result`
* `Result<T>`
* `GlucoseReadingsRequest`
* `LatestGlucoseReadingResult`
* `GlucoseReadingsResult`
* `CgmProviderMetadata`
* `GlucoseDashboardRequest`
* `GlucoseDashboardSnapshot`
* `ICgmLiveProvider`
* `ICgmHistoricalProvider`
* `ICgmMetadataProvider`
* `IGlucoseDataService`
* `GlucoseDataService`
* `ApplicationSettings`
* `IApplicationSettingsStore`
* `IApplicationSettingsService`
* `ApplicationSettingsService`
* `ApplicationServiceCollectionExtensions`

These contracts allow GlucoDesk to support multiple CGM data sources without coupling the UI or domain model to a specific provider.

The `IGlucoseDataService` acts as the application-level facade that future UI and reporting layers will use to retrieve:

* Provider metadata.
* Latest glucose reading.
* Recent readings.
* Historical readings.
* Dashboard snapshots.

The settings application layer currently supports:

* Active live provider.
* Active historical provider.
* Preferred glucose unit.
* Target range.
* Dashboard refresh interval.

## Infrastructure model

The current infrastructure layer includes:

* `MockCgmProviderOptions`
* `MockGlucoseReadingGenerator`
* `MockCgmProvider`
* `MockCgmProviderServiceCollectionExtensions`
* `LocalSettingsStorageOptions`
* `JsonApplicationSettingsStore`
* `LocalSettingsServiceCollectionExtensions`

The mock provider implements:

* `ICgmLiveProvider`
* `ICgmHistoricalProvider`
* `ICgmMetadataProvider`

It generates deterministic fake CGM readings that are useful for:

* Local development.
* Automated tests.
* UI development.
* README screenshots.
* Demo videos.
* Future demo mode.

The local JSON settings store implements:

* `IApplicationSettingsStore`

It currently persists non-secret application settings to a local `settings.json` file.

The mock provider is not intended to sit between real providers and the UI. In a real user configuration, GlucoDesk will use Nightscout and/or Dexcom directly through their own provider implementations.

The local JSON settings store is a foundation for user preferences. Future provider credentials and secrets must use a more secure storage strategy.

## Desktop model

The current desktop layer includes:

* `DesktopServiceProviderBuilder`
* `ViewModelBase`
* `MainWindowViewModel`
* `DashboardViewModel`
* `DashboardRefreshOptions`
* `GlucoseChartPoint`
* `GlucoseTrendChart`
* `MainWindow`
* `DashboardView`

The desktop app currently uses the mock CGM provider through the application-level `IGlucoseDataService`.

Current dashboard preview displays:

* Latest glucose value.
* Trend.
* Status.
* Provider name.
* Data freshness.
* Last updated timestamp.
* Recent readings count.
* Auto-refresh status.
* Lightweight recent glucose trend chart.
* Chart summary with reading count and min/max glucose values.
* Error state, when present.

The dashboard currently supports automatic refresh using a UI-thread dispatcher timer.

The current default refresh interval is:

```text
30 seconds
```

The dashboard includes a lightweight custom Avalonia trend chart based on recent glucose readings.

The chart highlights the standard 70-180 mg/dL target range and displays deterministic demo data while the app runs with the mock provider.

The current dashboard uses deterministic demo data and is not intended for treatment decisions.

## Provider strategy

GlucoDesk is designed to support multiple data sources:

```text
Nightscout
  Intended for near real-time glucose visualization.

Dexcom Official API
  Intended for delayed official historical data and metadata.

Mock provider
  Implemented for local development, tests, demos and future demo mode.
```

Expected future runtime behavior:

```text
User configures Nightscout
  GlucoDesk uses the Nightscout provider for live or near real-time readings.

User configures Dexcom Official API
  GlucoDesk uses the Dexcom provider for delayed official historical data and metadata.

User selects demo mode
  GlucoDesk uses the mock provider with deterministic fake glucose data.
```

The application should clearly communicate the freshness and source of each reading.

## Local settings strategy

GlucoDesk now includes a local settings foundation.

Current settings are represented by:

* `ApplicationSettings`

Current settings persistence is provided by:

* `JsonApplicationSettingsStore`

Default local settings path:

```text
%APPDATA%/GlucoDesk/settings.json on Windows-like environments
ApplicationData/GlucoDesk/settings.json on macOS/Linux depending on the runtime environment
```

The local settings foundation is intended for non-secret preferences such as:

* Active provider selection.
* Preferred glucose unit.
* Target range.
* Dashboard refresh interval.

Provider tokens, API secrets and OAuth credentials should not be stored in plain JSON in future production releases.

## Planned reporting and diary features

A future milestone will add local history and monthly diabetes diary exports.

The intended reporting flow is:

```text
Provider data
  -> Normalized local storage
    -> Diary aggregation service
      -> Excel / PDF / CSV export
```

The monthly diary feature should allow users to generate exports such as:

```text
September 2025 diabetes diary
```

Possible report contents:

* Daily glucose summaries.
* Meal windows.
* Pre-meal and post-meal glucose values, when data is available.
* Carbohydrates, when available from treatments/events.
* Insulin doses, when available from treatments/events.
* Notes, when available.
* Time in range.
* Time above range.
* Time below range.
* Average glucose.
* Data source and data completeness information.

This feature will depend on local storage, Nightscout treatments/events and future reporting services.

## Development principles

* Keep the domain model small, explicit and testable.
* Keep provider implementations behind application-level interfaces.
* Avoid coupling UI code to external APIs.
* Prefer local-first behavior.
* Do not log sensitive medical data or secrets.
* Do not store provider secrets in plain text in future production releases.
* Keep directories organized by business area and type.
* Add XML documentation to public contracts and interfaces.
* Keep private helper methods documented and grouped under `#region Helpers`.
* Keep mock/demo data clearly separated from real provider data.
* Keep desktop behavior testable through view models and application services.
* Prefer lightweight UI components before introducing external UI dependencies.

## Running build and tests

From the repository root:

```bash
dotnet restore
dotnet build -c Release
dotnet test -c Release
```

## Running the desktop app

From the repository root:

```bash
dotnet run --project src/GlucoDesk.Desktop/GlucoDesk.Desktop.csproj
```

The current desktop app uses the mock CGM provider and displays deterministic demo glucose data, including the latest value, status, auto-refresh state and a lightweight recent trend chart.

The dashboard refreshes automatically every 30 seconds and can also be refreshed manually.

The application also registers the local JSON settings store, although the settings screen is not implemented yet.

## Roadmap

* v0.1: Mock provider, application glucose data service, desktop shell, auto-refresh dashboard, lightweight trend chart and local settings.
* v0.2: Nightscout live provider.
* v0.3: Analytics engine and compact widget.
* v0.4: Dexcom Official API historical provider.
* v0.5: Local history, treatments/events and monthly diabetes diary export.
* v1.0: Production-ready cross-platform release.

## Medical disclaimer

GlucoDesk is provided for personal visualization, experimentation and software development purposes only.

It is not a medical device and must not be used for:

* Treatment decisions.
* Insulin dosing.
* Emergency alerts.
* Replacing Dexcom, Omnipod, Nightscout or any approved medical application.
* Replacing professional medical advice.

Always use approved medical devices and applications for diabetes management decisions.

## License

MIT
