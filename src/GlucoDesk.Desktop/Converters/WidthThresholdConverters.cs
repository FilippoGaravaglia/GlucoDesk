using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace GlucoDesk.Desktop.Converters;

/// <summary>
/// Returns true when the available width is below the configured threshold.
/// </summary>
public sealed class WidthBelowThresholdConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var width = WidthThresholdConverterHelpers.ReadWidth(value, culture);
        var threshold = WidthThresholdConverterHelpers.ReadThreshold(parameter, culture);

        return width > 0d && width < threshold;
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

/// <summary>
/// Returns true when the available width is greater than or equal to the configured threshold.
/// </summary>
public sealed class WidthAtLeastThresholdConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var width = WidthThresholdConverterHelpers.ReadWidth(value, culture);
        var threshold = WidthThresholdConverterHelpers.ReadThreshold(parameter, culture);

        return width <= 0d || width >= threshold;
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

/// <summary>
/// Shared helpers for width threshold converters.
/// </summary>
public static class WidthThresholdConverterHelpers
{
    /// <summary>
    /// Reads a width value from an Avalonia binding value.
    /// </summary>
    /// <param name="value">The binding value.</param>
    /// <param name="culture">The current culture.</param>
    /// <returns>The parsed width.</returns>
    public static double ReadWidth(object? value, CultureInfo culture)
    {
        return value switch
        {
            double doubleValue => doubleValue,
            float floatValue => floatValue,
            int intValue => intValue,
            long longValue => longValue,
            decimal decimalValue => (double)decimalValue,
            Rect rectValue => rectValue.Width,
            string stringValue when double.TryParse(
                stringValue,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var invariantValue) => invariantValue,
            string stringValue when double.TryParse(
                stringValue,
                NumberStyles.Float,
                culture,
                out var cultureValue) => cultureValue,
            _ => 0d,
        };
    }

    /// <summary>
    /// Reads a threshold value from a converter parameter.
    /// </summary>
    /// <param name="parameter">The converter parameter.</param>
    /// <param name="culture">The current culture.</param>
    /// <returns>The parsed threshold.</returns>
    public static double ReadThreshold(object? parameter, CultureInfo culture)
    {
        var threshold = ReadWidth(parameter, culture);

        return threshold > 0d ? threshold : 1020d;
    }
}
