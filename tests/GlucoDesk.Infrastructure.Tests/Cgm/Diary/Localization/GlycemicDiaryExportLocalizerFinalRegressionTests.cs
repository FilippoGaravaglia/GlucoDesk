using GlucoDesk.Infrastructure.Cgm.Diary.Localization;

namespace GlucoDesk.Infrastructure.Tests.Cgm.Diary.Localization;

public sealed class GlycemicDiaryExportLocalizerFinalRegressionTests
{
    [Fact]
    public void Translate_ShouldLocalizeHistoryReliabilityWithDecimalCoverage()
    {
        using var scope = GlycemicDiaryExportLocalizer.BeginScope("it");

        var result = GlycemicDiaryExportLocalizer.Translate(
            "History reliability: Reliable · 93.87%. Local history is mostly complete, but minor gaps or missing readings may exist.");

        Assert.Equal(
            "Affidabilità dello storico: Affidabile · 93,87%. Lo storico locale è quasi completo, ma potrebbero essere presenti piccoli intervalli o letture mancanti.",
            result);
    }

    [Fact]
    public void Translate_ShouldLocalizeMultipleWeeklyReviewSentences()
    {
        using var scope = GlycemicDiaryExportLocalizer.BeginScope("it");

        var result = GlycemicDiaryExportLocalizer.Translate(
            "Average glucose increased from 128 mg/dL to 137 mg/dL (+9 mg/dL). Time in range decreased from 93.62% to 85.46% (-8.16%).");

        Assert.Equal(
            "La glicemia media è aumentata da 128 mg/dL a 137 mg/dL (+9 mg/dL). Il tempo nel range è diminuito da 93,62% a 85,46% (-8,16%).",
            result);
    }

    [Theory]
    [InlineData(
        "Data coverage increased from 13.74% to 93.87% (+80.13%).",
        "La copertura dei dati è aumentata da 13,74% a 93,87% (+80,13%).")]
    [InlineData(
        "Readings increased from 1348 to 8513 (+7165).",
        "Le letture sono aumentate da 1348 a 8513 (+7165).")]
    [InlineData(
        "Incomplete days decreased from 20 to 11 (-9).",
        "I giorni incompleti sono diminuiti da 20 a 11 (-9).")]
    public void Translate_ShouldUseCorrectItalianMetricGrammar(
        string source,
        string expected)
    {
        using var scope = GlycemicDiaryExportLocalizer.BeginScope("it");

        Assert.Equal(
            expected,
            GlycemicDiaryExportLocalizer.Translate(source));
    }

    [Fact]
    public void FormatPatternSummary_ShouldLocalizeLimitedList()
    {
        using var scope = GlycemicDiaryExportLocalizer.BeginScope("it");

        Assert.Equal(
            "Prime 5 di 9",
            GlycemicDiaryExportLocalizer.FormatPatternSummary(5, 9));
    }

    [Fact]
    public void FormatPatternSummary_ShouldKeepEnglishLimitedList()
    {
        using var scope = GlycemicDiaryExportLocalizer.BeginScope("en");

        Assert.Equal(
            "Top 5 of 9",
            GlycemicDiaryExportLocalizer.FormatPatternSummary(5, 9));
    }

    [Fact]
    public void FormatDailyDiaryDescription_ShouldUseItalian()
    {
        using var scope = GlycemicDiaryExportLocalizer.BeginScope("it");

        Assert.Equal(
            "Riepiloghi glicemici giornalieri e valori delle principali fasce orarie espressi in mg/dL.",
            GlycemicDiaryExportLocalizer.FormatDailyDiaryDescription("mg/dL"));
    }

    [Fact]
    public void Translate_ShouldLocalizeGlucoseUnitHeader()
    {
        using var scope = GlycemicDiaryExportLocalizer.BeginScope("it");

        Assert.Equal(
            "Media mg/dL",
            GlycemicDiaryExportLocalizer.Translate("Average mg/dL"));
    }

    [Fact]
    public void LocalizeNumericTokens_ShouldNotChangeEnglish()
    {
        using var scope = GlycemicDiaryExportLocalizer.BeginScope("en");

        Assert.Equal(
            "93.87%",
            GlycemicDiaryExportLocalizer.LocalizeNumericTokens("93.87%"));
    }

    [Fact]
    public void LocalizeNumericTokens_ShouldUseItalianDecimalSeparator()
    {
        using var scope = GlycemicDiaryExportLocalizer.BeginScope("it");

        Assert.Equal(
            "93,87%",
            GlycemicDiaryExportLocalizer.LocalizeNumericTokens("93.87%"));
    }
}
