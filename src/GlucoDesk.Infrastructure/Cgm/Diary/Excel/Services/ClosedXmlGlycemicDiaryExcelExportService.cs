using ClosedXML.Excel;
using GlucoDesk.Application.Cgm.Diary.Exports.Requests;
using GlucoDesk.Application.Cgm.Diary.Exports.Results;
using GlucoDesk.Application.Cgm.Diary.Exports.Services.Abstractions;
using GlucoDesk.Application.Cgm.Diary.Results;
using GlucoDesk.Application.Cgm.Diary.Services.Abstractions;
using GlucoDesk.Application.Cgm.Diary.Stories.Services;
using GlucoDesk.Application.Cgm.Diary.Stories.Services.Abstractions;
using GlucoDesk.Application.Cgm.Diary.Patterns.Enums;
using GlucoDesk.Application.Cgm.Diary.Patterns.Results;
using GlucoDesk.Application.Cgm.Diary.Patterns.Services;
using GlucoDesk.Application.Cgm.Diary.Patterns.Services.Abstractions;
using GlucoDesk.Application.Cgm.Diary.Reviews.Requests;
using GlucoDesk.Application.Cgm.Diary.Reviews.Results;
using GlucoDesk.Application.Cgm.Diary.Reviews.Services;
using GlucoDesk.Application.Cgm.Diary.Reviews.Services.Abstractions;
using GlucoDesk.Application.Cgm.History.Completeness.Services;
using GlucoDesk.Application.Cgm.History.Completeness.Services.Abstractions;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Core.Glucose.ValueObjects;
using GlucoDesk.Infrastructure.Cgm.Diary.Excel.Options;

namespace GlucoDesk.Infrastructure.Cgm.Diary.Excel.Services;

/// <summary>
/// Exports glycemic diary reports to Excel using ClosedXML.
/// </summary>
public sealed class ClosedXmlGlycemicDiaryExcelExportService : IGlycemicDiaryExcelExportService
{
    private readonly IGlycemicDiaryService _diaryService;
    private readonly GlycemicDiaryExcelExportOptions _options;
    private readonly IGlucoseHistoryCompletenessScoringService _completenessScoringService;
    private readonly IGlycemicDiaryStoryService _storyService;
    private readonly IGlycemicDiaryPatternAnalysisService _patternAnalysisService;
    private readonly IGlycemicDiaryWeeklyReviewGenerationService _weeklyReviewGenerationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClosedXmlGlycemicDiaryExcelExportService"/> class.
    /// </summary>
    /// <param name="diaryService">The glycemic diary service.</param>
    /// <param name="options">The Excel export options.</param>
    /// <param name="completenessScoringService">The optional history completeness scoring service.</param>
    /// <param name="storyService">The optional glycemic diary story service.</param>
    /// <param name="patternAnalysisService">The optional glycemic diary pattern analysis service.</param>
    /// <param name="weeklyReviewGenerationService">The optional weekly review generation service.</param>
    public ClosedXmlGlycemicDiaryExcelExportService(
        IGlycemicDiaryService diaryService,
        GlycemicDiaryExcelExportOptions options,
        IGlucoseHistoryCompletenessScoringService? completenessScoringService = null,
        IGlycemicDiaryStoryService? storyService = null,
        IGlycemicDiaryPatternAnalysisService? patternAnalysisService = null,
        IGlycemicDiaryWeeklyReviewGenerationService? weeklyReviewGenerationService = null)
    {
        ArgumentNullException.ThrowIfNull(diaryService);
        ArgumentNullException.ThrowIfNull(options);

        _diaryService = diaryService;
        _options = options;
        _completenessScoringService = completenessScoringService
            ?? new GlucoseHistoryCompletenessScoringService();
        _storyService = storyService
            ?? new GlycemicDiaryStoryService(_completenessScoringService);
        _patternAnalysisService = patternAnalysisService
            ?? new GlycemicDiaryPatternAnalysisService(_completenessScoringService);
        _weeklyReviewGenerationService = weeklyReviewGenerationService
            ?? new GlycemicDiaryWeeklyReviewGenerationService(
                _diaryService,
                new GlycemicDiaryWeeklyReviewService(
                    _completenessScoringService,
                    _patternAnalysisService));
    }

