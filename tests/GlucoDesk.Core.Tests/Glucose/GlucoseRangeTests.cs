using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Core.Glucose.ValueObjects;

namespace GlucoDesk.Core.Tests.Glucose;

public sealed class GlucoseRangeTests
{
    [Fact]
    public void Constructor_ShouldRejectBoundariesWithDifferentUnits()
    {
        var low = new GlucoseValue(70, GlucoseUnit.MgDl);
        var high = new GlucoseValue(10, GlucoseUnit.MmolL);

        var exception = Assert.Throws<ArgumentException>(
            () => new GlucoseRange(low, high));

        Assert.Equal("high", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldRejectInvalidBoundaryOrder()
    {
        var low = new GlucoseValue(180, GlucoseUnit.MgDl);
        var high = new GlucoseValue(70, GlucoseUnit.MgDl);

        var exception = Assert.Throws<ArgumentException>(
            () => new GlucoseRange(low, high));

        Assert.Equal("high", exception.ParamName);
    }

    [Theory]
    [InlineData(69, GlucoseStatus.Low)]
    [InlineData(70, GlucoseStatus.InRange)]
    [InlineData(120, GlucoseStatus.InRange)]
    [InlineData(180, GlucoseStatus.InRange)]
    [InlineData(181, GlucoseStatus.High)]
    public void Classify_ShouldReturnExpectedStatus(decimal amount, GlucoseStatus expectedStatus)
    {
        var value = new GlucoseValue(amount, GlucoseUnit.MgDl);

        var status = GlucoseRange.StandardMgDl.Classify(value);

        Assert.Equal(expectedStatus, status);
    }

    [Fact]
    public void Classify_ShouldConvertValue_WhenValueUsesDifferentUnit()
    {
        var value = new GlucoseValue(10.0m, GlucoseUnit.MmolL);

        var status = GlucoseRange.StandardMgDl.Classify(value);

        Assert.Equal(GlucoseStatus.InRange, status);
    }

    [Fact]
    public void Contains_ShouldReturnTrue_WhenValueIsInsideRange()
    {
        var value = new GlucoseValue(100, GlucoseUnit.MgDl);

        var result = GlucoseRange.StandardMgDl.Contains(value);

        Assert.True(result);
    }

    [Fact]
    public void Contains_ShouldReturnFalse_WhenValueIsOutsideRange()
    {
        var value = new GlucoseValue(220, GlucoseUnit.MgDl);

        var result = GlucoseRange.StandardMgDl.Contains(value);

        Assert.False(result);
    }
}