using GlucoDesk.Application.Cgm.Diary.Results;
using GlucoDesk.Application.Cgm.Diary.Stories.Enums;
using GlucoDesk.Application.Cgm.Diary.Stories.Services;
using GlucoDesk.Application.Cgm.History.Completeness.Services;
using GlucoDesk.Application.Cgm.History.Continuity.Results;

namespace GlucoDesk.Application.Tests.Cgm.Diary.Stories.Services;

public sealed class GlycemicDiaryStoryServiceTests
{
    private static readonly DateTimeOffset PeriodStartsAt =
        new(2026, 6, 19, 0, 0, 0, TimeSpan.Zero);

    private static readonly DateTimeOffset PeriodEndsAt =
        new(2026, 6, 19, 23, 59, 59, TimeSpan.Zero);

    [Fact]
    public void CreateStory_ShouldReturnExcellent_WhenHistoryIsCompleteAndMetricsAreStable()
    {
        // Arrange
        var service = CreateService();
        var report = CreateReport(
            readingsCount: 288,
            averageMgDl: 120m,
            minimumMgDl: 80m,
            maximumMgDl: 160m,
            timeInRangePercentage: 95m,
            coveragePercentage: 100m,
            dailyEntries:
            [
                CreateDailyEntry(
                    readingsCount: 288,
                    averageMgDl: 120m,
                    minimumMgDl: 80m,
                    maximumMgDl: 160m,
                    timeInRangePercentage: 95m,
                    dataCoveragePercentage: 100m,
                    isDataComplete: true,
                    gapCount: 0)
            ]);

        // Act
        var story = service.CreateStory(report);

        // Assert
        Assert.Equal(GlycemicDiaryStoryLevel.Excellent, story.Level);
        Assert.Equal("Stable glucose period", story.Headline);
        Assert.Contains("Average glucose was 120 mg/dL", story.SummaryText);
        Assert.Contains("History reliability: Complete · 100%", story.HistoryReliabilityText);

        var dailyStory = Assert.Single(story.DailyStories);
        Assert.Equal(GlycemicDiaryStoryLevel.Excellent, dailyStory.Level);
        Assert.Equal("Stable day", dailyStory.Headline);
        Assert.False(dailyStory.RequiresCaution);
    }

    [Fact]
    public void CreateStory_ShouldReturnCaution_WhenHistoryCoverageIsPoor()
    {
        // Arrange
        var service = CreateService();
        var report = CreateReport(
            readingsCount: 96,
            averageMgDl: 129m,
            minimumMgDl: 60m,
            maximumMgDl: 262m,
            timeInRangePercentage: 91.17m,
            coveragePercentage: 33.77m,
            dailyEntries:
            [
                CreateDailyEntry(
                    readingsCount: 96,
                    averageMgDl: 129m,
                    minimumMgDl: 60m,
                    maximumMgDl: 262m,
                    timeInRangePercentage: 91.17m,
                    dataCoveragePercentage: 33.77m,
                    isDataComplete: false,
                    gapCount: 3)
            ]);

        // Act
        var story = service.CreateStory(report);

        // Assert
        Assert.Equal(GlycemicDiaryStoryLevel.Caution, story.Level);
        Assert.Equal("Glucose story limited by data gaps", story.Headline);
        Assert.Contains("33.77% local history coverage", story.SummaryText);
        Assert.Equal(1, story.CautionDaysCount);

        var dailyStory = Assert.Single(story.DailyStories);
        Assert.Equal(GlycemicDiaryStoryLevel.Caution, dailyStory.Level);
        Assert.Equal("Partial local history", dailyStory.Headline);
        Assert.True(dailyStory.RequiresCaution);
        Assert.Contains("Data coverage: 33.77% with 3 gaps.", dailyStory.Highlights);
    }

