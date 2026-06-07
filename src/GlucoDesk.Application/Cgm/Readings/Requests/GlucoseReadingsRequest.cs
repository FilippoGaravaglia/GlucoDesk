namespace GlucoDesk.Application.Cgm.Readings.Requests;

/// <summary>
/// Represents a request for glucose readings inside a time range.
/// </summary>
public sealed record GlucoseReadingsRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GlucoseReadingsRequest"/> class.
    /// </summary>
    /// <param name="from">The inclusive start timestamp.</param>
    /// <param name="to">The exclusive end timestamp.</param>
    /// <param name="limit">The optional maximum number of readings.</param>
    /// <exception cref="ArgumentException">Thrown when the time range is invalid.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the limit is invalid.</exception>
    public GlucoseReadingsRequest(DateTimeOffset from, DateTimeOffset to, int? limit = null)
    {
        if (from == default)
        {
            throw new ArgumentException("The start timestamp must be specified.", nameof(from));
        }

        if (to == default)
        {
            throw new ArgumentException("The end timestamp must be specified.", nameof(to));
        }

        if (from >= to)
        {
            throw new ArgumentException("The start timestamp must be lower than the end timestamp.", nameof(to));
        }

        if (limit <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(limit), limit, "Limit must be greater than zero.");
        }

        From = from;
        To = to;
        Limit = limit;
    }

    /// <summary>
    /// Gets the inclusive start timestamp.
    /// </summary>
    public DateTimeOffset From { get; }

    /// <summary>
    /// Gets the exclusive end timestamp.
    /// </summary>
    public DateTimeOffset To { get; }

    /// <summary>
    /// Gets the optional maximum number of readings.
    /// </summary>
    public int? Limit { get; }

    /// <summary>
    /// Creates a request for readings from the latest configured duration.
    /// </summary>
    /// <param name="duration">The requested duration.</param>
    /// <param name="now">The current timestamp.</param>
    /// <param name="limit">The optional maximum number of readings.</param>
    /// <returns>A glucose readings request.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when duration is invalid.</exception>
    /// <exception cref="ArgumentException">Thrown when now is not specified.</exception>
    public static GlucoseReadingsRequest ForLast(TimeSpan duration, DateTimeOffset now, int? limit = null)
    {
        if (duration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(duration), duration, "Duration must be greater than zero.");
        }

        if (now == default)
        {
            throw new ArgumentException("The current timestamp must be specified.", nameof(now));
        }

        return new GlucoseReadingsRequest(now.Subtract(duration), now, limit);
    }
}