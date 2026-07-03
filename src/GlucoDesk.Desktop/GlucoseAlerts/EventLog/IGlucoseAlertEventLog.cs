namespace GlucoDesk.Desktop.GlucoseAlerts.EventLog;

/// <summary>
/// Writes privacy-safe local glucose alert events.
/// </summary>
public interface IGlucoseAlertEventLog
{
    /// <summary>
    /// Writes a glucose alert event.
    /// </summary>
    /// <param name="eventEntry">The event entry.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous write operation.</returns>
    Task WriteAsync(
        GlucoseAlertEvent eventEntry,
        CancellationToken cancellationToken);
}
