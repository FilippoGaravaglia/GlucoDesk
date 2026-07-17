using GlucoDesk.Infrastructure.Cgm.Diary.Localization;

namespace GlucoDesk.Infrastructure.Tests.Cgm.Diary.Localization;

public sealed class GlycemicDiaryExportLocalizerRegressionTests
{
    [Fact]
    public void Translate_ShouldNotReplaceSubstringsInsideUnknownWords()
    {
        using var scope = GlycemicDiaryExportLocalizer.BeginScope("it");

        var result = GlycemicDiaryExportLocalizer.Translate(
            "glucose profile");

        Assert.Equal("glucose profile", result);
        Assert.DoesNotContain("prdiile", result, StringComparison.Ordinal);
    }

    [Fact]
    public void Translate_ShouldLocalizeRecurringVariabilityTemplate()
    {
        using var scope = GlycemicDiaryExportLocalizer.BeginScope("it");

        var result = GlycemicDiaryExportLocalizer.Translate(
            "Recurring variability around Breakfast");

        Assert.Equal(
            "Variabilità ricorrente nella fascia Colazione",
            result);
    }

    [Fact]
    public void Translate_ShouldLocalizePatternDescriptionTemplate()
    {
        using var scope = GlycemicDiaryExportLocalizer.BeginScope("it");

        var result = GlycemicDiaryExportLocalizer.Translate(
            "21 days show a glucose spread of at least 80 mg/dL around Breakfast.");

        Assert.Equal(
            "21 giorni mostrano un’escursione glicemica di almeno 80 mg/dL nella fascia Colazione.",
            result);
    }

    [Fact]
    public void Translate_ShouldKeepEnglishContentUnchanged()
    {
        using var scope = GlycemicDiaryExportLocalizer.BeginScope("en");

        const string text =
            "Recurring low tendency around Lunch";

        Assert.Equal(
            text,
            GlycemicDiaryExportLocalizer.Translate(text));
    }

    [Fact]
    public void FormatDate_ShouldUseItalianDateOrder()
    {
        using var scope = GlycemicDiaryExportLocalizer.BeginScope("it");

        var result = GlycemicDiaryExportLocalizer.FormatDate(
            new DateOnly(2026, 7, 17));

        Assert.Equal("17/07/2026", result);
    }

    [Fact]
    public void FormatDate_ShouldUseEnglishDateOrder()
    {
        using var scope = GlycemicDiaryExportLocalizer.BeginScope("en");

        var result = GlycemicDiaryExportLocalizer.FormatDate(
            new DateOnly(2026, 7, 17));

        Assert.Equal("07/17/2026", result);
    }
}
