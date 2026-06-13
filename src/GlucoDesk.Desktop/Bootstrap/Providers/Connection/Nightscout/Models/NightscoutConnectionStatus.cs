using GlucoDesk.Desktop.Bootstrap.Providers.Connection.Nightscout.Enums;

namespace GlucoDesk.Desktop.Bootstrap.Providers.Connection.Nightscout.Models;

/// <summary>
/// Represents the result of a Nightscout desktop connection diagnostic check.
/// </summary>
/// <param name="State">The Nightscout connection state.</param>
/// <param name="Message">The user-facing connection message.</param>
/// <param name="BaseUri">The configured Nightscout base URI.</param>
/// <param name="CheckedAtUtc">The UTC timestamp when the status was checked.</param>
public sealed record NightscoutConnectionStatus(
    NightscoutConnectionState State,
    string Message,
    Uri? BaseUri,
    DateTimeOffset? CheckedAtUtc)
{
    /// <summary>
    /// Gets a value indicating whether Nightscout is configured in the current desktop runtime.
    /// </summary>
    public bool IsConfigured => State is not NightscoutConnectionState.NotConfigured;

    /// <summary>
    /// Gets a value indicating whether the latest Nightscout diagnostic check succeeded.
    /// </summary>
    public bool IsConnected => State is NightscoutConnectionState.Connected;
}