using GlucoDesk.Application.Cgm.Diary.Enums;
using GlucoDesk.Application.Cgm.Diary.Patterns.Enums;
using GlucoDesk.Application.Cgm.Diary.Patterns.Services;
using GlucoDesk.Application.Cgm.Diary.Results;
using GlucoDesk.Application.Cgm.History.Completeness.Services;
using GlucoDesk.Application.Cgm.History.Continuity.Results;

namespace GlucoDesk.Application.Tests.Cgm.Diary.Patterns.Services;

public sealed class GlycemicDiaryPatternAnalysisServiceTests
{
    private static readonly DateTimeOffset PeriodStartsAt =
        new(2026, 6, 1, 0, 0, 0, TimeSpan.Zero);

    private static readonly DateTimeOffset PeriodEndsAt =
        new(2026, 6, 7, 23, 59, 59, TimeSpan.Zero);

    [Fact]
    public void Analyze_ShouldDetectLimitedCoveragePattern_WhenHistoryRequiresCaution()
    {
        // Arrange
        var service = CreateService();
        var report = CreateReport(
            readingsCount: 100,
            coveragePercentage: 40m,
            dailyEntries:
            [
                CreateDailyEntry(new DateOnly(2026, 6, 1), false, 40m, 1)
            ]);

        // Act
        var analysis = service.Analyze(report);

        // Assert
        var pattern = Assert.Single(
            analysis.Patterns,
            pattern => pattern.Kind == GlycemicDiaryPatternKind.LimitedDataCoverage);

        Assert.Equal(GlycemicDiaryPatternSeverity.Caution, pattern.Severity);
        Assert.Equal("Limited local history coverage", pattern.Title);
        Assert.Contains("40%", pattern.Description);
    }

    [Fact]
    public void Analyze_ShouldDetectRecurringHighPattern_ForRepeatedHighTimeBlockValues()
    {
        // Arrange
        var service = CreateService();
        var report = CreateReport(
            readingsCount: 300,
            coveragePercentage: 100m,
            dailyEntries:
            [
                CreateDailyEntryWithDinner(new DateOnly(2026, 6, 1), 190m, 170m, 210m),
                CreateDailyEntryWithDinner(new DateOnly(2026, 6, 2), 205m, 180m, 230m),
                CreateDailyEntryWithDinner(new DateOnly(2026, 6, 3), 145m, 120m, 160m)
            ]);

        // Act
        var analysis = service.Analyze(report);

        // Assert
        var pattern = Assert.Single(
            analysis.Patterns,
            pattern => pattern.Kind == GlycemicDiaryPatternKind.RecurringHigh);

        Assert.Equal(GlycemicDiaryPatternSeverity.Caution, pattern.Severity);
        Assert.Equal(GlycemicDiaryTimeBlockKind.Dinner, pattern.TimeBlockKind);
        Assert.Equal("Dinner", pattern.TimeBlockLabel);
        Assert.Equal(2, pattern.SupportingDaysCount);
    }

    [Fact]
    public void Analyze_ShouldDetectRecurringLowPattern_ForRepeatedLowTimeBlockValues()
    {
        // Arrange
        var service = CreateService();
        var report = CreateReport(
            readingsCount: 300,
            coveragePercentage: 100m,
            dailyEntries:
            [
                CreateDailyEntryWithBreakfast(new DateOnly(2026, 6, 1), 65m, 62m, 90m),
                CreateDailyEntryWithBreakfast(new DateOnly(2026, 6, 2), 68m, 60m, 88m),
                CreateDailyEntryWithBreakfast(new DateOnly(2026, 6, 3), 110m, 90m, 130m)
            ]);

        // Act
        var analysis = service.Analyze(report);

        // Assert
        var pattern = Assert.Single(
            analysis.Patterns,
            pattern => pattern.Kind == GlycemicDiaryPatternKind.RecurringLow);

        Assert.Equal(GlycemicDiaryPatternSeverity.Important, pattern.Severity);
        Assert.Equal(GlycemicDiaryTimeBlockKind.Breakfast, pattern.TimeBlockKind);
        Assert.Equal(2, pattern.SupportingDaysCount);
    }

