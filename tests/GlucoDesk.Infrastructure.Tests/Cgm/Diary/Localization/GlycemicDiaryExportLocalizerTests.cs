using GlucoDesk.Infrastructure.Cgm.Diary.Localization;

namespace GlucoDesk.Infrastructure.Tests.Cgm.Diary.Localization;

public sealed class GlycemicDiaryExportLocalizerTests
{
    [Fact]
    public void Translate_ShouldReturnEnglishText_InEnglishScope()
    {
        using var scope =
            GlycemicDiaryExportLocalizer.BeginScope("en");

        var result =
            GlycemicDiaryExportLocalizer.Translate(
                "Glycemic diary");

        Assert.Equal("Glycemic diary", result);
    }

    [Fact]
    public void Translate_ShouldReturnItalianText_InItalianScope()
    {
        using var scope =
            GlycemicDiaryExportLocalizer.BeginScope("it");

        var result =
            GlycemicDiaryExportLocalizer.Translate(
                "Glycemic diary");

        Assert.Equal("Diario glicemico", result);
    }

    [Fact]
    public void Scope_ShouldRestorePreviousLanguage_WhenDisposed()
    {
        using var englishScope =
            GlycemicDiaryExportLocalizer.BeginScope("en");

        using (GlycemicDiaryExportLocalizer.BeginScope("it"))
        {
            Assert.Equal(
                "Diario glicemico",
                GlycemicDiaryExportLocalizer.Translate(
                    "Glycemic diary"));
        }

        Assert.Equal(
            "Glycemic diary",
            GlycemicDiaryExportLocalizer.Translate(
                "Glycemic diary"));
    }

    [Fact]
    public void FormatDate_ShouldUseItalianDateFormat()
    {
        using var scope =
            GlycemicDiaryExportLocalizer.BeginScope("it");

        var result =
            GlycemicDiaryExportLocalizer.FormatDate(
                new DateTimeOffset(
                    2026,
                    7,
                    17,
                    9,
                    30,
                    0,
                    TimeSpan.FromHours(2)));

        Assert.Equal("17/07/2026", result);
    }
}
