# GlucoDesk

GlucoDesk is a local-first, cross-platform desktop companion for visualizing CGM glucose data while working at your computer.

The project is designed around a provider-based architecture:

* Mock provider for local development, automated tests, demos and future demo mode.
* Dexcom Official API provider for delayed official glucose data, historical readings and metadata.
* Nightscout provider foundation for near real-time CGM visualization through a third-party Nightscout setup.

> GlucoDesk is not a medical device. It must not be used for treatment decisions, insulin dosing, emergency alerts, or as a replacement for Dexcom, Omnipod, Nightscout, or any approved medical application.

## Goals

* Cross-platform desktop application for Windows, macOS and Linux.
* Local-first and privacy-first architecture.
* Provider-based CGM data access.
* Clear runtime separation between demo data and real provider data.
* Clean .NET architecture.
* Useful desktop dashboard and future compact widget.
* Open-source friendly design.
* Clear separation between domain, application, infrastructure and UI concerns.
* Support for fake demo data without exposing real personal glucose data.
* Support for delayed official Dexcom data through the Dexcom Official API.
* Foundation for near real-time third-party CGM visualization through Nightscout.
* Future support for local history, diabetes diary exports and reporting.

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
        Resolution/
          Abstractions/
          Models/
          Services/
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
        Connection/
          DependencyInjection/
          Enums/
          Models/
          Services/
        DependencyInjection/
        Egvs/
          Clients/
          Dtos/
          Mappers/
          Requests/
        Endpoints/
        Enums/
        Options/
        Providers/
          DependencyInjection/
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
      Nightscout/
        Clients/
        DependencyInjection/
        Dtos/
        Enums/
        Mappers/
        Options/
        Providers/
        Requests/
    Settings/
      DependencyInjection/
      Options/
      Stores/

  GlucoDesk.Desktop/
    Bootstrap/
      Providers/
        Connection/
          Models/
          Services/
        DependencyInjection/
        Options/
    ViewModels/
      Common/
      Dashboard/
        Chart/
        Errors/
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
        Resolution/
          Models/
          Services/
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
        Connection/
          DependencyInjection/
          Enums/
          Models/
          Services/
        DependencyInjection/
        Egvs/
          Clients/
          Dtos/
          Mappers/
          Requests/
        Endpoints/
        Options/
        Providers/
          DependencyInjection/
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
      Nightscout/
        Clients/
        DependencyInjection/
        Dtos/
        Mappers/
        Options/
        Providers/
        Requests/
    Settings/
      DependencyInjection/
      Options/
      Stores/

  GlucoDesk.Desktop.Tests/
    Bootstrap/
      Providers/
        Connection/
          Models/
          Services/
        DependencyInjection/
        Options/
    ViewModels/
      Dashboard/
        Chart/
        Errors/
        Options/
      Main/
      Settings/
        Selections/

docs/
  architecture-decisions/

