# GlucoDesk

GlucoDesk is a local-first, cross-platform desktop companion for visualizing CGM glucose data while working at your computer.

The project is designed around a provider-based architecture:

* Mock provider for local development and demos.
* Nightscout provider for near real-time CGM visualization.
* Dexcom Official API provider for delayed official historical data and metadata.

> GlucoDesk is not a medical device. It must not be used for treatment decisions, insulin dosing, emergency alerts, or as a replacement for Dexcom, Omnipod, Nightscout, or any approved medical application.

## Goals

* Cross-platform desktop application for Windows, macOS, and Linux.
* Local-first and privacy-first architecture.
* Provider-based CGM data access.
* Clean .NET architecture.
* Useful desktop dashboard and compact widget.
* Open-source friendly design.

## Tech stack

* .NET 10
* Avalonia UI
* CommunityToolkit.Mvvm
* xUnit

## Repository structure

```text
src/
  GlucoDesk.Core/
    Glucose/
      Enums/
      Readings/
      ValueObjects/
  GlucoDesk.Application/
  GlucoDesk.Infrastructure/

tests/
  GlucoDesk.Core.Tests/
    Glucose/
  GlucoDesk.Application.Tests/

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
* Initial glucose domain model.
* Unit tests for glucose value, range and reading behavior.

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

## Roadmap

* v0.1: Mock provider, dashboard, chart and local settings.
* v0.2: Nightscout live provider.
* v0.3: Analytics engine and compact widget.
* v0.4: Dexcom Official API historical provider.
* v1.0: Production-ready cross-platform release.

## License

MIT