    [Fact]
    public void Analyze_ShouldDetectRecurringVariabilityPattern_ForRepeatedWideRanges()
    {
        // Arrange
        var service = CreateService();
        var report = CreateReport(
            readingsCount: 300,
            coveragePercentage: 100m,
            dailyEntries:
            [
                CreateDailyEntryWithLunch(new DateOnly(2026, 6, 1), 140m, 90m, 190m),
                CreateDailyEntryWithLunch(new DateOnly(2026, 6, 2), 150m, 95m, 200m),
                CreateDailyEntryWithLunch(new DateOnly(2026, 6, 3), 130m, 120m, 150m)
            ]);

        // Act
        var analysis = service.Analyze(report);

        // Assert
        var pattern = Assert.Single(
            analysis.Patterns,
            pattern => pattern.Kind == GlycemicDiaryPatternKind.RecurringVariability);

        Assert.Equal(GlycemicDiaryPatternSeverity.Caution, pattern.Severity);
        Assert.Equal(GlycemicDiaryTimeBlockKind.Lunch, pattern.TimeBlockKind);
        Assert.Equal(2, pattern.SupportingDaysCount);
    }

    [Fact]
    public void Analyze_ShouldDetectStableTimeBlockPattern_WhenBlockIsConsistentlyStable()
    {
        // Arrange
        var service = CreateService();
        var report = CreateReport(
            readingsCount: 300,
            coveragePercentage: 100m,
            dailyEntries:
            [
                CreateDailyEntryWithBedtime(new DateOnly(2026, 6, 1), 120m, 110m, 135m),
                CreateDailyEntryWithBedtime(new DateOnly(2026, 6, 2), 125m, 115m, 140m),
                CreateDailyEntryWithBedtime(new DateOnly(2026, 6, 3), 118m, 108m, 132m)
            ]);

        // Act
        var analysis = service.Analyze(report);

        // Assert
        var pattern = Assert.Single(
            analysis.Patterns,
            pattern => pattern.Kind == GlycemicDiaryPatternKind.StableTimeBlock);

        Assert.Equal(GlycemicDiaryPatternSeverity.Info, pattern.Severity);
        Assert.Equal(GlycemicDiaryTimeBlockKind.Bedtime, pattern.TimeBlockKind);
        Assert.Equal(3, pattern.SupportingDaysCount);
        Assert.True(analysis.HasPatterns);
        Assert.Equal(1, analysis.TimeBlockPatternsCount);
    }

    [Fact]
    public void Analyze_ShouldReturnNoPatterns_WhenNoRecurringPatternExists()
    {
        // Arrange
        var service = CreateService();
        var report = CreateReport(
            readingsCount: 300,
            coveragePercentage: 100m,
            dailyEntries:
            [
                CreateDailyEntryWithDinner(new DateOnly(2026, 6, 1), 120m, 110m, 140m),
                CreateDailyEntryWithDinner(new DateOnly(2026, 6, 2), 150m, 140m, 165m)
            ]);

        // Act
        var analysis = service.Analyze(report);

        // Assert
        Assert.False(analysis.HasPatterns);
        Assert.Empty(analysis.Patterns);
    }

    [Fact]
    public void Analyze_ShouldThrow_WhenReportIsNull()
    {
        // Arrange
        var service = CreateService();

        // Act
        var exception = Assert.Throws<ArgumentNullException>(
            () => service.Analyze(null!));

        // Assert
        Assert.Equal("report", exception.ParamName);
    }

    #region Helpers

    /// <summary>
    /// Creates the pattern analysis service under test.
    /// </summary>
    /// <returns>The service.</returns>
    private static GlycemicDiaryPatternAnalysisService CreateService()
    {
        return new GlycemicDiaryPatternAnalysisService(
            new GlucoseHistoryCompletenessScoringService());
    }

    /// <summary>
    /// Creates a diary report.
    /// </summary>
    /// <param name="readingsCount">The readings count.</param>
    /// <param name="coveragePercentage">The coverage percentage.</param>
    /// <param name="dailyEntries">The daily entries.</param>
    /// <returns>The diary report.</returns>
    private static GlycemicDiaryReport CreateReport(
        int readingsCount,
        decimal coveragePercentage,
        IReadOnlyCollection<GlycemicDiaryDailyEntry> dailyEntries)
    {
        return new GlycemicDiaryReport(
            PeriodStartsAt,
            PeriodEndsAt,
            readingsCount,
            130m,
            80m,
            190m,
            90m,
            CreateContinuityReport(readingsCount, coveragePercentage),
            dailyEntries);
    }

