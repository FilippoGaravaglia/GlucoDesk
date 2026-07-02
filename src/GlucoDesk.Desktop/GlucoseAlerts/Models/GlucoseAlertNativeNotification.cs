namespace GlucoDesk.Desktop.GlucoseAlerts.Models;

/// <summary>
/// Represents a native OS glucose awareness notification.
/// </summary>
/// <param name="Title">The notification title.</param>
/// <param name="Message">The notification message.</param>
public sealed record GlucoseAlertNativeNotification(
    string Title,
    string Message);
