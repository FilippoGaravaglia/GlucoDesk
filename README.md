# GlucoDesk

GlucoDesk is a local-first, cross-platform desktop companion for visualizing CGM glucose data while working at your computer.

The project is designed around a provider-based architecture:

* Mock provider for local development, tests, demos and future demo mode.
* Dexcom Official API provider for delayed official historical data and metadata.
* Nightscout provider for future near real-time CGM visualization.

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
* Future support for Dexcom Official API authorization and historical glucose data import.

## Tech stack

* .NET 10
* Avalonia UI
* CommunityToolkit.Mvvm
* xUnit
* Microsoft.Extensions.DependencyInjection
* Microsoft.Extensions.Http
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
        Analytics/
          Requests/
          Results/
          Services/
            Abstractions/
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
      Dexcom/
        Authorization/
          Browsers/
          Callbacks/
          Listeners/
          Sessions/
          States/
        DependencyInjection/
        Egvs/
          Clients/
          Dtos/
          Mappers/
          Requests/
        Endpoints/
        Enums/
        Options/
        Tokens/
          Clients/
          Dtos/
          Models/
          Requests/
          Services/
          Stores/
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
        Analytics/
          Requests/
          Results/
          Services/
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
      Dexcom/
        Authorization/
          Browsers/
          Callbacks/
          Listeners/
          Sessions/
          States/
        DependencyInjection/
        Egvs/
          Clients/
          Dtos/
          Mappers/
          Requests/
        Endpoints/
        Options/
        Tokens/
          Clients/
          Models/
          Requests/
          Services/
          Stores/
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
* Dashboard glucose history persistence.
* Non-blocking local history cache update after dashboard refresh.
* Dashboard history status text.
* Local glucose history analytics request/result contracts.
* Application-level glucose history analytics service.
* Summary analytics over cached glucose history.
* Dexcom Official API environment model.
* Dexcom Official API options model.
* Dexcom Official API endpoint resolution.
* Dexcom OAuth authorization request model.
* Dexcom OAuth authorization URL builder.
* Dexcom OAuth token set model.
* Dexcom authorization code token exchange request model.
* Dexcom refresh token request model.
* Dexcom OAuth token response DTO.
* Dexcom OAuth token client foundation.
* Dexcom OAuth authorization code exchange client foundation.
* Dexcom OAuth refresh token client foundation.
* Dexcom OAuth state generation options.
* Secure Dexcom OAuth state generator.
* Dexcom OAuth callback validation result model.
* Dexcom OAuth callback parser.
* Dexcom OAuth callback state validation.
* Dexcom OAuth error callback handling.
* Dexcom local OAuth callback listener options.
* Dexcom local OAuth callback listener request/result models.
* Dexcom local OAuth callback listener foundation.
* Browser response handling for Dexcom OAuth callback completion.
* Callback path validation for Dexcom local OAuth redirects.
* Dexcom system browser abstraction.
* Dexcom OAuth authorization session request/result models.
* Dexcom OAuth authorization session service.
* End-to-end Dexcom OAuth session orchestration foundation.
* Dexcom OAuth token store abstraction.
* In-memory Dexcom OAuth token store.
* Dexcom authorization session token persistence into the configured token store.
* Dexcom OAuth token refresh options.
* Dexcom OAuth token refresh request model.
* Dexcom access token result model.
* Dexcom OAuth token service.
* On-demand access token refresh when the stored token is expired or close to expiration.
* Forced Dexcom access token refresh.
* Refreshed Dexcom token persistence into the configured token store.
* Dexcom EGV request model.
* Dexcom EGV response DTOs.
* Dexcom EGV HTTP client abstraction.
* Dexcom EGV HTTP client foundation.
* Authorized Dexcom EGV API request execution through `IDexcomOAuthTokenService`.
* HTTP-level Dexcom EGV response handling.
* Dexcom EGV mapper abstraction.
* Dexcom EGV mapper foundation.
* Mapping from Dexcom EGV records to normalized `GlucoseReading` values.
* Dexcom EGV timestamp mapping from `systemTime` to UTC `DateTimeOffset`.
* Dexcom glucose value and unit mapping to `GlucoseValue`.
* Dexcom trend mapping to `TrendDirection`.
* Dexcom provider mapping to `CgmProviderKind`.
* Dexcom freshness mapping to delayed glucose data.
* Dexcom device metadata mapping.
* Dependency injection registration for Dexcom Official API infrastructure.
* Typed HTTP client registration for Dexcom OAuth token operations.
* Typed HTTP client registration for Dexcom EGV operations.
* Unit tests for application contracts, glucose data service, mock provider options, provider behavior and DI registration.
* Unit tests for dashboard refresh options and dashboard view model behavior.
* Unit tests for dashboard chart point validation.
* Unit tests for application settings, settings service, settings change notifier, local settings options, JSON store and DI registration.
* Unit tests for settings view model load/save behavior.
* Unit tests for glucose history request/result, history service, storage options, JSON history store and DI registration.
* Unit tests for dashboard-to-history persistence behavior.
* Unit tests for glucose history analytics request/result and service behavior.
* Unit tests for Dexcom API options, endpoint resolution, authorization request validation, authorization URL generation and DI registration.
* Unit tests for Dexcom token models, token requests and token client behavior.
* Unit tests for Dexcom OAuth state generation and callback parsing.
* Unit tests for Dexcom local OAuth callback listener options, request/result models and listener behavior.
* Unit tests for Dexcom OAuth authorization session behavior.
* Unit tests for Dexcom OAuth token store behavior.
* Unit tests for Dexcom OAuth token refresh service behavior.
* Unit tests for Dexcom EGV request validation, DTO deserialization and HTTP client behavior.
* Unit tests for Dexcom EGV mapper behavior.

