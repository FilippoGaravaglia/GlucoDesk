using GlucoDesk.Application.Cgm.Diary.Enums;
using GlucoDesk.Application.Cgm.Diary.Patterns.Services;
using GlucoDesk.Application.Cgm.Diary.Results;
using GlucoDesk.Application.Cgm.Diary.Reviews.Enums;
using GlucoDesk.Application.Cgm.Diary.Reviews.Services;
using GlucoDesk.Application.Cgm.History.Completeness.Services;
using GlucoDesk.Application.Cgm.History.Continuity.Results;

namespace GlucoDesk.Application.Tests.Cgm.Diary.Reviews.Services;

public sealed class GlycemicDiaryWeeklyReviewServiceTests
{
    private static readonly DateTimeOffset PreviousStartsAt =
        new(2026, 6, 1, 0, 0, 0, TimeSpan.Zero);

    private static readonly DateTimeOffset PreviousEndsAt =
        new(2026, 6, 7, 23, 59, 59, TimeSpan.Zero);

    private static readonly DateTimeOffset CurrentStartsAt =
        new(2026, 6, 8, 0, 0, 0, TimeSpan.Zero);

    private static readonly DateTimeOffset CurrentEndsAt =
        new(2026, 6, 14, 23, 59, 59, TimeSpan.Zero);

    [Fact]
    public void CreateReview_ShouldDetectImprovedTimeInRange()
    {
        // Arrange
        var service = CreateService();

        var previous = CreateReport(
            PreviousStartsAt,
            PreviousEndsAt,
            readingsCount: 288,
            averageMgDl: 135m,
            minimumMgDl: 80m,
            maximumMgDl: 190m,
            timeInRangePercentage: 78m,
            coveragePercentage: 100m);

        var current = CreateReport(
            CurrentStartsAt,
            CurrentEndsAt,
            readingsCount: 288,
            averageMgDl: 125m,
            minimumMgDl: 80m,
            maximumMgDl: 160m,
            timeInRangePercentage: 91m,
            coveragePercentage: 100m);

        // Act
        var review = service.CreateReview(current, previous);

        // Assert
        Assert.Equal("Weekly review: time in range improved", review.Headline);
        Assert.False(review.RequiresCaution);

        var tirChange = Assert.Single(
            review.Changes,
            change => change.Kind == GlycemicDiaryReviewMetricKind.TimeInRange);

        Assert.Equal(GlycemicDiaryReviewChangeDirection.Increased, tirChange.Direction);
        Assert.Equal(GlycemicDiaryReviewSignalSeverity.Info, tirChange.Severity);
        Assert.Contains("Time in range increased", review.SummaryText);
    }

    [Fact]
    public void CreateReview_ShouldRequireCaution_WhenCurrentCoverageIsLimited()
    {
        // Arrange
        var service = CreateService();

        var previous = CreateReport(
            PreviousStartsAt,
            PreviousEndsAt,
            readingsCount: 288,
            averageMgDl: 125m,
            minimumMgDl: 80m,
            maximumMgDl: 160m,
            timeInRangePercentage: 91m,
            coveragePercentage: 100m);

        var current = CreateReport(
            CurrentStartsAt,
            CurrentEndsAt,
            readingsCount: 96,
            averageMgDl: 129m,
            minimumMgDl: 60m,
            maximumMgDl: 262m,
            timeInRangePercentage: 91m,
            coveragePercentage: 40m);

        // Act
        var review = service.CreateReview(current, previous);

        // Assert
        Assert.Equal("Weekly review: data quality needs attention", review.Headline);
        Assert.True(review.RequiresCaution);
        Assert.Contains("comparisons should be interpreted carefully", review.SummaryText);
        Assert.Contains("Current history reliability", review.CurrentHistoryReliabilityText);

        var coverageChange = Assert.Single(
            review.Changes,
            change => change.Kind == GlycemicDiaryReviewMetricKind.DataCoverage);

        Assert.Equal(GlycemicDiaryReviewChangeDirection.Decreased, coverageChange.Direction);
        Assert.Equal(GlycemicDiaryReviewSignalSeverity.Caution, coverageChange.Severity);
    }

