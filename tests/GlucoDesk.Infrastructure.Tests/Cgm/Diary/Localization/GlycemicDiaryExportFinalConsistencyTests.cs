using GlucoDesk.Application.Cgm.Diary.Results;
using GlucoDesk.Application.Cgm.History.Continuity.Results;
using GlucoDesk.Infrastructure.Cgm.Diary.Localization;

namespace GlucoDesk.Infrastructure.Tests.Cgm.Diary.Localization;

public sealed class GlycemicDiaryExportFinalConsistencyTests
{
    [Fact]
    public void ItalianLocalization_ShouldUsePeriodComparisonTerminology()
    {
        using var scope =
            GlycemicDiaryExportLocalizer.BeginScope("it");

        Assert.Equal(
            "Confronto con il periodo precedente",
            GlycemicDiaryExportLocalizer.Translate(
                "Weekly review"));

        Assert.Equal(
            "Confronto non disponibile: mancano dati nel periodo precedente",
            GlycemicDiaryExportLocalizer.Translate(
                "Period comparison unavailable: previous data missing"));
    }

    [Fact]
    public void DailyStatus_ShouldReturnNoData_WhenDayHasNoReadings()
    {
        using var scope =
            GlycemicDiaryExportLocalizer.BeginScope("it");

        var day = CreateDay(
            date: new DateOnly(2026, 7, 24),
            readingsCount: 0,
            isDataComplete: false);

        var report = CreateReport(
            new DateTimeOffset(
                2026,
                7,
                24,
                23,
                59,
                59,
                TimeSpan.FromHours(2)),
            [day]);

        Assert.Equal(
            "Nessun dato",
            GlycemicDiaryExportLocalizer.GetDailyDataStatus(
                day,
                report));

        Assert.Equal(
            "—",
            GlycemicDiaryExportLocalizer
                .FormatDailyGapCount(day));
    }

    [Fact]
    public void DailyStatus_ShouldReturnInProgress_ForFinalPartialCurrentDay()
    {
        using var scope =
            GlycemicDiaryExportLocalizer.BeginScope("it");

        var day = CreateDay(
            date: new DateOnly(2026, 7, 26),
            readingsCount: 89,
            isDataComplete: false);

        var report = CreateReport(
            new DateTimeOffset(
                2026,
                7,
                26,
                7,
                24,
                0,
                TimeSpan.FromHours(2)),
            [day]);

        Assert.Equal(
            "In corso",
            GlycemicDiaryExportLocalizer.GetDailyDataStatus(
                day,
                report));
    }

    [Fact]
    public void ReportDayCounts_ShouldBeMutuallyExclusive()
    {
        var entries =
            new[]
            {
                CreateDay(
                    new DateOnly(2026, 7, 24),
                    readingsCount: 100,
                    isDataComplete: false),

                CreateDay(
                    new DateOnly(2026, 7, 25),
                    readingsCount: 288,
                    isDataComplete: true),

                CreateDay(
                    new DateOnly(2026, 7, 26),
                    readingsCount: 0,
                    isDataComplete: false)
            };

        var report = CreateReport(
            new DateTimeOffset(
                2026,
                7,
                26,
                23,
                59,
                59,
                TimeSpan.FromHours(2)),
            entries);

        Assert.Equal(1, report.CompleteDaysCount);
        Assert.Equal(1, report.PartialDaysCount);
        Assert.Equal(1, report.IncompleteDaysCount);
        Assert.Equal(1, report.EmptyDaysCount);

        Assert.Equal(
            report.DailyEntries.Count,
            report.CompleteDaysCount +
            report.PartialDaysCount +
            report.EmptyDaysCount);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ReportDayCounts_ShouldClassifyActiveFinalDayOnlyAsInProgress(
        bool isDataComplete)
    {
        var day = CreateDay(
            date: new DateOnly(2026, 7, 30),
            readingsCount: 120,
            isDataComplete: isDataComplete);

        var report = CreateReport(
            new DateTimeOffset(
                2026,
                7,
                30,
                21,
                30,
                0,
                TimeSpan.FromHours(2)),
            [day]);

        Assert.True(report.IsDayInProgress(day));

        Assert.Equal(0, report.CompleteDaysCount);
        Assert.Equal(0, report.PartialDaysCount);
        Assert.Equal(1, report.InProgressDaysCount);
        Assert.Equal(0, report.EmptyDaysCount);

        Assert.Equal(
            report.DailyEntries.Count,
            report.CompleteDaysCount +
            report.PartialDaysCount +
            report.InProgressDaysCount +
            report.EmptyDaysCount);
    }


    private static GlycemicDiaryDailyEntry CreateDay(
        DateOnly date,
        int readingsCount,
        bool isDataComplete)
    {
        return new GlycemicDiaryDailyEntry(
            date,
            readingsCount,
            readingsCount == 0 ? null : 130m,
            readingsCount == 0 ? null : 80m,
            readingsCount == 0 ? null : 190m,
            readingsCount == 0 ? null : 90m,
            readingsCount == 0 ? 0m : 95m,
            isDataComplete,
            readingsCount == 0 ? 1 : 1,
            []);
    }

    private static GlycemicDiaryReport CreateReport(
        DateTimeOffset endsAt,
        IReadOnlyCollection<GlycemicDiaryDailyEntry> entries)
    {
        var startsAt =
            new DateTimeOffset(
                entries.Min(entry => entry.Date)
                    .ToDateTime(TimeOnly.MinValue),
                TimeSpan.FromHours(2));

        return new GlycemicDiaryReport(
            startsAt,
            endsAt,
            entries.Sum(entry => entry.ReadingsCount),
            130m,
            80m,
            190m,
            90m,
            new GlucoseHistoryContinuityReport(
                startsAt,
                endsAt,
                entries.Sum(entry => entry.ReadingsCount),
                95m,
                []),
            entries);
    }
}