## Architecture

GlucoDesk follows a layered architecture:

```text
GlucoDesk.Core
  Pure domain model.
  No dependency on UI, external APIs, storage or infrastructure concerns.

GlucoDesk.Application
  Application contracts, provider abstractions, request/result models, dashboard models,
  history contracts, history analytics contracts, settings models,
  in-process settings notifications, application-level errors and application services.
  No dependency on concrete CGM providers or storage implementations.

GlucoDesk.Infrastructure
  Implementation layer for concrete providers, local storage, HTTP clients and platform integrations.
  Currently includes the deterministic mock CGM provider, local JSON settings storage,
  local JSON glucose history storage and the first Dexcom Official API infrastructure foundation.

GlucoDesk.Desktop
  Avalonia desktop application.
  Currently includes the initial desktop shell, dashboard view model,
  settings view model, auto-refresh behavior, lightweight glucose trend chart,
  settings-backed dashboard configuration, live settings propagation,
  local settings screen, dashboard-to-history persistence and a mock-powered dashboard preview.
```

The goal is to keep the domain and application layers independent from concrete providers, storage implementations and UI frameworks.

The current application flow is:

```text
GlucoDesk.Desktop
  -> DashboardView / DashboardViewModel
    -> IGlucoseDataService
      -> ICgmLiveProvider / ICgmHistoricalProvider / ICgmMetadataProvider
        -> Mock / Dexcom / Nightscout
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
Dashboard refresh
  -> IGlucoseDataService.GetDashboardSnapshotAsync(...)
    -> DashboardViewModel
      -> IGlucoseHistoryService.SaveReadingsAsync(...)
        -> IGlucoseHistoryStore
          -> JsonGlucoseHistoryStore
            -> Local glucose-readings.json
```

After every successful dashboard refresh, GlucoDesk attempts to cache the latest and recent glucose readings into local history. History persistence is intentionally non-blocking from a product perspective: a history save failure updates the dashboard history status text, but it does not fail the dashboard refresh.

The current local analytics flow is:

```text
Local glucose history
  -> IGlucoseHistoryAnalyticsService.GetSummaryAsync(...)
    -> IGlucoseHistoryService.GetReadingsAsync(...)
      -> IGlucoseHistoryStore
        -> JsonGlucoseHistoryStore
          -> Local glucose-readings.json
    -> GlucoseHistorySummaryResult
```

The analytics layer reads from the application-level history service and remains independent from the concrete storage implementation.

The current Dexcom Official API authorization foundation flow is:

