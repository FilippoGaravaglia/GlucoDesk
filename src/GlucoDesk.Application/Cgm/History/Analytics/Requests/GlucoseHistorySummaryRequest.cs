using GlucoDesk.Core.Glucose.ValueObjects;

namespace GlucoDesk.Application.Cgm.History.Analytics.Requests;

/// <summary>
/// Represents a request for glucose history summary analytics.
/// </summary>
public sealed record GlucoseHistorySummaryRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GlucoseHistorySummaryRequest"/> class.
    /// </summary>
    /// <param name="from">The inclusive start timestamp.</param>
    /// <param name="to">The inclusive end timestamp.</param>
    /// <param name="targetRange">The glucose target range.</param>
    public GlucoseHistorySummaryRequest(
        DateTimeOffset from,
        DateTimeOffset to,
        GlucoseRange targetRange)
    {
        ArgumentNullException.ThrowIfNull(targetRange);

        if (to <= from)
        {
            throw new ArgumentOutOfRangeException(
                nameof(to),
                to,
                "Summary end timestamp must be greater than start timestamp.");
        }

        From = from;
        To = to;
        TargetRange = targetRange;
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
    public GlucoseRange TargetRange { get; }
}