using GlucoDesk.Desktop.GlucoseAlerts.Models;

namespace GlucoDesk.Desktop.GlucoseAlerts.EventLog;

/// <summary>
/// Tracks whether the current glucose alert presentation was already logged.
/// </summary>
public sealed class GlucoseAlertPresentedEventState
{
    private GlucoseAlertKind _lastLoggedPresentedKind = GlucoseAlertKind.None;

    /// <summary>
    /// Gets the last alert kind logged as presented.
    /// </summary>
    public GlucoseAlertKind LastLoggedPresentedKind => _lastLoggedPresentedKind;

    /// <summary>
    /// Returns whether a presented event should be logged for the specified alert kind.
    /// </summary>
    /// <param name="alertKind">The alert kind.</param>
    /// <returns>True when the presented event should be logged; otherwise false.</returns>
    public bool ShouldLogPresented(GlucoseAlertKind alertKind)
    {
        if (alertKind == GlucoseAlertKind.None)
        {
            Reset();
            return false;
        }

        if (_lastLoggedPresentedKind == alertKind)
        {
            return false;
        }

        _lastLoggedPresentedKind = alertKind;

        return true;
    }

    /// <summary>
    /// Resets the current presented-event tracking state.
    /// </summary>
    public void Reset()
    {
        _lastLoggedPresentedKind = GlucoseAlertKind.None;
    }
}