    /// <inheritdoc />
    public async Task<Result<GlycemicDiaryExportFile>> ExportAsync(
        GlycemicDiaryExcelExportRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var diaryResult = await _diaryService
            .CreateDiaryAsync(request.DiaryRequest, cancellationToken)
            .ConfigureAwait(false);

        if (diaryResult.IsFailure)
        {
            return Result<GlycemicDiaryExportFile>.Failure(diaryResult.Error);
        }

        using var workbook = new XLWorkbook();

        var weeklyReviewResult = await CreateWeeklyReviewAsync(diaryResult.Value, cancellationToken)
            .ConfigureAwait(false);

        CreateOverviewWorksheet(workbook, diaryResult.Value, request.PreferredUnit);

        if (weeklyReviewResult.IsSuccess)
        {
            CreateWeeklyReviewWorksheet(workbook, weeklyReviewResult.Value);
        }
        else
        {
            CreateWeeklyReviewUnavailableWorksheet(workbook);
        }

        CreatePatternsWorksheet(workbook, diaryResult.Value);
        CreateDailyDiaryWorksheet(workbook, diaryResult.Value, request.PreferredUnit);
        CreateTimeBlocksWorksheet(workbook, diaryResult.Value, request.PreferredUnit);
        CreateDataCompletenessWorksheet(workbook, diaryResult.Value);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        var file = new GlycemicDiaryExportFile(
            CreateFileName(request, diaryResult.Value),
            GlycemicDiaryExcelExportOptions.ExcelContentType,
            stream.ToArray());

        return Result<GlycemicDiaryExportFile>.Success(file);
    }

    #region Helpers

