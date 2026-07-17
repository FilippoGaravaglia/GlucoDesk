using GlucoDesk.Application.Cgm.Diary.Exports.Requests;
using GlucoDesk.Application.Cgm.Diary.Requests;

namespace GlucoDesk.Application.Tests.Cgm.Diary.Exports.Requests;

public sealed class GlycemicDiaryExportRequestLanguageTests
{
    private static readonly GlycemicDiaryRequest DiaryRequest =
        new(
            new DateTimeOffset(
                2026,
                7,
                1,
                0,
                0,
                0,
                TimeSpan.Zero),
            new DateTimeOffset(
                2026,
                7,
                15,
                0,
                0,
                0,
                TimeSpan.Zero));

    [Theory]
    [InlineData("it", "it")]
    [InlineData("it-IT", "it")]
    [InlineData("IT_it", "it")]
    [InlineData("en", "en")]
    [InlineData("en-US", "en")]
    [InlineData("fr", "en")]
    public void ExcelRequest_ShouldNormalizeLanguageCode(
        string languageCode,
        string expected)
    {
        var request = new GlycemicDiaryExcelExportRequest(
            DiaryRequest,
            languageCode: languageCode);

        Assert.Equal(expected, request.LanguageCode);
    }

    [Theory]
    [InlineData("it", "it")]
    [InlineData("it-IT", "it")]
    [InlineData("en", "en")]
    [InlineData("en-GB", "en")]
    [InlineData("de", "en")]
    public void PdfRequest_ShouldNormalizeLanguageCode(
        string languageCode,
        string expected)
    {
        var request = new GlycemicDiaryPdfExportRequest(
            DiaryRequest,
            languageCode: languageCode);

        Assert.Equal(expected, request.LanguageCode);
    }

    [Fact]
    public void ExcelRequest_ShouldRejectEmptyLanguageCode()
    {
        Assert.Throws<ArgumentException>(() =>
            new GlycemicDiaryExcelExportRequest(
                DiaryRequest,
                languageCode: " "));
    }

    [Fact]
    public void PdfRequest_ShouldRejectEmptyLanguageCode()
    {
        Assert.Throws<ArgumentException>(() =>
            new GlycemicDiaryPdfExportRequest(
                DiaryRequest,
                languageCode: " "));
    }
}