    [Fact]
    public void CreateStory_ShouldReturnNoData_WhenReportHasNoReadings()
    {
        // Arrange
        var service = CreateService();
        var report = CreateReport(
            readingsCount: 0,
            averageMgDl: null,
            minimumMgDl: null,
            maximumMgDl: null,
            timeInRangePercentage: null,
            coveragePercentage: 0m,
            dailyEntries:
            [
                CreateDailyEntry(
                    readingsCount: 0,
                    averageMgDl: null,
                    minimumMgDl: null,
                    maximumMgDl: null,
                    timeInRangePercentage: null,
                    dataCoveragePercentage: 0m,
                    isDataComplete: false,
                    gapCount: 1)
            ]);

        // Act
        var story = service.CreateStory(report);

        // Assert
        Assert.Equal(GlycemicDiaryStoryLevel.NoData, story.Level);
        Assert.Equal("No local glucose history available", story.Headline);
        Assert.Equal("No local glucose readings are available for the selected period.", story.SummaryText);
        Assert.Equal(1, story.NoDataDaysCount);

        var dailyStory = Assert.Single(story.DailyStories);
        Assert.Equal(GlycemicDiaryStoryLevel.NoData, dailyStory.Level);
        Assert.Equal("No local glucose data", dailyStory.Headline);
        Assert.Contains("No local readings available.", dailyStory.Highlights);
    }

    [Fact]
    public void CreateStory_ShouldReturnVariableDailyStory_WhenCompleteDayHasLowAndVeryHighValues()
    {
        // Arrange
        var service = CreateService();
        var report = CreateReport(
            readingsCount: 288,
            averageMgDl: 144m,
            minimumMgDl: 60m,
            maximumMgDl: 262m,
            timeInRangePercentage: 78m,
            coveragePercentage: 100m,
            dailyEntries:
            [
                CreateDailyEntry(
                    readingsCount: 288,
                    averageMgDl: 144m,
                    minimumMgDl: 60m,
                    maximumMgDl: 262m,
                    timeInRangePercentage: 78m,
                    dataCoveragePercentage: 100m,
                    isDataComplete: true,
                    gapCount: 0)
            ]);

        // Act
        var story = service.CreateStory(report);

        // Assert
        Assert.Equal(GlycemicDiaryStoryLevel.Variable, story.Level);

        var dailyStory = Assert.Single(story.DailyStories);
        Assert.Equal(GlycemicDiaryStoryLevel.Variable, dailyStory.Level);
        Assert.Equal("Variable day with low and high excursions", dailyStory.Headline);
        Assert.Contains("Low glucose observed: 60 mg/dL.", dailyStory.Highlights);
        Assert.Contains("High glucose observed: 262 mg/dL.", dailyStory.Highlights);
    }

    [Fact]
    public void CreateStory_ShouldCreateDailyStoriesInDateOrder()
    {
        // Arrange
        var service = CreateService();
        var firstDate = new DateOnly(2026, 6, 19);
        var secondDate = new DateOnly(2026, 6, 20);

        var report = CreateReport(
            readingsCount: 576,
            averageMgDl: 120m,
            minimumMgDl: 80m,
            maximumMgDl: 160m,
            timeInRangePercentage: 95m,
            coveragePercentage: 100m,
            dailyEntries:
            [
                CreateDailyEntry(secondDate, 288, 121m, 80m, 160m, 95m, 100m, true, 0),
                CreateDailyEntry(firstDate, 288, 119m, 80m, 160m, 95m, 100m, true, 0)
            ]);

        // Act
        var story = service.CreateStory(report);

        // Assert
        Assert.Equal(
            [firstDate, secondDate],
            story.DailyStories.Select(day => day.Date).ToArray());
    }

    [Fact]
    public void CreateStory_ShouldThrow_WhenReportIsNull()
    {
        // Arrange
        var service = CreateService();

        // Act
        var exception = Assert.Throws<ArgumentNullException>(
            () => service.CreateStory(null!));

        // Assert
        Assert.Equal("report", exception.ParamName);
    }

    #region Helpers

    /// <summary>
    /// Creates the story service under test.
    /// </summary>
    /// <returns>The story service.</returns>
    private static GlycemicDiaryStoryService CreateService()
    {
        return new GlycemicDiaryStoryService(
            new GlucoseHistoryCompletenessScoringService());
    }

