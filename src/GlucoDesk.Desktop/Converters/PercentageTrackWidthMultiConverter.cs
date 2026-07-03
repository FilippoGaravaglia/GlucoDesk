using System.Globalization;
using Avalonia.Data.Converters;

namespace GlucoDesk.Desktop.Converters;

/// <summary>
/// Converts a percentage value and an available track width into a clamped fill width.
/// </summary>
public sealed class PercentageTrackWidthMultiConverter : IMultiValueConverter
{
    private const double MinimumVisibleWidth = 6d;

    /// <inheritdoc />
    public object Convert(
        IList<object?> values,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        var percentage = ReadDouble(values, 0);
        var trackWidth = ReadDouble(values, 1);

        return CalculateFillWidth(percentage, trackWidth);
    }

    /// <summary>
    /// Calculates a fill width from a percentage and the available track width.
    /// </summary>
    /// <param name="percentage">The percentage value in the 0-100 range.</param>
    /// <param name="trackWidth">The available track width.</param>
    /// <returns>The clamped fill width.</returns>
    public static double CalculateFillWidth(double percentage, double trackWidth)
    {
        if (trackWidth <= 0d || percentage <= 0d)
        {
            return 0d;
        }

        var clampedPercentage = Math.Clamp(percentage, 0d, 100d);
        var computedWidth = trackWidth * clampedPercentage / 100d;
        var visibleWidth = Math.Max(computedWidth, MinimumVisibleWidth);

        return Math.Min(trackWidth, visibleWidth);
    }

    #region Helpers

    /// <summary>
    /// Reads a double value from a multi-binding value.
    /// </summary>
    /// <param name="values">The multi-binding values.</param>
    /// <param name="index">The value index.</param>
    /// <returns>The parsed double value, or zero when unavailable.</returns>
    private static double ReadDouble(IList<object?> values, int index)
    {
        if (values.Count <= index || values[index] is null)
        {
            return 0d;
        }

        return values[index] switch
        {
            double doubleValue => doubleValue,
            float floatValue => floatValue,
            decimal decimalValue => (double)decimalValue,
            int intValue => intValue,
            long longValue => longValue,
            string stringValue when double.TryParse(
                stringValue,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var invariantResult) => invariantResult,
            string stringValue when double.TryParse(
                stringValue,
                NumberStyles.Float,
                CultureInfo.CurrentCulture,
                out var cultureResult) => cultureResult,
            _ => 0d,
        };
    }

    #endregion
}
