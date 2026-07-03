using GlucoDesk.Desktop.GlucoseAlerts.Models;
using GlucoDesk.Desktop.GlucoseAlerts.Notifications.Results;

namespace GlucoDesk.Desktop.GlucoseAlerts.Services;

/// <summary>
/// Represents a no-op native glucose alert notification service.
/// </summary>
public sealed class NoOpGlucoseAlertNotificationService : IGlucoseAlertNotificationService
{
    /// <summary>
    /// Gets the singleton no-op notification service.
    /// </summary>
    public static NoOpGlucoseAlertNotificationService Instance { get; } = new();

    private NoOpGlucoseAlertNotificationService()
    {
    }

    /// <inheritdoc />
    public Task<NativeNotificationRequestResult> ShowAsync(
        GlucoseAlertNativeNotification notification,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notification);

        return Task.FromResult(
            NativeNotificationRequestResult.NotSupported("Native notifications are disabled."));
    }
}
