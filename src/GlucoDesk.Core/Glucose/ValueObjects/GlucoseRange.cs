using GlucoDesk.Core.Glucose.Enums;

namespace GlucoDesk.Core.Glucose.ValueObjects;

/// <summary>
/// Represents a glucose target range.
/// </summary>
public sealed record GlucoseRange
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GlucoseRange"/> class.
    /// </summary>
    /// <param name="low">The lower boundary.</param>
    /// <param name="high">The upper boundary.</param>
    /// <exception cref="ArgumentNullException">Thrown when one of the boundaries is null.</exception>
    /// <exception cref="ArgumentException">Thrown when boundaries use different units or are not ordered correctly.</exception>
    public GlucoseRange(GlucoseValue low, GlucoseValue high)
    {
        ArgumentNullException.ThrowIfNull(low);
        ArgumentNullException.ThrowIfNull(high);

        if (low.Unit != high.Unit)
        {
            throw new ArgumentException("Glucose range boundaries must use the same unit.", nameof(high));
        }

        if (low.Amount >= high.Amount)
        {
            throw new ArgumentException("The lower boundary must be lower than the upper boundary.", nameof(high));
        }

        Low = low;
        High = high;
    }

    /// <summary>
    /// Gets the standard glucose target range expressed in mg/dL.
    /// </summary>
    public static GlucoseRange StandardMgDl { get; } = new(
        new GlucoseValue(70, GlucoseUnit.MgDl),
        new GlucoseValue(180, GlucoseUnit.MgDl));

    /// <summary>
    /// Gets the lower boundary.
    /// </summary>
    public GlucoseValue Low { get; }

    /// <summary>
    /// Gets the upper boundary.
    /// </summary>
    public GlucoseValue High { get; }

    /// <summary>
    /// Gets the unit used by the range.
    /// </summary>
    public GlucoseUnit Unit => Low.Unit;

    /// <summary>
    /// Classifies a glucose value against this range.
    /// </summary>
    /// <param name="value">The glucose value to classify.</param>
    /// <returns>The glucose status.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the value is null.</exception>
    public GlucoseStatus Classify(GlucoseValue value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var normalizedValue = value.Unit == Unit
            ? value
            : value.ConvertTo(Unit);

        if (normalizedValue.Amount < Low.Amount)
        {
            return GlucoseStatus.Low;
        }

        if (normalizedValue.Amount > High.Amount)
        {
            return GlucoseStatus.High;
        }

        return GlucoseStatus.InRange;
    }

    /// <summary>
    /// Returns whether the supplied glucose value is inside this range.
    /// </summary>
    /// <param name="value">The glucose value to check.</param>
    /// <returns>True when the value is inside the range; otherwise false.</returns>
    public bool Contains(GlucoseValue value)
    {
        return Classify(value) == GlucoseStatus.InRange;
    }
}