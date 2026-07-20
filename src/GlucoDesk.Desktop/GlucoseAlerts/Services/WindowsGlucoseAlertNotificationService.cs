using GlucoDesk.Desktop.GlucoseAlerts.Models;
using GlucoDesk.Desktop.GlucoseAlerts.Notifications.Results;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;

namespace GlucoDesk.Desktop.GlucoseAlerts.Services;

/// <summary>
/// Sends native glucose awareness notifications on Windows through the
/// Windows App SDK app-notification platform.
/// </summary>
/// <remarks>
/// This source file is compiled only for Windows builds. Registration is
/// performed lazily so that notification initialization cannot affect the
/// application startup path.
/// </remarks>
internal sealed class WindowsGlucoseAlertNotificationService
    : OperatingSystemGlucoseAlertNotificationService
{
    private static readonly SemaphoreSlim RegistrationSemaphore = new(1, 1);

    private static bool _isRegistered;

    /// <inheritdoc />
    public override async Task<NativeNotificationRequestResult> ShowAsync(
        GlucoseAlertNativeNotification notification,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notification);
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            await EnsureRegisteredAsync(cancellationToken)
                .ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            var notificationBuilder = new AppNotificationBuilder()
                .AddArgument("source", "glucose-awareness")
                .AddText(notification.Title);

            if (!string.IsNullOrWhiteSpace(notification.Subtitle))
            {
                notificationBuilder.AddText(notification.Subtitle);
            }

            notificationBuilder.AddText(notification.Message);

            var appNotification =
                notificationBuilder.BuildNotification();

            AppNotificationManager.Default.Show(appNotification);

            return NativeNotificationRequestResult.UnknownDelivery();
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            return NativeNotificationRequestResult.Failed(
                "Unable to request the native Windows notification. "
                + $"{exception.GetType().Name}: {exception.Message}");
        }
    }

    /// <summary>
    /// Registers the unpackaged desktop process with the Windows app
    /// notification platform once for the lifetime of the application.
    /// </summary>
    private static async Task EnsureRegisteredAsync(
        CancellationToken cancellationToken)
    {
        if (_isRegistered)
        {
            return;
        }

        await RegistrationSemaphore
            .WaitAsync(cancellationToken)
            .ConfigureAwait(false);

        try
        {
            if (_isRegistered)
            {
                return;
            }

            AppNotificationManager.Default.Register();

            _isRegistered = true;
        }
        finally
        {
            RegistrationSemaphore.Release();
        }
    }
}
