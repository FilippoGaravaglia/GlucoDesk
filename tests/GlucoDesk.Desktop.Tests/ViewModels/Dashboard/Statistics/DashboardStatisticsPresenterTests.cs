using GlucoDesk.Application.Cgm.Statistics.Requests;
using GlucoDesk.Application.Cgm.Statistics.Results;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Desktop.ViewModels.Dashboard.Statistics;

namespace GlucoDesk.Desktop.Tests.ViewModels.Dashboard.Statistics;

public sealed class DashboardStatisticsPresenterTests
{
    [Fact]
    public void Present_ShouldCreatePresentation_WhenStatisticsHaveData()
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
            averageGlucose: 142.5m,
            minimumGlucose: 70,
            maximumGlucose: 210,
            belowRangeCount: 1,
            inRangeCount: 8,
            aboveRangeCount: 1,
            firstReadingAt: from,
            lastReadingAt: to);

        var presentation = DashboardStatisticsPresenter.Present(
            result,
            GlucoseStatisticsTargetRange.DefaultMgDl());

        Assert.True(presentation.HasStatisticsData);
        Assert.Equal("142.5 mg/dL", presentation.AverageGlucoseText);
        Assert.Equal("80%", presentation.TimeInRangeText);
        Assert.Equal("10%", presentation.BelowRangeText);
        Assert.Equal("10%", presentation.AboveRangeText);
        Assert.Equal("10 analyzed", presentation.ReadingsAnalyzedText);
        Assert.Equal("Target range: 70–180 mg/dL", presentation.TargetRangeText);
    }

    [Fact]
    public void Present_ShouldCreateEmptyPresentation_WhenStatisticsHaveNoData()
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

        var presentation = DashboardStatisticsPresenter.Present(
            result,
            GlucoseStatisticsTargetRange.DefaultMgDl());

        Assert.False(presentation.HasStatisticsData);
        Assert.Equal("—", presentation.AverageGlucoseText);
        Assert.Equal("—", presentation.TimeInRangeText);
        Assert.Equal("0 analyzed / 3 loaded", presentation.ReadingsAnalyzedText);
        Assert.Equal("Target range: 70–180 mg/dL", presentation.TargetRangeText);
    }

    [Fact]
    public void Disabled_ShouldCreateDisabledPresentation()
    {
        var presentation = DashboardStatisticsPresenter.Disabled();

        Assert.False(presentation.HasStatisticsData);
        Assert.Equal("Statistics are not available in the current desktop runtime.", presentation.StatusText);
        Assert.Equal("—", presentation.AverageGlucoseText);
    }

    [Fact]
    public void Failed_ShouldCreateFailedPresentation()
    {
        var presentation = DashboardStatisticsPresenter.Failed("History.LoadFailed");

        Assert.False(presentation.HasStatisticsData);
        Assert.Equal("Statistics update failed · History.LoadFailed", presentation.StatusText);
    }
}