    /// <summary>
    /// Creates the overview worksheet.
    /// </summary>
    /// <param name="workbook">The workbook.</param>
    /// <param name="report">The diary report.</param>
    /// <param name="preferredUnit">The preferred glucose display unit.</param>
    private void CreateOverviewWorksheet(
        XLWorkbook workbook,
        GlycemicDiaryReport report,
        GlucoseUnit preferredUnit)
    {
        var worksheet = workbook.Worksheets.Add("Overview");
        var completenessScore = _completenessScoringService.Calculate(report.OverallContinuity);
        var story = _storyService.CreateStory(report);

        worksheet.Cell("A1").Value = _options.ApplicationName;
        worksheet.Cell("A2").Value = "Glycemic diary";
        worksheet.Cell("A4").Value = "Period start";
        worksheet.Cell("B4").Value = report.PeriodStartsAt.LocalDateTime;
        worksheet.Cell("A5").Value = "Period end";
        worksheet.Cell("B5").Value = report.PeriodEndsAt.LocalDateTime;
        worksheet.Cell("A6").Value = "Readings";
        worksheet.Cell("B6").Value = report.ReadingsCount;
        worksheet.Cell("A7").Value = CreateGlucoseHeader("Average", preferredUnit);
        worksheet.Cell("B7").Value = ToNullableGlucoseDouble(report.AverageMgDl, preferredUnit);
        worksheet.Cell("A8").Value = CreateGlucoseHeader("Minimum", preferredUnit);
        worksheet.Cell("B8").Value = ToNullableGlucoseDouble(report.MinimumMgDl, preferredUnit);
        worksheet.Cell("A9").Value = CreateGlucoseHeader("Maximum", preferredUnit);
        worksheet.Cell("B9").Value = ToNullableGlucoseDouble(report.MaximumMgDl, preferredUnit);
        worksheet.Cell("A10").Value = "Time in range %";
        worksheet.Cell("B10").Value = ToNullableDouble(report.TimeInRangePercentage);
        worksheet.Cell("A11").Value = "Data coverage %";
        worksheet.Cell("B11").Value = ToNullableDouble(report.OverallContinuity.DataCoveragePercentage);
        worksheet.Cell("A12").Value = "Detected gaps";
        worksheet.Cell("B12").Value = report.OverallContinuity.Gaps.Count;
        worksheet.Cell("A13").Value = "Incomplete days";
        worksheet.Cell("B13").Value = report.IncompleteDaysCount;
        worksheet.Cell("A14").Value = "Empty days";
        worksheet.Cell("B14").Value = report.EmptyDaysCount;
        worksheet.Cell("A15").Value = "History reliability";
        worksheet.Cell("B15").Value = $"{completenessScore.StatusText} · {completenessScore.CoverageText}";
        worksheet.Cell("A16").Value = "Reliability details";
        worksheet.Cell("B16").Value = completenessScore.DetailText;
        worksheet.Cell("A17").Value = "Glucose story";
        worksheet.Cell("B17").Value = $"{story.Headline}: {story.SummaryText}";
        worksheet.Cell("A19").Value = "Safety notice";
        worksheet.Cell("B19").Value = _options.SafetyDisclaimer;

        worksheet.Cell("A1").Style.Font.Bold = true;
        worksheet.Cell("A1").Style.Font.FontSize = 18;
        worksheet.Cell("A2").Style.Font.Bold = true;
        worksheet.Cell("A2").Style.Font.FontSize = 14;

        worksheet.Range("A4:A19").Style.Font.Bold = true;
        worksheet.Range("A4:B17").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        worksheet.Range("A4:B17").Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        worksheet.Range("B7:B9").Style.NumberFormat.Format = GetGlucoseNumberFormat(preferredUnit);
        worksheet.Column("A").Width = 24;
        worksheet.Column("B").Width = 42;
        worksheet.Cell("B16").Style.Alignment.WrapText = true;
        worksheet.Cell("B17").Style.Alignment.WrapText = true;
        worksheet.Cell("B19").Style.Alignment.WrapText = true;
        worksheet.SheetView.FreezeRows(3);
    }

    /// <summary>
    /// Creates the weekly review unavailable worksheet.
    /// </summary>
    /// <param name="workbook">The workbook.</param>
    private static void CreateWeeklyReviewUnavailableWorksheet(
        XLWorkbook workbook)
    {
        var worksheet = workbook.Worksheets.Add("Weekly review");

        worksheet.Cell("A1").Value = "Weekly review";
        worksheet.Cell("A2").Value = "Weekly review unavailable";
        worksheet.Cell("A3").Value = "The weekly comparison could not be generated for this export. The diary data is still available.";

        worksheet.Range("A1:F1").Merge();
        worksheet.Range("A2:F2").Merge();
        worksheet.Range("A3:F3").Merge();

        worksheet.Cell("A1").Style.Font.Bold = true;
        worksheet.Cell("A1").Style.Font.FontSize = 16;
        worksheet.Cell("A2").Style.Font.Bold = true;
        worksheet.Cell("A3").Style.Alignment.WrapText = true;
        worksheet.Cell("A3").Style.Font.FontColor = XLColor.Gray;

        worksheet.Column("A").Width = 72;
    }

