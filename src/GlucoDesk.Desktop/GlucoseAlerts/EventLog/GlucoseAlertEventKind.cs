namespace GlucoDesk.Desktop.GlucoseAlerts.EventLog;

/// <summary>
/// Represents a glucose alert lifecycle event kind.
/// </summary>
public enum GlucoseAlertEventKind
{
    /// <summary>
    /// The alert banner was presented.
    /// </summary>
    Presented,

    /// <summary>
    /// The alert banner was dismissed.
    /// </summary>
    Dismissed,

    /// <summary>
    /// The alert banner was snoozed.
    /// </summary>
    Snoozed,

    /// <summary>
    /// A native OS notification was requested.
    /// </summary>
    NativeNotificationRequested
}
