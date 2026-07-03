using GlucoDesk.Desktop.Controls.Statistics;

namespace GlucoDesk.Desktop.Tests.Controls.Statistics;

public sealed class GlucoseRangeBarsLayoutCalculatorTests
{
    [Fact]
    public void Calculate_WhenTotalWidthIsInvalid_ShouldReturnEmptyLayout()
    {
        var result = GlucoseRangeBarsLayoutCalculator.Calculate(0d, 10d, 20d);

        Assert.Equal(0d, result.BelowFillWidth);
        Assert.Equal(0d, result.BelowRemainingWidth);
        Assert.Equal(0d, result.GapWidth);
        Assert.Equal(0d, result.AboveFillWidth);
        Assert.Equal(0d, result.AboveRemainingWidth);
    }

    [Fact]
    public void Calculate_ShouldRespectFixedGapAndTwoLanes()
    {
        var result = GlucoseRangeBarsLayoutCalculator.Calculate(212d, 50d, 25d);

        Assert.Equal(12d, result.GapWidth, 3);
        Assert.Equal(50d, result.BelowFillWidth, 3);
        Assert.Equal(50d, result.BelowRemainingWidth, 3);
        Assert.Equal(25d, result.AboveFillWidth, 3);
        Assert.Equal(75d, result.AboveRemainingWidth, 3);
    }

    [Fact]
    public void Calculate_WhenPercentIsPositiveButVerySmall_ShouldKeepMinimumVisibleWidth()
    {
        var result = GlucoseRangeBarsLayoutCalculator.Calculate(212d, 2.1d, 1.5d);

        Assert.Equal(8d, result.BelowFillWidth, 3);
        Assert.Equal(8d, result.AboveFillWidth, 3);
    }

    [Fact]
    public void Calculate_ShouldClampToLaneWidth()
    {
        var result = GlucoseRangeBarsLayoutCalculator.Calculate(212d, 150d, 200d);

        Assert.Equal(100d, result.BelowFillWidth, 3);
        Assert.Equal(0d, result.BelowRemainingWidth, 3);
        Assert.Equal(100d, result.AboveFillWidth, 3);
        Assert.Equal(0d, result.AboveRemainingWidth, 3);
    }
}
