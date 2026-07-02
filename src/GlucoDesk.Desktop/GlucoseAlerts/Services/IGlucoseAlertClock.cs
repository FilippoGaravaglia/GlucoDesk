namespace GlucoDesk.Desktop.GlucoseAlerts.Services;

/// <summary>
/// Provides the current time for glucose alert cooldown evaluation.
/// </summary>
public interface IGlucoseAlertClock
{
    /// <summary>
    /// Gets the current timestamp.
    /// </summary>
    DateTimeOffset Now { get; }
}
