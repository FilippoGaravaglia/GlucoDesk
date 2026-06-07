using System.Globalization;
using GlucoDesk.Core.Glucose.Enums;

namespace GlucoDesk.Core.Glucose.ValueObjects;

/// <summary>
/// Represents a glucose value together with its unit of measure.
/// </summary>
public sealed record GlucoseValue
{
    private const decimal MmolLToMgDlFactor = 18.0182m;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlucoseValue"/> class.
    /// </summary>
    /// <param name="amount">The glucose amount.</param>
    /// <param name="unit">The glucose unit.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the amount is less than or equal to zero.</exception>
    public GlucoseValue(decimal amount, GlucoseUnit unit)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Glucose amount must be greater than zero.");
        }

        Amount = amount;
        Unit = unit;
    }

    /// <summary>
    /// Gets the glucose amount.
    /// </summary>
    public decimal Amount { get; }

    /// <summary>
    /// Gets the glucose unit.
    /// </summary>
    public GlucoseUnit Unit { get; }

    /// <summary>
    /// Converts the current glucose value to the requested unit.
    /// </summary>
    /// <param name="targetUnit">The target glucose unit.</param>
    /// <returns>The converted glucose value.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the target unit is not supported.</exception>
    public GlucoseValue ConvertTo(GlucoseUnit targetUnit)
    {
        if (targetUnit == Unit)
        {
            return this;
        }

        return targetUnit switch
        {
            GlucoseUnit.MgDl => new GlucoseValue(
                decimal.Round(Amount * MmolLToMgDlFactor, 0, MidpointRounding.AwayFromZero),
                GlucoseUnit.MgDl),

            GlucoseUnit.MmolL => new GlucoseValue(
                decimal.Round(Amount / MmolLToMgDlFactor, 1, MidpointRounding.AwayFromZero),
                GlucoseUnit.MmolL),

            _ => throw new ArgumentOutOfRangeException(nameof(targetUnit), targetUnit, "Unsupported glucose unit.")
        };
    }

    /// <summary>
    /// Returns a display-friendly representation of the glucose value.
    /// </summary>
    /// <returns>A formatted glucose value.</returns>
    public override string ToString()
    {
        return Unit switch
        {
            GlucoseUnit.MgDl => string.Create(
                CultureInfo.InvariantCulture,
                $"{Amount:0} mg/dL"),

            GlucoseUnit.MmolL => string.Create(
                CultureInfo.InvariantCulture,
                $"{Amount:0.0} mmol/L"),

            _ => Amount.ToString(CultureInfo.InvariantCulture)
        };
    }
}