build/
```

## Current status

The project is currently in an advanced foundation phase.

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
* Initial dashboard shell.
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
* Runtime CGM provider resolver abstraction.
* Runtime CGM provider resolver implementation.
* Active live provider selection from local application settings.
* Active historical provider selection from local application settings.
* Safe fallback to Mock provider when the selected provider is unavailable.
* Glucose data service integration with runtime provider resolution.
* Desktop CGM provider switching foundation.
* Desktop provider registration with Mock always available.
* Optional Dexcom provider registration through environment variables.
* Desktop Dexcom provider bootstrap options.
* Settings provider availability foundation.
* Provider availability status in the settings screen.
* Unavailable provider display labels in settings.
* Save-time validation that prevents selecting unavailable CGM providers.
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
* Specific Dexcom EGV HTTP error mapping for unauthorized, forbidden, rate-limited and server-side responses.
* Dexcom EGV mapper abstraction.
* Dexcom EGV mapper foundation.
* Mapping from Dexcom EGV records to normalized `GlucoseReading` values.
* Dexcom EGV timestamp mapping from `systemTime` to UTC `DateTimeOffset`.
* Dexcom glucose value and unit mapping to `GlucoseValue`.
* Dexcom trend mapping to `TrendDirection`.
* Dexcom provider mapping to `CgmProviderKind`.
* Dexcom freshness mapping to delayed glucose data.
* Dexcom device metadata mapping.
* Dexcom Official CGM provider options.
* Dexcom Official CGM provider foundation.
* Dexcom Official CGM provider dependency injection registration.
* Dexcom Official provider metadata support.
* Dexcom latest delayed reading retrieval foundation.
* Dexcom recent delayed readings retrieval foundation.
* Dexcom historical readings retrieval foundation.
* Dependency injection registration for Dexcom Official API infrastructure.
* Typed HTTP client registration for Dexcom OAuth token operations.
* Typed HTTP client registration for Dexcom EGV operations.
* Dexcom connection status foundation.
* Dexcom connection state model.
* Dexcom connection status service.
* Dexcom token store inspection for connection status.
* Settings screen Dexcom connection status text.
* Dexcom connect action foundation.
* Desktop Dexcom connection service.
* Settings screen Connect Dexcom command.
* Conditional Connect Dexcom button visibility.
* Dexcom active provider selection after successful connection.
* Automatic selection of Dexcom as live and historical provider after successful OAuth connection.
* Automatic persistence of Dexcom provider preferences after successful connection.
* Dashboard provider error presentation foundation.
* User-facing dashboard error mapping for Dexcom and provider failures.
* Dashboard data source status text.
* Nightscout authentication mode model.
* Nightscout provider options model.
* Nightscout entries request model.
* Nightscout entries DTO.
* Nightscout entries HTTP client foundation.
* Nightscout entry mapper foundation.
* Mapping from Nightscout entries to normalized `GlucoseReading` values.
* Nightscout trend direction mapping to `TrendDirection`.
* Nightscout provider mapping to `CgmProviderKind.Nightscout`.
* Nightscout freshness mapping to near real-time glucose data.
* Nightscout CGM provider foundation.
* Nightscout CGM provider dependency injection registration.
* Unit tests for application contracts, glucose data service, mock provider options, provider behavior and DI registration.
* Unit tests for dashboard refresh options and dashboard view model behavior.
* Unit tests for dashboard chart point validation.
* Unit tests for dashboard refresh error presentation.
* Unit tests for application settings, settings service, settings change notifier, local settings options, JSON store and DI registration.
* Unit tests for settings view model load/save behavior.
* Unit tests for glucose history request/result, history service, storage options, JSON history store and DI registration.
* Unit tests for dashboard-to-history persistence behavior.
* Unit tests for glucose history analytics request/result and service behavior.
* Unit tests for runtime provider resolution.
* Unit tests for desktop provider switching.
* Unit tests for settings provider availability.
* Unit tests for Dexcom API options, endpoint resolution, authorization request validation, authorization URL generation and DI registration.
* Unit tests for Dexcom token models, token requests and token client behavior.
* Unit tests for Dexcom OAuth state generation and callback parsing.
* Unit tests for Dexcom local OAuth callback listener options, request/result models and listener behavior.
* Unit tests for Dexcom OAuth authorization session behavior.
* Unit tests for Dexcom OAuth token store behavior.
* Unit tests for Dexcom OAuth token refresh service behavior.
* Unit tests for Dexcom EGV request validation, DTO deserialization and HTTP client behavior.
* Unit tests for Dexcom EGV mapper behavior.
* Unit tests for Dexcom Official CGM provider behavior.
* Unit tests for Dexcom Official CGM provider dependency injection registration.
* Unit tests for Dexcom connection status.
* Unit tests for Dexcom desktop connection action.
* Unit tests for Dexcom active provider selection after connection.
* Unit tests for Nightscout options, requests, DTOs, HTTP client, mapper, provider and DI registration.

## Architecture

GlucoDesk follows a layered architecture:

```text
GlucoDesk.Core
  Pure domain model.
  No dependency on UI, external APIs, storage or infrastructure concerns.

GlucoDesk.Application
  Application contracts, provider abstractions, provider resolution, request/result models,
  dashboard models, history contracts, history analytics contracts, settings models,
  in-process settings notifications, application-level errors and application services.
  No dependency on concrete CGM providers or storage implementations.

GlucoDesk.Infrastructure
  Implementation layer for concrete providers, local storage, HTTP clients and platform integrations.
  Currently includes the deterministic mock CGM provider, local JSON settings storage,
  local JSON glucose history storage, Dexcom Official API infrastructure and Nightscout provider foundation.

GlucoDesk.Desktop
  Avalonia desktop application.
  Currently includes the desktop shell, dashboard view model, settings view model,
  auto-refresh behavior, lightweight glucose trend chart, settings-backed dashboard configuration,
  live settings propagation, dashboard-to-history persistence, provider availability,
  Dexcom connection status, Dexcom connect action and dashboard error presentation.
