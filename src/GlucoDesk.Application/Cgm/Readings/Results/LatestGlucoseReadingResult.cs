using GlucoDesk.Core.Glucose.Readings;

namespace GlucoDesk.Application.Cgm.Readings.Results;

/// <summary>
/// Represents the latest glucose reading returned by a CGM provider.
/// </summary>
public sealed record LatestGlucoseReadingResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LatestGlucoseReadingResult"/> class.
    /// </summary>
    /// <param name="reading">The latest glucose reading, when available.</param>
    /// <param name="retrievedAt">The timestamp when the data was retrieved.</param>
    /// <exception cref="ArgumentException">Thrown when retrievedAt is not specified.</exception>
    public LatestGlucoseReadingResult(GlucoseReading? reading, DateTimeOffset retrievedAt)
    {
        if (retrievedAt == default)
        {
            throw new ArgumentException("The retrieval timestamp must be specified.", nameof(retrievedAt));
        }

        Reading = reading;
        RetrievedAt = retrievedAt;
    }

    /// <summary>
    /// Gets the latest glucose reading, when available.
    /// </summary>
    public GlucoseReading? Reading { get; }

    /// <summary>
    /// Gets the timestamp when the data was retrieved.
    /// </summary>
    public DateTimeOffset RetrievedAt { get; }

    /// <summary>
    /// Gets a value indicating whether a glucose reading is available.
    /// </summary>
    public bool HasReading => Reading is not null;
}