```text
DexcomApiEnvironment
  -> IDexcomApiEndpointProvider
    -> DexcomApiEndpoints

DexcomAuthorizationRequest
  -> IDexcomAuthorizationUrlBuilder
    -> Dexcom OAuth authorization URI
```

The current Dexcom OAuth token client foundation flow is:

```text
DexcomAuthorizationCodeTokenRequest
  -> IDexcomTokenClient.ExchangeAuthorizationCodeAsync(...)
    -> Dexcom /v3/oauth2/token
      -> DexcomOAuthTokenSet

DexcomRefreshTokenRequest
  -> IDexcomTokenClient.RefreshAccessTokenAsync(...)
    -> Dexcom /v3/oauth2/token
      -> DexcomOAuthTokenSet
```

The current Dexcom OAuth callback foundation flow is:

```text
IDexcomOAuthStateGenerator
  -> Generate secure state
    -> DexcomAuthorizationRequest.State
      -> Dexcom authorization URL

Dexcom callback URI
  -> IDexcomOAuthCallbackParser.ParseCallback(...)
    -> Validate authorization code
    -> Validate returned state
    -> Handle OAuth error callbacks
    -> DexcomOAuthCallbackResult
```

The current Dexcom local OAuth callback listener foundation flow is:

```text
DexcomLocalOAuthCallbackListenRequest
  -> IDexcomLocalOAuthCallbackListener.ListenForCallbackAsync(...)
    -> Local loopback HttpListener
      -> Receive browser redirect
      -> Validate callback path
      -> IDexcomOAuthCallbackParser.ParseCallback(...)
        -> Validate authorization code
        -> Validate returned state
        -> Handle OAuth error callbacks
      -> Browser success/failure response
      -> DexcomLocalOAuthCallbackListenResult
```

The current Dexcom OAuth authorization session foundation flow is:

```text
DexcomOAuthAuthorizationSessionRequest
  -> IDexcomOAuthAuthorizationSessionService.StartAuthorizationSessionAsync(...)
    -> IDexcomOAuthStateGenerator.GenerateState()
      -> IDexcomAuthorizationUrlBuilder.BuildAuthorizationUri(...)
        -> IDexcomSystemBrowser.OpenAsync(...)
          -> IDexcomLocalOAuthCallbackListener.ListenForCallbackAsync(...)
            -> Validate callback path
            -> Validate OAuth state
            -> Extract authorization code
              -> IDexcomTokenClient.ExchangeAuthorizationCodeAsync(...)
                -> DexcomOAuthTokenSet
                  -> IDexcomOAuthTokenStore.SaveTokenSetAsync(...)
                    -> DexcomOAuthAuthorizationSessionResult
```

The current Dexcom OAuth token store foundation flow is:

```text
IDexcomOAuthTokenStore
  -> SaveTokenSetAsync(...)
  -> GetTokenSetAsync(...)
  -> HasTokenSetAsync(...)
  -> ClearTokenSetAsync(...)

InMemoryDexcomOAuthTokenStore
  -> Stores tokens only for the current application process
  -> Does not persist tokens to disk
  -> Intended as a safe foundation before platform-secure persistent storage
```

The current Dexcom OAuth token refresh foundation flow is:

```text
IDexcomOAuthTokenService
  -> GetValidAccessTokenAsync(...)

DexcomOAuthTokenService
  -> IDexcomOAuthTokenStore.GetTokenSetAsync(...)
    -> Check access token expiration against RefreshSafetyWindow
      -> Return stored token when still usable
      -> Or refresh through IDexcomTokenClient.RefreshAccessTokenAsync(...)
        -> IDexcomOAuthTokenStore.SaveTokenSetAsync(...)
          -> DexcomAccessTokenResult
```

The current Dexcom EGV HTTP client foundation flow is:

```text
DexcomEgvRequest
  -> IDexcomEgvClient.GetEgvsAsync(...)
    -> IDexcomOAuthTokenService.GetValidAccessTokenAsync(...)
      -> DexcomAccessTokenResult
        -> Build authorized GET /v3/users/self/egvs request
          -> Execute HTTP request
            -> Deserialize DexcomEgvResponseDto
              -> Return raw Dexcom EGV response DTO
```

