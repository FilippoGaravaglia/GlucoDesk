using GlucoDesk.Application.Cgm.Diary.Exports.Requests;
using GlucoDesk.Application.Cgm.Diary.Exports.Results;
using GlucoDesk.Application.Cgm.Diary.Exports.Services.Abstractions;
using GlucoDesk.Application.Cgm.Diary.Results;
using GlucoDesk.Application.Cgm.Diary.Services.Abstractions;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Infrastructure.Cgm.Diary.Pdf.Options;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GlucoDesk.Infrastructure.Cgm.Diary.Pdf.Services;

/// <summary>
/// Exports glycemic diary reports to PDF using QuestPDF.
/// </summary>
public sealed class QuestPdfGlycemicDiaryPdfExportService : IGlycemicDiaryPdfExportService
{
    private readonly IGlycemicDiaryService _diaryService;
    private readonly GlycemicDiaryPdfExportOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="QuestPdfGlycemicDiaryPdfExportService"/> class.
    /// </summary>
    /// <param name="diaryService">The glycemic diary service.</param>
    /// <param name="options">The PDF export options.</param>
    public QuestPdfGlycemicDiaryPdfExportService(
        IGlycemicDiaryService diaryService,
        GlycemicDiaryPdfExportOptions options)
    {
        ArgumentNullException.ThrowIfNull(diaryService);
        ArgumentNullException.ThrowIfNull(options);

        _diaryService = diaryService;
        _options = options;
    }

    /// <inheritdoc />
    public async Task<Result<GlycemicDiaryExportFile>> ExportAsync(
        GlycemicDiaryPdfExportRequest request,
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

        QuestPDF.Settings.License = LicenseType.Community;

        var content = Document
            .Create(container => ComposeDocument(container, diaryResult.Value))
            .GeneratePdf();

        var file = new GlycemicDiaryExportFile(
            CreateFileName(request, diaryResult.Value),
            GlycemicDiaryPdfExportOptions.PdfContentType,
            content);

        return Result<GlycemicDiaryExportFile>.Success(file);
    }

    #region Helpers

    /// <summary>
    /// Composes the PDF document.
    /// </summary>
    /// <param name="container">The document container.</param>
    /// <param name="report">The glycemic diary report.</param>
    private void ComposeDocument(
        IDocumentContainer container,
        GlycemicDiaryReport report)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(32);
            page.DefaultTextStyle(style => style.FontSize(9));

