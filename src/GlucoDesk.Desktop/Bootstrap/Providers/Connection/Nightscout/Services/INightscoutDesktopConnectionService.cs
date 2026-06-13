using GlucoDesk.Desktop.Bootstrap.Providers.Connection.Nightscout.Models;

namespace GlucoDesk.Desktop.Bootstrap.Providers.Connection.Nightscout.Services;

/// <summary>
/// Provides Nightscout desktop connection diagnostics.
/// </summary>
public interface INightscoutDesktopConnectionService
{
    /// <summary>
    /// Gets the current Nightscout configuration status without performing a network request.
    /// </summary>
    /// <returns>The current Nightscout configuration status.</returns>
    NightscoutConnectionStatus GetConfigurationStatus();

    /// <summary>
    /// Tests the configured Nightscout endpoint by performing a lightweight entries request.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The Nightscout connection status.</returns>
    Task<NightscoutConnectionStatus> TestConnectionAsync(CancellationToken cancellationToken);
}