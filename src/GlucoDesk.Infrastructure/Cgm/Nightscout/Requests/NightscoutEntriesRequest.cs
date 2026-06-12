namespace GlucoDesk.Infrastructure.Cgm.Nightscout.Requests;

/// <summary>
/// Represents a Nightscout entries API request.
/// </summary>
public sealed record NightscoutEntriesRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NightscoutEntriesRequest"/> class.
    /// </summary>
    /// <param name="from">The inclusive range start.</param>
    /// <param name="to">The inclusive range end.</param>
    /// <param name="count">The maximum number of entries to request.</param>
    /// <exception cref="ArgumentException">Thrown when timestamps are invalid.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when count is invalid.</exception>
    public NightscoutEntriesRequest(
        DateTimeOffset from,
        DateTimeOffset to,
        int count)
    {
        if (from == default)
        {
            throw new ArgumentException("From timestamp must be specified.", nameof(from));
        }

        if (to == default)
        {
            throw new ArgumentException("To timestamp must be specified.", nameof(to));
        }

        if (from > to)
        {
            throw new ArgumentException("From timestamp must be earlier than or equal to the To timestamp.", nameof(from));
        }

        if (count <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(count),
                count,
                "Count must be greater than zero.");
        }

        From = from.ToUniversalTime();
        To = to.ToUniversalTime();
        Count = count;
    }

    /// <summary>
    /// Gets the inclusive range start.
    /// </summary>
    public DateTimeOffset From { get; }

    /// <summary>
    /// Gets the inclusive range end.
    /// </summary>
    public DateTimeOffset To { get; }

    /// <summary>
    /// Gets the maximum number of entries to request.
    /// </summary>
    public int Count { get; }
}