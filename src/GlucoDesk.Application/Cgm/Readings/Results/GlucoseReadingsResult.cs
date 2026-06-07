using GlucoDesk.Core.Glucose.Readings;

namespace GlucoDesk.Application.Cgm.Readings.Results;

/// <summary>
/// Represents a collection of glucose readings returned by a CGM provider.
/// </summary>
public sealed record GlucoseReadingsResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GlucoseReadingsResult"/> class.
    /// </summary>
    /// <param name="readings">The glucose readings.</param>
    /// <param name="retrievedAt">The timestamp when the data was retrieved.</param>
    /// <exception cref="ArgumentNullException">Thrown when readings is null.</exception>
    /// <exception cref="ArgumentException">Thrown when retrievedAt is not specified.</exception>
    public GlucoseReadingsResult(IReadOnlyCollection<GlucoseReading> readings, DateTimeOffset retrievedAt)
    {
        ArgumentNullException.ThrowIfNull(readings);

        if (retrievedAt == default)
        {
            throw new ArgumentException("The retrieval timestamp must be specified.", nameof(retrievedAt));
        }

        Readings = readings
            .OrderBy(reading => reading.Timestamp)
            .ToArray();

        RetrievedAt = retrievedAt;
    }

    /// <summary>
    /// Gets the glucose readings ordered by timestamp.
    /// </summary>
    public IReadOnlyCollection<GlucoseReading> Readings { get; }

    /// <summary>
    /// Gets the timestamp when the data was retrieved.
    /// </summary>
    public DateTimeOffset RetrievedAt { get; }

    /// <summary>
    /// Gets a value indicating whether at least one reading is available.
    /// </summary>
    public bool HasReadings => Readings.Count > 0;
}