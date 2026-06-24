using ClosedXML.Excel;
using GlucoDesk.Application.Cgm.Diary.Exports.Requests;
using GlucoDesk.Application.Cgm.Diary.Exports.Results;
using GlucoDesk.Application.Cgm.Diary.Exports.Services.Abstractions;
using GlucoDesk.Application.Cgm.Diary.Results;
using GlucoDesk.Application.Cgm.Diary.Services.Abstractions;
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

    /// <summary>
    /// Initializes a new instance of the <see cref="ClosedXmlGlycemicDiaryExcelExportService"/> class.
    /// </summary>
    /// <param name="diaryService">The glycemic diary service.</param>
    /// <param name="options">The Excel export options.</param>
    /// <param name="completenessScoringService">The optional history completeness scoring service.</param>
    public ClosedXmlGlycemicDiaryExcelExportService(
        IGlycemicDiaryService diaryService,
        GlycemicDiaryExcelExportOptions options,
        IGlucoseHistoryCompletenessScoringService? completenessScoringService = null)
    {
        ArgumentNullException.ThrowIfNull(diaryService);
        ArgumentNullException.ThrowIfNull(options);

        _diaryService = diaryService;
        _options = options;
        _completenessScoringService = completenessScoringService
            ?? new GlucoseHistoryCompletenessScoringService();
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

        CreateOverviewWorksheet(workbook, diaryResult.Value, request.PreferredUnit);
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
        worksheet.Cell("A18").Value = "Safety notice";
        worksheet.Cell("B18").Value = _options.SafetyDisclaimer;

        worksheet.Cell("A1").Style.Font.Bold = true;
        worksheet.Cell("A1").Style.Font.FontSize = 18;
        worksheet.Cell("A2").Style.Font.Bold = true;
        worksheet.Cell("A2").Style.Font.FontSize = 14;

        worksheet.Range("A4:A18").Style.Font.Bold = true;
        worksheet.Range("A4:B16").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        worksheet.Range("A4:B16").Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        worksheet.Range("B7:B9").Style.NumberFormat.Format = GetGlucoseNumberFormat(preferredUnit);
        worksheet.Column("A").Width = 24;
        worksheet.Column("B").Width = 42;
        worksheet.Cell("B16").Style.Alignment.WrapText = true;
        worksheet.Cell("B18").Style.Alignment.WrapText = true;
        worksheet.SheetView.FreezeRows(3);
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