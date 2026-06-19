using GlucoDesk.Application.Cgm.Diary.Enums;
using GlucoDesk.Application.Cgm.Diary.Exports.Requests;
using GlucoDesk.Application.Cgm.Diary.Requests;
using GlucoDesk.Application.Cgm.Diary.Results;
using GlucoDesk.Application.Cgm.Diary.Services.Abstractions;
using GlucoDesk.Application.Cgm.History.Continuity.Results;
using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Infrastructure.Cgm.Diary.Pdf.Options;
using GlucoDesk.Infrastructure.Cgm.Diary.Pdf.Services;

namespace GlucoDesk.Infrastructure.Tests.Cgm.Diary.Pdf.Services;

public sealed class QuestPdfGlycemicDiaryPdfExportServiceTests
{
    [Fact]
    public async Task ExportAsync_ShouldReturnPdfFile()
    {
        // Arrange
        var report = CreateReport();
        var service = CreateService(Result<GlycemicDiaryReport>.Success(report));

        // Act
        var result = await service.ExportAsync(
            new GlycemicDiaryPdfExportRequest(
                new GlycemicDiaryRequest(report.PeriodStartsAt, report.PeriodEndsAt)),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.EndsWith(".pdf", result.Value.FileName, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(GlycemicDiaryPdfExportOptions.PdfContentType, result.Value.ContentType);
        Assert.NotEmpty(result.Value.Content);
    }

    [Fact]
    public async Task ExportAsync_ShouldGeneratePdfHeader()
    {
        // Arrange
        var report = CreateReport();
        var service = CreateService(Result<GlycemicDiaryReport>.Success(report));

        // Act
        var result = await service.ExportAsync(
            new GlycemicDiaryPdfExportRequest(
                new GlycemicDiaryRequest(report.PeriodStartsAt, report.PeriodEndsAt)),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        var header = System.Text.Encoding.ASCII.GetString(
            result.Value.Content,
            0,
            Math.Min(4, result.Value.Content.Length));

        Assert.Equal("%PDF", header);
    }

    [Fact]
    public async Task ExportAsync_ShouldUseCustomFileName()
    {
        // Arrange
        var report = CreateReport();
        var service = CreateService(Result<GlycemicDiaryReport>.Success(report));

        // Act
        var result = await service.ExportAsync(
            new GlycemicDiaryPdfExportRequest(
                new GlycemicDiaryRequest(report.PeriodStartsAt, report.PeriodEndsAt),
                "my-diary"),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("my-diary.pdf", result.Value.FileName);
    }

    [Fact]
    public async Task ExportAsync_ShouldKeepPdfExtension_WhenCustomFileNameAlreadyHasExtension()
    {
        // Arrange
        var report = CreateReport();
        var service = CreateService(Result<GlycemicDiaryReport>.Success(report));

        // Act
        var result = await service.ExportAsync(
            new GlycemicDiaryPdfExportRequest(
                new GlycemicDiaryRequest(report.PeriodStartsAt, report.PeriodEndsAt),
                "my-diary.pdf"),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("my-diary.pdf", result.Value.FileName);
    }

    [Fact]
    public async Task ExportAsync_ShouldPropagateDiaryFailure()
    {
        // Arrange
        var service = CreateService(Result<GlycemicDiaryReport>.Failure(
            new Error(
                "Diary.Failed",
                "Unable to create diary.")));

        // Act
        var result = await service.ExportAsync(
            new GlycemicDiaryPdfExportRequest(
                new GlycemicDiaryRequest(
                    new DateTimeOffset(2026, 6, 19, 0, 0, 0, TimeSpan.Zero),
                    new DateTimeOffset(2026, 6, 19, 23, 59, 59, TimeSpan.Zero))),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Diary.Failed", result.Error.Code);
    }

    #region Helpers

    /// <summary>
    /// Creates the PDF export service.
    /// </summary>
    /// <param name="diaryResult">The diary service result.</param>
    /// <returns>The PDF export service.</returns>
    private static QuestPdfGlycemicDiaryPdfExportService CreateService(
        Result<GlycemicDiaryReport> diaryResult)
    {
        return new QuestPdfGlycemicDiaryPdfExportService(
            new FakeGlycemicDiaryService(diaryResult),
            GlycemicDiaryPdfExportOptions.Default);
    }

    /// <summary>
    /// Creates a glycemic diary report for PDF export tests.
    /// </summary>
    /// <returns>The glycemic diary report.</returns>
    private static GlycemicDiaryReport CreateReport()
    {
        var periodStartsAt = new DateTimeOffset(2026, 6, 19, 0, 0, 0, TimeSpan.Zero);
        var periodEndsAt = new DateTimeOffset(2026, 6, 19, 23, 59, 59, TimeSpan.Zero);

        var continuity = new GlucoseHistoryContinuityReport(
            periodStartsAt,
            periodEndsAt,
            4,
            100m,
            []);

        var dailyEntry = new GlycemicDiaryDailyEntry(
            new DateOnly(2026, 6, 19),
            4,
            140m,
            110m,
            170m,
            100m,
            100m,
            true,
            0,
            [
                new GlycemicDiaryTimeBlockEntry(
                    GlycemicDiaryTimeBlockKind.Breakfast,
                    "Breakfast",
                    new TimeOnly(6, 0),
                    new TimeOnly(10, 59, 59),
                    1,
                    110m,
                    new DateTimeOffset(2026, 6, 19, 8, 0, 0, TimeSpan.Zero),
                    110m,
                    110m,
                    110m),
                new GlycemicDiaryTimeBlockEntry(
                    GlycemicDiaryTimeBlockKind.Lunch,
                    "Lunch",
                    new TimeOnly(11, 0),
                    new TimeOnly(15, 59, 59),
                    1,
                    150m,
                    new DateTimeOffset(2026, 6, 19, 12, 0, 0, TimeSpan.Zero),
                    150m,
                    150m,
                    150m),
                new GlycemicDiaryTimeBlockEntry(
                    GlycemicDiaryTimeBlockKind.Dinner,
                    "Dinner",
                    new TimeOnly(18, 0),
                    new TimeOnly(21, 59, 59),
                    1,
                    170m,
                    new DateTimeOffset(2026, 6, 19, 19, 0, 0, TimeSpan.Zero),
                    170m,
                    170m,
                    170m),
                new GlycemicDiaryTimeBlockEntry(
                    GlycemicDiaryTimeBlockKind.Bedtime,
                    "Pre-night",
                    new TimeOnly(22, 0),
                    new TimeOnly(23, 59, 59),
                    1,
                    130m,
                    new DateTimeOffset(2026, 6, 19, 22, 30, 0, TimeSpan.Zero),
                    130m,
                    130m,
                    130m)
            ]);

        return new GlycemicDiaryReport(
            periodStartsAt,
            periodEndsAt,
            4,
            140m,
            110m,
            170m,
            100m,
            continuity,
            [dailyEntry]);
    }

    private sealed class FakeGlycemicDiaryService : IGlycemicDiaryService
    {
        private readonly Result<GlycemicDiaryReport> _result;

        public FakeGlycemicDiaryService(Result<GlycemicDiaryReport> result)
        {
            _result = result;
        }

        /// <inheritdoc />
        public Task<Result<GlycemicDiaryReport>> CreateDiaryAsync(
            GlycemicDiaryRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(_result);
        }
    }

    #endregion
}