The current Dexcom EGV mapper foundation flow is:

```text
DexcomEgvResponseDto
  -> IDexcomEgvMapper.MapResponse(...)
    -> For each DexcomEgvRecordDto
      -> Parse systemTime as UTC DateTimeOffset
      -> Map value and unit to GlucoseValue
      -> Map trend to TrendDirection
      -> Map Dexcom environment to CgmProviderKind
      -> Mark freshness as Delayed
      -> Build optional device metadata
      -> Return normalized GlucoseReading
```

The Dexcom foundation can now build authorization URLs, generate secure OAuth state values, open an authorization URI through a browser abstraction, listen for a local loopback OAuth redirect, parse Dexcom OAuth callbacks, validate returned state values, exchange authorization codes for tokens through the token client foundation, save the resulting token set into the configured token store, retrieve a valid access token by refreshing the stored token set when necessary, execute authorized Dexcom EGV HTTP requests and map raw Dexcom EGV records into normalized GlucoDesk domain readings.

It does not yet store OAuth tokens in platform-secure persistent storage, restore tokens after application restart, expose a Dexcom provider through the application-level CGM provider abstractions, switch the runtime dashboard provider from Mock to Dexcom or surface Dexcom connection actions in the desktop UI.

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
* `GlucoseHistorySummaryRequest`
* `GlucoseHistorySummaryResult`
* `ICgmLiveProvider`
* `ICgmHistoricalProvider`
* `ICgmMetadataProvider`
* `IGlucoseDataService`
* `IGlucoseHistoryStore`
* `IGlucoseHistoryService`
* `IGlucoseHistoryAnalyticsService`
* `GlucoseDataService`
* `GlucoseHistoryService`
* `GlucoseHistoryAnalyticsService`
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

The `IGlucoseHistoryAnalyticsService` acts as the application-level facade that future UI and reporting layers will use to calculate summaries over locally cached glucose history.

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
* `DexcomApiEnvironment`
* `DexcomApiOptions`
* `DexcomApiEndpoints`
* `IDexcomApiEndpointProvider`
* `DexcomApiEndpointProvider`
* `DexcomAuthorizationRequest`
* `IDexcomAuthorizationUrlBuilder`
* `DexcomAuthorizationUrlBuilder`
* `DexcomOAuthTokenSet`
* `DexcomAuthorizationCodeTokenRequest`
* `DexcomRefreshTokenRequest`
* `IDexcomTokenClient`
* `DexcomTokenClient`
* `DexcomOAuthStateOptions`
* `IDexcomOAuthStateGenerator`
* `DexcomOAuthStateGenerator`
* `DexcomOAuthCallbackResult`
* `IDexcomOAuthCallbackParser`
* `DexcomOAuthCallbackParser`
* `DexcomLocalOAuthCallbackOptions`
* `DexcomLocalOAuthCallbackListenRequest`
* `DexcomLocalOAuthCallbackListenResult`
* `IDexcomLocalOAuthCallbackListener`
* `DexcomLocalOAuthCallbackListener`
* `IDexcomSystemBrowser`
* `DexcomSystemBrowser`
* `DexcomOAuthAuthorizationSessionRequest`
* `DexcomOAuthAuthorizationSessionResult`
* `IDexcomOAuthAuthorizationSessionService`
* `DexcomOAuthAuthorizationSessionService`
* `IDexcomOAuthTokenStore`
* `InMemoryDexcomOAuthTokenStore`
* `DexcomOAuthTokenRefreshOptions`
* `DexcomOAuthTokenRefreshRequest`
* `DexcomAccessTokenResult`
* `IDexcomOAuthTokenService`
* `DexcomOAuthTokenService`
* `DexcomEgvRequest`
* `DexcomEgvResponseDto`
* `DexcomEgvRecordDto`
* `IDexcomEgvClient`
* `DexcomEgvClient`
* `IDexcomEgvMapper`
* `DexcomEgvMapper`
* `DexcomOfficialApiServiceCollectionExtensions`

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

The Dexcom Official API foundation currently provides:

