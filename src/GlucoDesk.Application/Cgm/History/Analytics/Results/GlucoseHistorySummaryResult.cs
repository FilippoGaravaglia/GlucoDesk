namespace GlucoDesk.Application.Cgm.History.Analytics.Results;

/// <summary>
/// Represents summary analytics calculated from local glucose history.
/// </summary>
public sealed record GlucoseHistorySummaryResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GlucoseHistorySummaryResult"/> class.
    /// </summary>
    /// <param name="from">The summary start timestamp.</param>
    /// <param name="to">The summary end timestamp.</param>
    /// <param name="readingsCount">The readings count.</param>
    /// <param name="averageMgDl">The average glucose value in mg/dL.</param>
    /// <param name="minimumMgDl">The minimum glucose value in mg/dL.</param>
    /// <param name="maximumMgDl">The maximum glucose value in mg/dL.</param>
    /// <param name="inRangeCount">The number of readings in range.</param>
    /// <param name="belowRangeCount">The number of readings below range.</param>
    /// <param name="aboveRangeCount">The number of readings above range.</param>
    public GlucoseHistorySummaryResult(
        DateTimeOffset from,
        DateTimeOffset to,
        int readingsCount,
        decimal? averageMgDl,
        decimal? minimumMgDl,
        decimal? maximumMgDl,
        int inRangeCount,
        int belowRangeCount,
        int aboveRangeCount)
    {
        if (to <= from)
        {
            throw new ArgumentOutOfRangeException(
                nameof(to),
                to,
                "Summary end timestamp must be greater than start timestamp.");
        }

        if (readingsCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(readingsCount),
                readingsCount,
                "Readings count cannot be negative.");
        }

        if (inRangeCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(inRangeCount),
                inRangeCount,
                "In-range count cannot be negative.");
        }

        if (belowRangeCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(belowRangeCount),
                belowRangeCount,
                "Below-range count cannot be negative.");
        }

        if (aboveRangeCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(aboveRangeCount),
                aboveRangeCount,
                "Above-range count cannot be negative.");
        }

        if (inRangeCount + belowRangeCount + aboveRangeCount != readingsCount)
        {
            throw new ArgumentException(
                "Range bucket counts must match the readings count.",
                nameof(readingsCount));
        }

        From = from;
        To = to;
        ReadingsCount = readingsCount;
        AverageMgDl = averageMgDl;
        MinimumMgDl = minimumMgDl;
        MaximumMgDl = maximumMgDl;
        InRangeCount = inRangeCount;
        BelowRangeCount = belowRangeCount;
        AboveRangeCount = aboveRangeCount;
    }

    /// <summary>
    /// Gets the summary start timestamp.
    /// </summary>
    public DateTimeOffset From { get; }

    /// <summary>
    /// Gets the summary end timestamp.
    /// </summary>
    public DateTimeOffset To { get; }

    /// <summary>
    /// Gets the readings count.
    /// </summary>
    public int ReadingsCount { get; }

    /// <summary>
    /// Gets the average glucose value in mg/dL.
    /// </summary>
    public decimal? AverageMgDl { get; }

    /// <summary>
    /// Gets the minimum glucose value in mg/dL.
    /// </summary>
    public decimal? MinimumMgDl { get; }

    /// <summary>
    /// Gets the maximum glucose value in mg/dL.
    /// </summary>
    public decimal? MaximumMgDl { get; }

    /// <summary>
    /// Gets the number of readings in range.
    /// </summary>
    public int InRangeCount { get; }

    /// <summary>
    /// Gets the number of readings below range.
    /// </summary>
    public int BelowRangeCount { get; }

    /// <summary>
    /// Gets the number of readings above range.
    /// </summary>
    public int AboveRangeCount { get; }

    /// <summary>
    /// Gets the in-range percentage.
    /// </summary>
    public decimal InRangePercentage => CalculatePercentage(InRangeCount, ReadingsCount);

    /// <summary>
    /// Gets the below-range percentage.
    /// </summary>
    public decimal BelowRangePercentage => CalculatePercentage(BelowRangeCount, ReadingsCount);

    /// <summary>
    /// Gets the above-range percentage.
    /// </summary>
    public decimal AboveRangePercentage => CalculatePercentage(AboveRangeCount, ReadingsCount);

    /// <summary>
    /// Gets a value indicating whether the summary contains at least one reading.
    /// </summary>
    public bool HasReadings => ReadingsCount > 0;

    #region Helpers

    /// <summary>
    /// Calculates a percentage value.
    /// </summary>
    /// <param name="part">The percentage part.</param>
    /// <param name="total">The percentage total.</param>
    /// <returns>The calculated percentage.</returns>
    private static decimal CalculatePercentage(
        int part,
        int total)
    {
        if (total == 0)
        {
            return 0m;
        }

        return Math.Round(part * 100m / total, 2, MidpointRounding.AwayFromZero);
    }

    #endregion
}