    /// <summary>
    /// Creates the weekly review worksheet.
    /// </summary>
    /// <param name="workbook">The workbook.</param>
    /// <param name="weeklyReview">The weekly review.</param>
    private static void CreateWeeklyReviewWorksheet(
        XLWorkbook workbook,
        GlycemicDiaryWeeklyReview weeklyReview)
    {
        var worksheet = workbook.Worksheets.Add("Weekly review");

        worksheet.Cell("A1").Value = "Weekly review";
        worksheet.Cell("A2").Value = weeklyReview.Headline;
        worksheet.Cell("A3").Value = weeklyReview.SummaryText;
        worksheet.Cell("A4").Value = weeklyReview.CurrentHistoryReliabilityText;

        worksheet.Range("A1:F1").Merge();
        worksheet.Range("A2:F2").Merge();
        worksheet.Range("A3:F3").Merge();
        worksheet.Range("A4:F4").Merge();

        worksheet.Cell("A1").Style.Font.Bold = true;
        worksheet.Cell("A1").Style.Font.FontSize = 16;
        worksheet.Cell("A2").Style.Font.Bold = true;
        worksheet.Cell("A3").Style.Alignment.WrapText = true;
        worksheet.Cell("A4").Style.Alignment.WrapText = true;
        worksheet.Cell("A4").Style.Font.FontColor = XLColor.Gray;

        worksheet.Cell("A6").Value = "Metric";
        worksheet.Cell("B6").Value = "Previous";
        worksheet.Cell("C6").Value = "Current";
        worksheet.Cell("D6").Value = "Delta";
        worksheet.Cell("E6").Value = "Direction";
        worksheet.Cell("F6").Value = "Severity";
        worksheet.Cell("G6").Value = "Description";

        var header = worksheet.Range("A6:G6");
        header.Style.Font.Bold = true;
        header.Style.Fill.BackgroundColor = XLColor.FromHtml("#EAF4FF");
        header.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        header.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        var row = 7;

        foreach (var change in weeklyReview.Changes)
        {
            worksheet.Cell(row, 1).Value = change.DisplayName;
            worksheet.Cell(row, 2).Value = change.PreviousValueText;
            worksheet.Cell(row, 3).Value = change.CurrentValueText;
            worksheet.Cell(row, 4).Value = change.DeltaText;
            worksheet.Cell(row, 5).Value = change.Direction.ToString();
            worksheet.Cell(row, 6).Value = change.Severity.ToString();
            worksheet.Cell(row, 7).Value = change.Description;

            row++;
        }

        if (weeklyReview.Highlights.Count > 0)
        {
            row += 2;
            worksheet.Cell(row, 1).Value = "Highlights";
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            row++;

            foreach (var highlight in weeklyReview.Highlights)
            {
                worksheet.Cell(row, 1).Value = highlight;
                worksheet.Range(row, 1, row, 7).Merge();
                row++;
            }
        }

        worksheet.Range(6, 1, Math.Max(row - 1, 6), 7).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        worksheet.Range(6, 1, Math.Max(row - 1, 6), 7).Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        worksheet.Column("A").Width = 24;
        worksheet.Column("B").Width = 18;
        worksheet.Column("C").Width = 18;
        worksheet.Column("D").Width = 14;
        worksheet.Column("E").Width = 18;
        worksheet.Column("F").Width = 14;
        worksheet.Column("G").Width = 72;
        worksheet.Column("G").Style.Alignment.WrapText = true;
        worksheet.SheetView.FreezeRows(6);
    }

    /// <summary>
    /// Creates a weekly review for the exported diary report.
    /// </summary>
    /// <param name="report">The diary report.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The weekly review generation result.</returns>
    private Task<Result<GlycemicDiaryWeeklyReview>> CreateWeeklyReviewAsync(
        GlycemicDiaryReport report,
        CancellationToken cancellationToken)
    {
        return _weeklyReviewGenerationService.GenerateAsync(
            new GlycemicDiaryWeeklyReviewRequest(
                report.PeriodStartsAt,
                report.PeriodEndsAt),
            cancellationToken);
    }

