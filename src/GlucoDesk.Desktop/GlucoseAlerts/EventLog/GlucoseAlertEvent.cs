using GlucoDesk.Desktop.GlucoseAlerts.Models;

namespace GlucoDesk.Desktop.GlucoseAlerts.EventLog;

/// <summary>
/// Represents a privacy-safe local glucose alert event.
/// </summary>
/// <param name="Timestamp">The event timestamp.</param>
/// <param name="EventKind">The event kind.</param>
/// <param name="AlertKind">The glucose alert kind.</param>
/// <param name="Message">The privacy-safe event message.</param>
public sealed record GlucoseAlertEvent(
    DateTimeOffset Timestamp,
    GlucoseAlertEventKind EventKind,
    GlucoseAlertKind AlertKind,
    string Message);
