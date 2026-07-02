using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Desktop.GlucoseAlerts.Models;

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
    public async Task<Result> SendTestNotificationAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _notificationService
                .ShowAsync(CreatePrivacySafeTestNotification(), cancellationToken)
                .ConfigureAwait(false);

            return Result.Success();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return Result.Failure(
                new Error(
                    "GlucoseAlerts.TestNotificationFailed",
                    "Unable to send the native test notification."));
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