    /// <summary>
    /// Creates the local patterns worksheet.
    /// </summary>
    /// <param name="workbook">The workbook.</param>
    /// <param name="report">The glycemic diary report.</param>
    private void CreatePatternsWorksheet(
        XLWorkbook workbook,
        GlycemicDiaryReport report)
    {
        var worksheet = workbook.Worksheets.Add("Patterns");
        var analysis = _patternAnalysisService.Analyze(report);

        worksheet.Cell("A1").Value = "Local patterns";
        worksheet.Cell("A2").Value = "Recurring local glucose tendencies detected from diary time blocks.";

        worksheet.Cell("A4").Value = "Severity";
        worksheet.Cell("B4").Value = "Kind";
        worksheet.Cell("C4").Value = "Time block";
        worksheet.Cell("D4").Value = "Supporting days";
        worksheet.Cell("E4").Value = "Title";
        worksheet.Cell("F4").Value = "Description";

        var header = worksheet.Range("A4:F4");
        header.Style.Font.Bold = true;
        header.Style.Fill.BackgroundColor = XLColor.FromHtml("#EAF4FF");
        header.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        header.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        if (!analysis.HasPatterns)
        {
            worksheet.Cell("A5").Value = "No recurring local patterns detected.";
            worksheet.Range("A5:F5").Merge();
            worksheet.Cell("A5").Style.Font.Italic = true;
            worksheet.Cell("A5").Style.Font.FontColor = XLColor.Gray;
            worksheet.Columns().AdjustToContents();

            return;
        }

        var row = 5;

        foreach (var pattern in analysis.Patterns.OrderByDescending(GetPatternSeverityRank).ThenBy(pattern => pattern.Kind))
        {
            worksheet.Cell(row, 1).Value = pattern.Severity.ToString();
            worksheet.Cell(row, 2).Value = pattern.Kind.ToString();
            worksheet.Cell(row, 3).Value = pattern.TimeBlockLabel ?? "Overall";
            worksheet.Cell(row, 4).Value = pattern.SupportingDaysCount;
            worksheet.Cell(row, 5).Value = pattern.Title;
            worksheet.Cell(row, 6).Value = pattern.Description;

            row++;
        }

        var dataRange = worksheet.Range(4, 1, row - 1, 6);
        dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        worksheet.Column("A").Width = 14;
        worksheet.Column("B").Width = 24;
        worksheet.Column("C").Width = 18;
        worksheet.Column("D").Width = 18;
        worksheet.Column("E").Width = 34;
        worksheet.Column("F").Width = 72;
        worksheet.Column("F").Style.Alignment.WrapText = true;
        worksheet.SheetView.FreezeRows(4);
    }

    /// <summary>
    /// Gets a deterministic severity rank for pattern ordering.
    /// </summary>
    /// <param name="pattern">The pattern.</param>
    /// <returns>The severity rank.</returns>
    private static int GetPatternSeverityRank(GlycemicDiaryPattern pattern)
    {
        return pattern.Severity switch
        {
            GlycemicDiaryPatternSeverity.Important => 3,
            GlycemicDiaryPatternSeverity.Caution => 2,
            GlycemicDiaryPatternSeverity.Info => 1,
            _ => 0
        };
    }

