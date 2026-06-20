namespace GlucoDesk.Application.Cgm.Backfill.Requests;

/// <summary>
/// Represents a local historical glucose gap detected before planning a backfill operation.
/// </summary>
/// <param name="StartsAt">The original gap start timestamp.</param>
/// <param name="EndsAt">The original gap end timestamp.</param>
public sealed record CgmBackfillDetectedGap(
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt)
{
    /// <summary>
    /// Gets the gap duration.
    /// </summary>
    public TimeSpan Duration => EndsAt - StartsAt;
}