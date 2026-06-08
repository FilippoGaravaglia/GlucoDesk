using GlucoDesk.Core.Glucose.Readings;

namespace GlucoDesk.Application.Cgm.History.Results;

/// <summary>
/// Represents glucose readings loaded from local history.
/// </summary>
public sealed record GlucoseHistoryResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GlucoseHistoryResult"/> class.
    /// </summary>
    /// <param name="readings">The glucose readings.</param>
    public GlucoseHistoryResult(IReadOnlyCollection<GlucoseReading> readings)
    {
        ArgumentNullException.ThrowIfNull(readings);

        Readings = readings
            .OrderBy(reading => reading.Timestamp)
            .ToArray();
    }

    /// <summary>
    /// Gets the glucose readings.
    /// </summary>
    public IReadOnlyCollection<GlucoseReading> Readings { get; }

    /// <summary>
    /// Gets the number of glucose readings.
    /// </summary>
    public int Count => Readings.Count;
}