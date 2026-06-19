namespace GlucoDesk.Application.Cgm.Diary.Results;

/// <summary>
/// Represents a daily glycemic diary entry.
/// </summary>
public sealed record GlycemicDiaryDailyEntry
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GlycemicDiaryDailyEntry"/> class.
    /// </summary>
    /// <param name="date">The local diary date.</param>
    /// <param name="readingsCount">The number of readings in the day.</param>
    /// <param name="averageMgDl">The daily average glucose value in mg/dL.</param>
    /// <param name="minimumMgDl">The daily minimum glucose value in mg/dL.</param>
    /// <param name="maximumMgDl">The daily maximum glucose value in mg/dL.</param>
    /// <param name="timeInRangePercentage">The daily time-in-range percentage.</param>
    /// <param name="dataCoveragePercentage">The estimated daily data coverage percentage.</param>
    /// <param name="isDataComplete">A value indicating whether the daily data is complete.</param>
    /// <param name="gapCount">The number of detected daily gaps.</param>
    /// <param name="timeBlocks">The standard time block entries.</param>
    public GlycemicDiaryDailyEntry(
        DateOnly date,
        int readingsCount,
        decimal? averageMgDl,
        decimal? minimumMgDl,
        decimal? maximumMgDl,
        decimal? timeInRangePercentage,
        decimal dataCoveragePercentage,
        bool isDataComplete,
        int gapCount,
        IReadOnlyCollection<GlycemicDiaryTimeBlockEntry> timeBlocks)
    {
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

        if (gapCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(gapCount),
                gapCount,
                "Gap count cannot be negative.");
        }

        ArgumentNullException.ThrowIfNull(timeBlocks);

        Date = date;
        ReadingsCount = readingsCount;
        AverageMgDl = averageMgDl;
        MinimumMgDl = minimumMgDl;
        MaximumMgDl = maximumMgDl;
        TimeInRangePercentage = timeInRangePercentage;
        DataCoveragePercentage = dataCoveragePercentage;
        IsDataComplete = isDataComplete;
        GapCount = gapCount;
        TimeBlocks = timeBlocks;
    }

    /// <summary>
    /// Gets the local diary date.
    /// </summary>
    public DateOnly Date { get; }

    /// <summary>
    /// Gets the number of readings in the day.
    /// </summary>
    public int ReadingsCount { get; }

    /// <summary>
    /// Gets the daily average glucose value in mg/dL.
    /// </summary>
    public decimal? AverageMgDl { get; }

    /// <summary>
    /// Gets the daily minimum glucose value in mg/dL.
    /// </summary>
    public decimal? MinimumMgDl { get; }

    /// <summary>
    /// Gets the daily maximum glucose value in mg/dL.
    /// </summary>
    public decimal? MaximumMgDl { get; }

    /// <summary>
    /// Gets the daily time-in-range percentage.
    /// </summary>
    public decimal? TimeInRangePercentage { get; }

    /// <summary>
    /// Gets the estimated daily data coverage percentage.
    /// </summary>
    public decimal DataCoveragePercentage { get; }

    /// <summary>
    /// Gets a value indicating whether the daily data is complete.
    /// </summary>
    public bool IsDataComplete { get; }

    /// <summary>
    /// Gets the number of detected daily gaps.
    /// </summary>
    public int GapCount { get; }

    /// <summary>
    /// Gets the standard time block entries.
    /// </summary>
    public IReadOnlyCollection<GlycemicDiaryTimeBlockEntry> TimeBlocks { get; }

    /// <summary>
    /// Gets a value indicating whether the day contains at least one reading.
    /// </summary>
    public bool HasData => ReadingsCount > 0;
}