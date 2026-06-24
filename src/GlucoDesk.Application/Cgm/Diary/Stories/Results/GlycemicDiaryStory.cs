using GlucoDesk.Application.Cgm.Diary.Stories.Enums;

namespace GlucoDesk.Application.Cgm.Diary.Stories.Results;

/// <summary>
/// Represents a user-facing glycemic diary story for a selected period.
/// </summary>
public sealed record GlycemicDiaryStory
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GlycemicDiaryStory"/> class.
    /// </summary>
    /// <param name="periodStartsAt">The story period start.</param>
    /// <param name="periodEndsAt">The story period end.</param>
    /// <param name="level">The overall story level.</param>
    /// <param name="headline">The story headline.</param>
    /// <param name="summaryText">The story summary text.</param>
    /// <param name="historyReliabilityText">The local history reliability text.</param>
    /// <param name="dailyStories">The daily stories.</param>
    public GlycemicDiaryStory(
        DateTimeOffset periodStartsAt,
        DateTimeOffset periodEndsAt,
        GlycemicDiaryStoryLevel level,
        string headline,
        string summaryText,
        string historyReliabilityText,
        IReadOnlyCollection<GlycemicDiaryDailyStory> dailyStories)
    {
        if (periodEndsAt <= periodStartsAt)
        {
            throw new ArgumentOutOfRangeException(
                nameof(periodEndsAt),
                periodEndsAt,
                "Story period end must be greater than start.");
        }

        if (level == GlycemicDiaryStoryLevel.Unknown)
        {
            throw new ArgumentOutOfRangeException(
                nameof(level),
                level,
                "Story level must be specified.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(headline);
        ArgumentException.ThrowIfNullOrWhiteSpace(summaryText);
        ArgumentException.ThrowIfNullOrWhiteSpace(historyReliabilityText);
        ArgumentNullException.ThrowIfNull(dailyStories);

        PeriodStartsAt = periodStartsAt;
        PeriodEndsAt = periodEndsAt;
        Level = level;
        Headline = headline;
        SummaryText = summaryText;
        HistoryReliabilityText = historyReliabilityText;
        DailyStories = dailyStories;
    }

    /// <summary>
    /// Gets the story period start.
    /// </summary>
    public DateTimeOffset PeriodStartsAt { get; }

    /// <summary>
    /// Gets the story period end.
    /// </summary>
    public DateTimeOffset PeriodEndsAt { get; }

    /// <summary>
    /// Gets the overall story level.
    /// </summary>
    public GlycemicDiaryStoryLevel Level { get; }

    /// <summary>
    /// Gets the story headline.
    /// </summary>
    public string Headline { get; }

    /// <summary>
    /// Gets the story summary text.
    /// </summary>
    public string SummaryText { get; }

    /// <summary>
    /// Gets the local history reliability text.
    /// </summary>
    public string HistoryReliabilityText { get; }

    /// <summary>
    /// Gets the daily stories.
    /// </summary>
    public IReadOnlyCollection<GlycemicDiaryDailyStory> DailyStories { get; }

    /// <summary>
    /// Gets the number of daily stories requiring cautious interpretation.
    /// </summary>
    public int CautionDaysCount => DailyStories.Count(day => day.RequiresCaution);

    /// <summary>
    /// Gets the number of days without local glucose data.
    /// </summary>
    public int NoDataDaysCount => DailyStories.Count(day => day.Level == GlycemicDiaryStoryLevel.NoData);
}
