namespace GlucoDesk.Desktop.GlucoseAlerts.Models;

/// <summary>
/// Represents the glucose awareness alert kind.
/// </summary>
public enum GlucoseAlertKind
{
    /// <summary>
    /// No glucose alert is active.
    /// </summary>
    None = 0,

    /// <summary>
    /// The current glucose value is below the configured target range.
    /// </summary>
    Low = 1,

    /// <summary>
    /// The current glucose value is above the configured target range.
    /// </summary>
    High = 2
}