```

The goal is to keep the domain and application layers independent from concrete providers, storage implementations and UI frameworks.

The current application flow is:

```text
GlucoDesk.Desktop
  -> DashboardView / DashboardViewModel
    -> IGlucoseDataService
      -> ICgmProviderResolver
        -> IApplicationSettingsService
          -> ApplicationSettings.ActiveLiveProvider / ActiveHistoricalProvider
        -> ICgmLiveProvider / ICgmHistoricalProvider / ICgmMetadataProvider
          -> Mock / Dexcom / Nightscout
```

The dashboard does not depend directly on a concrete provider. It asks the application service for a dashboard snapshot. The application service resolves the active provider through `ICgmProviderResolver`, which reads the current local settings and selects the matching registered provider.

Mock remains the safe fallback provider when a selected provider is unavailable in the current runtime.

The current settings flow is:

```text
GlucoDesk.Desktop
  -> SettingsView / SettingsViewModel
    -> IApplicationSettingsService
      -> IApplicationSettingsStore
        -> JsonApplicationSettingsStore
          -> Local settings.json
```

The current desktop provider bootstrap flow is:

```text
DesktopServiceProviderBuilder
  -> AddGlucoDeskApplication()
  -> AddDesktopCgmProviders()
    -> Mock provider always registered
    -> Dexcom provider registered only when explicitly enabled
      -> Dexcom API options from environment variables
      -> Dexcom provider options from environment variables
      -> Dexcom connection status service
      -> Dexcom desktop connection service
```

Dexcom is not hardcoded into the desktop runtime. It is registered only when the current process is started with the required Dexcom environment variables.

Nightscout is currently implemented at infrastructure-provider level. Desktop runtime configuration for Nightscout will be introduced in a future step.

The current dashboard configuration flow is:

```text
Local settings.json
  -> JsonApplicationSettingsStore
    -> IApplicationSettingsService
      -> DashboardViewModel
        -> Dashboard refresh interval
        -> Glucose target range
        -> Active live provider
        -> Active historical provider
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

The current dashboard error presentation flow is:

```text
Dashboard refresh failure
  -> Application Error
    -> DashboardRefreshErrorPresenter
      -> User-facing status text
      -> User-facing error message
      -> Technical error code
```

The dashboard maps common Dexcom and provider errors to clear user-facing messages instead of exposing only raw low-level errors.

## Dexcom architecture

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

The current Dexcom desktop connection flow is:

```text
SettingsViewModel
  -> ConnectDexcomCommand
    -> IDexcomDesktopConnectionService.ConnectAsync(...)
      -> IDexcomOAuthAuthorizationSessionService.StartAuthorizationSessionAsync(...)
        -> Open Dexcom authorization URI in the system browser
        -> Listen for the local loopback OAuth callback
        -> Validate OAuth state
        -> Exchange authorization code for DexcomOAuthTokenSet
        -> Store token set through IDexcomOAuthTokenStore
      -> Refresh provider availability
      -> Select available Dexcom provider
      -> Save Dexcom as active live and historical provider
      -> Refresh Dexcom connection status
```

After a successful Dexcom OAuth connection, GlucoDesk selects the available Dexcom provider and saves it as the active provider for both live and historical readings.

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

The current Dexcom connection status flow is:

```text
SettingsViewModel
  -> IDexcomConnectionStatusService
    -> IDexcomOAuthTokenStore.GetTokenSetAsync(...)
      -> Token missing
      -> Access token usable
      -> Access token refresh required
      -> Refresh token expired
      -> Token store unavailable
```

The settings screen distinguishes provider availability from account connection status. Dexcom can be available in the runtime but still not connected if no OAuth token set is currently stored.

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

The current Dexcom Official CGM provider foundation flow is:

```text
DexcomOfficialCgmProvider
  -> ICgmLiveProvider
  -> ICgmHistoricalProvider
  -> ICgmMetadataProvider

GetLatestReadingAsync(...)
  -> Build recent lookback request
  -> IDexcomEgvClient.GetEgvsAsync(...)
  -> IDexcomEgvMapper.MapResponse(...)
  -> Return latest delayed GlucoseReading

GetRecentReadingsAsync(...)
  -> IDexcomEgvClient.GetEgvsAsync(...)
  -> IDexcomEgvMapper.MapResponse(...)
  -> Return delayed recent GlucoseReading collection

GetReadingsAsync(...)
  -> IDexcomEgvClient.GetEgvsAsync(...)
  -> IDexcomEgvMapper.MapResponse(...)
  -> Return delayed historical GlucoseReading collection

GetMetadataAsync(...)
  -> Return Dexcom provider metadata
```

