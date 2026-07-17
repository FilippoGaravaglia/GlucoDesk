using GlucoDesk.Infrastructure.Cgm.Diary.Localization;

namespace GlucoDesk.Infrastructure.Tests.Cgm.Diary.Localization;

public sealed class GlycemicDiaryExportFinalPolishTests
{
    [Theory]
    [InlineData("93.71%", "93,71%")]
    [InlineData("85.46%", "85,46%")]
    [InlineData("-8.25%", "-8,25%")]
    [InlineData("+79.91%", "+79,91%")]
    public void FormatReviewValue_ShouldUseItalianDecimalSeparator(
        string source,
        string expected)
    {
        using var scope =
            GlycemicDiaryExportLocalizer.BeginScope("it");

        Assert.Equal(
            expected,
            GlycemicDiaryExportLocalizer.FormatReviewValue(source));
    }

    [Theory]
    [InlineData("93.71%")]
    [InlineData("-8.25%")]
    public void FormatReviewValue_ShouldPreserveEnglishValue(
        string source)
    {
        using var scope =
            GlycemicDiaryExportLocalizer.BeginScope("en");

        Assert.Equal(
            source,
            GlycemicDiaryExportLocalizer.FormatReviewValue(source));
    }

    [Theory]
    [InlineData("Average glucose", "Glicemia media")]
    [InlineData("Time in range", "Tempo nel range")]
    [InlineData("Data coverage", "Copertura dei dati")]
    [InlineData("Readings", "Letture")]
    public void Translate_ShouldLocalizeWeeklyReviewMetric(
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
    public void FormatPercentage_ShouldUseItalianDecimalSeparator()
    {
        using var scope =
            GlycemicDiaryExportLocalizer.BeginScope("it");

        Assert.Equal(
            "93,87%",
            GlycemicDiaryExportLocalizer.FormatPercentage(93.87m));
    }

    [Fact]
    public void FormatPercentage_ShouldUseEnglishDecimalSeparator()
    {
        using var scope =
            GlycemicDiaryExportLocalizer.BeginScope("en");

        Assert.Equal(
            "93.87%",
            GlycemicDiaryExportLocalizer.FormatPercentage(93.87m));
    }
}
