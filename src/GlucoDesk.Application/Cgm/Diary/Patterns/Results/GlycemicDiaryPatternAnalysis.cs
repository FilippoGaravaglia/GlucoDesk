namespace GlucoDesk.Application.Cgm.Diary.Patterns.Results;

/// <summary>
/// Represents the local pattern analysis for a glycemic diary report.
/// </summary>
public sealed record GlycemicDiaryPatternAnalysis
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GlycemicDiaryPatternAnalysis"/> class.
    /// </summary>
    /// <param name="periodStartsAt">The analyzed period start.</param>
    /// <param name="periodEndsAt">The analyzed period end.</param>
    /// <param name="analyzedDaysCount">The number of analyzed days.</param>
    /// <param name="daysWithDataCount">The number of days with local glucose data.</param>
    /// <param name="patterns">The detected patterns.</param>
    public GlycemicDiaryPatternAnalysis(
        DateTimeOffset periodStartsAt,
        DateTimeOffset periodEndsAt,
        int analyzedDaysCount,
        int daysWithDataCount,
        IReadOnlyCollection<GlycemicDiaryPattern> patterns)
    {
        if (periodEndsAt <= periodStartsAt)
        {
            throw new ArgumentOutOfRangeException(
                nameof(periodEndsAt),
                periodEndsAt,
                "Pattern analysis period end must be greater than start.");
        }

        if (analyzedDaysCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(analyzedDaysCount),
                analyzedDaysCount,
                "Analyzed days count cannot be negative.");
        }

        if (daysWithDataCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(daysWithDataCount),
                daysWithDataCount,
                "Days with data count cannot be negative.");
        }

        if (daysWithDataCount > analyzedDaysCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(daysWithDataCount),
                daysWithDataCount,
                "Days with data count cannot be greater than analyzed days count.");
        }

        ArgumentNullException.ThrowIfNull(patterns);

        PeriodStartsAt = periodStartsAt;
        PeriodEndsAt = periodEndsAt;
        AnalyzedDaysCount = analyzedDaysCount;
        DaysWithDataCount = daysWithDataCount;
        Patterns = patterns;
    }

    /// <summary>
    /// Gets the analyzed period start.
    /// </summary>
    public DateTimeOffset PeriodStartsAt { get; }

    /// <summary>
    /// Gets the analyzed period end.
    /// </summary>
    public DateTimeOffset PeriodEndsAt { get; }

    /// <summary>
    /// Gets the number of analyzed days.
    /// </summary>
    public int AnalyzedDaysCount { get; }

    /// <summary>
    /// Gets the number of days with local glucose data.
    /// </summary>
    public int DaysWithDataCount { get; }

    /// <summary>
    /// Gets the detected patterns.
    /// </summary>
    public IReadOnlyCollection<GlycemicDiaryPattern> Patterns { get; }

    /// <summary>
    /// Gets whether at least one pattern was detected.
    /// </summary>
    public bool HasPatterns => Patterns.Count > 0;

    /// <summary>
    /// Gets the number of detected time block patterns.
    /// </summary>
    public int TimeBlockPatternsCount => Patterns.Count(pattern => pattern.IsTimeBlockPattern);
}