The Dexcom foundation can now build authorization URLs, generate secure OAuth state values, open an authorization URI through a browser abstraction, listen for a local loopback OAuth redirect, parse Dexcom OAuth callbacks, validate returned state values, exchange authorization codes for tokens through the token client foundation, save the resulting token set into the configured token store, retrieve a valid access token by refreshing the stored token set when necessary, execute authorized Dexcom EGV HTTP requests, map raw Dexcom EGV records into normalized GlucoDesk domain readings and expose a Dexcom Official CGM provider through the application-level provider interfaces.

Dexcom OAuth tokens are currently stored only in memory. Closing the app clears the Dexcom connection until platform-secure persistent token storage is introduced.

## Nightscout architecture

The current Nightscout provider foundation flow is:

```text
NightscoutOptions
  -> NightscoutAuthenticationMode
    -> None
    -> api-secret SHA1 header
    -> access-token query string

NightscoutEntriesRequest
  -> INightscoutEntriesClient.GetEntriesAsync(...)
    -> GET /api/v1/entries/sgv.json
      -> Deserialize NightscoutEntryDto collection
        -> INightscoutEntryMapper.MapEntries(...)
          -> GlucoseReading collection
```

The current Nightscout CGM provider foundation flow is:

```text
NightscoutCgmProvider
  -> ICgmLiveProvider
  -> ICgmHistoricalProvider
  -> ICgmMetadataProvider

GetLatestReadingAsync(...)
  -> Build recent lookback request
  -> INightscoutEntriesClient.GetEntriesAsync(...)
  -> INightscoutEntryMapper.MapEntries(...)
  -> Return latest near real-time GlucoseReading

GetRecentReadingsAsync(...)
  -> INightscoutEntriesClient.GetEntriesAsync(...)
  -> INightscoutEntryMapper.MapEntries(...)
  -> Return near real-time recent GlucoseReading collection

GetReadingsAsync(...)
  -> INightscoutEntriesClient.GetEntriesAsync(...)
  -> INightscoutEntryMapper.MapEntries(...)
  -> Return historical GlucoseReading collection

GetMetadataAsync(...)
  -> Return Nightscout provider metadata
```

Nightscout is currently implemented at infrastructure-provider level. Desktop runtime configuration, settings availability and dashboard selection for Nightscout will be introduced in a future step.

Nightscout data is treated as near real-time provider data. GlucoDesk must clearly communicate that Nightscout is a third-party source and must not present it as official Dexcom real-time data.

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
* `ICgmProviderResolver`
* `CgmLiveProviderResolution`
* `CgmHistoricalProviderResolution`
* `CgmProviderResolver`
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

The `IGlucoseDataService` acts as the application-level facade that UI and reporting layers use to retrieve:

* Provider metadata.
* Latest glucose reading.
* Recent readings.
* Historical readings.
* Dashboard snapshots.

The `ICgmProviderResolver` resolves active runtime providers from local application settings and registered provider metadata.

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
* `DexcomCgmProviderOptions`
* `DexcomOfficialCgmProvider`
* `DexcomOfficialCgmProviderServiceCollectionExtensions`
* `DexcomOfficialApiServiceCollectionExtensions`
* `DexcomConnectionState`
* `DexcomConnectionStatus`
* `IDexcomConnectionStatusService`
* `DexcomConnectionStatusService`
* `DexcomConnectionStatusServiceCollectionExtensions`
* `NightscoutAuthenticationMode`
* `NightscoutOptions`
* `NightscoutEntriesRequest`
* `NightscoutEntryDto`
* `INightscoutEntriesClient`
* `NightscoutEntriesClient`
* `INightscoutEntryMapper`
* `NightscoutEntryMapper`
* `NightscoutCgmProvider`
* `NightscoutCgmProviderServiceCollectionExtensions`

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

The Dexcom Official CGM provider implements:

* `ICgmLiveProvider`
* `ICgmHistoricalProvider`
* `ICgmMetadataProvider`

It uses:

* `IDexcomEgvClient`
* `IDexcomEgvMapper`
* `DexcomApiOptions`
* `DexcomCgmProviderOptions`

The Nightscout CGM provider implements:

* `ICgmLiveProvider`
* `ICgmHistoricalProvider`
* `ICgmMetadataProvider`

It uses:

