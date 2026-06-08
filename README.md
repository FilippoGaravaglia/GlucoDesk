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
      History/
        Abstractions/
        Requests/
        Results/
        Services/
          Abstractions/
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
      Events/
      Models/
      Services/

  GlucoDesk.Infrastructure/
    Cgm/
      History/
        DependencyInjection/
        Options/
        Stores/
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
      Settings/
        Selections/
    Views/
      Dashboard/
        Controls/
      Main/
      Settings/

tests/
  GlucoDesk.Core.Tests/
    Glucose/

  GlucoDesk.Application.Tests/
    Cgm/
      Dashboard/
        Requests/
        Results/
      History/
        Requests/
        Results/
        Services/
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
      History/
        DependencyInjection/
        Options/
        Stores/
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
      Main/
      Settings/

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
* Settings-backed dashboard refresh interval.
* Settings-backed glucose target range.
* Dashboard initialization command.
* Settings screen foundation.
* Desktop navigation between dashboard and settings.
* Editable non-secret local settings form.
* In-process settings change notification.
* Live dashboard update after settings save.
* Automatic dashboard timer interval update after settings changes.
* Local glucose history request/result contracts.
* Application-level glucose history service.
* Local JSON glucose history store.
* Dependency injection registration for local glucose history infrastructure.
* Unit tests for application contracts, glucose data service, mock provider options, provider behavior and DI registration.
* Unit tests for dashboard refresh options and dashboard view model behavior.
* Unit tests for dashboard chart point validation.
* Unit tests for application settings, settings service, settings change notifier, local settings options, JSON store and DI registration.
* Unit tests for settings view model load/save behavior.
* Unit tests for glucose history request/result, history service, storage options, JSON history store and DI registration.

## Architecture

GlucoDesk follows a layered architecture:

```text
GlucoDesk.Core
  Pure domain model.
  No dependency on UI, external APIs, storage or infrastructure concerns.

GlucoDesk.Application
  Application contracts, provider abstractions, request/result models, dashboard models,
  history contracts, settings models, in-process settings notifications,
  application-level errors and application services.
  No dependency on concrete CGM providers or storage implementations.

GlucoDesk.Infrastructure
  Implementation layer for concrete providers, local storage, HTTP clients and platform integrations.
  Currently includes the deterministic mock CGM provider, local JSON settings storage
  and local JSON glucose history storage.

GlucoDesk.Desktop
  Avalonia desktop application.
  Currently includes the initial desktop shell, dashboard view model,
  settings view model, auto-refresh behavior, lightweight glucose trend chart,
  settings-backed dashboard configuration, live settings propagation,
  local settings screen and a mock-powered dashboard preview.
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
  -> SettingsView / SettingsViewModel
    -> IApplicationSettingsService
      -> IApplicationSettingsStore
        -> JsonApplicationSettingsStore
          -> Local settings.json
```

The current dashboard configuration flow is:

```text
Local settings.json
  -> JsonApplicationSettingsStore
    -> IApplicationSettingsService
      -> DashboardViewModel
        -> Dashboard refresh interval
        -> Glucose target range
        -> GlucoseTrendChart target range
```

The current live settings propagation flow is:

```text
SettingsViewModel
  -> IApplicationSettingsService.SaveSettingsAsync(...)
    -> IApplicationSettingsStore.SaveAsync(...)
    -> IApplicationSettingsChangeNotifier.NotifySettingsChanged(...)
      -> DashboardViewModel
        -> Apply new refresh interval
        -> Apply new glucose target range
        -> Notify DashboardView about AutoRefreshInterval change
          -> Update DispatcherTimer interval
```

The current local glucose history flow is:

```text
GlucoseReading
  -> IGlucoseHistoryService
    -> IGlucoseHistoryStore
      -> JsonGlucoseHistoryStore
        -> Local glucose-readings.json
```

