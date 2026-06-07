# GlucoDesk

GlucoDesk is a local-first, cross-platform desktop companion for visualizing CGM glucose data while working at your computer.

The project is designed around a provider-based architecture:

- Mock provider for local development and demos.
- Nightscout provider for near real-time CGM visualization.
- Dexcom Official API provider for delayed official historical data and metadata.

> GlucoDesk is not a medical device. It must not be used for treatment decisions, insulin dosing, emergency alerts, or as a replacement for Dexcom, Omnipod, Nightscout, or any approved medical application.

## Goals

- Cross-platform desktop application for Windows, macOS, and Linux.
- Local-first and privacy-first architecture.
- Provider-based CGM data access.
- Clean .NET architecture.
- Useful desktop dashboard and compact widget.
- Open-source friendly design.

## Tech stack

- .NET 10
- Avalonia UI
- CommunityToolkit.Mvvm
- xUnit
- FluentAssertions

## Repository structure

```text
src/
  GlucoDesk.Core/
  GlucoDesk.Application/
  GlucoDesk.Infrastructure/

tests/
  GlucoDesk.Core.Tests/
  GlucoDesk.Application.Tests/

docs/
  architecture-decisions/

build/