* `INightscoutEntriesClient`
* `INightscoutEntryMapper`
* `NightscoutOptions`

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
* Specific EGV error mapping for unauthorized, forbidden, rate-limited and server-side responses.
* Mapping raw Dexcom EGV records into normalized GlucoDesk domain readings.
* Dexcom trend mapping to `TrendDirection`.
* Dexcom provider mapping to `CgmProviderKind`.
* Dexcom unit mapping to `GlucoseUnit`.
* Dexcom timestamp mapping from `systemTime` to UTC `DateTimeOffset`.
* Dexcom device metadata mapping.
* Dexcom Official CGM provider implementation behind application-level provider interfaces.
* Delayed latest reading retrieval through Dexcom EGV data.
* Delayed recent readings retrieval through Dexcom EGV data.
* Historical readings retrieval through Dexcom EGV data.
* Provider metadata for Dexcom sandbox and production environments.
* Dependency injection registration for Dexcom infrastructure services.
* Dependency injection registration for the Dexcom Official CGM provider.
* Typed HTTP client registration for Dexcom token operations.
* Typed HTTP client registration for Dexcom EGV operations.
* Connection status inspection through the configured Dexcom token store.

The Nightscout provider foundation currently provides:

* Nightscout provider option modeling.
* Nightscout authentication strategy modeling.
* Nightscout entries request modeling.
* Nightscout entries DTO modeling.
* Nightscout entries HTTP client foundation.
* HTTP-level Nightscout entries response handling.
* Specific Nightscout entries error mapping for unauthorized, forbidden, rate-limited and server-side responses.
* Mapping raw Nightscout entries into normalized GlucoDesk domain readings.
* Nightscout trend mapping to `TrendDirection`.
* Nightscout provider mapping to `CgmProviderKind.Nightscout`.
* Nightscout timestamp mapping from `dateString` or epoch milliseconds to UTC `DateTimeOffset`.
* Nightscout SGV mapping to `GlucoseValue`.
* Nightscout freshness mapping to near real-time glucose data.
* Nightscout CGM provider implementation behind application-level provider interfaces.
* Near real-time latest reading retrieval through Nightscout entries.
* Near real-time recent readings retrieval through Nightscout entries.
* Historical readings retrieval through Nightscout entries.
* Provider metadata for Nightscout.
* Dependency injection registration for the Nightscout CGM provider.
* Typed HTTP client registration for Nightscout entries operations.

The mock provider is not intended to sit between real providers and the UI. In a real user configuration, GlucoDesk uses Dexcom and future Nightscout runtime configuration directly through their own provider implementations.

The local JSON settings store is a foundation for user preferences. Provider credentials and secrets must not be stored in plain JSON in future production releases.

The local JSON glucose history store is a foundation for personal glucose history. Glucose history contains sensitive health data, so future production releases should evaluate encryption, retention settings, export controls and clear privacy documentation before storing real personal data extensively.

The current Dexcom OAuth token store abstraction is intentionally backed by an in-memory implementation only. This prevents writing access tokens and refresh tokens to disk in plain text while the secure persistent storage strategy is still being designed.

Future persistent Dexcom OAuth token storage should use platform-secure storage, such as macOS Keychain, Windows Credential Manager, Linux Secret Service or an equivalent secure mechanism.

The Dexcom OAuth token service is intended to be the only infrastructure component used by Dexcom API clients when they need an access token. EGV clients request a valid token from `IDexcomOAuthTokenService` instead of reading directly from the token store.

The Dexcom EGV client returns raw Dexcom DTOs. The Dexcom EGV mapper is responsible for converting those raw DTOs into normalized `GlucoseReading` values while keeping timestamp parsing, trend mapping, status handling and unit normalization independently testable.

The Nightscout entries client returns raw Nightscout DTOs. The Nightscout entry mapper is responsible for converting those raw entries into normalized `GlucoseReading` values while keeping timestamp parsing, trend mapping and source normalization independently testable.

## Desktop model

The current desktop layer includes:

* `DesktopServiceProviderBuilder`
* `DesktopDexcomProviderOptions`
* `DesktopCgmProviderServiceCollectionExtensions`
* `DexcomDesktopConnectionResult`
* `IDexcomDesktopConnectionService`
* `DexcomDesktopConnectionService`
* `ViewModelBase`
* `MainWindowViewModel`
* `DashboardViewModel`
* `SettingsViewModel`
* `DashboardRefreshOptions`
* `DashboardRefreshErrorPresentation`
* `DashboardRefreshErrorPresenter`
* `ProviderSelectionItem`
* `GlucoseUnitSelectionItem`
* `GlucoseChartPoint`
* `GlucoseTrendChart`
* `MainWindow`
* `DashboardView`
* `SettingsView`

The desktop app uses the application-level `IGlucoseDataService`, which resolves the active provider at runtime through `ICgmProviderResolver`.

