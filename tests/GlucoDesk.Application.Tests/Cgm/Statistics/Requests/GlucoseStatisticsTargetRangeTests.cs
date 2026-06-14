using GlucoDesk.Application.Cgm.Statistics.Requests;
using GlucoDesk.Core.Glucose.Enums;

namespace GlucoDesk.Application.Tests.Cgm.Statistics.Requests;

public sealed class GlucoseStatisticsTargetRangeTests
{
    [Fact]
    public void Constructor_ShouldCreateTargetRange_WhenValuesAreValid()
    {
        var targetRange = new GlucoseStatisticsTargetRange(70, 180, GlucoseUnit.MgDl);

        Assert.Equal(70, targetRange.Low);
        Assert.Equal(180, targetRange.High);
        Assert.Equal(GlucoseUnit.MgDl, targetRange.Unit);
    }

    [Fact]
    public void DefaultMgDl_ShouldCreateDefaultTargetRange()
    {
        var targetRange = GlucoseStatisticsTargetRange.DefaultMgDl();

        Assert.Equal(70, targetRange.Low);
        Assert.Equal(180, targetRange.High);
        Assert.Equal(GlucoseUnit.MgDl, targetRange.Unit);
    }

    [Theory]
    [InlineData(0, 180, "low")]
    [InlineData(-1, 180, "low")]
    [InlineData(70, 70, "high")]
    [InlineData(70, 69, "high")]
    public void Constructor_ShouldRejectInvalidRanges(
        decimal low,
        decimal high,
        string expectedParameterName)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new GlucoseStatisticsTargetRange(low, high, GlucoseUnit.MgDl));

        Assert.Equal(expectedParameterName, exception.ParamName);
    }
}