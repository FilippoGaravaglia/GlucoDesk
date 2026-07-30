using GlucoDesk.Infrastructure.Cgm.Diary.Localization;

namespace GlucoDesk.Infrastructure.Tests.Cgm.Diary.Localization;

/// <summary>
/// Protects the exact narrative variants observed in real PDF and Excel
/// exports generated on macOS and Windows.
/// </summary>
public sealed class GlycemicDiaryExportRealDocumentRegressionTests
{
    [Theory]
    [InlineData(
        "Poor",
        "Scarsa")]
    [InlineData(
        "Partial",
        "Parziale")]
    [InlineData(
        "Glucose story limited by data gaps",
        "Storia glicemica limitata dai dati mancanti")]
    [InlineData(
        "Limited local history coverage",
        "Copertura limitata dello storico locale")]
    [InlineData(
        "Local history has limited coverage. Summary quality is low.",
        "Lo storico locale ha una copertura limitata. La qualità del riepilogo è bassa.")]
    [InlineData(
        "Local history is partially complete. Interpret summaries with caution.",
        "Lo storico locale è parzialmente completo. Interpreta i riepiloghi con cautela.")]
    public void Translate_ShouldLocalizeNarrativesObservedInRealExports(
        string source,
        string expected)
    {
        using var scope =
            GlycemicDiaryExportLocalizer.BeginScope("it");

        Assert.Equal(
            expected,
            GlycemicDiaryExportLocalizer.Translate(source));
    }

    [Theory]
    [InlineData(
        "The previous equivalent period has no local readings, so this review cannot produce a true week-over-week comparison. " +
        "The current period is summarized on its own. " +
        "Current local history reliability is Poor · 22%, so values should be interpreted carefully.",
        "Scarsa · 22%")]
    [InlineData(
        "The previous equivalent period has no local readings, so this review cannot produce a true comparison with the previous equivalent period. " +
        "The current period is summarized on its own. " +
        "Current local history reliability is Partial · 73.34%, so comparisons should be interpreted carefully.",
        "Parziale · 73,34%")]
    public void Translate_ShouldFullyLocalizeMissingPreviousPeriodVariants(
        string source,
        string expectedReliability)
    {
        using var scope =
            GlycemicDiaryExportLocalizer.BeginScope("it");

        var translated =
            GlycemicDiaryExportLocalizer.Translate(source);

        Assert.Contains(
            "Il periodo equivalente precedente",
            translated,
            StringComparison.Ordinal);

        Assert.Contains(
            "Il periodo corrente viene riepilogato singolarmente",
            translated,
            StringComparison.Ordinal);

        Assert.Contains(
            expectedReliability,
            translated,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            "The previous equivalent period",
            translated,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            "Current local history reliability",
            translated,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            "should be interpreted carefully",
            translated,
            StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(
        "Current local history reliability is Partial · 73.34%, so comparisons should be interpreted carefully.",
        "L’affidabilità dello storico locale corrente è Parziale · 73,34%; i confronti devono essere interpretati con cautela.")]
    [InlineData(
        "Current local history reliability is Poor · 22%, so values should be interpreted carefully.",
        "L’affidabilità dello storico locale corrente è Scarsa · 22%; i valori devono essere interpretati con cautela.")]
    public void Translate_ShouldLocalizeStandaloneCurrentReliabilitySentence(
        string source,
        string expected)
    {
        using var scope =
            GlycemicDiaryExportLocalizer.BeginScope("it");

        Assert.Equal(
            expected,
            GlycemicDiaryExportLocalizer.Translate(source));
    }

    [Fact]
    public void Translate_ShouldKeepRealExportNarrativesUnchangedInEnglish()
    {
        using var scope =
            GlycemicDiaryExportLocalizer.BeginScope("en");

        const string source =
            "Current local history reliability is Partial · 73.34%, " +
            "so comparisons should be interpreted carefully.";

        Assert.Equal(
            source,
            GlycemicDiaryExportLocalizer.Translate(source));
    }
}
