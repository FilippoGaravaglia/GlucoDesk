using GlucoDesk.Application.Cgm.History.Completeness.Enums;
using GlucoDesk.Application.Cgm.History.Completeness.Services;
using GlucoDesk.Application.Cgm.History.Continuity.Results;

namespace GlucoDesk.Application.Tests.Cgm.History.Completeness.Services;

public sealed class GlucoseHistoryCompletenessScoringServiceTests
{
    private static readonly DateTimeOffset PeriodStartsAt =
        new(2026, 6, 19, 0, 0, 0, TimeSpan.Zero);

    private static readonly DateTimeOffset PeriodEndsAt =
        new(2026, 6, 20, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Calculate_ShouldReturnComplete_WhenCoverageIsFullAndNoGaps()
    {
        // Arrange
        var service = new GlucoseHistoryCompletenessScoringService();
        var report = CreateReport(readingsCount: 288, dataCoveragePercentage: 100m);

        // Act
        var score = service.Calculate(report);

        // Assert
        Assert.Equal(GlucoseHistoryCompletenessLevel.Complete, score.Level);
        Assert.True(score.IsComplete);
        Assert.False(score.RequiresCaution);
        Assert.Equal("Complete", score.StatusText);
        Assert.Equal("100%", score.CoverageText);
        Assert.Equal(288, score.AvailableReadingsCount);
        Assert.Equal(288, score.EstimatedExpectedReadingsCount);
        Assert.Equal(0, score.DetectedGapCount);
    }

    [Fact]
    public void Calculate_ShouldReturnReliable_WhenCoverageIsHighButNotComplete()
    {
        // Arrange
        var service = new GlucoseHistoryCompletenessScoringService();
        var report = CreateReport(readingsCount: 276, dataCoveragePercentage: 96m);

        // Act
        var score = service.Calculate(report);

        // Assert
        Assert.Equal(GlucoseHistoryCompletenessLevel.Reliable, score.Level);
        Assert.False(score.IsComplete);
        Assert.False(score.RequiresCaution);
        Assert.Equal("Reliable", score.StatusText);
        Assert.Equal(288, score.EstimatedExpectedReadingsCount);
    }

    [Fact]
    public void Calculate_ShouldReturnPartial_WhenCoverageIsModerate()
    {
        // Arrange
        var service = new GlucoseHistoryCompletenessScoringService();
        var report = CreateReport(readingsCount: 144, dataCoveragePercentage: 50m);

        // Act
        var score = service.Calculate(report);

        // Assert
        Assert.Equal(GlucoseHistoryCompletenessLevel.Partial, score.Level);
        Assert.False(score.IsComplete);
        Assert.True(score.RequiresCaution);
        Assert.Equal("Partial", score.StatusText);
        Assert.Equal(288, score.EstimatedExpectedReadingsCount);
    }

    [Fact]
    public void Calculate_ShouldReturnPoor_WhenCoverageIsLow()
    {
        // Arrange
        var service = new GlucoseHistoryCompletenessScoringService();
        var report = CreateReport(readingsCount: 72, dataCoveragePercentage: 25m);

        // Act
        var score = service.Calculate(report);

        // Assert
        Assert.Equal(GlucoseHistoryCompletenessLevel.Poor, score.Level);
        Assert.False(score.IsComplete);
        Assert.True(score.RequiresCaution);
        Assert.Equal("Poor", score.StatusText);
        Assert.Equal(288, score.EstimatedExpectedReadingsCount);
    }

    [Fact]
    public void Calculate_ShouldReturnEmpty_WhenThereAreNoReadings()
    {
        // Arrange
        var service = new GlucoseHistoryCompletenessScoringService();
        var report = CreateReport(readingsCount: 0, dataCoveragePercentage: 0m);

        // Act
        var score = service.Calculate(report);

        // Assert
        Assert.Equal(GlucoseHistoryCompletenessLevel.Empty, score.Level);
        Assert.False(score.IsComplete);
        Assert.True(score.RequiresCaution);
        Assert.Equal("No local history", score.StatusText);
        Assert.Equal(0, score.EstimatedExpectedReadingsCount);
    }

    [Fact]
    public void Calculate_ShouldRoundCoverageToTwoDecimals()
    {
        // Arrange
        var service = new GlucoseHistoryCompletenessScoringService();
        var report = CreateReport(readingsCount: 279, dataCoveragePercentage: 96.875m);

        // Act
        var score = service.Calculate(report);

        // Assert
        Assert.Equal(96.88m, score.DataCoveragePercentage);
        Assert.Equal("96.88%", score.CoverageText);
    }

    [Fact]
    public void Calculate_ShouldThrow_WhenReportIsNull()
    {
        // Arrange
        var service = new GlucoseHistoryCompletenessScoringService();

        // Act
        var exception = Assert.Throws<ArgumentNullException>(
            () => service.Calculate(null!));

        // Assert
        Assert.Equal("continuityReport", exception.ParamName);
    }

    #region Helpers

    /// <summary>
    /// Creates a continuity report for completeness scoring tests.
    /// </summary>
    /// <param name="readingsCount">The readings count.</param>
    /// <param name="dataCoveragePercentage">The data coverage percentage.</param>
    /// <returns>The continuity report.</returns>
    private static GlucoseHistoryContinuityReport CreateReport(
        int readingsCount,
        decimal dataCoveragePercentage)
    {
        return new GlucoseHistoryContinuityReport(
            PeriodStartsAt,
            PeriodEndsAt,
            readingsCount,
            dataCoveragePercentage,
            []);
    }

    #endregion
}
