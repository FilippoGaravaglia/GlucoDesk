namespace GlucoDesk.Application.Cgm.Diary.Requests;

/// <summary>
/// Represents a request for glycemic diary generation.
/// </summary>
public sealed record GlycemicDiaryRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GlycemicDiaryRequest"/> class.
    /// </summary>
    /// <param name="periodStartsAt">The diary period start timestamp.</param>
    /// <param name="periodEndsAt">The diary period end timestamp.</param>
    public GlycemicDiaryRequest(
        DateTimeOffset periodStartsAt,
        DateTimeOffset periodEndsAt)
    {
        if (periodEndsAt <= periodStartsAt)
        {
            throw new ArgumentOutOfRangeException(
                nameof(periodEndsAt),
                periodEndsAt,
                "Diary period end timestamp must be greater than start timestamp.");
        }

        PeriodStartsAt = periodStartsAt;
        PeriodEndsAt = periodEndsAt;
    }

    /// <summary>
    /// Gets the diary period start timestamp.
    /// </summary>
    public DateTimeOffset PeriodStartsAt { get; }

    /// <summary>
    /// Gets the diary period end timestamp.
    /// </summary>
    public DateTimeOffset PeriodEndsAt { get; }
}