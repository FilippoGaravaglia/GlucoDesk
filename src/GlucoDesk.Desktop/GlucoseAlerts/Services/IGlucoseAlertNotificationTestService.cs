using GlucoDesk.Application.Common.Results;

namespace GlucoDesk.Desktop.GlucoseAlerts.Services;

/// <summary>
/// Sends privacy-safe native test notifications for glucose awareness settings.
/// </summary>
public interface IGlucoseAlertNotificationTestService
{
    /// <summary>
    /// Sends a privacy-safe native test notification.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The test notification result.</returns>
    Task<Result> SendTestNotificationAsync(CancellationToken cancellationToken);
}
