using GlucoDesk.Infrastructure.Cgm.Diary.Localization;

namespace GlucoDesk.Infrastructure.Tests.Cgm.Diary.Localization;

public sealed class GlycemicDiaryExportFinalPolishTests
{
    [Fact]
    public void Translate_ShouldLocalizeCompositeMissingPreviousPeriodSummary()
    {
        using var scope =
            GlycemicDiaryExportLocalizer.BeginScope("it");

        const string source =
            "The previous equivalent period has no local readings, so this review cannot produce a true comparison with the previous equivalent period. " +
            "The current period is summarized on its own. " +
            "Current local history reliability is Poor · 7.85%, so values should be interpreted carefully.";

        var result =
            GlycemicDiaryExportLocalizer.Translate(source);

        Assert.DoesNotContain(
            "The previous equivalent period",
            result,
            StringComparison.Ordinal);

        Assert.Contains(
            "Il periodo equivalente precedente",
            result,
            StringComparison.Ordinal);

        Assert.Contains(
            "Scarsa · 7,85%",
            result,
            StringComparison.Ordinal);

        Assert.Contains(
            "interpretati con cautela",
            result,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Translate_ShouldLocalizeInProgressDays()
    {
        using var scope =
            GlycemicDiaryExportLocalizer.BeginScope("it");

        Assert.Equal(
            "Giorni in corso",
            GlycemicDiaryExportLocalizer.Translate(
                "In-progress days"));
    }
}
