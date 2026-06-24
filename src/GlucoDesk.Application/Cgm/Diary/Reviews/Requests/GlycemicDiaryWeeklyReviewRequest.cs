namespace GlucoDesk.Application.Cgm.Diary.Reviews.Requests;

/// <summary>
/// Represents a request to generate a comparison-based weekly glycemic diary review.
/// </summary>
public sealed record GlycemicDiaryWeeklyReviewRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GlycemicDiaryWeeklyReviewRequest"/> class.
    /// </summary>
    /// <param name="currentPeriodStartsAt">The current period start timestamp.</param>
    /// <param name="currentPeriodEndsAt">The current period end timestamp.</param>
    /// <param name="previousPeriodStartsAt">The optional previous period start timestamp.</param>
    /// <param name="previousPeriodEndsAt">The optional previous period end timestamp.</param>
    public GlycemicDiaryWeeklyReviewRequest(
        DateTimeOffset currentPeriodStartsAt,
        DateTimeOffset currentPeriodEndsAt,
        DateTimeOffset? previousPeriodStartsAt = null,
        DateTimeOffset? previousPeriodEndsAt = null)
    {
        if (currentPeriodEndsAt <= currentPeriodStartsAt)
        {
            throw new ArgumentOutOfRangeException(
                nameof(currentPeriodEndsAt),
                currentPeriodEndsAt,
                "Current period end timestamp must be greater than start timestamp.");
        }

        if (previousPeriodStartsAt.HasValue != previousPeriodEndsAt.HasValue)
        {
            throw new ArgumentException(
                "Previous period start and end must either both be specified or both be omitted.",
                nameof(previousPeriodStartsAt));
        }

        if (previousPeriodStartsAt.HasValue &&
            previousPeriodEndsAt <= previousPeriodStartsAt)
        {
            throw new ArgumentOutOfRangeException(
                nameof(previousPeriodEndsAt),
                previousPeriodEndsAt,
                "Previous period end timestamp must be greater than start timestamp.");
        }

        CurrentPeriodStartsAt = currentPeriodStartsAt;
        CurrentPeriodEndsAt = currentPeriodEndsAt;
        PreviousPeriodStartsAt = previousPeriodStartsAt;
        PreviousPeriodEndsAt = previousPeriodEndsAt;
    }

    /// <summary>
    /// Gets the current period start timestamp.
    /// </summary>
    public DateTimeOffset CurrentPeriodStartsAt { get; }

    /// <summary>
    /// Gets the current period end timestamp.
    /// </summary>
    public DateTimeOffset CurrentPeriodEndsAt { get; }

    /// <summary>
    /// Gets the optional previous period start timestamp.
    /// </summary>
    public DateTimeOffset? PreviousPeriodStartsAt { get; }

    /// <summary>
    /// Gets the optional previous period end timestamp.
    /// </summary>
    public DateTimeOffset? PreviousPeriodEndsAt { get; }

    /// <summary>
    /// Gets whether the request contains an explicit previous comparison period.
    /// </summary>
    public bool HasExplicitPreviousPeriod =>
        PreviousPeriodStartsAt.HasValue &&
        PreviousPeriodEndsAt.HasValue;
}