    [Fact]
    public void CreateReview_ShouldDetectIncreasedPatternCount()
    {
        // Arrange
        var service = CreateService();

        var previous = CreateReport(
            PreviousStartsAt,
            PreviousEndsAt,
            readingsCount: 288,
            averageMgDl: 125m,
            minimumMgDl: 80m,
            maximumMgDl: 160m,
            timeInRangePercentage: 91m,
            coveragePercentage: 100m);

        var current = CreateReport(
            CurrentStartsAt,
            CurrentEndsAt,
            readingsCount: 288,
            averageMgDl: 145m,
            minimumMgDl: 80m,
            maximumMgDl: 230m,
            timeInRangePercentage: 86m,
            coveragePercentage: 100m,
            dailyEntries:
            [
                CreateDailyEntryWithDinner(new DateOnly(2026, 6, 8), 190m, 170m, 210m),
                CreateDailyEntryWithDinner(new DateOnly(2026, 6, 9), 205m, 180m, 230m)
            ]);

        // Act
        var review = service.CreateReview(current, previous);

        // Assert
        var patternChange = Assert.Single(
            review.Changes,
            change => change.Kind == GlycemicDiaryReviewMetricKind.PatternCount);

        Assert.Equal(GlycemicDiaryReviewChangeDirection.Increased, patternChange.Direction);
        Assert.Equal(GlycemicDiaryReviewSignalSeverity.Caution, patternChange.Severity);
        Assert.Equal("0", patternChange.PreviousValueText);
        Assert.Equal("1", patternChange.CurrentValueText);
    }

    [Fact]
    public void CreateReview_ShouldReturnNoDataHeadline_WhenCurrentReportHasNoReadings()
    {
        // Arrange
        var service = CreateService();

        var previous = CreateReport(
            PreviousStartsAt,
            PreviousEndsAt,
            readingsCount: 288,
            averageMgDl: 125m,
            minimumMgDl: 80m,
            maximumMgDl: 160m,
            timeInRangePercentage: 91m,
            coveragePercentage: 100m);

        var current = CreateReport(
            CurrentStartsAt,
            CurrentEndsAt,
            readingsCount: 0,
            averageMgDl: null,
            minimumMgDl: null,
            maximumMgDl: null,
            timeInRangePercentage: null,
            coveragePercentage: 0m);

        // Act
        var review = service.CreateReview(current, previous);

        // Assert
        Assert.Equal("Weekly review: no local readings available", review.Headline);
        Assert.Contains("no local readings", review.SummaryText);
    }

    [Fact]
    public void CreateReview_ShouldThrow_WhenCurrentReportIsNull()
    {
        // Arrange
        var service = CreateService();
        var previous = CreateReport(
            PreviousStartsAt,
            PreviousEndsAt,
            1,
            120m,
            100m,
            140m,
            100m,
            100m);

        // Act
        var exception = Assert.Throws<ArgumentNullException>(
            () => service.CreateReview(null!, previous));

        // Assert
        Assert.Equal("currentReport", exception.ParamName);
    }

    [Fact]
    public void CreateReview_ShouldThrow_WhenPreviousReportIsNull()
    {
        // Arrange
        var service = CreateService();
        var current = CreateReport(
            CurrentStartsAt,
            CurrentEndsAt,
            1,
            120m,
            100m,
            140m,
            100m,
            100m);

        // Act
        var exception = Assert.Throws<ArgumentNullException>(
            () => service.CreateReview(current, null!));

        // Assert
        Assert.Equal("previousReport", exception.ParamName);
    }

    #region Helpers

    /// <summary>
    /// Creates the weekly review service under test.
    /// </summary>
    /// <returns>The weekly review service.</returns>
    private static GlycemicDiaryWeeklyReviewService CreateService()
    {
        var completenessService = new GlucoseHistoryCompletenessScoringService();

        return new GlycemicDiaryWeeklyReviewService(
            completenessService,
            new GlycemicDiaryPatternAnalysisService(completenessService));
    }