    /// <summary>
    /// Creates a glycemic diary report for story tests.
    /// </summary>
    /// <param name="readingsCount">The readings count.</param>
    /// <param name="averageMgDl">The average glucose value.</param>
    /// <param name="minimumMgDl">The minimum glucose value.</param>
    /// <param name="maximumMgDl">The maximum glucose value.</param>
    /// <param name="timeInRangePercentage">The time-in-range percentage.</param>
    /// <param name="coveragePercentage">The coverage percentage.</param>
    /// <param name="dailyEntries">The daily entries.</param>
    /// <returns>The report.</returns>
    private static GlycemicDiaryReport CreateReport(
        int readingsCount,
        decimal? averageMgDl,
        decimal? minimumMgDl,
        decimal? maximumMgDl,
        decimal? timeInRangePercentage,
        decimal coveragePercentage,
        IReadOnlyCollection<GlycemicDiaryDailyEntry> dailyEntries)
    {
        return new GlycemicDiaryReport(
            PeriodStartsAt,
            PeriodEndsAt,
            readingsCount,
            averageMgDl,
            minimumMgDl,
            maximumMgDl,
            timeInRangePercentage,
            CreateContinuityReport(readingsCount, coveragePercentage),
            dailyEntries);
    }

    /// <summary>
    /// Creates a daily entry using the default test date.
    /// </summary>
    /// <param name="readingsCount">The readings count.</param>
    /// <param name="averageMgDl">The average glucose value.</param>
    /// <param name="minimumMgDl">The minimum glucose value.</param>
    /// <param name="maximumMgDl">The maximum glucose value.</param>
    /// <param name="timeInRangePercentage">The time-in-range percentage.</param>
    /// <param name="dataCoveragePercentage">The data coverage percentage.</param>
    /// <param name="isDataComplete">Whether the data is complete.</param>
    /// <param name="gapCount">The gap count.</param>
    /// <returns>The daily entry.</returns>
    private static GlycemicDiaryDailyEntry CreateDailyEntry(
        int readingsCount,
        decimal? averageMgDl,
        decimal? minimumMgDl,
        decimal? maximumMgDl,
        decimal? timeInRangePercentage,
        decimal dataCoveragePercentage,
        bool isDataComplete,
        int gapCount)
    {
        return CreateDailyEntry(
            new DateOnly(2026, 6, 19),
            readingsCount,
            averageMgDl,
            minimumMgDl,
            maximumMgDl,
            timeInRangePercentage,
            dataCoveragePercentage,
            isDataComplete,
            gapCount);
    }

    /// <summary>
    /// Creates a daily entry.
    /// </summary>
    /// <param name="date">The diary date.</param>
    /// <param name="readingsCount">The readings count.</param>
    /// <param name="averageMgDl">The average glucose value.</param>
    /// <param name="minimumMgDl">The minimum glucose value.</param>
    /// <param name="maximumMgDl">The maximum glucose value.</param>
    /// <param name="timeInRangePercentage">The time-in-range percentage.</param>
    /// <param name="dataCoveragePercentage">The data coverage percentage.</param>
    /// <param name="isDataComplete">Whether the data is complete.</param>
    /// <param name="gapCount">The gap count.</param>
    /// <returns>The daily entry.</returns>
    private static GlycemicDiaryDailyEntry CreateDailyEntry(
        DateOnly date,
        int readingsCount,
        decimal? averageMgDl,
        decimal? minimumMgDl,
        decimal? maximumMgDl,
        decimal? timeInRangePercentage,
        decimal dataCoveragePercentage,
        bool isDataComplete,
        int gapCount)
    {
        return new GlycemicDiaryDailyEntry(
            date,
            readingsCount,
            averageMgDl,
            minimumMgDl,
            maximumMgDl,
            timeInRangePercentage,
            dataCoveragePercentage,
            isDataComplete,
            gapCount,
            []);
    }

    /// <summary>
    /// Creates a continuity report.
    /// </summary>
    /// <param name="readingsCount">The readings count.</param>
    /// <param name="coveragePercentage">The coverage percentage.</param>
    /// <returns>The continuity report.</returns>
    private static GlucoseHistoryContinuityReport CreateContinuityReport(
        int readingsCount,
        decimal coveragePercentage)
    {
        return new GlucoseHistoryContinuityReport(
            PeriodStartsAt,
            PeriodEndsAt,
            readingsCount,
            coveragePercentage,
            []);
    }

    #endregion
}
