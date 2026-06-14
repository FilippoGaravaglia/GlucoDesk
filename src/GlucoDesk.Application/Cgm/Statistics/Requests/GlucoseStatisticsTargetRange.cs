using GlucoDesk.Core.Glucose.Enums;

namespace GlucoDesk.Application.Cgm.Statistics.Requests;

/// <summary>
/// Represents the target glucose range used by statistics calculations.
/// </summary>
public sealed record GlucoseStatisticsTargetRange
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GlucoseStatisticsTargetRange"/> class.
    /// </summary>
    /// <param name="low">The lower inclusive target value.</param>
    /// <param name="high">The upper inclusive target value.</param>
    /// <param name="unit">The glucose unit.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the target range is invalid.</exception>
    public GlucoseStatisticsTargetRange(
        decimal low,
        decimal high,
        GlucoseUnit unit)
    {
        if (low <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(low),
                low,
                "Target low value must be greater than zero.");
        }

        if (high <= low)
        {
            throw new ArgumentOutOfRangeException(
                nameof(high),
                high,
                "Target high value must be greater than target low value.");
        }

        Low = low;
        High = high;
        Unit = unit;
    }

    /// <summary>
    /// Gets the lower inclusive target value.
    /// </summary>
    public decimal Low { get; }

    /// <summary>
    /// Gets the upper inclusive target value.
    /// </summary>
    public decimal High { get; }

    /// <summary>
    /// Gets the glucose unit.
    /// </summary>
    public GlucoseUnit Unit { get; }

    /// <summary>
    /// Creates the default mg/dL target range used by the application.
    /// </summary>
    /// <returns>The default target range.</returns>
    public static GlucoseStatisticsTargetRange DefaultMgDl()
    {
        return new GlucoseStatisticsTargetRange(70, 180, GlucoseUnit.MgDl);
    }
}