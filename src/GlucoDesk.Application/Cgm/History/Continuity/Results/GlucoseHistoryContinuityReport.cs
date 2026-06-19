namespace GlucoDesk.Application.Cgm.History.Continuity.Results;

/// <summary>
/// Represents a continuity report for a local glucose history window.
/// </summary>
public sealed record GlucoseHistoryContinuityReport
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GlucoseHistoryContinuityReport"/> class.
    /// </summary>
    /// <param name="windowStartsAt">The analyzed window start timestamp.</param>
    /// <param name="windowEndsAt">The analyzed window end timestamp.</param>
    /// <param name="readingsCount">The number of readings found in the window.</param>
    /// <param name="dataCoveragePercentage">The estimated data coverage percentage.</param>
    /// <param name="gaps">The detected history gaps.</param>
    public GlucoseHistoryContinuityReport(
        DateTimeOffset windowStartsAt,
        DateTimeOffset windowEndsAt,
        int readingsCount,
        decimal dataCoveragePercentage,
        IReadOnlyCollection<GlucoseHistoryGap> gaps)
    {
        if (windowEndsAt <= windowStartsAt)
        {
            throw new ArgumentException(
                "Window end timestamp must be greater than window start timestamp.",
                nameof(windowEndsAt));
        }

        if (readingsCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(readingsCount),
                readingsCount,
                "Readings count cannot be negative.");
        }

        if (dataCoveragePercentage is < 0m or > 100m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(dataCoveragePercentage),
                dataCoveragePercentage,
                "Data coverage percentage must be between 0 and 100.");
        }

        ArgumentNullException.ThrowIfNull(gaps);

        WindowStartsAt = windowStartsAt;
        WindowEndsAt = windowEndsAt;
        ReadingsCount = readingsCount;
        DataCoveragePercentage = dataCoveragePercentage;
        Gaps = gaps;
    }

    /// <summary>
    /// Gets the analyzed window start timestamp.
    /// </summary>
    public DateTimeOffset WindowStartsAt { get; }

    /// <summary>
    /// Gets the analyzed window end timestamp.
    /// </summary>
    public DateTimeOffset WindowEndsAt { get; }

    /// <summary>
    /// Gets the number of readings found in the analyzed window.
    /// </summary>
    public int ReadingsCount { get; }

    /// <summary>
    /// Gets the estimated data coverage percentage.
    /// </summary>
    public decimal DataCoveragePercentage { get; }

    /// <summary>
    /// Gets the detected history gaps.
    /// </summary>
    public IReadOnlyCollection<GlucoseHistoryGap> Gaps { get; }

    /// <summary>
    /// Gets a value indicating whether the analyzed window has no detected gaps.
    /// </summary>
    public bool IsComplete => Gaps.Count == 0;

    /// <summary>
    /// Gets the analyzed window duration.
    /// </summary>
    public TimeSpan WindowDuration => WindowEndsAt - WindowStartsAt;

    /// <summary>
    /// Gets the total estimated missing readings.
    /// </summary>
    public int EstimatedMissingReadings => Gaps.Sum(gap => gap.EstimatedMissingReadings);
}