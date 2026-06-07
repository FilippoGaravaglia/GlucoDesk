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

## Tech stack

* .NET 10
* Avalonia UI
* CommunityToolkit.Mvvm
* xUnit
* Microsoft.Extensions.DependencyInjection

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
      Providers/
        Abstractions/
        Metadata/
      Readings/
        Requests/
        Results/
    Common/
      Errors/
      Results/

  GlucoDesk.Infrastructure/
    Cgm/
      Mock/
        DependencyInjection/
        Generators/
        Options/
        Providers/

tests/
  GlucoDesk.Core.Tests/
    Glucose/

  GlucoDesk.Application.Tests/
    Cgm/
      Providers/
        Metadata/
      Readings/
        Requests/
        Results/
    Common/
      Errors/
      Results/

  GlucoDesk.Infrastructure.Tests/
    Cgm/
      Mock/
        DependencyInjection/
        Options/
        Providers/

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
* Core test project.
* Application test project.
* Infrastructure test project.
* Initial glucose domain model.
* Unit tests for glucose value, range and reading behavior.
* Application-level result/error model.
* CGM provider abstractions.
* CGM readings request and result contracts.
* Provider metadata contract.
* Deterministic mock CGM provider.
* Mock provider dependency injection registration.
* Unit tests for mock provider options, provider behavior and DI registration.

## Architecture

GlucoDesk follows a layered architecture:

```text
GlucoDesk.Core
  Pure domain model.
  No dependency on UI, external APIs, storage or infrastructure concerns.

GlucoDesk.Application
  Application contracts, provider abstractions, request/result models and application-level errors.
  No dependency on concrete CGM providers.

GlucoDesk.Infrastructure
  Implementation layer for concrete providers, local storage, HTTP clients and platform integrations.
  Currently includes the deterministic mock CGM provider.

GlucoDesk.Desktop
  Future Avalonia desktop application.
```

The goal is to keep the domain and application layers independent from concrete providers and UI frameworks.

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
* `ICgmLiveProvider`
* `ICgmHistoricalProvider`
* `ICgmMetadataProvider`

These contracts allow GlucoDesk to support multiple CGM data sources without coupling the UI or domain model to a specific provider.

## Infrastructure model

The current infrastructure layer includes:

* `MockCgmProviderOptions`
* `MockGlucoseReadingGenerator`
* `MockCgmProvider`
* `MockCgmProviderServiceCollectionExtensions`

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

The mock provider is not intended to sit between real providers and the UI. In a real user configuration, GlucoDesk will use Nightscout and/or Dexcom directly through their own provider implementations.

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

## Running build and tests

From the repository root:

```bash
dotnet restore
dotnet build -c Release
dotnet test -c Release
```

## Roadmap

* v0.1: Mock provider, dashboard, chart and local settings.
* v0.2: Nightscout live provider.
* v0.3: Analytics engine and compact widget.
* v0.4: Dexcom Official API historical provider.
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
