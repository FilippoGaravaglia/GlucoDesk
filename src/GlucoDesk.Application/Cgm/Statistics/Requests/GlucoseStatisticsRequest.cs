namespace GlucoDesk.Application.Cgm.Statistics.Requests;

/// <summary>
/// Represents a request to calculate glucose statistics from local history.
/// </summary>
public sealed record GlucoseStatisticsRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GlucoseStatisticsRequest"/> class.
    /// </summary>
    /// <param name="from">The inclusive start timestamp.</param>
    /// <param name="to">The inclusive end timestamp.</param>
    /// <param name="targetRange">The glucose target range.</param>
    /// <param name="includeMockData">A value indicating whether Mock provider readings should be included.</param>
    /// <exception cref="ArgumentNullException">Thrown when targetRange is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the date range is invalid.</exception>
    public GlucoseStatisticsRequest(
        DateTimeOffset from,
        DateTimeOffset to,
        GlucoseStatisticsTargetRange targetRange,
        bool includeMockData = false)
    {
        ArgumentNullException.ThrowIfNull(targetRange);

        if (to <= from)
        {
            throw new ArgumentOutOfRangeException(
                nameof(to),
                to,
                "Statistics end timestamp must be greater than start timestamp.");
        }

        From = from;
        To = to;
        TargetRange = targetRange;
        IncludeMockData = includeMockData;
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
    /// Gets the glucose target range.
    /// </summary>
    public GlucoseStatisticsTargetRange TargetRange { get; }

    /// <summary>
    /// Gets a value indicating whether Mock provider readings should be included.
    /// </summary>
    public bool IncludeMockData { get; }
}