using GlucoDesk.Application.Cgm.History.Continuity.Results;

namespace GlucoDesk.Application.Cgm.Diary.Results;

/// <summary>
/// Represents a glycemic diary report for a requested period.
/// </summary>
public sealed record GlycemicDiaryReport
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GlycemicDiaryReport"/> class.
    /// </summary>
    /// <param name="periodStartsAt">The diary period start timestamp.</param>
    /// <param name="periodEndsAt">The diary period end timestamp.</param>
    /// <param name="readingsCount">The number of readings in the period.</param>
    /// <param name="averageMgDl">The period average glucose value in mg/dL.</param>
    /// <param name="minimumMgDl">The period minimum glucose value in mg/dL.</param>
    /// <param name="maximumMgDl">The period maximum glucose value in mg/dL.</param>
    /// <param name="timeInRangePercentage">The period time-in-range percentage.</param>
    /// <param name="overallContinuity">The overall data continuity report.</param>
    /// <param name="dailyEntries">The daily diary entries.</param>
    public GlycemicDiaryReport(
        DateTimeOffset periodStartsAt,
        DateTimeOffset periodEndsAt,
        int readingsCount,
        decimal? averageMgDl,
        decimal? minimumMgDl,
        decimal? maximumMgDl,
        decimal? timeInRangePercentage,
        GlucoseHistoryContinuityReport overallContinuity,
        IReadOnlyCollection<GlycemicDiaryDailyEntry> dailyEntries)
    {
        if (periodEndsAt <= periodStartsAt)
        {
            throw new ArgumentOutOfRangeException(
                nameof(periodEndsAt),
                periodEndsAt,
                "Diary period end timestamp must be greater than start timestamp.");
        }

        if (readingsCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(readingsCount),
                readingsCount,
                "Readings count cannot be negative.");
        }

        ArgumentNullException.ThrowIfNull(overallContinuity);
        ArgumentNullException.ThrowIfNull(dailyEntries);

        PeriodStartsAt = periodStartsAt;
        PeriodEndsAt = periodEndsAt;
        ReadingsCount = readingsCount;
        AverageMgDl = averageMgDl;
        MinimumMgDl = minimumMgDl;
        MaximumMgDl = maximumMgDl;
        TimeInRangePercentage = timeInRangePercentage;
        OverallContinuity = overallContinuity;
        DailyEntries = dailyEntries;
    }

    /// <summary>
    /// Gets the diary period start timestamp.
    /// </summary>
    public DateTimeOffset PeriodStartsAt { get; }

    /// <summary>
    /// Gets the diary period end timestamp.
    /// </summary>
    public DateTimeOffset PeriodEndsAt { get; }

    /// <summary>
    /// Gets the number of readings in the period.
    /// </summary>
    public int ReadingsCount { get; }

    /// <summary>
    /// Gets the period average glucose value in mg/dL.
    /// </summary>
    public decimal? AverageMgDl { get; }

    /// <summary>
    /// Gets the period minimum glucose value in mg/dL.
    /// </summary>
    public decimal? MinimumMgDl { get; }

    /// <summary>
    /// Gets the period maximum glucose value in mg/dL.
    /// </summary>
    public decimal? MaximumMgDl { get; }

    /// <summary>
    /// Gets the period time-in-range percentage.
    /// </summary>
    public decimal? TimeInRangePercentage { get; }

    /// <summary>
    /// Gets the overall data continuity report.
    /// </summary>
    public GlucoseHistoryContinuityReport OverallContinuity { get; }

    /// <summary>
    /// Gets the daily diary entries.
    /// </summary>
    public IReadOnlyCollection<GlycemicDiaryDailyEntry> DailyEntries { get; }

    /// <summary>
    /// Gets the number of days with incomplete data.
    /// </summary>
    public int IncompleteDaysCount => DailyEntries.Count(day => !day.IsDataComplete);

    /// <summary>
    /// Gets the number of days without data.
    /// </summary>
    public int EmptyDaysCount => DailyEntries.Count(day => !day.HasData);
}