using System.Globalization;
using Avalonia;
using GlucoDesk.Desktop.Converters;

namespace GlucoDesk.Desktop.Tests.Converters;

public sealed class WidthThresholdConvertersTests
{
    [Theory]
    [InlineData(900d, 1020d, true)]
    [InlineData(1020d, 1020d, false)]
    [InlineData(1200d, 1020d, false)]
    public void WidthBelowThresholdConverter_ShouldReturnExpectedResult(
        double width,
        double threshold,
        bool expectedResult)
    {
        var converter = new WidthBelowThresholdConverter();

        var result = converter.Convert(
            width,
            typeof(bool),
            threshold,
            CultureInfo.InvariantCulture);

        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData(900d, 1020d, false)]
    [InlineData(1020d, 1020d, true)]
    [InlineData(1200d, 1020d, true)]
    public void WidthAtLeastThresholdConverter_ShouldReturnExpectedResult(
        double width,
        double threshold,
        bool expectedResult)
    {
        var converter = new WidthAtLeastThresholdConverter();

        var result = converter.Convert(
            width,
            typeof(bool),
            threshold,
            CultureInfo.InvariantCulture);

        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void WidthThresholdConverterHelpers_ShouldReadRectWidth()
    {
        var result = WidthThresholdConverterHelpers.ReadWidth(
            new Rect(0d, 0d, 875d, 400d),
            CultureInfo.InvariantCulture);

        Assert.Equal(875d, result);
    }

    [Fact]
    public void WidthAtLeastThresholdConverter_WhenWidthIsNotReady_ShouldKeepWideLayoutVisible()
    {
        var converter = new WidthAtLeastThresholdConverter();

        var result = converter.Convert(
            0d,
            typeof(bool),
            1020d,
            CultureInfo.InvariantCulture);

        Assert.True((bool)result);
    }
}
