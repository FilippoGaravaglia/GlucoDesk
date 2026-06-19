using GlucoDesk.Application.Cgm.History.Continuity.Enums;

namespace GlucoDesk.Application.Cgm.History.Continuity.Results;

/// <summary>
/// Represents a detected gap in local glucose history.
/// </summary>
public sealed record GlucoseHistoryGap
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GlucoseHistoryGap"/> class.
    /// </summary>
    /// <param name="kind">The gap kind.</param>
    /// <param name="startsAt">The gap start timestamp.</param>
    /// <param name="endsAt">The gap end timestamp.</param>
    /// <param name="estimatedMissingReadings">The estimated number of missing readings.</param>
    public GlucoseHistoryGap(
        GlucoseHistoryGapKind kind,
        DateTimeOffset startsAt,
        DateTimeOffset endsAt,
        int estimatedMissingReadings)
    {
        if (endsAt < startsAt)
        {
            throw new ArgumentException(
                "Gap end timestamp cannot be earlier than gap start timestamp.",
                nameof(endsAt));
        }

        if (estimatedMissingReadings < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(estimatedMissingReadings),
                estimatedMissingReadings,
                "Estimated missing readings cannot be negative.");
        }

        Kind = kind;
        StartsAt = startsAt;
        EndsAt = endsAt;
        EstimatedMissingReadings = estimatedMissingReadings;
    }

    /// <summary>
    /// Gets the gap kind.
    /// </summary>
    public GlucoseHistoryGapKind Kind { get; }

    /// <summary>
    /// Gets the gap start timestamp.
    /// </summary>
    public DateTimeOffset StartsAt { get; }

    /// <summary>
    /// Gets the gap end timestamp.
    /// </summary>
    public DateTimeOffset EndsAt { get; }

    /// <summary>
    /// Gets the estimated number of missing readings.
    /// </summary>
    public int EstimatedMissingReadings { get; }

    /// <summary>
    /// Gets the gap duration.
    /// </summary>
    public TimeSpan Duration => EndsAt - StartsAt;
}