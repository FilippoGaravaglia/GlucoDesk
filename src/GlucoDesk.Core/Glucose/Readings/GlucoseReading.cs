using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Core.Glucose.ValueObjects;

namespace GlucoDesk.Core.Glucose.Readings;

/// <summary>
/// Represents a single CGM glucose reading normalized by GlucoDesk.
/// </summary>
public sealed record GlucoseReading
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GlucoseReading"/> class.
    /// </summary>
    /// <param name="timestamp">The timestamp associated with the reading.</param>
    /// <param name="value">The glucose value.</param>
    /// <param name="trend">The glucose trend direction.</param>
    /// <param name="provider">The source provider.</param>
    /// <param name="freshness">The freshness of the reading.</param>
    /// <param name="device">The optional source device name.</param>
    /// <exception cref="ArgumentException">Thrown when the timestamp is not valid.</exception>
    /// <exception cref="ArgumentNullException">Thrown when the value is null.</exception>
    public GlucoseReading(
        DateTimeOffset timestamp,
        GlucoseValue value,
        TrendDirection trend,
        CgmProviderKind provider,
        GlucoseDataFreshness freshness,
        string? device = null)
    {
        if (timestamp == default)
        {
            throw new ArgumentException("The glucose reading timestamp must be specified.", nameof(timestamp));
        }

        ArgumentNullException.ThrowIfNull(value);

        Timestamp = timestamp;
        Value = value;
        Trend = trend;
        Provider = provider;
        Freshness = freshness;
        Device = string.IsNullOrWhiteSpace(device) ? null : device.Trim();
    }

    /// <summary>
    /// Gets the timestamp associated with the reading.
    /// </summary>
    public DateTimeOffset Timestamp { get; }

    /// <summary>
    /// Gets the glucose value.
    /// </summary>
    public GlucoseValue Value { get; }

    /// <summary>
    /// Gets the glucose trend direction.
    /// </summary>
    public TrendDirection Trend { get; }

    /// <summary>
    /// Gets the source provider.
    /// </summary>
    public CgmProviderKind Provider { get; }

    /// <summary>
    /// Gets the freshness of the reading.
    /// </summary>
    public GlucoseDataFreshness Freshness { get; }

    /// <summary>
    /// Gets the optional source device name.
    /// </summary>
    public string? Device { get; }

    /// <summary>
    /// Classifies this reading against a glucose target range.
    /// </summary>
    /// <param name="range">The glucose target range.</param>
    /// <returns>The glucose status.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the range is null.</exception>
    public GlucoseStatus GetStatus(GlucoseRange range)
    {
        ArgumentNullException.ThrowIfNull(range);

        return range.Classify(Value);
    }

    /// <summary>
    /// Returns whether this reading is older than the configured maximum age.
    /// </summary>
    /// <param name="now">The current timestamp.</param>
    /// <param name="maxAge">The maximum allowed age.</param>
    /// <returns>True when the reading is stale; otherwise false.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when maxAge is less than or equal to zero.</exception>
    public bool IsStale(DateTimeOffset now, TimeSpan maxAge)
    {
        if (maxAge <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(maxAge), maxAge, "Maximum age must be greater than zero.");
        }

        if (now <= Timestamp)
        {
            return false;
        }

        return now - Timestamp > maxAge;
    }
}