The local history foundation is not yet connected to automatic dashboard persistence. It is the storage and application-service foundation for future historical analysis, provider reconciliation and diary exports.

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
* `GlucoseHistoryRequest`
* `GlucoseHistoryResult`
* `ICgmLiveProvider`
* `ICgmHistoricalProvider`
* `ICgmMetadataProvider`
* `IGlucoseDataService`
* `IGlucoseHistoryStore`
* `IGlucoseHistoryService`
* `GlucoseDataService`
* `GlucoseHistoryService`
* `ApplicationSettings`
* `ApplicationSettingsChangedEventArgs`
* `IApplicationSettingsStore`
* `IApplicationSettingsService`
* `IApplicationSettingsChangeNotifier`
* `ApplicationSettingsService`
* `ApplicationSettingsChangeNotifier`
* `ApplicationServiceCollectionExtensions`

These contracts allow GlucoDesk to support multiple CGM data sources without coupling the UI or domain model to a specific provider.

The `IGlucoseDataService` acts as the application-level facade that future UI and reporting layers will use to retrieve:

* Provider metadata.
* Latest glucose reading.
* Recent readings.
* Historical readings.
* Dashboard snapshots.

The `IGlucoseHistoryService` acts as the application-level facade that future UI, analytics and reporting layers will use to persist and query local glucose history.

The settings application layer currently supports:

* Active live provider.
* Active historical provider.
* Preferred glucose unit.
* Target range.
* Dashboard refresh interval.
* In-process settings change notifications after successful saves.

The in-process settings change notifier allows desktop view models to react to settings changes without coupling the settings screen directly to the dashboard.

## Infrastructure model

The current infrastructure layer includes:

* `MockCgmProviderOptions`
* `MockGlucoseReadingGenerator`
* `MockCgmProvider`
* `MockCgmProviderServiceCollectionExtensions`
* `LocalSettingsStorageOptions`
* `JsonApplicationSettingsStore`
* `LocalSettingsServiceCollectionExtensions`
* `LocalGlucoseHistoryStorageOptions`
* `JsonGlucoseHistoryStore`
* `LocalGlucoseHistoryServiceCollectionExtensions`

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

The local JSON glucose history store implements:

* `IGlucoseHistoryStore`

It currently persists normalized glucose readings to a local `glucose-readings.json` file and de-duplicates readings by timestamp and provider.

The mock provider is not intended to sit between real providers and the UI. In a real user configuration, GlucoDesk will use Nightscout and/or Dexcom directly through their own provider implementations.

The local JSON settings store is a foundation for user preferences. Future provider credentials and secrets must use a more secure storage strategy.

The local JSON glucose history store is a foundation for personal glucose history. Glucose history contains sensitive health data, so future production releases should evaluate encryption, retention settings, export controls and clear privacy documentation before storing real personal data extensively.

## Desktop model

The current desktop layer includes:

* `DesktopServiceProviderBuilder`
* `ViewModelBase`
* `MainWindowViewModel`
* `DashboardViewModel`
* `SettingsViewModel`
* `DashboardRefreshOptions`
* `ProviderSelectionItem`
* `GlucoseUnitSelectionItem`
* `GlucoseChartPoint`
* `GlucoseTrendChart`
* `MainWindow`
* `DashboardView`
* `SettingsView`

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
* Settings status.
* Configured glucose target range.
* Lightweight recent glucose trend chart.
* Chart summary with reading count and min/max glucose values.
* Error state, when present.

The desktop shell includes a simple navigation area with dashboard and settings sections.

The settings section can load and save non-secret local preferences through the application settings service.

Current settings screen supports editing:

* Active live provider.
* Active historical provider.
* Preferred glucose unit.
* Target low value in mg/dL.
* Target high value in mg/dL.
* Dashboard refresh interval in seconds.

Provider selection currently persists preferences only. Runtime provider switching will be introduced in a future step.

The dashboard supports automatic refresh using a UI-thread dispatcher timer.

The dashboard loads local application settings before starting the automatic refresh timer. The configured dashboard refresh interval and glucose target range are applied to the dashboard view model and chart.

After settings are saved successfully, the dashboard can now receive an in-process settings change notification and update its configuration without restarting the application.

Live settings propagation currently updates:

* Dashboard refresh interval.
* Dashboard auto-refresh status text.
* Lower glucose target.
* Upper glucose target.
* Target range text.
* Chart target range.
* Dashboard settings status text.

