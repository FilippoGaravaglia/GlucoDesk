using GlucoDesk.Desktop.Converters;

namespace GlucoDesk.Desktop.Tests.Converters;

public sealed class PercentageTrackWidthMultiConverterTests
{
    [Theory]
    [InlineData(0, 120, 0)]
    [InlineData(-10, 120, 0)]
    [InlineData(50, 120, 60)]
    [InlineData(100, 120, 120)]
    [InlineData(150, 120, 120)]
    public void CalculateFillWidth_ShouldClampFillInsideTrack(
        double percentage,
        double trackWidth,
        double expectedWidth)
    {
        var result = PercentageTrackWidthMultiConverter.CalculateFillWidth(
            percentage,
            trackWidth);

        Assert.Equal(expectedWidth, result, 3);
    }

    [Fact]
    public void CalculateFillWidth_WhenPercentageIsSmallButPositive_ShouldReturnMinimumVisibleWidth()
    {
        var result = PercentageTrackWidthMultiConverter.CalculateFillWidth(
            2.1d,
            120d);

        Assert.Equal(6d, result, 3);
    }

    [Theory]
    [InlineData(10, 0)]
    [InlineData(10, -1)]
    public void CalculateFillWidth_WhenTrackWidthIsInvalid_ShouldReturnZero(
        double percentage,
        double trackWidth)
    {
        var result = PercentageTrackWidthMultiConverter.CalculateFillWidth(
            percentage,
            trackWidth);

        Assert.Equal(0d, result, 3);
    }
}
