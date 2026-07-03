namespace GlucoDesk.Desktop.GlucoseAlerts.EventLog;

/// <summary>
/// Ignores glucose alert events.
/// </summary>
public sealed class NoOpGlucoseAlertEventLog : IGlucoseAlertEventLog
{
    /// <inheritdoc />
    public Task WriteAsync(
        GlucoseAlertEvent eventEntry,
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
