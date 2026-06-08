using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Desktop.ViewModels.Dashboard.Chart;

namespace GlucoDesk.Desktop.Tests.ViewModels.Dashboard.Chart;

public sealed class GlucoseChartPointTests
{
    [Fact]
    public void Constructor_ShouldRejectDefaultTimestamp()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new GlucoseChartPoint(default, 120, GlucoseStatus.InRange));

        Assert.Equal("timestamp", exception.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_ShouldRejectInvalidValue(decimal valueMgDl)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new GlucoseChartPoint(DateTimeOffset.UtcNow, valueMgDl, GlucoseStatus.InRange));

        Assert.Equal("valueMgDl", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldCreateChartPoint()
    {
        var timestamp = new DateTimeOffset(2026, 6, 7, 10, 0, 0, TimeSpan.Zero);

        var point = new GlucoseChartPoint(timestamp, 123, GlucoseStatus.InRange);

        Assert.Equal(timestamp, point.Timestamp);
        Assert.Equal(123, point.ValueMgDl);
        Assert.Equal(GlucoseStatus.InRange, point.Status);
    }
}