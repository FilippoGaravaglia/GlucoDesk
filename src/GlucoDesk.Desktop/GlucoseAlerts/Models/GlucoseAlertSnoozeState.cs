namespace GlucoDesk.Desktop.GlucoseAlerts.Models;

/// <summary>
/// Tracks a temporary user snooze for the current glucose awareness alert condition.
/// </summary>
public sealed class GlucoseAlertSnoozeState
{
    private GlucoseAlertKind _snoozedKind = GlucoseAlertKind.None;
    private DateTimeOffset? _snoozedUntil;

    /// <summary>
    /// Gets the currently snoozed alert kind.
    /// </summary>
    public GlucoseAlertKind SnoozedKind => _snoozedKind;

    /// <summary>
    /// Gets the moment until which the current alert kind is snoozed.
    /// </summary>
    public DateTimeOffset? SnoozedUntil => _snoozedUntil;

    /// <summary>
    /// Snoozes the specified alert kind for the specified duration.
    /// </summary>
    /// <param name="kind">The alert kind to snooze.</param>
    /// <param name="duration">The snooze duration.</param>
    /// <param name="now">The current time.</param>
    /// <returns>The moment until which the alert kind is snoozed.</returns>
    public DateTimeOffset Snooze(
        GlucoseAlertKind kind,
        TimeSpan duration,
        DateTimeOffset now)
    {
        if (kind == GlucoseAlertKind.None)
        {
            Clear();
            return now;
        }

        if (duration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(duration),
                duration,
                "Snooze duration must be greater than zero.");
        }

        _snoozedKind = kind;
        _snoozedUntil = now.Add(duration);

        return _snoozedUntil.Value;
    }

    /// <summary>
    /// Checks whether the specified alert kind is currently snoozed.
    /// </summary>
    /// <param name="kind">The alert kind to check.</param>
    /// <param name="now">The current time.</param>
    /// <returns>True when the specified alert kind is currently snoozed; otherwise false.</returns>
    public bool IsSnoozed(
        GlucoseAlertKind kind,
        DateTimeOffset now)
    {
        if (kind == GlucoseAlertKind.None ||
            _snoozedKind != kind ||
            _snoozedUntil is null)
        {
            return false;
        }

        if (now >= _snoozedUntil.Value)
        {
            Clear();
            return false;
        }

        return true;
    }

    /// <summary>
    /// Gets the remaining snooze duration for the specified alert kind.
    /// </summary>
    /// <param name="kind">The alert kind.</param>
    /// <param name="now">The current time.</param>
    /// <returns>The remaining snooze duration, or zero when the kind is not snoozed.</returns>
    public TimeSpan GetRemaining(
        GlucoseAlertKind kind,
        DateTimeOffset now)
    {
        if (!IsSnoozed(kind, now) ||
            _snoozedUntil is null)
        {
            return TimeSpan.Zero;
        }

        return _snoozedUntil.Value - now;
    }

    /// <summary>
    /// Clears the current snooze state.
    /// </summary>
    public void Clear()
    {
        _snoozedKind = GlucoseAlertKind.None;
        _snoozedUntil = null;
    }
}
