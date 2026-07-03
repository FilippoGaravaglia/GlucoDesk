using GlucoDesk.Desktop.GlucoseAlerts.Notifications.Results;
using GlucoDesk.Desktop.GlucoseAlerts.Models;

namespace GlucoDesk.Desktop.GlucoseAlerts.Services;

/// <summary>
/// Sends native OS notifications for glucose awareness alerts.
/// </summary>
public interface IGlucoseAlertNotificationService
{
    /// <summary>
    /// Shows a native OS notification.
    /// </summary>
    /// <param name="notification">The notification to show.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous notification operation.</returns>
    Task<NativeNotificationRequestResult> ShowAsync(
        GlucoseAlertNativeNotification notification,
        CancellationToken cancellationToken);
}
