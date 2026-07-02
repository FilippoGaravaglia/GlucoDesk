namespace GlucoDesk.Desktop.GlucoseAlerts.Models;

/// <summary>
/// Represents the UI and native-notification presentation for a glucose awareness alert.
/// </summary>
/// <param name="Kind">The alert kind.</param>
/// <param name="Title">The alert title.</param>
/// <param name="Message">The alert message.</param>
/// <param name="BadgeText">The compact alert badge text.</param>
/// <param name="ActionText">The safety action text.</param>
/// <param name="ShouldSendNativeNotification">A value indicating whether a native notification should be sent.</param>
public sealed record GlucoseAlertPresentation(
    GlucoseAlertKind Kind,
    string Title,
    string Message,
    string BadgeText,
    string ActionText,
    bool ShouldSendNativeNotification)
{
    /// <summary>
    /// Gets the empty alert presentation.
    /// </summary>
    public static GlucoseAlertPresentation None { get; } = new(
        GlucoseAlertKind.None,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        ShouldSendNativeNotification: false);

    /// <summary>
    /// Converts this presentation to a native OS notification.
    /// </summary>
    /// <returns>The native OS notification.</returns>
    public GlucoseAlertNativeNotification ToNativeNotification()
    {
        return new GlucoseAlertNativeNotification(Title, Message);
    }
}
