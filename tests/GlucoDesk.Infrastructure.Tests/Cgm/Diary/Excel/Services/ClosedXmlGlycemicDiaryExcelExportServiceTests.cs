using ClosedXML.Excel;
using GlucoDesk.Application.Cgm.Diary.Exports.Requests;
using GlucoDesk.Application.Cgm.Diary.Requests;
using GlucoDesk.Application.Cgm.Diary.Results;
using GlucoDesk.Application.Cgm.Diary.Services.Abstractions;
using GlucoDesk.Application.Cgm.History.Continuity.Results;
using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Infrastructure.Cgm.Diary.Excel.Options;
using GlucoDesk.Infrastructure.Cgm.Diary.Excel.Services;

namespace GlucoDesk.Infrastructure.Tests.Cgm.Diary.Excel.Services;

public sealed class ClosedXmlGlycemicDiaryExcelExportServiceTests
{
    [Fact]
    public async Task ExportAsync_ShouldReturnExcelFile()
    {
        // Arrange
        var report = CreateReport();
        var service = CreateService(Result<GlycemicDiaryReport>.Success(report));

        // Act
        var result = await service.ExportAsync(
            new GlycemicDiaryExcelExportRequest(
                new GlycemicDiaryRequest(report.PeriodStartsAt, report.PeriodEndsAt)),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.EndsWith(".xlsx", result.Value.FileName, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(GlycemicDiaryExcelExportOptions.ExcelContentType, result.Value.ContentType);
        Assert.NotEmpty(result.Value.Content);
    }

    [Fact]
    public async Task ExportAsync_ShouldCreateExpectedWorksheets()
    {
        // Arrange
        var report = CreateReport();
        var service = CreateService(Result<GlycemicDiaryReport>.Success(report));

        // Act
        var result = await service.ExportAsync(
            new GlycemicDiaryExcelExportRequest(
                new GlycemicDiaryRequest(report.PeriodStartsAt, report.PeriodEndsAt)),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        using var stream = new MemoryStream(result.Value.Content);
        using var workbook = new XLWorkbook(stream);

        Assert.Contains("Overview", workbook.Worksheets.Select(sheet => sheet.Name));
        Assert.Contains("Daily diary", workbook.Worksheets.Select(sheet => sheet.Name));
        Assert.Contains("Time blocks", workbook.Worksheets.Select(sheet => sheet.Name));
        Assert.Contains("Data completeness", workbook.Worksheets.Select(sheet => sheet.Name));
    }

    [Fact]
    public async Task ExportAsync_ShouldWriteDailyDiaryValues()
    {
        // Arrange
        var report = CreateReport();
        var service = CreateService(Result<GlycemicDiaryReport>.Success(report));

        // Act
        var result = await service.ExportAsync(
            new GlycemicDiaryExcelExportRequest(
                new GlycemicDiaryRequest(report.PeriodStartsAt, report.PeriodEndsAt)),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        using var stream = new MemoryStream(result.Value.Content);
        using var workbook = new XLWorkbook(stream);

        var worksheet = workbook.Worksheet("Daily diary");

        Assert.Equal("Date", worksheet.Cell(1, 1).GetString());
        Assert.Equal("Readings", worksheet.Cell(1, 2).GetString());
        Assert.Equal(4, worksheet.Cell(2, 2).GetValue<int>());
        Assert.Equal(140d, worksheet.Cell(2, 3).GetValue<double>());
        Assert.Equal("Yes", worksheet.Cell(2, 8).GetString());
    }

    [Fact]
    public async Task ExportAsync_ShouldUseCustomFileName()
    {
        // Arrange
        var report = CreateReport();
        var service = CreateService(Result<GlycemicDiaryReport>.Success(report));

        // Act
        var result = await service.ExportAsync(
            new GlycemicDiaryExcelExportRequest(
                new GlycemicDiaryRequest(report.PeriodStartsAt, report.PeriodEndsAt),
                "my-diary"),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("my-diary.xlsx", result.Value.FileName);
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
            new GlycemicDiaryExcelExportRequest(
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
    /// Creates the Excel export service.
    /// </summary>
    /// <param name="diaryResult">The diary service result.</param>
    /// <returns>The Excel export service.</returns>
    private static ClosedXmlGlycemicDiaryExcelExportService CreateService(
        Result<GlycemicDiaryReport> diaryResult)
    {
        return new ClosedXmlGlycemicDiaryExcelExportService(
            new FakeGlycemicDiaryService(diaryResult),
            GlycemicDiaryExcelExportOptions.Default);
    }

    /// <summary>
    /// Creates a glycemic diary report for Excel export tests.
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
                    Application.Cgm.Diary.Enums.GlycemicDiaryTimeBlockKind.Breakfast,
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
                    Application.Cgm.Diary.Enums.GlycemicDiaryTimeBlockKind.Lunch,
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
                    Application.Cgm.Diary.Enums.GlycemicDiaryTimeBlockKind.Dinner,
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
                    Application.Cgm.Diary.Enums.GlycemicDiaryTimeBlockKind.Bedtime,
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