    /// <summary>
    /// Creates the daily diary worksheet.
    /// </summary>
    /// <param name="workbook">The workbook.</param>
    /// <param name="report">The diary report.</param>
    /// <param name="preferredUnit">The preferred glucose display unit.</param>
    private static void CreateDailyDiaryWorksheet(
        XLWorkbook workbook,
        GlycemicDiaryReport report,
        GlucoseUnit preferredUnit)
    {
        var worksheet = workbook.Worksheets.Add("Daily diary");

        var headers = new[]
        {
            "Date",
            "Readings",
            CreateGlucoseHeader("Average", preferredUnit),
            CreateGlucoseHeader("Minimum", preferredUnit),
            CreateGlucoseHeader("Maximum", preferredUnit),
            "Time in range %",
            "Data coverage %",
            "Complete data",
            "Gaps",
            CreateGlucoseHeader("Breakfast", preferredUnit),
            CreateGlucoseHeader("Lunch", preferredUnit),
            CreateGlucoseHeader("Dinner", preferredUnit),
            CreateGlucoseHeader("Pre-night", preferredUnit)
        };

        WriteHeaderRow(worksheet, headers);

        var row = 2;

        foreach (var day in report.DailyEntries.OrderBy(day => day.Date))
        {
            worksheet.Cell(row, 1).Value = day.Date.ToDateTime(TimeOnly.MinValue);
            worksheet.Cell(row, 2).Value = day.ReadingsCount;
            worksheet.Cell(row, 3).Value = ToNullableGlucoseDouble(day.AverageMgDl, preferredUnit);
            worksheet.Cell(row, 4).Value = ToNullableGlucoseDouble(day.MinimumMgDl, preferredUnit);
            worksheet.Cell(row, 5).Value = ToNullableGlucoseDouble(day.MaximumMgDl, preferredUnit);
            worksheet.Cell(row, 6).Value = ToNullableDouble(day.TimeInRangePercentage);
            worksheet.Cell(row, 7).Value = ToNullableDouble(day.DataCoveragePercentage);
            worksheet.Cell(row, 8).Value = day.IsDataComplete ? "Yes" : "Partial";
            worksheet.Cell(row, 9).Value = day.GapCount;
            worksheet.Cell(row, 10).Value = ToNullableGlucoseDouble(GetBlockValue(day, "Breakfast"), preferredUnit);
            worksheet.Cell(row, 11).Value = ToNullableGlucoseDouble(GetBlockValue(day, "Lunch"), preferredUnit);
            worksheet.Cell(row, 12).Value = ToNullableGlucoseDouble(GetBlockValue(day, "Dinner"), preferredUnit);
            worksheet.Cell(row, 13).Value = ToNullableGlucoseDouble(GetBlockValue(day, "Pre-night"), preferredUnit);

            row++;
        }

        FormatUsedRangeAsTable(worksheet, "DailyDiaryTable");
        worksheet.Column(1).Style.DateFormat.Format = "yyyy-mm-dd";

        foreach (var columnIndex in new[] { 3, 4, 5, 10, 11, 12, 13 })
        {
            worksheet.Column(columnIndex).Style.NumberFormat.Format = GetGlucoseNumberFormat(preferredUnit);
        }

        worksheet.Columns().AdjustToContents();
        worksheet.SheetView.FreezeRows(1);
    }

    /// <summary>
    /// Creates the time blocks worksheet.
    /// </summary>
    /// <param name="workbook">The workbook.</param>
    /// <param name="report">The diary report.</param>
    /// <param name="preferredUnit">The preferred glucose display unit.</param>
    private static void CreateTimeBlocksWorksheet(
        XLWorkbook workbook,
        GlycemicDiaryReport report,
        GlucoseUnit preferredUnit)
    {
        var worksheet = workbook.Worksheets.Add("Time blocks");

        var headers = new[]
        {
            "Date",
            "Block",
            "From",
            "To",
            "Readings",
            CreateGlucoseHeader("Representative", preferredUnit),
            "Representative timestamp",
            CreateGlucoseHeader("Average", preferredUnit),
            CreateGlucoseHeader("Minimum", preferredUnit),
            CreateGlucoseHeader("Maximum", preferredUnit)
        };

        WriteHeaderRow(worksheet, headers);

        var row = 2;

        foreach (var day in report.DailyEntries.OrderBy(day => day.Date))
        {
            foreach (var block in day.TimeBlocks)
            {
                worksheet.Cell(row, 1).Value = day.Date.ToDateTime(TimeOnly.MinValue);
                worksheet.Cell(row, 2).Value = block.Label;
                worksheet.Cell(row, 3).Value = block.StartsAt.ToString("HH:mm");
                worksheet.Cell(row, 4).Value = block.EndsAt.ToString("HH:mm");
                worksheet.Cell(row, 5).Value = block.ReadingsCount;
                worksheet.Cell(row, 6).Value = ToNullableGlucoseDouble(block.RepresentativeValueMgDl, preferredUnit);
                worksheet.Cell(row, 7).Value = block.RepresentativeTimestamp?.LocalDateTime;
                worksheet.Cell(row, 8).Value = ToNullableGlucoseDouble(block.AverageMgDl, preferredUnit);
                worksheet.Cell(row, 9).Value = ToNullableGlucoseDouble(block.MinimumMgDl, preferredUnit);
                worksheet.Cell(row, 10).Value = ToNullableGlucoseDouble(block.MaximumMgDl, preferredUnit);

                row++;
            }
        }

        FormatUsedRangeAsTable(worksheet, "TimeBlocksTable");
        worksheet.Column(1).Style.DateFormat.Format = "yyyy-mm-dd";
        worksheet.Column(7).Style.DateFormat.Format = "yyyy-mm-dd hh:mm";

        foreach (var columnIndex in new[] { 6, 8, 9, 10 })
        {
            worksheet.Column(columnIndex).Style.NumberFormat.Format = GetGlucoseNumberFormat(preferredUnit);
        }

        worksheet.Columns().AdjustToContents();
        worksheet.SheetView.FreezeRows(1);
    }