Mock is always registered and remains the safe default provider. Dexcom can be registered at desktop startup through environment variables. When Dexcom is configured, connected and selected, the dashboard uses the Dexcom provider instead of the mock provider.

Nightscout is currently implemented at infrastructure-provider level. Desktop runtime configuration for Nightscout will be introduced in the next provider integration step.

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
* Data source status text.
* Configured glucose target range.
* Lightweight recent glucose trend chart.
* Chart summary with reading count and min/max glucose values.
* User-facing provider/Dexcom error messages.
* Technical error code in error states.

The desktop shell includes a simple navigation area with dashboard and settings sections.

The settings section can load and save non-secret local preferences through the application settings service.

Current settings screen supports editing:

* Active live provider.
* Active historical provider.
* Preferred glucose unit.
* Target low value in mg/dL.
* Target high value in mg/dL.
* Dashboard refresh interval in seconds.

Provider selection now participates in runtime provider switching.

The settings screen shows which providers are available in the current desktop runtime. Mock is always available. Dexcom providers are shown as unavailable unless Dexcom is explicitly configured and registered at startup.

Unavailable providers remain visible for transparency, but saving settings with an unavailable provider selected is blocked by validation.

When Dexcom is configured in the desktop runtime, the settings screen shows the Dexcom connection status and exposes a Connect Dexcom action. After a successful OAuth connection, GlucoDesk automatically selects the available Dexcom provider for both live and historical readings and saves those non-secret provider preferences locally.

The Dexcom Official API foundation and Dexcom Official CGM provider are surfaced in the desktop UI when Dexcom is configured through environment variables. The settings screen can start the Dexcom OAuth flow, receive the local callback, store the token set in memory, show connection status and select Dexcom as the active provider.

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

The dashboard history status text communicates whether local history was updated, disabled, had no readings to cache, or failed with an application-level error code.

The dashboard data source status text communicates which provider produced the latest successful dashboard data and whether the data is near real-time, delayed, stale or unavailable.

The current default refresh interval is:

```text
30 seconds
```

The dashboard includes a lightweight custom Avalonia trend chart based on recent glucose readings.

The chart highlights the configured target range and displays deterministic demo data while the app runs with the mock provider.

The current dashboard and settings screen are not intended for treatment decisions.

## Provider strategy

GlucoDesk is designed to support multiple CGM data sources:

```text
Mock provider
  Implemented for local development, tests, demos and future demo mode.
  Always registered in the desktop runtime.

Dexcom Official API
  Implemented for delayed official glucose data and metadata.
  Registered only when Dexcom environment variables are provided.
  Can be connected from the settings screen through the Dexcom OAuth flow.

Nightscout
  Implemented at infrastructure provider level for near real-time CGM visualization.
  Desktop runtime configuration will be introduced in the next step.
```

Current runtime behavior:

```text
Dexcom not configured
  -> Mock is available.
  -> Dexcom is shown as not configured.
  -> Connect Dexcom is hidden.
  -> Dashboard uses Mock.

Dexcom configured but not connected
  -> Mock and Dexcom are available.
  -> Dexcom is shown as configured, not connected.
  -> Connect Dexcom is visible.
  -> Dashboard keeps using the active provider from settings.

Dexcom connected
  -> Token set is stored in memory.
  -> Dexcom is shown as connected.
  -> Dexcom is selected as live and historical provider.
  -> Dashboard uses Dexcom on next refresh.

Dexcom connection lost after app restart
  -> Token set is lost because storage is currently in-memory only.
  -> Dexcom must be connected again until platform-secure persistent token storage is introduced.

Nightscout configured
  -> Nightscout provider registration at desktop runtime is planned next.
  -> Nightscout is intended to provide near real-time readings from a user-owned Nightscout setup.
```

The application should clearly communicate the freshness and source of each reading.

The mock provider is not intended to sit between real providers and the UI. In a real user configuration, GlucoDesk uses Dexcom and Nightscout directly through their own provider implementations.

## Local settings strategy

GlucoDesk includes a local settings foundation.

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

The settings screen allows users to edit and save these non-secret preferences.

The dashboard reads local settings during initialization and applies:

* Active live provider.
* Active historical provider.
* Dashboard refresh interval.
* Lower glucose target.
* Upper glucose target.
* Target range text.
* Chart target range.

After a successful settings save, the application settings service emits an in-process settings change notification.

The dashboard reacts to settings changes and applies:

* New dashboard refresh interval.
* New glucose target range.
* Updated target range text.
* Updated chart target range.
* Updated settings status text.

Provider tokens, API secrets and OAuth credentials must not be stored in plain JSON in future production releases.

## Local history strategy

