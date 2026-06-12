## README alignment update

### Repository structure

Update the repository structure sections with the new folders.

In `GlucoDesk.Application/Cgm/Providers/`, add:

```text
      Providers/
        Abstractions/
        Metadata/
        Resolution/
          Abstractions/
          Models/
          Services/
```

In `GlucoDesk.Infrastructure/Cgm/Dexcom/`, add:

```text
      Dexcom/
        Connection/
          DependencyInjection/
          Enums/
          Models/
          Services/
```

In `GlucoDesk.Desktop/Bootstrap/`, add:

```text
    Bootstrap/
      Providers/
        Connection/
          Models/
          Services/
        DependencyInjection/
        Options/
```

In `GlucoDesk.Desktop/ViewModels/Dashboard/`, add:

```text
      Dashboard/
        Chart/
        Errors/
        Options/
```

In `GlucoDesk.Desktop.Tests/`, add the matching test folders:

```text
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
      Settings/
        Selections/
```

---

## Current status

Replace the outdated final part of the `Implemented:` list by adding these items after the Dexcom Official CGM provider items:

```markdown
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
* More specific Dexcom EGV HTTP error mapping.
* Dashboard data source status text.
* Unit tests for runtime provider resolution.
* Unit tests for desktop provider switching.
* Unit tests for settings provider availability.
* Unit tests for Dexcom connection status.
* Unit tests for Dexcom desktop connection action.
* Unit tests for Dexcom active provider selection after connection.
* Unit tests for dashboard refresh error presentation.
```

---

## Architecture

Replace the current application flow with this:

````markdown
The current application flow is:

```text
GlucoDesk.Desktop
  -> DashboardView / DashboardViewModel
    -> IGlucoseDataService
      -> ICgmProviderResolver
        -> IApplicationSettingsService
          -> ApplicationSettings.ActiveLiveProvider / HistoricalProvider
        -> ICgmLiveProvider / ICgmHistoricalProvider / ICgmMetadataProvider
          -> Mock / Dexcom / future Nightscout
````

The dashboard does not depend directly on a concrete provider. It asks the application service for a dashboard snapshot. The application service resolves the active provider through `ICgmProviderResolver`, which reads the current local settings and selects the matching registered provider.

Mock remains the safe fallback provider when a selected provider is unavailable in the current runtime.

````

Add this new flow after the settings flow:

```markdown
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
````

Dexcom is not hardcoded into the desktop runtime. It is registered only when the current process is started with the required Dexcom environment variables.

````

Add this new flow after the Dexcom OAuth authorization session flow:

```markdown
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
````

After a successful Dexcom OAuth connection, GlucoDesk selects the available Dexcom provider and saves it as the active provider for both live and historical readings.

````

Add this new flow after the token refresh flow:

```markdown
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
````

The settings screen distinguishes provider availability from account connection status. Dexcom can be available in the runtime but still not connected if no OAuth token set is currently stored.

````

Add this new flow near the dashboard flow:

```markdown
The current dashboard error presentation flow is:

```text
Dashboard refresh failure
  -> Application Error
    -> DashboardRefreshErrorPresenter
      -> User-facing status text
      -> User-facing error message
      -> Technical error code
````

The dashboard maps common Dexcom and provider errors to clear user-facing messages instead of exposing only raw low-level errors.

````

---

## Desktop model

Replace this outdated paragraph:

```markdown
The desktop app currently uses the mock CGM provider through the application-level `IGlucoseDataService`.
````

with:

```markdown
The desktop app uses the application-level `IGlucoseDataService`, which resolves the active provider at runtime through `ICgmProviderResolver`.

Mock is always registered and remains the safe default provider. Dexcom can be registered at desktop startup through environment variables. When Dexcom is configured, connected and selected, the dashboard uses the Dexcom provider instead of the mock provider.
```

Replace this outdated paragraph:

```markdown
Provider selection currently persists preferences only. Runtime provider switching will be introduced in a future step.
```

with:

```markdown
Provider selection now participates in runtime provider switching.

The settings screen shows which providers are available in the current desktop runtime. Mock is always available. Dexcom providers are shown as unavailable unless Dexcom is explicitly configured and registered at startup.

