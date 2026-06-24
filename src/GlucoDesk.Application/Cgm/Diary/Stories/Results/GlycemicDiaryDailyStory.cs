using GlucoDesk.Application.Cgm.Diary.Stories.Enums;

namespace GlucoDesk.Application.Cgm.Diary.Stories.Results;

/// <summary>
/// Represents the user-facing story for a single glycemic diary day.
/// </summary>
public sealed record GlycemicDiaryDailyStory
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GlycemicDiaryDailyStory"/> class.
    /// </summary>
    /// <param name="date">The diary date.</param>
    /// <param name="level">The story level.</param>
    /// <param name="headline">The short story headline.</param>
    /// <param name="summaryText">The story summary text.</param>
    /// <param name="dataQualityText">The data quality explanation.</param>
    /// <param name="highlights">The story highlights.</param>
    public GlycemicDiaryDailyStory(
        DateOnly date,
        GlycemicDiaryStoryLevel level,
        string headline,
        string summaryText,
        string dataQualityText,
        IReadOnlyCollection<string> highlights)
    {
        if (level == GlycemicDiaryStoryLevel.Unknown)
        {
            throw new ArgumentOutOfRangeException(
                nameof(level),
                level,
                "Daily story level must be specified.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(headline);
        ArgumentException.ThrowIfNullOrWhiteSpace(summaryText);
        ArgumentException.ThrowIfNullOrWhiteSpace(dataQualityText);
        ArgumentNullException.ThrowIfNull(highlights);

        Date = date;
        Level = level;
        Headline = headline;
        SummaryText = summaryText;
        DataQualityText = dataQualityText;
        Highlights = highlights;
    }

    /// <summary>
    /// Gets the diary date.
    /// </summary>
    public DateOnly Date { get; }

    /// <summary>
    /// Gets the story level.
    /// </summary>
    public GlycemicDiaryStoryLevel Level { get; }

    /// <summary>
    /// Gets the short story headline.
    /// </summary>
    public string Headline { get; }

    /// <summary>
    /// Gets the story summary text.
    /// </summary>
    public string SummaryText { get; }

    /// <summary>
    /// Gets the data quality explanation.
    /// </summary>
    public string DataQualityText { get; }

    /// <summary>
    /// Gets the story highlights.
    /// </summary>
    public IReadOnlyCollection<string> Highlights { get; }

    /// <summary>
    /// Gets whether this story requires cautious interpretation.
    /// </summary>
    public bool RequiresCaution => Level is
        GlycemicDiaryStoryLevel.NoData or
        GlycemicDiaryStoryLevel.Caution;
}
