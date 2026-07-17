using System.Diagnostics;
using System.Text;
using GlucoDesk.Desktop.GlucoseAlerts.Models;
using GlucoDesk.Desktop.GlucoseAlerts.Notifications.Results;

namespace GlucoDesk.Desktop.GlucoseAlerts.Services;

/// <summary>
/// Sends best-effort native OS notifications for glucose awareness alerts.
/// </summary>
public abstract class OperatingSystemGlucoseAlertNotificationService : IGlucoseAlertNotificationService
{
    /// <summary>
    /// Creates the appropriate notification service for the current operating system.
    /// </summary>
    /// <returns>The native notification service.</returns>
    public static IGlucoseAlertNotificationService Create()
    {
        if (OperatingSystem.IsMacOS())
        {
            return new MacOsGlucoseAlertNotificationService();
        }

        if (OperatingSystem.IsWindows())
        {
            return new WindowsGlucoseAlertNotificationService();
        }

        return NoOpGlucoseAlertNotificationService.Instance;
    }

    /// <inheritdoc />
    public abstract Task<NativeNotificationRequestResult> ShowAsync(
        GlucoseAlertNativeNotification notification,
        CancellationToken cancellationToken);

    /// <summary>
    /// Requests a best-effort native notification by running the specified process.
    /// </summary>
    /// <param name="startInfo">The process start information.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The native notification request result.</returns>
    private static async Task<NativeNotificationRequestResult> RequestBestEffortProcessAsync(
        ProcessStartInfo startInfo,
        CancellationToken cancellationToken)
    {
        try
        {
            using var process = Process.Start(startInfo);

            if (process is null)
            {
                return NativeNotificationRequestResult.Failed(
                    "Unable to start the native notification process.");
            }

            await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

            return NativeNotificationRequestResult.UnknownDelivery();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            return NativeNotificationRequestResult.Failed(
                $"Unable to request native notification. {exception.GetType().Name}: {exception.Message}");
        }
    }

    private sealed class MacOsGlucoseAlertNotificationService : OperatingSystemGlucoseAlertNotificationService
    {
        private const string HelperAppName = "GlucoDeskNotificationHelper.app";

        /// <inheritdoc />
        public override Task<NativeNotificationRequestResult> ShowAsync(
            GlucoseAlertNativeNotification notification,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);

            var helperAppPath = FindNotificationHelperAppPath();

            if (!string.IsNullOrWhiteSpace(helperAppPath))
            {
                return RequestUserNotificationsHelperAsync(
                    helperAppPath,
                    notification,
                    cancellationToken);
            }

            return RequestAppleScriptFallbackAsync(notification, cancellationToken);
        }

        /// <summary>
        /// Requests a macOS notification through the bundled UserNotifications helper app.
        /// </summary>
        /// <param name="helperAppPath">The helper app bundle path.</param>
        /// <param name="notification">The notification to show.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The native notification request result.</returns>
        private static Task<NativeNotificationRequestResult> RequestUserNotificationsHelperAsync(
            string helperAppPath,
            GlucoseAlertNativeNotification notification,
            CancellationToken cancellationToken)
        {
            var startInfo = new ProcessStartInfo("/usr/bin/open")
            {
                ArgumentList =
                {
                    "-g",
                    "-n",
                    "-W",
                    helperAppPath,
                    "--args",
                    notification.Title,
                    notification.Message,
                    string.IsNullOrWhiteSpace(notification.Subtitle)
                        ? "GlucoDesk"
                        : notification.Subtitle
                }
            };

            return RequestBestEffortProcessAsync(startInfo, cancellationToken);
        }

        /// <summary>
        /// Requests a legacy AppleScript notification when the native helper is not available.
        /// </summary>
        /// <param name="notification">The notification to show.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The native notification request result.</returns>
        private static Task<NativeNotificationRequestResult> RequestAppleScriptFallbackAsync(
            GlucoseAlertNativeNotification notification,
            CancellationToken cancellationToken)
        {
            var script = $"display notification \"{EscapeAppleScript(notification.Message)}\" with title \"{EscapeAppleScript(notification.Title)}\"";
            var startInfo = new ProcessStartInfo("/usr/bin/osascript")
            {
                ArgumentList =
                {
                    "-e",
                    script
                }
            };

            return RequestBestEffortProcessAsync(startInfo, cancellationToken);
        }