            page.Header().Element(header => ComposeHeader(header, report));
            page.Content().Element(content => ComposeContent(content, report));
            page.Footer().Element(ComposeFooter);
        });
    }

    /// <summary>
    /// Composes the PDF header.
    /// </summary>
    /// <param name="container">The container.</param>
    /// <param name="report">The glycemic diary report.</param>
    private void ComposeHeader(
        IContainer container,
        GlycemicDiaryReport report)
    {
        container.Column(column =>
        {
            column.Spacing(4);

            column.Item().Text(_options.ApplicationName)
                .FontSize(20)
                .SemiBold();

            column.Item().Text("Glycemic diary")
                .FontSize(14)
                .FontColor(Colors.Grey.Darken2);

            column.Item().Text($"{FormatDate(report.PeriodStartsAt)} - {FormatDate(report.PeriodEndsAt)}")
                .FontSize(10)
                .FontColor(Colors.Grey.Darken1);

            column.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
        });
    }

    /// <summary>
    /// Composes the PDF content.
    /// </summary>
    /// <param name="container">The container.</param>
    /// <param name="report">The glycemic diary report.</param>
    private void ComposeContent(
        IContainer container,
        GlycemicDiaryReport report)
    {
        container.Column(column =>
        {
            column.Spacing(16);

            column.Item().Element(content => ComposeOverview(content, report));
            column.Item().Element(content => ComposeDailyDiaryTable(content, report));
            column.Item().Element(content => ComposeDataCompleteness(content, report));
            column.Item().Element(ComposeSafetyNotice);
        });
    }

    /// <summary>
    /// Composes the overview section.
    /// </summary>
    /// <param name="container">The container.</param>
    /// <param name="report">The glycemic diary report.</param>
    private static void ComposeOverview(
        IContainer container,
        GlycemicDiaryReport report)
    {
        container.Column(column =>
        {
            column.Spacing(8);

            column.Item().Text("Overview")
                .FontSize(13)
                .SemiBold();

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                WriteMetric(table, "Average", FormatMgDl(report.AverageMgDl));
                WriteMetric(table, "Range", $"{FormatPlain(report.MinimumMgDl)} - {FormatPlain(report.MaximumMgDl)} mg/dL");
                WriteMetric(table, "Time in range", FormatPercentage(report.TimeInRangePercentage));
                WriteMetric(table, "Data coverage", FormatPercentage(report.OverallContinuity.DataCoveragePercentage));
                WriteMetric(table, "Readings", report.ReadingsCount.ToString());
                WriteMetric(table, "Detected gaps", report.OverallContinuity.Gaps.Count.ToString());
                WriteMetric(table, "Incomplete days", report.IncompleteDaysCount.ToString());
                WriteMetric(table, "Empty days", report.EmptyDaysCount.ToString());
            });
        });
    }

    /// <summary>
    /// Composes the daily diary table.
    /// </summary>
    /// <param name="container">The container.</param>
    /// <param name="report">The glycemic diary report.</param>
    private static void ComposeDailyDiaryTable(
        IContainer container,
        GlycemicDiaryReport report)
    {
        container.Column(column =>
        {
            column.Spacing(8);

            column.Item().Text("Daily diary")
                .FontSize(13)
                .SemiBold();

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(1.1f);
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    HeaderCell(header.Cell()).Text("Date");
                    HeaderCell(header.Cell()).Text("Avg");
                    HeaderCell(header.Cell()).Text("Min");
                    HeaderCell(header.Cell()).Text("Max");
                    HeaderCell(header.Cell()).Text("TIR");
                    HeaderCell(header.Cell()).Text("Breakfast");
                    HeaderCell(header.Cell()).Text("Lunch");
                    HeaderCell(header.Cell()).Text("Dinner");
                    HeaderCell(header.Cell()).Text("Pre-night");
                });

                foreach (var day in report.DailyEntries.OrderBy(day => day.Date))
                {
                    BodyCell(table.Cell()).Text(day.Date.ToString("yyyy-MM-dd"));
                    BodyCell(table.Cell()).Text(FormatPlain(day.AverageMgDl));
                    BodyCell(table.Cell()).Text(FormatPlain(day.MinimumMgDl));
                    BodyCell(table.Cell()).Text(FormatPlain(day.MaximumMgDl));
                    BodyCell(table.Cell()).Text(FormatPercentage(day.TimeInRangePercentage));
                    BodyCell(table.Cell()).Text(FormatPlain(GetBlockValue(day, "Breakfast")));
                    BodyCell(table.Cell()).Text(FormatPlain(GetBlockValue(day, "Lunch")));
                    BodyCell(table.Cell()).Text(FormatPlain(GetBlockValue(day, "Dinner")));
                    BodyCell(table.Cell()).Text(FormatPlain(GetBlockValue(day, "Pre-night")));
                }
            });
        });
    }

    /// <summary>
    /// Composes the data completeness section.
    /// </summary>
    /// <param name="container">The container.</param>
    /// <param name="report">The glycemic diary report.</param>
    private static void ComposeDataCompleteness(
        IContainer container,
        GlycemicDiaryReport report)
    {
        container.Column(column =>
        {
            column.Spacing(8);

            column.Item().Text("Data completeness")
                .FontSize(13)
                .SemiBold();

            column.Item().Text("Days marked as partial may contain missing CGM history and should be interpreted carefully.")
                .FontSize(9)
                .FontColor(Colors.Grey.Darken2);

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(1.4f);
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    HeaderCell(header.Cell()).Text("Date");
                    HeaderCell(header.Cell()).Text("Coverage");
                    HeaderCell(header.Cell()).Text("Status");
                    HeaderCell(header.Cell()).Text("Gaps");
                });

                foreach (var day in report.DailyEntries.OrderBy(day => day.Date))
                {
                    BodyCell(table.Cell()).Text(day.Date.ToString("yyyy-MM-dd"));
                    BodyCell(table.Cell()).Text(FormatPercentage(day.DataCoveragePercentage));
                    BodyCell(table.Cell()).Text(day.IsDataComplete ? "Complete" : "Partial");
                    BodyCell(table.Cell()).Text(day.GapCount.ToString());
                }
            });
        });
    }

    /// <summary>
    /// Composes the safety notice section.
    /// </summary>
    /// <param name="container">The container.</param>
    private void ComposeSafetyNotice(IContainer container)
    {
        container
            .Border(1)
            .BorderColor(Colors.Grey.Lighten2)
            .Padding(10)
            .Column(column =>
            {
                column.Spacing(4);
                column.Item().Text("Safety notice").SemiBold();
                column.Item().Text(_options.SafetyDisclaimer)
                    .FontSize(8)
                    .FontColor(Colors.Grey.Darken2);
            });
    }

    /// <summary>
    /// Composes the PDF footer.
    /// </summary>
    /// <param name="container">The container.</param>
    private static void ComposeFooter(IContainer container)
    {
        container
            .AlignCenter()
            .Text(text =>
            {
                text.Span("Page ");
                text.CurrentPageNumber();
                text.Span(" of ");
                text.TotalPages();
            });
    }

    /// <summary>
    /// Writes a metric in the overview table.
    /// </summary>
    /// <param name="table">The table descriptor.</param>
    /// <param name="label">The metric label.</param>
    /// <param name="value">The metric value.</param>
    private static void WriteMetric(
        TableDescriptor table,
        string label,
        string value)
    {
        table.Cell().Element(MetricCell).Column(column =>
        {
            column.Spacing(2);
            column.Item().Text(label).FontSize(8).FontColor(Colors.Grey.Darken2);
            column.Item().Text(value).FontSize(11).SemiBold();
        });
    }

    /// <summary>
    /// Styles an overview metric cell.
    /// </summary>
    /// <param name="container">The container.</param>
    /// <returns>The styled container.</returns>
    private static IContainer MetricCell(IContainer container)
    {
        return container
            .Border(1)
            .BorderColor(Colors.Grey.Lighten2)
            .Padding(8);
    }

    /// <summary>
    /// Styles a table header cell.
    /// </summary>
    /// <param name="container">The container.</param>
    /// <returns>The styled container.</returns>
    private static IContainer HeaderCell(IContainer container)
    {
        return container
            .Background(Colors.Grey.Lighten3)
            .Border(1)
            .BorderColor(Colors.Grey.Lighten2)
            .PaddingVertical(4)
            .PaddingHorizontal(3)
            .DefaultTextStyle(style => style.FontSize(7).SemiBold());
    }

    /// <summary>
    /// Styles a table body cell.
    /// </summary>
    /// <param name="container">The container.</param>
    /// <returns>The styled container.</returns>
    private static IContainer BodyCell(IContainer container)
    {
        return container
            .Border(1)
            .BorderColor(Colors.Grey.Lighten3)
            .PaddingVertical(4)
            .PaddingHorizontal(3)
            .DefaultTextStyle(style => style.FontSize(7));
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
    /// Formats a timestamp as a date.
    /// </summary>
    /// <param name="timestamp">The timestamp.</param>
    /// <returns>The formatted date.</returns>
    private static string FormatDate(DateTimeOffset timestamp)
    {
        return timestamp.ToLocalTime().ToString("yyyy-MM-dd");
    }

    /// <summary>
    /// Formats a nullable glucose value with unit.
    /// </summary>
    /// <param name="value">The glucose value.</param>
    /// <returns>The formatted value.</returns>
    private static string FormatMgDl(decimal? value)
    {
        return value is null ? "—" : $"{value:0} mg/dL";
    }

    /// <summary>
    /// Formats a nullable decimal without unit.
    /// </summary>
    /// <param name="value">The decimal value.</param>
    /// <returns>The formatted value.</returns>
    private static string FormatPlain(decimal? value)
    {
        return value is null ? "—" : $"{value:0}";
    }

    /// <summary>
    /// Formats a nullable percentage value.
    /// </summary>
    /// <param name="value">The percentage value.</param>
    /// <returns>The formatted percentage.</returns>
    private static string FormatPercentage(decimal? value)
    {
        return value is null ? "—" : $"{value:0.##}%";
    }

    /// <summary>
    /// Creates the exported file name.
    /// </summary>
    /// <param name="request">The export request.</param>
    /// <param name="report">The diary report.</param>
    /// <returns>The exported file name.</returns>
    private static string CreateFileName(
        GlycemicDiaryPdfExportRequest request,
        GlycemicDiaryReport report)
    {
        if (!string.IsNullOrWhiteSpace(request.FileName))
        {
            return request.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
                ? request.FileName
                : $"{request.FileName}.pdf";
        }

        return $"glucodesk-diary-{report.PeriodStartsAt:yyyyMMdd}-{report.PeriodEndsAt:yyyyMMdd}.pdf";
    }

    #endregion
}