The `DashboardView` listens for `AutoRefreshInterval` changes and updates the running dispatcher timer interval accordingly.

The current default refresh interval is:

```text
30 seconds
```

The dashboard includes a lightweight custom Avalonia trend chart based on recent glucose readings.

The chart highlights the configured target range and displays deterministic demo data while the app runs with the mock provider.

The current dashboard and settings screen use deterministic demo/local data and are not intended for treatment decisions.

The local glucose history foundation is currently registered in the application and infrastructure layers, but it is not yet surfaced in the desktop UI.

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

The settings screen currently allows users to edit and save these non-secret preferences.

The dashboard reads local settings during initialization and applies:

* Dashboard refresh interval.
* Lower glucose target.
* Upper glucose target.
* Target range text.
* Chart target range.

After a successful settings save, the application settings service emits an in-process settings change notification.

The dashboard currently reacts to settings changes and applies:

* New dashboard refresh interval.
* New glucose target range.
* Updated target range text.
* Updated chart target range.
* Updated settings status text.

Provider tokens, API secrets and OAuth credentials should not be stored in plain JSON in future production releases.

## Local history strategy

GlucoDesk now includes a local glucose history foundation.

Current local history contracts are represented by:

* `GlucoseHistoryRequest`
* `GlucoseHistoryResult`
* `IGlucoseHistoryStore`
* `IGlucoseHistoryService`

Current local history persistence is provided by:

* `JsonGlucoseHistoryStore`

Default local glucose history path:

```text
LocalApplicationData/GlucoDesk/history/glucose-readings.json depending on the runtime environment
```

The local history layer is intended to support future features such as:

* Historical glucose exploration.
* Monthly diabetes diary exports.
* Data completeness checks.
* Provider reconciliation.
* Pattern analytics.
* Time-in-range calculations over locally cached data.

The JSON history store currently persists normalized glucose readings locally and de-duplicates readings by timestamp and provider.

The current history store can:

* Save glucose readings.
* Merge new readings with existing readings.
* De-duplicate readings by timestamp and provider.
* Query readings by date range.
* Return empty results when no history file exists.
* Return application-level errors when the history file contains invalid JSON or invalid glucose readings.

Glucose history contains sensitive health data. Future production releases should evaluate encryption, retention settings, export controls and clear privacy documentation before using real personal data extensively.

## Planned reporting and diary features

A future milestone will add local history UI, treatments/events and monthly diabetes diary exports.

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
* Local history completeness information.

This feature will depend on local storage, Nightscout treatments/events and future reporting services.

## Development principles

* Keep the domain model small, explicit and testable.
* Keep provider implementations behind application-level interfaces.
* Avoid coupling UI code to external APIs.
* Prefer local-first behavior.
* Do not log sensitive medical data or secrets.
* Do not store provider secrets in plain text in future production releases.
* Treat glucose history as sensitive health data.
* Keep directories organized by business area and type.
* Add XML documentation to public contracts and interfaces.
* Keep private helper methods documented and grouped under `#region Helpers`.
* Keep mock/demo data clearly separated from real provider data.
* Keep desktop behavior testable through view models and application services.
* Prefer lightweight UI components before introducing external UI dependencies.
* Use in-process events only for local application coordination, not for cross-process or external integration.

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

The current desktop app uses the mock CGM provider and displays deterministic demo glucose data, including the latest value, status, auto-refresh state, settings status and a lightweight recent trend chart.

The dashboard refreshes automatically using the configured local settings interval and can also be refreshed manually.

The application includes a settings screen for editing non-secret local preferences.

After saving settings, the dashboard can update its target range and refresh interval without restarting the app.

The local glucose history foundation is registered, but the UI does not yet automatically persist dashboard readings into history.

## Roadmap

* v0.1: Mock provider, application glucose data service, desktop shell, auto-refresh dashboard, lightweight trend chart, local settings, settings screen, live settings propagation and local glucose history foundation.
* v0.2: Nightscout live provider.
* v0.3: Analytics engine and compact widget.
* v0.4: Dexcom Official API historical provider.
* v0.5: Local history UI, treatments/events and monthly diabetes diary export.
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
