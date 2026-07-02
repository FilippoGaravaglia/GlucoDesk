namespace GlucoDesk.Desktop.GlucoseAlerts.Services;

/// <summary>
/// Provides system time for glucose alert cooldown evaluation.
/// </summary>
public sealed class SystemGlucoseAlertClock : IGlucoseAlertClock
{
    /// <summary>
    /// Gets the singleton system clock instance.
    /// </summary>
    public static SystemGlucoseAlertClock Instance { get; } = new();

    private SystemGlucoseAlertClock()
    {
    }

    /// <inheritdoc />
    public DateTimeOffset Now => DateTimeOffset.Now;
}
