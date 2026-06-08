namespace GlucoDesk.Application.Cgm.History.Requests;

/// <summary>
/// Represents a request for glucose readings stored in local history.
/// </summary>
public sealed record GlucoseHistoryRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GlucoseHistoryRequest"/> class.
    /// </summary>
    /// <param name="from">The inclusive start timestamp.</param>
    /// <param name="to">The inclusive end timestamp.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the date range is invalid.</exception>
    public GlucoseHistoryRequest(
        DateTimeOffset from,
        DateTimeOffset to)
    {
        if (to <= from)
        {
            throw new ArgumentOutOfRangeException(
                nameof(to),
                to,
                "History end timestamp must be greater than start timestamp.");
        }

        From = from;
        To = to;
    }

    /// <summary>
    /// Gets the inclusive start timestamp.
    /// </summary>
    public DateTimeOffset From { get; }

    /// <summary>
    /// Gets the inclusive end timestamp.
    /// </summary>
    public DateTimeOffset To { get; }
}