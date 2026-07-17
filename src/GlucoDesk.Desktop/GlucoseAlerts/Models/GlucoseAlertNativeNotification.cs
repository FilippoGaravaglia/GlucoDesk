namespace GlucoDesk.Desktop.GlucoseAlerts.Models;

/// <summary>
/// Represents a native OS glucose awareness notification.
/// </summary>
/// <param name="Title">The notification title.</param>
/// <param name="Message">The notification message.</param>
/// <param name="Subtitle">The optional notification subtitle.</param>
public sealed record GlucoseAlertNativeNotification(
    string Title,
    string Message,
    string Subtitle = "");