    /// <summary>
    /// Creates the data completeness worksheet.
    /// </summary>
    /// <param name="workbook">The workbook.</param>
    /// <param name="report">The diary report.</param>
    private static void CreateDataCompletenessWorksheet(
        XLWorkbook workbook,
        GlycemicDiaryReport report)
    {
        var worksheet = workbook.Worksheets.Add("Data completeness");

        var headers = new[]
        {
            "Date",
            "Coverage %",
            "Complete",
            "Gap count",
            "Readings"
        };

        WriteHeaderRow(worksheet, headers);

        var row = 2;

        foreach (var day in report.DailyEntries.OrderBy(day => day.Date))
        {
            worksheet.Cell(row, 1).Value = day.Date.ToDateTime(TimeOnly.MinValue);
            worksheet.Cell(row, 2).Value = ToNullableDouble(day.DataCoveragePercentage);
            worksheet.Cell(row, 3).Value = day.IsDataComplete ? "Yes" : "Partial";
            worksheet.Cell(row, 4).Value = day.GapCount;
            worksheet.Cell(row, 5).Value = day.ReadingsCount;

            row++;
        }

        FormatUsedRangeAsTable(worksheet, "DataCompletenessTable");
        worksheet.Column(1).Style.DateFormat.Format = "yyyy-mm-dd";
        worksheet.Columns().AdjustToContents();
        worksheet.SheetView.FreezeRows(1);
    }

    /// <summary>
    /// Writes a worksheet header row.
    /// </summary>
    /// <param name="worksheet">The worksheet.</param>
    /// <param name="headers">The headers.</param>
    private static void WriteHeaderRow(
        IXLWorksheet worksheet,
        IReadOnlyList<string> headers)
    {
        for (var index = 0; index < headers.Count; index++)
        {
            worksheet.Cell(1, index + 1).Value = headers[index];
        }

        var headerRange = worksheet.Range(1, 1, 1, headers.Count);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
    }

    /// <summary>
    /// Formats the used worksheet range as an Excel table.
    /// </summary>
    /// <param name="worksheet">The worksheet.</param>
    /// <param name="tableName">The table name.</param>
    private static void FormatUsedRangeAsTable(
        IXLWorksheet worksheet,
        string tableName)
    {
        var usedRange = worksheet.RangeUsed();

        if (usedRange is null)
        {
            return;
        }

        usedRange.CreateTable(tableName);
        usedRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        usedRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
    }

    /// <summary>
    /// Gets a representative time block value by label.
    /// </summary>
    /// <param name="day">The daily diary entry.</param>
    /// <param name="label">The time block label.</param>
    /// <returns>The representative value in mg/dL.</returns>
    private static decimal? GetBlockValue(
        GlycemicDiaryDailyEntry day,
        string label)
    {
        return day
            .TimeBlocks
            .FirstOrDefault(block => string.Equals(
                block.Label,
                label,
                StringComparison.OrdinalIgnoreCase))
            ?.RepresentativeValueMgDl;
    }

