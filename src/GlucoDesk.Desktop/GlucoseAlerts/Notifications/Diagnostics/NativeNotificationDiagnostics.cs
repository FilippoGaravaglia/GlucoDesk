namespace GlucoDesk.Desktop.GlucoseAlerts.Notifications.Diagnostics;

/// <summary>
/// Describes the runtime native notification environment.
/// </summary>
/// <param name="Platform">The detected native notification platform.</param>
/// <param name="PlatformName">The human-readable platform name.</param>
/// <param name="BackendName">The notification backend name.</param>
/// <param name="IsSupportedPlatform">Whether the platform is supported by the current native notification implementation.</param>
/// <param name="IsPackagedAppLikely">Whether the current process looks like a packaged application.</param>
/// <param name="DeliveryConfirmationAvailable">Whether the backend can confirm that the OS displayed the notification.</param>
/// <param name="PermissionHint">The permission hint for the current platform.</param>
/// <param name="PackagingHint">The packaging hint for the current runtime mode.</param>
public sealed record NativeNotificationDiagnostics(
    NativeNotificationPlatform Platform,
    string PlatformName,
    string BackendName,
    bool IsSupportedPlatform,
    bool IsPackagedAppLikely,
    bool DeliveryConfirmationAvailable,
    string PermissionHint,
    string PackagingHint)
{
    /// <summary>
    /// Formats the diagnostics as a compact Settings page message.
    /// </summary>
    /// <returns>The formatted settings message.</returns>
    public string ToSettingsText()
    {
        if (!IsSupportedPlatform)
        {
            return "Native OS notifications are not available on this platform. Use the in-app banner as the reliable alert surface.";
        }

        return Platform switch
        {
            NativeNotificationPlatform.MacOS => "Native notifications are optional and depend on macOS notification permissions.",
            NativeNotificationPlatform.Windows => "Native notifications are optional and depend on Windows notification permissions.",
            _ => "Native notifications are optional and depend on OS notification permissions."
        };
    }
}
