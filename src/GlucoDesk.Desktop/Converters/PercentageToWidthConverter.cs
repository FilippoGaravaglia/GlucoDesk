using System.Globalization;
using Avalonia.Data.Converters;

namespace GlucoDesk.Desktop.Converters;

/// <summary>
/// Converts a percentage value to a fixed pixel width.
/// </summary>
public sealed class PercentageToWidthConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        var percentage = ReadDouble(value, culture);
        var maximumWidth = ReadDouble(parameter, culture);

        percentage = Math.Clamp(percentage, 0d, 100d);
        maximumWidth = Math.Max(0d, maximumWidth);

        return maximumWidth * percentage / 100d;
    }

    /// <inheritdoc />
    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        throw new NotSupportedException("Range bar width is one-way only.");
    }

    #region Helpers

    /// <summary>
    /// Reads a double value from the supplied object.
    /// </summary>
    /// <param name="value">The value to read.</param>
    /// <param name="culture">The culture to use for parsing.</param>
    /// <returns>The parsed double value.</returns>
    private static double ReadDouble(object? value, CultureInfo culture)
    {
        return value switch
        {
            null => 0d,
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
                culture,
                out var cultureResult) => cultureResult,
            _ => 0d,
        };
    }

    #endregion
}