GlucoDesk includes a local glucose history foundation connected to dashboard refreshes.

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

The dashboard attempts to persist the latest and recent readings into local history after every successful refresh. Readings are de-duplicated before being sent to the history service, and the history store also performs storage-level de-duplication.

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

GlucoDesk includes a local glucose history analytics foundation.

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

The analytics layer reads from `IGlucoseHistoryService`, so it remains independent from the concrete storage implementation.

This foundation will support future features such as:

* History UI.
* Time-in-range reporting.
* Monthly diabetes diary exports.
* Pattern analytics.
* Data completeness reports.
* Provider reconciliation summaries.
* Dexcom official historical statistics.
* Nightscout near real-time data summaries.

Analytics are descriptive only and must not be interpreted as treatment recommendations.

## Dexcom Official API strategy

GlucoDesk includes a Dexcom Official API foundation and a Dexcom Official CGM provider foundation.

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
* Specific Dexcom EGV HTTP error mapping.
* Mapping raw Dexcom EGV records into normalized GlucoDesk domain readings.
* Dexcom trend mapping to `TrendDirection`.
* Dexcom provider mapping to `CgmProviderKind`.
* Dexcom unit mapping to `GlucoseUnit`.
* Dexcom timestamp mapping from `systemTime` to UTC `DateTimeOffset`.
* Dexcom device metadata mapping.
* Dexcom Official CGM provider implementation behind application-level provider interfaces.
* Delayed latest reading retrieval through Dexcom EGV data.
* Delayed recent readings retrieval through Dexcom EGV data.
* Historical readings retrieval through Dexcom EGV data.
* Provider metadata for Dexcom sandbox and production environments.
* Dependency injection registration for Dexcom infrastructure services.
* Dependency injection registration for the Dexcom Official CGM provider.
* Typed HTTP client registration for Dexcom token operations.
* Typed HTTP client registration for Dexcom EGV operations.
* Optional Dexcom provider registration in the desktop runtime.
* Dexcom provider availability in the settings screen.
* Dexcom connection status in the settings screen.
* Dexcom OAuth connection action from the desktop UI.
* Local loopback OAuth callback handling from the desktop app.
* In-memory token storage after successful OAuth connection.
* Automatic selection of Dexcom as live and historical provider after successful connection.
* Dashboard provider switching from Mock to Dexcom through application settings.
* User-facing dashboard error presentation for common Dexcom failures.

The current Dexcom foundation does not yet:

* Store OAuth tokens in platform-secure persistent storage.
* Restore tokens after application restart.
* Provide a Disconnect Dexcom UI action.
* Provide a Reconnect Dexcom UX separate from the Connect Dexcom action.
* Surface full Dexcom account/profile details.
* Provide production-grade privacy controls for long-term real glucose history.

The next Dexcom steps will introduce:

* Secure persistent token storage strategy.
* Disconnect/Reconnect Dexcom actions.
* Dashboard hardening around stale data and empty Dexcom responses.
* Production environment testing against a real Dexcom account.

Dexcom Official API data is intended to be treated as delayed official glucose data and metadata, not as a replacement for approved real-time diabetes applications.

## Nightscout strategy

GlucoDesk includes a Nightscout provider foundation.

Current Nightscout infrastructure supports:

* Nightscout provider option modeling.
* Authentication mode modeling.
* Unauthenticated Nightscout access.
* SHA1 api-secret header authentication.
* Access-token query-string authentication.
* Nightscout entries request modeling.
* Nightscout entries DTO deserialization.
* Nightscout entries HTTP client foundation.
* Nightscout entries HTTP error mapping.
* Nightscout entries invalid-response handling.
* Nightscout entries network-error handling.
* Nightscout entry mapper foundation.
* Mapping Nightscout SGV values to `GlucoseValue`.
* Mapping Nightscout timestamps from `dateString` or epoch milliseconds.
* Mapping Nightscout direction values to `TrendDirection`.
* Mapping Nightscout provider source to `CgmProviderKind.Nightscout`.
* Marking Nightscout readings as near real-time.
* Nightscout CGM provider implementation behind application-level provider interfaces.
* Dependency injection registration for the Nightscout CGM provider.
* Typed HTTP client registration for Nightscout entries operations.

The current Nightscout foundation does not yet:

* Register Nightscout in the desktop runtime through environment variables.
* Show Nightscout provider availability in the settings screen.
* Allow selecting Nightscout as the active live provider from the desktop UI.
* Provide a dedicated Nightscout configuration UI.
* Persist Nightscout secrets through a secure storage strategy.

