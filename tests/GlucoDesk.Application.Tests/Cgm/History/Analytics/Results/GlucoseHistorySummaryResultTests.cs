using GlucoDesk.Application.Cgm.History.Analytics.Results;

namespace GlucoDesk.Application.Tests.Cgm.History.Analytics.Results;

public sealed class GlucoseHistorySummaryResultTests
{
    [Fact]
    public void Constructor_ShouldCalculatePercentages()
    {
        var from = new DateTimeOffset(2026, 6, 8, 8, 0, 0, TimeSpan.Zero);

        var result = new GlucoseHistorySummaryResult(
            from,
            from.AddHours(1),
            readingsCount: 4,
            averageMgDl: 125,
            minimumMgDl: 60,
            maximumMgDl: 190,
            inRangeCount: 2,
            belowRangeCount: 1,
            aboveRangeCount: 1);

        Assert.True(result.HasReadings);
        Assert.Equal(50m, result.InRangePercentage);
        Assert.Equal(25m, result.BelowRangePercentage);
        Assert.Equal(25m, result.AboveRangePercentage);
    }

    [Fact]
    public void Constructor_ShouldReturnZeroPercentages_WhenThereAreNoReadings()
    {
        var from = new DateTimeOffset(2026, 6, 8, 8, 0, 0, TimeSpan.Zero);

        var result = new GlucoseHistorySummaryResult(
            from,
            from.AddHours(1),
            readingsCount: 0,
            averageMgDl: null,
            minimumMgDl: null,
            maximumMgDl: null,
            inRangeCount: 0,
            belowRangeCount: 0,
            aboveRangeCount: 0);

        Assert.False(result.HasReadings);
        Assert.Equal(0m, result.InRangePercentage);
        Assert.Equal(0m, result.BelowRangePercentage);
        Assert.Equal(0m, result.AboveRangePercentage);
    }

    [Fact]
    public void Constructor_ShouldRejectMismatchingBucketCounts()
    {
        var from = new DateTimeOffset(2026, 6, 8, 8, 0, 0, TimeSpan.Zero);

        var exception = Assert.Throws<ArgumentException>(
            () => new GlucoseHistorySummaryResult(
                from,
                from.AddHours(1),
                readingsCount: 3,
                averageMgDl: 120,
                minimumMgDl: 80,
                maximumMgDl: 180,
                inRangeCount: 1,
                belowRangeCount: 1,
                aboveRangeCount: 0));

        Assert.Equal("readingsCount", exception.ParamName);
    }
}