Unavailable providers remain visible for transparency, but saving settings with an unavailable provider selected is blocked by validation.
```

Add these bullets to the dashboard preview list:

```markdown
* Data source status text.
* User-facing provider/Dexcom error messages.
* Technical error code in error states.
```

Add this paragraph after the settings screen description:

```markdown
When Dexcom is configured in the desktop runtime, the settings screen shows the Dexcom connection status and exposes a Connect Dexcom action. After a successful OAuth connection, GlucoDesk automatically selects the available Dexcom provider for both live and historical readings and saves those non-secret provider preferences locally.
```

Replace this outdated paragraph:

```markdown
The Dexcom Official API foundation and Dexcom Official CGM provider are currently available at infrastructure-service level and are not yet surfaced in the desktop UI.
```

with:

```markdown
The Dexcom Official API foundation and Dexcom Official CGM provider are now surfaced in the desktop UI when Dexcom is configured through environment variables. The settings screen can start the Dexcom OAuth flow, receive the local callback, store the token set in memory, show connection status and select Dexcom as the active provider.
```

---

## Provider strategy

Replace the current `Provider strategy` section with:

````markdown
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
  Intended for future near real-time glucose visualization when a user already has a Nightscout setup.
````

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
```

The application should clearly communicate the freshness and source of each reading.

The mock provider is not intended to sit between real providers and the UI. In a real user configuration, GlucoDesk uses Dexcom and future Nightscout directly through their own provider implementations.

````

---

## Dexcom Official API strategy

Replace the outdated final part of the `Dexcom Official API strategy` section.

Replace this:

```markdown
The current Dexcom foundation does not yet:

* Store OAuth tokens in platform-secure persistent storage.
* Restore tokens after application restart.
* Switch the runtime dashboard provider from Mock to Dexcom.
* Surface Dexcom connection actions in the desktop UI.

The next Dexcom steps will introduce:

* Runtime provider selection.
* Dashboard provider switching from Mock to Dexcom Official.
* Secure persistent token storage strategy.
* Desktop UI actions for connecting and disconnecting Dexcom.
````

with:

```markdown
The current Dexcom foundation now supports:

* Optional Dexcom provider registration in the desktop runtime.
* Dexcom provider availability in the settings screen.
* Dexcom connection status in the settings screen.
* Dexcom OAuth connection action from the desktop UI.
* Local loopback OAuth callback handling from the desktop app.
* In-memory token storage after successful OAuth connection.
* Automatic selection of Dexcom as live and historical provider after successful connection.
* Dashboard provider switching from Mock to Dexcom through application settings.
* User-facing dashboard error presentation for common Dexcom failures.
* Specific Dexcom EGV HTTP error mapping for unauthorized, forbidden, rate-limited and server-side responses.

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
```

---

## Running the desktop app

Replace the current section content after the command with:

````markdown
The desktop app starts with Mock as the safe default provider.

Without Dexcom environment variables, the settings screen shows:

```text
Available providers: Mock.
Dexcom: not configured in this desktop runtime.
````

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

````

---

## Roadmap

Replace the roadmap with:

```markdown
## Roadmap

* v0.1: Mock provider, application glucose data service, desktop shell, auto-refresh dashboard, lightweight trend chart, local settings, settings screen, live settings propagation, local glucose history foundation, dashboard-to-history persistence and local history analytics foundation.
* v0.2: Dexcom Official API OAuth foundation, local callback listener, token client, in-memory token store, token refresh service, EGV HTTP client, EGV mapper and Dexcom Official CGM provider.
* v0.3: Runtime provider resolution, desktop provider switching, settings provider availability, Dexcom connection status, Dexcom connect action, active Dexcom provider selection and dashboard provider error hardening.
* v0.4: Secure persistent token storage, Disconnect/Reconnect Dexcom actions, real-account Dexcom production testing and improved dashboard stale/empty-data states.
* v0.5: History UI, reporting foundation and compact widget.
* v0.6: Nightscout provider for users who already have a Nightscout setup.
* v0.7: Treatments/events, local history UI and monthly diabetes diary export.
* v1.0: Production-ready cross-platform release.
````

---

## Development principles

Add these bullets near the existing Dexcom/provider principles:

```markdown
* Do not silently hide a real-provider failure behind mock data.
* Make the active data source visible in the dashboard.
* Make provider availability and account connection status separate concepts.
* Keep Dexcom connection status free from OAuth secrets.
* Keep dashboard error messages user-facing while preserving technical error codes for troubleshooting.
* Do not show real CGM data as if it were demo data.
* Do not show demo data as if it were real CGM data.
```
