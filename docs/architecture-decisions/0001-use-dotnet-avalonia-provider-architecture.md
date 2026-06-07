# ADR 0001: Use .NET, Avalonia and provider-based architecture

## Status

Accepted

## Context

GlucoDesk aims to be a cross-platform desktop companion for CGM data visualization.

The application should run on:

- Windows
- macOS
- Linux
- eventually Linux ARM64 / Raspberry Pi

The application must not be tightly coupled to a single CGM data source.

## Decision

We will use:

- .NET 10 as the main runtime.
- Avalonia UI for the desktop application.
- A provider-based architecture for CGM data sources.
- A local-first approach for settings, cache and analytics.

Initial providers will be:

- Mock provider.
- Nightscout provider.
- Dexcom Official API provider.

Nightscout will be used for near real-time data when configured by the user.
Dexcom Official API will be used only for delayed official historical data and metadata.

## Consequences

Positive:

- The core domain remains independent from UI and external APIs.
- New providers can be added without changing the UI.
- The app can support multiple platforms from a shared codebase.
- The project remains suitable for an open-source GitHub repository.

Trade-offs:

- Platform-specific features such as tray icon, notifications and secure storage will require dedicated abstractions.
- Dexcom Official API cannot be used as the live data source in regions where data is delayed.