    /// <summary>
    /// Converts a nullable decimal to a nullable double for Excel cells.
    /// </summary>
    /// <param name="value">The decimal value.</param>
    /// <returns>The nullable double value.</returns>
    private static double? ToNullableDouble(decimal? value)
    {
        return value is null ? null : decimal.ToDouble(value.Value);
    }

    /// <summary>
    /// Converts a nullable mg/dL glucose value to the preferred unit for Excel cells.
    /// </summary>
    /// <param name="valueMgDl">The nullable glucose value expressed in mg/dL.</param>
    /// <param name="preferredUnit">The preferred glucose display unit.</param>
    /// <returns>The converted nullable double value.</returns>
    private static double? ToNullableGlucoseDouble(
        decimal? valueMgDl,
        GlucoseUnit preferredUnit)
    {
        var convertedValue = ConvertGlucoseAmount(valueMgDl, preferredUnit);

        return convertedValue is null
            ? null
            : decimal.ToDouble(convertedValue.Value);
    }

    /// <summary>
    /// Converts a nullable mg/dL glucose value to the preferred unit.
    /// </summary>
    /// <param name="valueMgDl">The nullable glucose value expressed in mg/dL.</param>
    /// <param name="preferredUnit">The preferred glucose display unit.</param>
    /// <returns>The converted nullable glucose amount.</returns>
    private static decimal? ConvertGlucoseAmount(
        decimal? valueMgDl,
        GlucoseUnit preferredUnit)
    {
        if (valueMgDl is null)
        {
            return null;
        }

        if (preferredUnit == GlucoseUnit.MgDl)
        {
            return decimal.Round(valueMgDl.Value, 0, MidpointRounding.AwayFromZero);
        }

        return new GlucoseValue(valueMgDl.Value, GlucoseUnit.MgDl)
            .ConvertTo(preferredUnit)
            .Amount;
    }

    /// <summary>
    /// Creates a glucose worksheet header with the selected unit label.
    /// </summary>
    /// <param name="label">The metric label.</param>
    /// <param name="preferredUnit">The preferred glucose display unit.</param>
    /// <returns>The worksheet header text.</returns>
    private static string CreateGlucoseHeader(
        string label,
        GlucoseUnit preferredUnit)
    {
        return $"{label} {FormatGlucoseUnitLabel(preferredUnit)}";
    }

    /// <summary>
    /// Formats a glucose unit label.
    /// </summary>
    /// <param name="unit">The glucose unit.</param>
    /// <returns>The formatted unit label.</returns>
    private static string FormatGlucoseUnitLabel(GlucoseUnit unit)
    {
        return unit switch
        {
            GlucoseUnit.MgDl => "mg/dL",
            GlucoseUnit.MmolL => "mmol/L",
            _ => throw new ArgumentOutOfRangeException(nameof(unit), unit, "Unsupported glucose unit.")
        };
    }

    /// <summary>
    /// Gets the Excel number format for the selected glucose unit.
    /// </summary>
    /// <param name="unit">The glucose unit.</param>
    /// <returns>The Excel number format.</returns>
    private static string GetGlucoseNumberFormat(GlucoseUnit unit)
    {
        return unit switch
        {
            GlucoseUnit.MgDl => "0",
            GlucoseUnit.MmolL => "0.0",
            _ => throw new ArgumentOutOfRangeException(nameof(unit), unit, "Unsupported glucose unit.")
        };
    }

    /// <summary>
    /// Creates the exported file name.
    /// </summary>
    /// <param name="request">The export request.</param>
    /// <param name="report">The diary report.</param>
    /// <returns>The exported file name.</returns>
    private static string CreateFileName(
        GlycemicDiaryExcelExportRequest request,
        GlycemicDiaryReport report)
    {
        if (!string.IsNullOrWhiteSpace(request.FileName))
        {
            return request.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase)
                ? request.FileName
                : $"{request.FileName}.xlsx";
        }

        return $"glucodesk-diary-{report.PeriodStartsAt:yyyyMMdd}-{report.PeriodEndsAt:yyyyMMdd}.xlsx";
    }

    #endregion
}