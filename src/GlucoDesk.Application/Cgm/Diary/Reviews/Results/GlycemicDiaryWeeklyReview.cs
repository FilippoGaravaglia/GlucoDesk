using GlucoDesk.Application.Cgm.Diary.Reviews.Enums;

namespace GlucoDesk.Application.Cgm.Diary.Reviews.Results;

/// <summary>
/// Represents a comparison-based weekly glycemic diary review.
/// </summary>
public sealed record GlycemicDiaryWeeklyReview
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GlycemicDiaryWeeklyReview"/> class.
    /// </summary>
    /// <param name="previousPeriodStartsAt">The previous period start.</param>
    /// <param name="previousPeriodEndsAt">The previous period end.</param>
    /// <param name="currentPeriodStartsAt">The current period start.</param>
    /// <param name="currentPeriodEndsAt">The current period end.</param>
    /// <param name="headline">The review headline.</param>
    /// <param name="summaryText">The review summary.</param>
    /// <param name="currentHistoryReliabilityText">The current history reliability text.</param>
    /// <param name="changes">The metric changes.</param>
    /// <param name="highlights">The review highlights.</param>
    public GlycemicDiaryWeeklyReview(
        DateTimeOffset previousPeriodStartsAt,
        DateTimeOffset previousPeriodEndsAt,
        DateTimeOffset currentPeriodStartsAt,
        DateTimeOffset currentPeriodEndsAt,
        string headline,
        string summaryText,
        string currentHistoryReliabilityText,
        IReadOnlyCollection<GlycemicDiaryMetricChange> changes,
        IReadOnlyCollection<string> highlights)
    {
        if (previousPeriodEndsAt <= previousPeriodStartsAt)
        {
            throw new ArgumentOutOfRangeException(nameof(previousPeriodEndsAt), previousPeriodEndsAt, "Previous period end must be greater than start.");
        }

        if (currentPeriodEndsAt <= currentPeriodStartsAt)
        {
            throw new ArgumentOutOfRangeException(nameof(currentPeriodEndsAt), currentPeriodEndsAt, "Current period end must be greater than start.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(headline);
        ArgumentException.ThrowIfNullOrWhiteSpace(summaryText);
        ArgumentException.ThrowIfNullOrWhiteSpace(currentHistoryReliabilityText);
        ArgumentNullException.ThrowIfNull(changes);
        ArgumentNullException.ThrowIfNull(highlights);

        PreviousPeriodStartsAt = previousPeriodStartsAt;
        PreviousPeriodEndsAt = previousPeriodEndsAt;
        CurrentPeriodStartsAt = currentPeriodStartsAt;
        CurrentPeriodEndsAt = currentPeriodEndsAt;
        Headline = headline;
        SummaryText = summaryText;
        CurrentHistoryReliabilityText = currentHistoryReliabilityText;
        Changes = changes;
        Highlights = highlights;
    }

    /// <summary>
    /// Gets the previous period start.
    /// </summary>
    public DateTimeOffset PreviousPeriodStartsAt { get; }

    /// <summary>
    /// Gets the previous period end.
    /// </summary>
    public DateTimeOffset PreviousPeriodEndsAt { get; }

    /// <summary>
    /// Gets the current period start.
    /// </summary>
    public DateTimeOffset CurrentPeriodStartsAt { get; }

    /// <summary>
    /// Gets the current period end.
    /// </summary>
    public DateTimeOffset CurrentPeriodEndsAt { get; }

    /// <summary>
    /// Gets the review headline.
    /// </summary>
    public string Headline { get; }

    /// <summary>
    /// Gets the review summary.
    /// </summary>
    public string SummaryText { get; }

    /// <summary>
    /// Gets the current history reliability text.
    /// </summary>
    public string CurrentHistoryReliabilityText { get; }

    /// <summary>
    /// Gets the metric changes.
    /// </summary>
    public IReadOnlyCollection<GlycemicDiaryMetricChange> Changes { get; }

    /// <summary>
    /// Gets the review highlights.
    /// </summary>
    public IReadOnlyCollection<string> Highlights { get; }

    /// <summary>
    /// Gets whether the review contains caution or important signals.
    /// </summary>
    public bool RequiresCaution => Changes.Any(change =>
        change.Severity is GlycemicDiaryReviewSignalSeverity.Caution or GlycemicDiaryReviewSignalSeverity.Important);

    /// <summary>
    /// Gets the number of meaningful changes.
    /// </summary>
    public int MeaningfulChangesCount => Changes.Count(change => change.HasMeaningfulChange);
}
