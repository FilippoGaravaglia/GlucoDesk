using GlucoDesk.Desktop.GlucoseAlerts.Models;
using GlucoDesk.Desktop.GlucoseAlerts.Notifications.Results;

namespace GlucoDesk.Desktop.GlucoseAlerts.Services;

/// <summary>
/// Sends privacy-safe native test notifications for glucose awareness settings.
/// </summary>
public sealed class GlucoseAlertNotificationTestService : IGlucoseAlertNotificationTestService
{
    private readonly IGlucoseAlertNotificationService _notificationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlucoseAlertNotificationTestService"/> class.
    /// </summary>
    /// <param name="notificationService">The native notification service.</param>
    public GlucoseAlertNotificationTestService(IGlucoseAlertNotificationService notificationService)
    {
        ArgumentNullException.ThrowIfNull(notificationService);

        _notificationService = notificationService;
    }

    /// <inheritdoc />
    public async Task<NativeNotificationRequestResult> SendTestNotificationAsync(CancellationToken cancellationToken)
    {
        try
        {
            var result = await _notificationService
                .ShowAsync(CreatePrivacySafeTestNotification(), cancellationToken)
                .ConfigureAwait(false);

            return result;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            return NativeNotificationRequestResult.Failed(
                $"Unable to send the native test notification. {exception.GetType().Name}: {exception.Message}");
        }
    }

    #region Helpers

    /// <summary>
    /// Creates a privacy-safe native test notification.
    /// </summary>
    /// <returns>The privacy-safe native test notification.</returns>
    private static GlucoseAlertNativeNotification CreatePrivacySafeTestNotification()
    {
        return new GlucoseAlertNativeNotification(
            "GlucoDesk notification test",
            "Native glucose awareness notifications are enabled on this desktop.");
    }

    #endregion
}