        /// <summary>
        /// Finds the bundled macOS notification helper app.
        /// </summary>
        /// <returns>The helper app path when available; otherwise null.</returns>
        private static string? FindNotificationHelperAppPath()
        {
            foreach (var candidate in BuildNotificationHelperAppPathCandidates())
            {
                if (Directory.Exists(candidate))
                {
                    return candidate;
                }
            }

            return null;
        }

        /// <summary>
        /// Builds candidate paths for development and packaged macOS app layouts.
        /// </summary>
        /// <returns>The candidate helper app paths.</returns>
        private static string[] BuildNotificationHelperAppPathCandidates()
        {
            var baseDirectory = AppContext.BaseDirectory;
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            return
            [
                Path.GetFullPath(Path.Combine(baseDirectory, HelperAppName)),
                Path.GetFullPath(Path.Combine(baseDirectory, "..", "Helpers", HelperAppName)),
                Path.Combine(userProfile, "Applications", "GlucoDeskNotificationHelper", HelperAppName),
                "/Applications/GlucoDesk.app/Contents/Helpers/GlucoDeskNotificationHelper.app"
            ];
        }

        /// <summary>
        /// Escapes text for AppleScript string literals.
        /// </summary>
        /// <param name="value">The raw value.</param>
        /// <returns>The escaped value.</returns>
        private static string EscapeAppleScript(string value)
        {
            return value
                .Replace("\\", "\\\\", StringComparison.Ordinal)
                .Replace("\"", "\\\"", StringComparison.Ordinal);
        }
    }

    private sealed class WindowsGlucoseAlertNotificationService : OperatingSystemGlucoseAlertNotificationService
    {
        /// <inheritdoc />
        public override Task<NativeNotificationRequestResult> ShowAsync(
            GlucoseAlertNativeNotification notification,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);

            var script = BuildWindowsToastScript(notification);
            var encodedCommand = Convert.ToBase64String(Encoding.Unicode.GetBytes(script));

            var startInfo = new ProcessStartInfo("powershell.exe")
            {
                ArgumentList =
                {
                    "-NoProfile",
                    "-ExecutionPolicy",
                    "Bypass",
                    "-EncodedCommand",
                    encodedCommand
                }
            };

            return RequestBestEffortProcessAsync(startInfo, cancellationToken);
        }

        /// <summary>
        /// Builds a best-effort Windows toast script.
        /// </summary>
        /// <param name="notification">The notification to show.</param>
        /// <returns>The PowerShell script.</returns>
        private static string BuildWindowsToastScript(GlucoseAlertNativeNotification notification)
        {
            var title = EscapePowerShellSingleQuotedString(notification.Title);
            var message = EscapePowerShellSingleQuotedString(notification.Message);

            return $$"""
try {
    [Windows.UI.Notifications.ToastNotificationManager, Windows.UI.Notifications, ContentType = WindowsRuntime] | Out-Null
    [Windows.Data.Xml.Dom.XmlDocument, Windows.Data.Xml.Dom.XmlDocument, ContentType = WindowsRuntime] | Out-Null

    $title = '{{title}}'
    $message = '{{message}}'

    $template = @"
<toast>
  <visual>
    <binding template="ToastGeneric">
      <text>$title</text>
      <text>$message</text>
    </binding>
  </visual>
</toast>
"@

    $xml = New-Object Windows.Data.Xml.Dom.XmlDocument
    $xml.LoadXml($template)

    $toast = [Windows.UI.Notifications.ToastNotification]::new($xml)
    $notifier = [Windows.UI.Notifications.ToastNotificationManager]::CreateToastNotifier('GlucoDesk')
    $notifier.Show($toast)
} catch {
}
""";
        }

        /// <summary>
        /// Escapes text for a PowerShell single-quoted string literal.
        /// </summary>
        /// <param name="value">The raw value.</param>
        /// <returns>The escaped value.</returns>
        private static string EscapePowerShellSingleQuotedString(string value)
        {
            return value.Replace("'", "''", StringComparison.Ordinal);
        }
    }
}
