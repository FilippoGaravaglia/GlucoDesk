using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Desktop.ViewModels.Dashboard.Chart;
using GlucoDesk.Desktop.ViewModels.Dashboard.Summaries;
using GlucoDesk.Desktop.Tests.Localization;

namespace GlucoDesk.Desktop.Tests.ViewModels.Dashboard.Summaries;

public sealed class AmbientGlucoseSummaryServiceTests : EnglishLocalizationTestBase
{
    private static readonly DateTimeOffset BaseTimestamp =
        new(2026, 7, 8, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CreateSummary_ShouldReturnNoData_WhenNoPointsAreAvailable()
    {
        var summary = AmbientGlucoseSummaryService.CreateSummary([], 70m, 180m);

        Assert.Equal("No recent glucose data.", summary);
    }

    [Fact]
    public void CreateSummary_ShouldReturnBelowTarget_WhenLatestPointIsBelowTarget()
    {
        var points = new[]
        {
            CreatePoint(minutesOffset: -5, valueMgDl: 82m),
            CreatePoint(minutesOffset: 0, valueMgDl: 66m)
        };

        var summary = AmbientGlucoseSummaryService.CreateSummary(points, 70m, 180m);

        Assert.Equal("Below target.", summary);
    }

    [Fact]
    public void CreateSummary_ShouldReturnAboveTarget_WhenLatestPointIsAboveTarget()
    {
        var points = new[]
        {
            CreatePoint(minutesOffset: -5, valueMgDl: 172m),
            CreatePoint(minutesOffset: 0, valueMgDl: 206m)
        };

        var summary = AmbientGlucoseSummaryService.CreateSummary(points, 70m, 180m);

        Assert.Equal("Above target.", summary);
    }

    [Fact]
    public void CreateSummary_ShouldReturnStableAndInRange_WhenLatestPointIsStable()
    {
        var points = new[]
        {
            CreatePoint(minutesOffset: -5, valueMgDl: 124m),
            CreatePoint(minutesOffset: 0, valueMgDl: 127m)
        };

        var summary = AmbientGlucoseSummaryService.CreateSummary(points, 70m, 180m);

        Assert.Equal("Stable and in range.", summary);
    }

    [Fact]
    public void CreateSummary_ShouldReturnRisingSlowly_WhenLatestPointIsSlightlyRising()
    {
        var points = new[]
        {
            CreatePoint(minutesOffset: -5, valueMgDl: 120m),
            CreatePoint(minutesOffset: 0, valueMgDl: 129m)
        };

        var summary = AmbientGlucoseSummaryService.CreateSummary(points, 70m, 180m);

        Assert.Equal("Rising slowly, still in range.", summary);
    }

    [Fact]
    public void CreateSummary_ShouldReturnFallingSlowly_WhenLatestPointIsSlightlyFalling()
    {
        var points = new[]
        {
            CreatePoint(minutesOffset: -5, valueMgDl: 129m),
            CreatePoint(minutesOffset: 0, valueMgDl: 120m)
        };

        var summary = AmbientGlucoseSummaryService.CreateSummary(points, 70m, 180m);

        Assert.Equal("Falling slowly, still in range.", summary);
    }

    [Fact]
    public void CreateSummary_ShouldReturnRising_WhenLatestPointIsMeaningfullyRising()
    {
        var points = new[]
        {
            CreatePoint(minutesOffset: -5, valueMgDl: 118m),
            CreatePoint(minutesOffset: 0, valueMgDl: 138m)
        };

        var summary = AmbientGlucoseSummaryService.CreateSummary(points, 70m, 180m);

        Assert.Equal("Rising, still in range.", summary);
    }

    [Fact]
    public void CreateSummary_ShouldReturnFalling_WhenLatestPointIsMeaningfullyFalling()
    {
        var points = new[]
        {
            CreatePoint(minutesOffset: -5, valueMgDl: 145m),
            CreatePoint(minutesOffset: 0, valueMgDl: 124m)
        };

        var summary = AmbientGlucoseSummaryService.CreateSummary(points, 70m, 180m);

        Assert.Equal("Falling, still in range.", summary);
    }

    [Fact]
    public void CreateSummary_ShouldReturnRecentlyBackInRange_WhenRecentPointWasOutOfRange()
    {
        var points = new[]
        {
            CreatePoint(minutesOffset: -10, valueMgDl: 191m),
            CreatePoint(minutesOffset: -5, valueMgDl: 176m),
            CreatePoint(minutesOffset: 0, valueMgDl: 148m)
        };

        var summary = AmbientGlucoseSummaryService.CreateSummary(points, 70m, 180m);

        Assert.Equal("Recently back in range.", summary);
    }

    private static GlucoseChartPoint CreatePoint(
        int minutesOffset,
        decimal valueMgDl)
    {
        return new GlucoseChartPoint(
            BaseTimestamp.AddMinutes(minutesOffset),
            valueMgDl,
            GlucoseStatus.InRange);
    }
}
