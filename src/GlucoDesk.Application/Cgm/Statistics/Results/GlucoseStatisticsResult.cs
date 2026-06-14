using GlucoDesk.Core.Glucose.Enums;

namespace GlucoDesk.Application.Cgm.Statistics.Results;

/// <summary>
/// Represents calculated glucose statistics for a local history interval.
/// </summary>
public sealed record GlucoseStatisticsResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GlucoseStatisticsResult"/> class.
    /// </summary>
    /// <param name="from">The inclusive start timestamp.</param>
    /// <param name="to">The inclusive end timestamp.</param>
    /// <param name="unit">The statistics glucose unit.</param>
    /// <param name="includeMockData">A value indicating whether Mock data was included.</param>
    /// <param name="loadedReadingsCount">The number of readings loaded from history.</param>
    /// <param name="analyzedReadingsCount">The number of readings used for statistics.</param>
    /// <param name="ignoredMockReadingsCount">The number of ignored Mock readings.</param>
    /// <param name="ignoredDifferentUnitReadingsCount">The number of ignored readings with a different unit.</param>
    /// <param name="averageGlucose">The average glucose value.</param>
    /// <param name="minimumGlucose">The minimum glucose value.</param>
    /// <param name="maximumGlucose">The maximum glucose value.</param>
    /// <param name="belowRangeCount">The number of readings below target range.</param>
    /// <param name="inRangeCount">The number of readings within target range.</param>
    /// <param name="aboveRangeCount">The number of readings above target range.</param>
    /// <param name="firstReadingAt">The first analyzed reading timestamp.</param>
    /// <param name="lastReadingAt">The last analyzed reading timestamp.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when one of the counts is invalid.</exception>
    public GlucoseStatisticsResult(
        DateTimeOffset from,
        DateTimeOffset to,
        GlucoseUnit unit,
        bool includeMockData,
        int loadedReadingsCount,
        int analyzedReadingsCount,
        int ignoredMockReadingsCount,
        int ignoredDifferentUnitReadingsCount,
        decimal? averageGlucose,
        decimal? minimumGlucose,
        decimal? maximumGlucose,
        int belowRangeCount,
        int inRangeCount,
        int aboveRangeCount,
        DateTimeOffset? firstReadingAt,
        DateTimeOffset? lastReadingAt)
    {
        ValidateNonNegative(loadedReadingsCount, nameof(loadedReadingsCount));
        ValidateNonNegative(analyzedReadingsCount, nameof(analyzedReadingsCount));
        ValidateNonNegative(ignoredMockReadingsCount, nameof(ignoredMockReadingsCount));
        ValidateNonNegative(ignoredDifferentUnitReadingsCount, nameof(ignoredDifferentUnitReadingsCount));
        ValidateNonNegative(belowRangeCount, nameof(belowRangeCount));
        ValidateNonNegative(inRangeCount, nameof(inRangeCount));
        ValidateNonNegative(aboveRangeCount, nameof(aboveRangeCount));

        From = from;
        To = to;
        Unit = unit;
        IncludeMockData = includeMockData;
        LoadedReadingsCount = loadedReadingsCount;
        AnalyzedReadingsCount = analyzedReadingsCount;
        IgnoredMockReadingsCount = ignoredMockReadingsCount;
        IgnoredDifferentUnitReadingsCount = ignoredDifferentUnitReadingsCount;
        AverageGlucose = averageGlucose;
        MinimumGlucose = minimumGlucose;
        MaximumGlucose = maximumGlucose;
        BelowRangeCount = belowRangeCount;
        InRangeCount = inRangeCount;
        AboveRangeCount = aboveRangeCount;
        FirstReadingAt = firstReadingAt;
        LastReadingAt = lastReadingAt;

        BelowRangePercentage = CalculatePercentage(belowRangeCount, analyzedReadingsCount);
        InRangePercentage = CalculatePercentage(inRangeCount, analyzedReadingsCount);
        AboveRangePercentage = CalculatePercentage(aboveRangeCount, analyzedReadingsCount);
    }

    /// <summary>
    /// Gets the inclusive start timestamp.
    /// </summary>
    public DateTimeOffset From { get; }

    /// <summary>
    /// Gets the inclusive end timestamp.
    /// </summary>
    public DateTimeOffset To { get; }

    /// <summary>
    /// Gets the statistics glucose unit.
    /// </summary>
    public GlucoseUnit Unit { get; }

    /// <summary>
    /// Gets a value indicating whether Mock data was included.
    /// </summary>
    public bool IncludeMockData { get; }

    /// <summary>
    /// Gets the number of readings loaded from history.
    /// </summary>
    public int LoadedReadingsCount { get; }

    /// <summary>
    /// Gets the number of readings used for statistics.
    /// </summary>
    public int AnalyzedReadingsCount { get; }

    /// <summary>
    /// Gets the number of ignored Mock readings.
    /// </summary>
    public int IgnoredMockReadingsCount { get; }

    /// <summary>
    /// Gets the number of ignored readings with a different unit.
    /// </summary>
    public int IgnoredDifferentUnitReadingsCount { get; }

    /// <summary>
    /// Gets the average glucose value.
    /// </summary>
    public decimal? AverageGlucose { get; }

    /// <summary>
    /// Gets the minimum glucose value.
    /// </summary>
    public decimal? MinimumGlucose { get; }

    /// <summary>
    /// Gets the maximum glucose value.
    /// </summary>
    public decimal? MaximumGlucose { get; }

    /// <summary>
    /// Gets the number of readings below target range.
    /// </summary>
    public int BelowRangeCount { get; }

    /// <summary>
    /// Gets the number of readings within target range.
    /// </summary>
    public int InRangeCount { get; }

    /// <summary>
    /// Gets the number of readings above target range.
    /// </summary>
    public int AboveRangeCount { get; }

    /// <summary>
    /// Gets the percentage of readings below target range.
    /// </summary>
    public decimal BelowRangePercentage { get; }

    /// <summary>
    /// Gets the percentage of readings within target range.
    /// </summary>
    public decimal InRangePercentage { get; }

    /// <summary>
    /// Gets the percentage of readings above target range.
    /// </summary>
    public decimal AboveRangePercentage { get; }

    /// <summary>
    /// Gets the first analyzed reading timestamp.
    /// </summary>
    public DateTimeOffset? FirstReadingAt { get; }

    /// <summary>
    /// Gets the last analyzed reading timestamp.
    /// </summary>
    public DateTimeOffset? LastReadingAt { get; }

    /// <summary>
    /// Gets a value indicating whether the statistics contain analyzed readings.
    /// </summary>
    public bool HasData => AnalyzedReadingsCount > 0;

    /// <summary>
    /// Creates an empty statistics result.
    /// </summary>
    /// <param name="from">The inclusive start timestamp.</param>
    /// <param name="to">The inclusive end timestamp.</param>
    /// <param name="unit">The statistics glucose unit.</param>
    /// <param name="includeMockData">A value indicating whether Mock data was included.</param>
    /// <param name="loadedReadingsCount">The number of readings loaded from history.</param>
    /// <param name="ignoredMockReadingsCount">The number of ignored Mock readings.</param>
    /// <param name="ignoredDifferentUnitReadingsCount">The number of ignored readings with a different unit.</param>
    /// <returns>The empty statistics result.</returns>
    public static GlucoseStatisticsResult Empty(
        DateTimeOffset from,
        DateTimeOffset to,
        GlucoseUnit unit,
        bool includeMockData,
        int loadedReadingsCount,
        int ignoredMockReadingsCount,
        int ignoredDifferentUnitReadingsCount)
    {
        return new GlucoseStatisticsResult(
            from,
            to,
            unit,
            includeMockData,
            loadedReadingsCount,
            0,
            ignoredMockReadingsCount,
            ignoredDifferentUnitReadingsCount,
            null,
            null,
            null,
            0,
            0,
            0,
            null,
            null);
    }

    #region Helpers

    /// <summary>
    /// Validates that a count is not negative.
    /// </summary>
    /// <param name="value">The count value.</param>
    /// <param name="parameterName">The parameter name.</param>
    private static void ValidateNonNegative(int value, string parameterName)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "Count cannot be negative.");
        }
    }

    /// <summary>
    /// Calculates a rounded percentage from a count and total.
    /// </summary>
    /// <param name="count">The count.</param>
    /// <param name="total">The total.</param>
    /// <returns>The rounded percentage.</returns>
    private static decimal CalculatePercentage(int count, int total)
    {
        if (total <= 0)
        {
            return 0;
        }

        return Math.Round(count * 100m / total, 1, MidpointRounding.AwayFromZero);
    }

    #endregion
}