* Supported Dexcom API environment modeling.
* Environment-specific endpoint resolution.
* OAuth authorization request modeling.
* OAuth authorization URL generation.
* OAuth authorization code token exchange client foundation.
* OAuth refresh token client foundation.
* Secure OAuth state generation.
* OAuth callback parsing.
* OAuth callback state validation.
* OAuth error callback handling.
* Local loopback OAuth callback listening.
* Callback path validation.
* Browser success/failure response rendering after OAuth callback.
* System browser opening abstraction.
* OAuth authorization session orchestration.
* Coordination between state generation, authorization URL generation, browser opening, local callback listening and token exchange.
* OAuth token store abstraction.
* In-memory token storage for the current application process.
* Token persistence after successful Dexcom OAuth authorization sessions.
* Access token retrieval through a token service.
* On-demand refresh of stored access tokens when they are expired or close to expiration.
* Forced access token refresh.
* Persistence of refreshed token sets into the configured token store.
* EGV date range request validation.
* Dexcom EGV response DTOs.
* Authorized EGV API calls through `IDexcomOAuthTokenService`.
* HTTP-level Dexcom EGV response handling.
* Mapping raw Dexcom EGV records into normalized GlucoDesk domain readings.
* Dexcom trend mapping to `TrendDirection`.
* Dexcom provider mapping to `CgmProviderKind`.
* Dexcom unit mapping to `GlucoseUnit`.
* Dexcom timestamp mapping from `systemTime` to UTC `DateTimeOffset`.
* Dexcom device metadata mapping.
* Dependency injection registration for Dexcom infrastructure services.
* Typed HTTP client registration for Dexcom token operations.
* Typed HTTP client registration for Dexcom EGV operations.

The mock provider is not intended to sit between real providers and the UI. In a real user configuration, GlucoDesk will use Dexcom and/or Nightscout directly through their own provider implementations.

The local JSON settings store is a foundation for user preferences. Future provider credentials and secrets must use a more secure storage strategy.

The local JSON glucose history store is a foundation for personal glucose history. Glucose history contains sensitive health data, so future production releases should evaluate encryption, retention settings, export controls and clear privacy documentation before storing real personal data extensively.

The current Dexcom OAuth token store abstraction is intentionally backed by an in-memory implementation only. This prevents writing access tokens and refresh tokens to disk in plain text while the secure persistent storage strategy is still being designed.

Future persistent Dexcom OAuth token storage should use platform-secure storage, such as macOS Keychain, Windows Credential Manager, Linux Secret Service or an equivalent secure mechanism.

The Dexcom OAuth token service is intended to be the only infrastructure component used by Dexcom API clients when they need an access token. EGV clients request a valid token from `IDexcomOAuthTokenService` instead of reading directly from the token store.

The Dexcom EGV client returns raw Dexcom DTOs. The Dexcom EGV mapper is responsible for converting those raw DTOs into normalized `GlucoseReading` values while keeping timestamp parsing, trend mapping, status handling and unit normalization independently testable.

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
* History status.
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

After settings are saved successfully, the dashboard can receive an in-process settings change notification and update its configuration without restarting the application.

Live settings propagation currently updates:

* Dashboard refresh interval.
* Dashboard auto-refresh status text.
* Lower glucose target.
* Upper glucose target.
* Target range text.
* Chart target range.
* Dashboard settings status text.

The `DashboardView` listens for `AutoRefreshInterval` changes and updates the running dispatcher timer interval accordingly.

After every successful dashboard refresh, the dashboard attempts to persist the latest and recent glucose readings into local glucose history. History persistence is non-blocking from a product perspective: a history save failure updates the dashboard history status text but does not fail the dashboard refresh.

The dashboard history status text currently communicates whether local history was updated, disabled, had no readings to cache, or failed with an application-level error code.

The current default refresh interval is:

```text
30 seconds
```

The dashboard includes a lightweight custom Avalonia trend chart based on recent glucose readings.

The chart highlights the configured target range and displays deterministic demo data while the app runs with the mock provider.

The current dashboard and settings screen use deterministic demo/local data and are not intended for treatment decisions.

The Dexcom Official API foundation is currently available at infrastructure-service level and is not yet surfaced in the desktop UI.

