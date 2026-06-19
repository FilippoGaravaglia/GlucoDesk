using GlucoDesk.Application.Cgm.History.Continuity.Enums;
using GlucoDesk.Application.Cgm.History.Continuity.Options;
using GlucoDesk.Application.Cgm.History.Continuity.Services;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Core.Glucose.Readings;
using GlucoDesk.Core.Glucose.ValueObjects;

namespace GlucoDesk.Application.Tests.Cgm.History.Continuity.Services;

public sealed class GlucoseHistoryContinuityServiceTests
{
    [Fact]
    public void AnalyzeWindow_ShouldReturnCompleteReport_WhenReadingsAreContinuous()
    {
        // Arrange
        var service = CreateService();
        var windowStartsAt = new DateTimeOffset(2026, 6, 19, 10, 0, 0, TimeSpan.Zero);
        var windowEndsAt = windowStartsAt.AddMinutes(20);

        var readings = new[]
        {
            CreateReading(windowStartsAt),
            CreateReading(windowStartsAt.AddMinutes(5)),
            CreateReading(windowStartsAt.AddMinutes(10)),
            CreateReading(windowStartsAt.AddMinutes(15)),
            CreateReading(windowEndsAt)
        };

        // Act
        var result = service.AnalyzeWindow(readings, windowStartsAt, windowEndsAt);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value.IsComplete);
        Assert.Empty(result.Value.Gaps);
        Assert.Equal(100m, result.Value.DataCoveragePercentage);
        Assert.Equal(5, result.Value.ReadingsCount);
    }

    [Fact]
    public void AnalyzeWindow_ShouldDetectGapBetweenReadings()
    {
        // Arrange
        var service = CreateService();
        var windowStartsAt = new DateTimeOffset(2026, 6, 19, 10, 0, 0, TimeSpan.Zero);
        var windowEndsAt = windowStartsAt.AddMinutes(30);

        var readings = new[]
        {
            CreateReading(windowStartsAt),
            CreateReading(windowStartsAt.AddMinutes(5)),
            CreateReading(windowStartsAt.AddMinutes(25)),
            CreateReading(windowEndsAt)
        };

        // Act
        var result = service.AnalyzeWindow(readings, windowStartsAt, windowEndsAt);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.Value.IsComplete);

        var gap = Assert.Single(result.Value.Gaps);

        Assert.Equal(GlucoseHistoryGapKind.BetweenReadings, gap.Kind);
        Assert.Equal(windowStartsAt.AddMinutes(5).ToUniversalTime(), gap.StartsAt);
        Assert.Equal(windowStartsAt.AddMinutes(25).ToUniversalTime(), gap.EndsAt);
        Assert.True(result.Value.DataCoveragePercentage < 100m);
    }

    [Fact]
    public void AnalyzeWindow_ShouldDetectEmptyWindow()
    {
        // Arrange
        var service = CreateService();
        var windowStartsAt = new DateTimeOffset(2026, 6, 19, 10, 0, 0, TimeSpan.Zero);
        var windowEndsAt = windowStartsAt.AddHours(1);

        // Act
        var result = service.AnalyzeWindow([], windowStartsAt, windowEndsAt);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.Value.IsComplete);
        Assert.Equal(0, result.Value.DataCoveragePercentage);

        var gap = Assert.Single(result.Value.Gaps);

        Assert.Equal(GlucoseHistoryGapKind.EmptyWindow, gap.Kind);
        Assert.Equal(windowStartsAt.ToUniversalTime(), gap.StartsAt);
        Assert.Equal(windowEndsAt.ToUniversalTime(), gap.EndsAt);
    }

    [Fact]
    public void AnalyzeWindow_ShouldDetectLeadingAndTrailingGaps()
    {
        // Arrange
        var service = CreateService();
        var windowStartsAt = new DateTimeOffset(2026, 6, 19, 10, 0, 0, TimeSpan.Zero);
        var windowEndsAt = windowStartsAt.AddHours(1);

        var readings = new[]
        {
            CreateReading(windowStartsAt.AddMinutes(20)),
            CreateReading(windowStartsAt.AddMinutes(25)),
            CreateReading(windowStartsAt.AddMinutes(30))
        };

        // Act
        var result = service.AnalyzeWindow(readings, windowStartsAt, windowEndsAt);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.Value.IsComplete);
        Assert.Equal(2, result.Value.Gaps.Count);

        Assert.Contains(
            result.Value.Gaps,
            gap => gap.Kind == GlucoseHistoryGapKind.Leading);

        Assert.Contains(
            result.Value.Gaps,
            gap => gap.Kind == GlucoseHistoryGapKind.Trailing);
    }

    [Fact]
    public void AnalyzeWindow_ShouldIgnoreReadingsOutsideWindow()
    {
        // Arrange
        var service = CreateService();
        var windowStartsAt = new DateTimeOffset(2026, 6, 19, 10, 0, 0, TimeSpan.Zero);
        var windowEndsAt = windowStartsAt.AddMinutes(10);

        var readings = new[]
        {
            CreateReading(windowStartsAt.AddMinutes(-30)),
            CreateReading(windowStartsAt),
            CreateReading(windowStartsAt.AddMinutes(5)),
            CreateReading(windowEndsAt),
            CreateReading(windowEndsAt.AddMinutes(30))
        };

        // Act
        var result = service.AnalyzeWindow(readings, windowStartsAt, windowEndsAt);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.ReadingsCount);
        Assert.True(result.Value.IsComplete);
    }

    [Fact]
    public void AnalyzeWindow_ShouldReturnFailure_WhenWindowIsInvalid()
    {
        // Arrange
        var service = CreateService();
        var timestamp = new DateTimeOffset(2026, 6, 19, 10, 0, 0, TimeSpan.Zero);

        // Act
        var result = service.AnalyzeWindow([], timestamp, timestamp);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("HistoryContinuity.InvalidWindow", result.Error.Code);
    }

    #region Helpers

    /// <summary>
    /// Creates the history continuity service.
    /// </summary>
    /// <returns>The history continuity service.</returns>
    private static GlucoseHistoryContinuityService CreateService()
    {
        return new GlucoseHistoryContinuityService(
            HistoryContinuityOptions.Default);
    }

    /// <summary>
    /// Creates a glucose reading for history continuity tests.
    /// </summary>
    /// <param name="timestamp">The reading timestamp.</param>
    /// <returns>The glucose reading.</returns>
    private static GlucoseReading CreateReading(DateTimeOffset timestamp)
    {
        return new GlucoseReading(
            timestamp,
            new GlucoseValue(120m, GlucoseUnit.MgDl),
            TrendDirection.Flat,
            CgmProviderKind.Mock,
            GlucoseDataFreshness.NearRealTime);
    }

    #endregion
}