    /// <summary>
    /// Creates a diary report.
    /// </summary>
    /// <param name="periodStartsAt">The period start.</param>
    /// <param name="periodEndsAt">The period end.</param>
    /// <param name="readingsCount">The readings count.</param>
    /// <param name="averageMgDl">The average glucose value.</param>
    /// <param name="minimumMgDl">The minimum glucose value.</param>
    /// <param name="maximumMgDl">The maximum glucose value.</param>
    /// <param name="timeInRangePercentage">The time-in-range percentage.</param>
    /// <param name="coveragePercentage">The coverage percentage.</param>
    /// <param name="dailyEntries">The optional daily entries.</param>
    /// <returns>The diary report.</returns>
    private static GlycemicDiaryReport CreateReport(
        DateTimeOffset periodStartsAt,
        DateTimeOffset periodEndsAt,
        int readingsCount,
        decimal? averageMgDl,
        decimal? minimumMgDl,
        decimal? maximumMgDl,
        decimal? timeInRangePercentage,
        decimal coveragePercentage,
        IReadOnlyCollection<GlycemicDiaryDailyEntry>? dailyEntries = null)
    {
        dailyEntries ??=
        [
            new GlycemicDiaryDailyEntry(
                DateOnly.FromDateTime(periodStartsAt.Date),
                readingsCount,
                averageMgDl,
                minimumMgDl,
                maximumMgDl,
                timeInRangePercentage,
                coveragePercentage,
                coveragePercentage >= 99.5m,
                coveragePercentage >= 99.5m ? 0 : 1,
                [])
        ];

        return new GlycemicDiaryReport(
            periodStartsAt,
            periodEndsAt,
            readingsCount,
            averageMgDl,
            minimumMgDl,
            maximumMgDl,
            timeInRangePercentage,
            CreateContinuityReport(periodStartsAt, periodEndsAt, readingsCount, coveragePercentage),
            dailyEntries);
    }

    /// <summary>
    /// Creates a daily entry with dinner data.
    /// </summary>
    /// <param name="date">The diary date.</param>
    /// <param name="representativeValueMgDl">The representative glucose value.</param>
    /// <param name="minimumMgDl">The minimum glucose value.</param>
    /// <param name="maximumMgDl">The maximum glucose value.</param>
    /// <returns>The daily entry.</returns>
    private static GlycemicDiaryDailyEntry CreateDailyEntryWithDinner(
        DateOnly date,
        decimal representativeValueMgDl,
        decimal minimumMgDl,
        decimal maximumMgDl)
    {
        return new GlycemicDiaryDailyEntry(
            date,
            96,
            145m,
            minimumMgDl,
            maximumMgDl,
            86m,
            100m,
            true,
            0,
            [
                new GlycemicDiaryTimeBlockEntry(
                    GlycemicDiaryTimeBlockKind.Dinner,
                    "Dinner",
                    new TimeOnly(18, 0),
                    new TimeOnly(21, 59, 59),
                    8,
                    representativeValueMgDl,
                    null,
                    representativeValueMgDl,
                    minimumMgDl,
                    maximumMgDl)
            ]);
    }

    /// <summary>
    /// Creates a continuity report.
    /// </summary>
    /// <param name="periodStartsAt">The period start.</param>
    /// <param name="periodEndsAt">The period end.</param>
    /// <param name="readingsCount">The readings count.</param>
    /// <param name="coveragePercentage">The coverage percentage.</param>
    /// <returns>The continuity report.</returns>
    private static GlucoseHistoryContinuityReport CreateContinuityReport(
        DateTimeOffset periodStartsAt,
        DateTimeOffset periodEndsAt,
        int readingsCount,
        decimal coveragePercentage)
    {
        return new GlucoseHistoryContinuityReport(
            periodStartsAt,
            periodEndsAt,
            readingsCount,
            coveragePercentage,
            []);
    }

    #endregion
}
