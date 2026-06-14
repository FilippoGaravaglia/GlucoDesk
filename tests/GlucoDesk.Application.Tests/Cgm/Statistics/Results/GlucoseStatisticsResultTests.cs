using GlucoDesk.Application.Cgm.Statistics.Results;
using GlucoDesk.Core.Glucose.Enums;

namespace GlucoDesk.Application.Tests.Cgm.Statistics.Results;

public sealed class GlucoseStatisticsResultTests
{
    [Fact]
    public void Constructor_ShouldCalculatePercentages_WhenAnalyzedReadingsExist()
    {
        var from = new DateTimeOffset(2026, 6, 14, 8, 0, 0, TimeSpan.Zero);
        var to = from.AddHours(12);

        var result = new GlucoseStatisticsResult(
            from,
            to,
            GlucoseUnit.MgDl,
            includeMockData: false,
            loadedReadingsCount: 10,
            analyzedReadingsCount: 10,
            ignoredMockReadingsCount: 0,
            ignoredDifferentUnitReadingsCount: 0,
            averageGlucose: 140,
            minimumGlucose: 60,
            maximumGlucose: 220,
            belowRangeCount: 2,
            inRangeCount: 6,
            aboveRangeCount: 2,
            firstReadingAt: from,
            lastReadingAt: to);

        Assert.True(result.HasData);
        Assert.Equal(20, result.BelowRangePercentage);
        Assert.Equal(60, result.InRangePercentage);
        Assert.Equal(20, result.AboveRangePercentage);
    }

    [Fact]
    public void Empty_ShouldCreateEmptyResult()
    {
        var from = new DateTimeOffset(2026, 6, 14, 8, 0, 0, TimeSpan.Zero);
        var to = from.AddHours(12);

        var result = GlucoseStatisticsResult.Empty(
            from,
            to,
            GlucoseUnit.MgDl,
            includeMockData: false,
            loadedReadingsCount: 3,
            ignoredMockReadingsCount: 3,
            ignoredDifferentUnitReadingsCount: 0);

        Assert.False(result.HasData);
        Assert.Equal(3, result.LoadedReadingsCount);
        Assert.Equal(0, result.AnalyzedReadingsCount);
        Assert.Equal(0, result.InRangePercentage);
        Assert.Null(result.AverageGlucose);
    }

    [Theory]
    [InlineData(-1, 0, 0, 0, 0, 0, 0, "loadedReadingsCount")]
    [InlineData(0, -1, 0, 0, 0, 0, 0, "analyzedReadingsCount")]
    [InlineData(0, 0, -1, 0, 0, 0, 0, "ignoredMockReadingsCount")]
    [InlineData(0, 0, 0, -1, 0, 0, 0, "ignoredDifferentUnitReadingsCount")]
    [InlineData(0, 0, 0, 0, -1, 0, 0, "belowRangeCount")]
    [InlineData(0, 0, 0, 0, 0, -1, 0, "inRangeCount")]
    [InlineData(0, 0, 0, 0, 0, 0, -1, "aboveRangeCount")]
    public void Constructor_ShouldRejectNegativeCounts(
        int loadedReadingsCount,
        int analyzedReadingsCount,
        int ignoredMockReadingsCount,
        int ignoredDifferentUnitReadingsCount,
        int belowRangeCount,
        int inRangeCount,
        int aboveRangeCount,
        string expectedParameterName)
    {
        var from = new DateTimeOffset(2026, 6, 14, 8, 0, 0, TimeSpan.Zero);
        var to = from.AddHours(12);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new GlucoseStatisticsResult(
                from,
                to,
                GlucoseUnit.MgDl,
                includeMockData: false,
                loadedReadingsCount,
                analyzedReadingsCount,
                ignoredMockReadingsCount,
                ignoredDifferentUnitReadingsCount,
                averageGlucose: null,
                minimumGlucose: null,
                maximumGlucose: null,
                belowRangeCount,
                inRangeCount,
                aboveRangeCount,
                firstReadingAt: null,
                lastReadingAt: null));

        Assert.Equal(expectedParameterName, exception.ParamName);
    }
}