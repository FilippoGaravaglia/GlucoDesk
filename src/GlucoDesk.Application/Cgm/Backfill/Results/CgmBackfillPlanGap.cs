namespace GlucoDesk.Application.Cgm.Backfill.Results;

/// <summary>
/// Represents a recoverable gap included in a historical CGM backfill plan.
/// </summary>
/// <param name="OriginalStartsAt">The original detected gap start timestamp.</param>
/// <param name="OriginalEndsAt">The original detected gap end timestamp.</param>
/// <param name="StartsAt">The planned recoverable start timestamp.</param>
/// <param name="EndsAt">The planned recoverable end timestamp.</param>
/// <param name="WasClampedByMaximumLookback">A value indicating whether the gap was clamped by the provider maximum lookback.</param>
public sealed record CgmBackfillPlanGap(
    DateTimeOffset OriginalStartsAt,
    DateTimeOffset OriginalEndsAt,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    bool WasClampedByMaximumLookback)
{
    /// <summary>
    /// Gets the planned recoverable gap duration.
    /// </summary>
    public TimeSpan Duration => EndsAt - StartsAt;
}