The next Nightscout steps will introduce:

* Desktop Nightscout runtime configuration.
* Settings provider availability for Nightscout.
* Nightscout selection as active live provider.
* Dashboard refresh through Nightscout entries.
* User-facing Nightscout dashboard error mapping.
* Clear source labeling for third-party near real-time data.

Nightscout data is intended for personal visualization from a user-owned Nightscout setup. GlucoDesk must clearly communicate that Nightscout is a third-party data source.

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
* Treat Nightscout API secrets and access tokens as sensitive secrets.
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
* Keep Nightscout entries HTTP access behind `INightscoutEntriesClient`.
* Keep Nightscout DTO parsing separate from domain mapping.
* Keep Nightscout entry mapping behind `INightscoutEntryMapper`.
* Keep provider implementations behind `ICgmLiveProvider`, `ICgmHistoricalProvider` and `ICgmMetadataProvider`.
* Do not silently hide a real-provider failure behind mock data.
* Make the active data source visible in the dashboard.
* Make provider availability and account connection status separate concepts.
* Keep Dexcom connection status free from OAuth secrets.
* Keep dashboard error messages user-facing while preserving technical error codes for troubleshooting.
* Do not show real CGM data as if it were demo data.
* Do not show demo data as if it were real CGM data.
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

The desktop app starts with Mock as the safe default provider.

Without Dexcom environment variables, the settings screen shows:

```text
Available providers: Mock.
Dexcom: not configured in this desktop runtime.
```

To enable Dexcom Sandbox for local development, start the app from a shell that contains the required environment variables:

```bash
export GLUCODESK_DEXCOM_ENABLED=true
export GLUCODESK_DEXCOM_ENVIRONMENT=Sandbox
export GLUCODESK_DEXCOM_CLIENT_ID="your-dexcom-client-id"
export GLUCODESK_DEXCOM_CLIENT_SECRET="your-dexcom-client-secret"
export GLUCODESK_DEXCOM_REDIRECT_URI="http://127.0.0.1:51234/callback"
export GLUCODESK_DEXCOM_SCOPES="offline_access"
export GLUCODESK_DEXCOM_LATEST_LOOKBACK_MINUTES=1440

dotnet run --project src/GlucoDesk.Desktop/GlucoDesk.Desktop.csproj
```

When Dexcom is configured, the settings screen shows Dexcom provider availability and exposes the Connect Dexcom action.

After a successful Dexcom OAuth connection:

```text
Dexcom: connected.
Live provider: Dexcom Sandbox
Historical provider: Dexcom Sandbox
```

The dashboard uses the selected provider on the next refresh.

Dexcom Sandbox data is simulated and is intended for integration testing. It is not the user's real glucose data.

At this stage, Dexcom OAuth tokens are stored only in memory for the current application process. Closing the app clears the Dexcom connection until platform-secure persistent token storage is introduced.

Nightscout desktop runtime configuration is planned for the next provider integration step. The Nightscout infrastructure provider foundation is already available behind application-level provider abstractions.

## Current limitations

* GlucoDesk is not a medical device.
* Dexcom OAuth tokens are currently stored only in memory.
* Dexcom connection is lost after app restart.
* Disconnect/Reconnect Dexcom UX is not implemented yet.
* Dexcom production real-account testing is not completed yet.
* Nightscout is implemented at infrastructure-provider level but not yet surfaced in desktop runtime configuration.
* Long-term local glucose history privacy controls are not production-grade yet.
* No installer or packaged release artifacts are available yet.
* No compact widget is available yet.
* No history UI is available yet.
* No reporting/export UI is available yet.

## Roadmap

* v0.1: Mock provider, application glucose data service, desktop shell, auto-refresh dashboard, lightweight trend chart, local settings, settings screen, live settings propagation, local glucose history foundation, dashboard-to-history persistence and local history analytics foundation.
* v0.2: Dexcom Official API OAuth foundation, local callback listener, token client, in-memory token store, token refresh service, EGV HTTP client, EGV mapper and Dexcom Official CGM provider.
* v0.3: Runtime provider resolution, desktop provider switching, settings provider availability, Dexcom connection status, Dexcom connect action, active Dexcom provider selection and dashboard provider error hardening.
* v0.4: Nightscout provider foundation, desktop Nightscout configuration, secure persistent token storage, Disconnect/Reconnect Dexcom actions and improved dashboard stale/empty-data states.
* v0.5: Real-account Dexcom production testing, History UI, reporting foundation and compact widget.
* v0.6: Treatments/events, local history UI and monthly diabetes diary export.
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
