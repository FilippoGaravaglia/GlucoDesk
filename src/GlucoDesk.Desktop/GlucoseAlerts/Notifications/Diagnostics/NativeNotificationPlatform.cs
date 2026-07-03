namespace GlucoDesk.Desktop.GlucoseAlerts.Notifications.Diagnostics;

/// <summary>
/// Represents the current native notification platform.
/// </summary>
public enum NativeNotificationPlatform
{
    /// <summary>
    /// macOS native notification environment.
    /// </summary>
    MacOS,

    /// <summary>
    /// Windows native notification environment.
    /// </summary>
    Windows,

    /// <summary>
    /// Linux desktop environment.
    /// </summary>
    Linux,

    /// <summary>
    /// Unsupported or unknown native notification environment.
    /// </summary>
    Unsupported
}
