using GlucoDesk.Core.Glucose.Enums;

namespace GlucoDesk.Desktop.ViewModels.Dashboard.Chart;

/// <summary>
/// Represents a glucose reading point displayed by the dashboard chart.
/// </summary>
public sealed record GlucoseChartPoint
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GlucoseChartPoint"/> class.
    /// </summary>
    /// <param name="timestamp">The glucose reading timestamp.</param>
    /// <param name="valueMgDl">The glucose value expressed in mg/dL.</param>
    /// <param name="status">The glucose status.</param>
    /// <exception cref="ArgumentException">Thrown when timestamp is not specified.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when valueMgDl is invalid.</exception>
    public GlucoseChartPoint(
        DateTimeOffset timestamp,
        decimal valueMgDl,
        GlucoseStatus status)
    {
        if (timestamp == default)
        {
            throw new ArgumentException("Timestamp must be specified.", nameof(timestamp));
        }

        if (valueMgDl <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(valueMgDl),
                valueMgDl,
                "Glucose chart value must be greater than zero.");
        }

        Timestamp = timestamp;
        ValueMgDl = valueMgDl;
        Status = status;
    }

    /// <summary>
    /// Gets the glucose reading timestamp.
    /// </summary>
    public DateTimeOffset Timestamp { get; }

    /// <summary>
    /// Gets the glucose value expressed in mg/dL.
    /// </summary>
    public decimal ValueMgDl { get; }

    /// <summary>
    /// Gets the glucose status.
    /// </summary>
    public GlucoseStatus Status { get; }
}