## Provider strategy

GlucoDesk is designed to support multiple data sources:

```text
Mock provider
  Implemented for local development, tests, demos and future demo mode.

Dexcom Official API
  Intended for delayed official historical glucose data, metadata and future diary/report features.

Nightscout
  Intended for future near real-time glucose visualization when a user already has a Nightscout setup.
```

Expected future runtime behavior:

```text
User selects demo mode
  GlucoDesk uses the mock provider with deterministic fake glucose data.

User configures Dexcom Official API
  GlucoDesk uses the Dexcom provider for delayed official historical data and metadata.

User configures Nightscout
  GlucoDesk uses the Nightscout provider for live or near real-time readings.
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

GlucoDesk now includes a local glucose history foundation connected to dashboard refreshes.

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

The dashboard currently attempts to persist the latest and recent readings into local history after every successful refresh. Readings are de-duplicated before being sent to the history service, and the history store also performs storage-level de-duplication.

The current history store can:

* Save glucose readings.
* Merge new readings with existing readings.
* De-duplicate readings by timestamp and provider.
* Query readings by date range.
* Return empty results when no history file exists.
* Return application-level errors when the history file contains invalid JSON or invalid glucose readings.

History persistence failures do not break the dashboard refresh flow. They are surfaced through the dashboard history status text.

Glucose history contains sensitive health data. Future production releases should evaluate encryption, retention settings, export controls and clear privacy documentation before using real personal data extensively.

## Local analytics strategy

GlucoDesk now includes a local glucose history analytics foundation.

Current analytics contracts are represented by:

* `GlucoseHistorySummaryRequest`
* `GlucoseHistorySummaryResult`
* `IGlucoseHistoryAnalyticsService`

Current analytics implementation is provided by:

* `GlucoseHistoryAnalyticsService`

Current analytics can calculate summary information over locally cached glucose readings:

* Readings count.
* Average glucose in mg/dL.
* Minimum glucose in mg/dL.
* Maximum glucose in mg/dL.
* In-range readings count and percentage.
* Below-range readings count and percentage.
* Above-range readings count and percentage.

The analytics layer currently reads from `IGlucoseHistoryService`, so it remains independent from the concrete storage implementation.

This foundation will support future features such as:

* History UI.
* Time-in-range reporting.
* Monthly diabetes diary exports.
* Pattern analytics.
* Data completeness reports.
* Provider reconciliation summaries.

Analytics are currently descriptive only and must not be interpreted as treatment recommendations.

## Dexcom Official API strategy

GlucoDesk now includes the first Dexcom Official API foundation.

Current Dexcom infrastructure supports:

* Sandbox endpoint resolution.
* Production US endpoint resolution.
* Production EU endpoint resolution.
* Production Japan endpoint resolution.
* OAuth authorization request modeling.
* OAuth authorization URL generation.
* OAuth authorization code token exchange client foundation.
* OAuth refresh token client foundation.
* Secure OAuth state generation.
* OAuth callback parsing.
* OAuth callback state validation.
* OAuth error callback handling.
* Local loopback OAuth callback listening.
* Callback path validation.
* Browser success/failure response rendering after OAuth callback.
* System browser opening abstraction.
* OAuth authorization session orchestration.
* Coordination between state generation, authorization URL generation, browser opening, local callback listening and token exchange.
* OAuth token store abstraction.
* In-memory token storage for the current application process.
* Token persistence after successful Dexcom OAuth authorization sessions.
* Access token retrieval through a token service.
* On-demand refresh of stored access tokens when they are expired or close to expiration.
* Forced access token refresh.
* Persistence of refreshed token sets into the configured token store.
* EGV date range request validation.
* Dexcom EGV response DTOs.
* Authorized EGV API calls through `IDexcomOAuthTokenService`.
* HTTP-level Dexcom EGV response handling.
* Mapping raw Dexcom EGV records into normalized GlucoDesk domain readings.
* Dexcom trend mapping to `TrendDirection`.
* Dexcom provider mapping to `CgmProviderKind`.
* Dexcom unit mapping to `GlucoseUnit`.
* Dexcom timestamp mapping from `systemTime` to UTC `DateTimeOffset`.
* Dexcom device metadata mapping.
* Dependency injection registration for Dexcom Official API infrastructure.
* Typed HTTP client registration for Dexcom OAuth token operations.
* Typed HTTP client registration for Dexcom EGV operations.

The current Dexcom foundation does not yet:

* Store OAuth tokens in platform-secure persistent storage.
* Restore tokens after application restart.
* Expose a Dexcom provider through the application-level CGM provider abstractions.
* Switch the runtime dashboard provider from Mock to Dexcom.
* Surface Dexcom connection actions in the desktop UI.

The next Dexcom steps will introduce:

* Dexcom historical provider implementation.
* Runtime provider switching from Mock to Dexcom Official.
* Secure persistent token storage strategy.
* Desktop UI actions for connecting and disconnecting Dexcom.

Dexcom Official API data is intended to be treated as delayed official historical data and metadata, not as a replacement for approved real-time diabetes applications.

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

This feature will depend on local storage, Dexcom Official API data, optional Nightscout treatments/events and future reporting services.

## Development principles

* Keep the domain model small, explicit and testable.
* Keep provider implementations behind application-level interfaces.
* Avoid coupling UI code to external APIs.
* Prefer local-first behavior.
* Do not log sensitive medical data or secrets.
* Do not store provider secrets in plain text in future production releases.
* Treat glucose history as sensitive health data.
* Treat OAuth tokens as sensitive secrets.
* Treat Dexcom client secrets as sensitive secrets.
* Validate OAuth state values to protect against callback tampering and CSRF-style flows.
* Use local OAuth callback listeners only on loopback HTTP redirect URIs.
* Keep OAuth authorization session orchestration testable through abstractions.
* Keep token storage behind an abstraction.
* Never persist OAuth access tokens or refresh tokens to plain JSON.
* Prefer platform-secure storage for future persistent token storage.
* Retrieve Dexcom access tokens through `IDexcomOAuthTokenService` instead of reading directly from the token store.
* Keep Dexcom EGV HTTP access behind `IDexcomEgvClient`.
* Keep Dexcom DTO parsing separate from domain mapping.
* Keep Dexcom EGV mapping behind `IDexcomEgvMapper`.
* Keep directories organized by business area and type.
* Add XML documentation to public contracts and interfaces.
* Keep private helper methods documented and grouped under `#region Helpers`.
* Keep mock/demo data clearly separated from real provider data.
* Keep desktop behavior testable through view models and application services.
* Prefer lightweight UI components before introducing external UI dependencies.
* Use in-process events only for local application coordination, not for cross-process or external integration.
* Keep persistence failures non-blocking when they should not break the user-facing dashboard experience.
* Keep analytics descriptive and avoid treatment-oriented recommendations.

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

