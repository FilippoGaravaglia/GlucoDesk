using GlucoDesk.Infrastructure.Cgm.Diary.Localization;

namespace GlucoDesk.Infrastructure.Tests.Cgm.Diary.Localization;

public sealed class GlycemicDiaryExportAccuracyLocalizationTests
{
    [Theory]
    [InlineData(
        "Poor",
        "Scarsa")]
    [InlineData(
        "Glucose story limited by data gaps",
        "Storia glicemica limitata dai dati mancanti")]
    [InlineData(
        "Limited local history coverage",
        "Copertura limitata dello storico locale")]
    [InlineData(
        "Local history has limited coverage. Summary quality is low.",
        "Lo storico locale ha una copertura limitata. La qualità del riepilogo è bassa.")]
    public void Translate_ShouldTranslateMissingItalianReportTexts(
        string source,
        string expected)
    {
        using var scope =
            GlycemicDiaryExportLocalizer.BeginScope("it");

        var translated =
            GlycemicDiaryExportLocalizer.Translate(source);

        Assert.Equal(expected, translated);
    }

    [Fact]
    public void Translate_ShouldLocalizeCommaDecimalCoverageSentence()
    {
        using var scope =
            GlycemicDiaryExportLocalizer.BeginScope("it");

        var translated =
            GlycemicDiaryExportLocalizer.Translate(
                "The selected period has 6,38% local history coverage. " +
                "Interpret averages, time-in-range, and daily summaries carefully.");

        Assert.Contains(
            "6,38%",
            translated,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            "The selected period",
            translated,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            "Interpret averages",
            translated,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Translate_ShouldLocalizeCommaDecimalReliabilitySentence()
    {
        using var scope =
            GlycemicDiaryExportLocalizer.BeginScope("it");

        var translated =
            GlycemicDiaryExportLocalizer.Translate(
                "History reliability: Poor · 6,38%. " +
                "Local history has limited coverage. Summary quality is low.");

        Assert.Contains(
            "6,38%",
            translated,
            StringComparison.Ordinal);

        Assert.Contains(
            "Scarsa",
            translated,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            "History reliability",
            translated,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            "Summary quality is low",
            translated,
            StringComparison.Ordinal);
    }
}
