using System.Diagnostics;
using System.Text;
using GlucoDesk.Desktop.GlucoseAlerts.Models;

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
    public abstract Task ShowAsync(
        GlucoseAlertNativeNotification notification,
        CancellationToken cancellationToken);

    /// <summary>
    /// Starts a native process and waits for completion without surfacing notification errors to the caller.
    /// </summary>
    /// <param name="startInfo">The process start info.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the process execution.</returns>
    protected static async Task RunBestEffortProcessAsync(
        ProcessStartInfo startInfo,
        CancellationToken cancellationToken)
    {
        try
        {
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;

            using var process = Process.Start(startInfo);

            if (process is null)
            {
                return;
            }

            await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            // Native notifications are best-effort and must not break the dashboard.
        }
    }

    private sealed class MacOsGlucoseAlertNotificationService : OperatingSystemGlucoseAlertNotificationService
    {
        /// <inheritdoc />
        public override Task ShowAsync(
            GlucoseAlertNativeNotification notification,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);

            var script = $"display notification \"{EscapeAppleScript(notification.Message)}\" with title \"{EscapeAppleScript(notification.Title)}\"";
            var startInfo = new ProcessStartInfo("/usr/bin/osascript")
            {
                ArgumentList =
                {
                    "-e",
                    script
                }
            };

            return RunBestEffortProcessAsync(startInfo, cancellationToken);
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
        public override Task ShowAsync(
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

            return RunBestEffortProcessAsync(startInfo, cancellationToken);
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
