using GlucoDesk.Desktop.GlucoseAlerts.Models;
using GlucoDesk.Desktop.GlucoseAlerts.Notifications.Results;
using Microsoft.Toolkit.Uwp.Notifications;

namespace GlucoDesk.Desktop.GlucoseAlerts.Services;

/// <summary>
/// Sends native Windows toast notifications for an unpackaged desktop app.
/// This implementation is compiled only for the Windows target.
/// </summary>
internal sealed class WindowsGlucoseAlertNotificationService
    : OperatingSystemGlucoseAlertNotificationService
{
    /// <inheritdoc />
    public override Task<NativeNotificationRequestResult> ShowAsync(
        GlucoseAlertNativeNotification notification,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notification);
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var toastBuilder = new ToastContentBuilder()
                .AddArgument("source", "glucose-awareness")
                .AddText(notification.Title);

            if (!string.IsNullOrWhiteSpace(notification.Subtitle))
            {
                toastBuilder.AddText(notification.Subtitle);
            }

            if (!string.IsNullOrWhiteSpace(notification.Message))
            {
                toastBuilder.AddText(notification.Message);
            }

            cancellationToken.ThrowIfCancellationRequested();

            /*
             * ToastContentBuilder.Show uses ToastNotificationManagerCompat
             * internally. This supports unpackaged Win32 desktop applications
             * and does not require Windows App SDK registration.
             */
            toastBuilder.Show();

            /*
             * The Windows toast API accepts the notification request but does
             * not provide a reliable synchronous confirmation that Windows
             * displayed it to the user.
             */
            return Task.FromResult(
                NativeNotificationRequestResult.UnknownDelivery());
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            return Task.FromResult(
                NativeNotificationRequestResult.Failed(
                    "Unable to request the native Windows notification. "
                    + $"{exception.GetType().Name}: {exception.Message}"));
        }
    }
}