The current desktop app uses the mock CGM provider and displays deterministic demo glucose data, including the latest value, status, auto-refresh state, settings status, history status and a lightweight recent trend chart.

The dashboard refreshes automatically using the configured local settings interval and can also be refreshed manually.

The application includes a settings screen for editing non-secret local preferences.

After saving settings, the dashboard can update its target range and refresh interval without restarting the app.

After every successful dashboard refresh, the app attempts to cache the latest and recent readings into local glucose history.

The local analytics foundation is currently available at application-service level and is not yet surfaced in the desktop UI.

The Dexcom Official API foundation is currently available at infrastructure-service level and is not yet surfaced in the desktop UI.

## Roadmap

* v0.1: Mock provider, application glucose data service, desktop shell, auto-refresh dashboard, lightweight trend chart, local settings, settings screen, live settings propagation, local glucose history foundation, dashboard-to-history persistence, local history analytics foundation and Dexcom Official API foundation.
* v0.2: Dexcom Official API OAuth session coordination, token storage strategy, token refresh service, EGV HTTP client, EGV mapper, delayed historical glucose provider and runtime provider switching.
* v0.3: History UI, reporting foundation and compact widget.
* v0.4: Nightscout provider for users who already have a Nightscout setup.
* v0.5: Treatments/events, local history UI and monthly diabetes diary export.
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