    /// <summary>
    /// Creates a generic daily entry.
    /// </summary>
    /// <param name="date">The diary date.</param>
    /// <param name="isDataComplete">Whether data is complete.</param>
    /// <param name="coveragePercentage">The coverage percentage.</param>
    /// <param name="gapCount">The gap count.</param>
    /// <returns>The daily entry.</returns>
    private static GlycemicDiaryDailyEntry CreateDailyEntry(
        DateOnly date,
        bool isDataComplete,
        decimal coveragePercentage,
        int gapCount)
    {
        return new GlycemicDiaryDailyEntry(
            date,
            10,
            130m,
            80m,
            190m,
            90m,
            coveragePercentage,
            isDataComplete,
            gapCount,
            []);
    }

    /// <summary>
    /// Creates a daily entry with breakfast data.
    /// </summary>
    /// <param name="date">The diary date.</param>
    /// <param name="representativeValueMgDl">The representative glucose value.</param>
    /// <param name="minimumMgDl">The minimum glucose value.</param>
    /// <param name="maximumMgDl">The maximum glucose value.</param>
    /// <returns>The daily entry.</returns>
    private static GlycemicDiaryDailyEntry CreateDailyEntryWithBreakfast(
        DateOnly date,
        decimal representativeValueMgDl,
        decimal minimumMgDl,
        decimal maximumMgDl)
    {
        return CreateDailyEntryWithBlock(
            date,
            GlycemicDiaryTimeBlockKind.Breakfast,
            "Breakfast",
            representativeValueMgDl,
            minimumMgDl,
            maximumMgDl);
    }

    /// <summary>
    /// Creates a daily entry with lunch data.
    /// </summary>
    /// <param name="date">The diary date.</param>
    /// <param name="representativeValueMgDl">The representative glucose value.</param>
    /// <param name="minimumMgDl">The minimum glucose value.</param>
    /// <param name="maximumMgDl">The maximum glucose value.</param>
    /// <returns>The daily entry.</returns>
    private static GlycemicDiaryDailyEntry CreateDailyEntryWithLunch(
        DateOnly date,
        decimal representativeValueMgDl,
        decimal minimumMgDl,
        decimal maximumMgDl)
    {
        return CreateDailyEntryWithBlock(
            date,
            GlycemicDiaryTimeBlockKind.Lunch,
            "Lunch",
            representativeValueMgDl,
            minimumMgDl,
            maximumMgDl);
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
        return CreateDailyEntryWithBlock(
            date,
            GlycemicDiaryTimeBlockKind.Dinner,
            "Dinner",
            representativeValueMgDl,
            minimumMgDl,
            maximumMgDl);
    }

    /// <summary>
    /// Creates a daily entry with bedtime data.
    /// </summary>
    /// <param name="date">The diary date.</param>
    /// <param name="representativeValueMgDl">The representative glucose value.</param>
    /// <param name="minimumMgDl">The minimum glucose value.</param>
    /// <param name="maximumMgDl">The maximum glucose value.</param>
    /// <returns>The daily entry.</returns>
    private static GlycemicDiaryDailyEntry CreateDailyEntryWithBedtime(
        DateOnly date,
        decimal representativeValueMgDl,
        decimal minimumMgDl,
        decimal maximumMgDl)
    {
        return CreateDailyEntryWithBlock(
            date,
            GlycemicDiaryTimeBlockKind.Bedtime,
            "Pre-night",
            representativeValueMgDl,
            minimumMgDl,
            maximumMgDl);
    }

    /// <summary>
    /// Creates a daily entry with a single time block.
    /// </summary>
    /// <param name="date">The diary date.</param>
    /// <param name="kind">The block kind.</param>
    /// <param name="label">The block label.</param>
    /// <param name="representativeValueMgDl">The representative glucose value.</param>
    /// <param name="minimumMgDl">The minimum glucose value.</param>
    /// <param name="maximumMgDl">The maximum glucose value.</param>
    /// <returns>The daily entry.</returns>
    private static GlycemicDiaryDailyEntry CreateDailyEntryWithBlock(
        DateOnly date,
        GlycemicDiaryTimeBlockKind kind,
        string label,
        decimal representativeValueMgDl,
        decimal minimumMgDl,
        decimal maximumMgDl)
    {
        return new GlycemicDiaryDailyEntry(
            date,
            96,
            130m,
            minimumMgDl,
            maximumMgDl,
            95m,
            100m,
            true,
            0,
            [
                new GlycemicDiaryTimeBlockEntry(
                    kind,
                    label,
                    TimeOnly.MinValue,
                    new TimeOnly(23, 59, 59),
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
