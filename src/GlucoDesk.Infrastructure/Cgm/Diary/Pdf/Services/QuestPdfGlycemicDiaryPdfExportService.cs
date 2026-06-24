using System.Globalization;
using System.Reflection;
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
using GlucoDesk.Application.Cgm.Diary.Reviews.Enums;
using GlucoDesk.Application.Cgm.Diary.Reviews.Requests;
using GlucoDesk.Application.Cgm.Diary.Reviews.Results;
using GlucoDesk.Application.Cgm.Diary.Reviews.Services;
using GlucoDesk.Application.Cgm.Diary.Reviews.Services.Abstractions;
using GlucoDesk.Application.Cgm.History.Completeness.Services;
using GlucoDesk.Application.Cgm.History.Completeness.Services.Abstractions;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Core.Glucose.ValueObjects;
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
    private const string BrandBlue = "#0F7BFF";
    private const string BrandBlueDark = "#0A4FA3";
    private const string BrandBlueSoft = "#EAF4FF";
    private const string BrandBorder = "#B9DAFF";
    private const string SuccessSoft = "#EAFBF1";
    private const string WarningSoft = "#FFF6E5";
    private const string NeutralSoft = "#F7F9FC";
    private const string TextSecondary = "#5C6B82";
    private const string TextMuted = "#8A96A8";
    private const string White = "#FFFFFF";
    private const string SuccessBorder = "#B8E6C9";
    private const string SuccessText = "#137333";
    private const string WarningText = "#B26A00";
    private const string WarningBorder = "#F1D18A";

    private static readonly byte[]? BrandLogoBytes = LoadBrandLogoBytes();

    private readonly IGlycemicDiaryService _diaryService;
    private readonly GlycemicDiaryPdfExportOptions _options;
    private readonly IGlucoseHistoryCompletenessScoringService _completenessScoringService;
    private readonly IGlycemicDiaryStoryService _storyService;
    private readonly IGlycemicDiaryPatternAnalysisService _patternAnalysisService;
    private readonly IGlycemicDiaryWeeklyReviewGenerationService _weeklyReviewGenerationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="QuestPdfGlycemicDiaryPdfExportService"/> class.
    /// </summary>
    /// <param name="diaryService">The glycemic diary service.</param>
    /// <param name="options">The PDF export options.</param>
    /// <param name="completenessScoringService">The optional history completeness scoring service.</param>
    /// <param name="storyService">The optional glycemic diary story service.</param>
    /// <param name="patternAnalysisService">The optional glycemic diary pattern analysis service.</param>
    /// <param name="weeklyReviewGenerationService">The optional weekly review generation service.</param>
    public QuestPdfGlycemicDiaryPdfExportService(
        IGlycemicDiaryService diaryService,
        GlycemicDiaryPdfExportOptions options,
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

        var report = diaryResult.Value;
        var weeklyReviewResult = await CreateWeeklyReviewAsync(report, cancellationToken)
            .ConfigureAwait(false);

        QuestPDF.Settings.License = LicenseType.Community;

        var content = Document
            .Create(container => ComposeDocument(
                container,
                report,
                request.PreferredUnit,
                weeklyReviewResult.IsSuccess ? weeklyReviewResult.Value : null))
            .GeneratePdf();

        var file = new GlycemicDiaryExportFile(
            CreateFileName(request, report),
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
    /// <param name="preferredUnit">The preferred glucose display unit.</param>
    /// <param name="weeklyReview">The optional weekly review.</param>
    private void ComposeDocument(
        IDocumentContainer container,
        GlycemicDiaryReport report,
        GlucoseUnit preferredUnit,
        GlycemicDiaryWeeklyReview? weeklyReview)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(32);
            page.DefaultTextStyle(style => style.FontSize(9));

            page.Header().Element(header => ComposeHeader(header, report));
            page.Content().Element(content => ComposeContent(content, report, preferredUnit, weeklyReview));
            page.Footer().Element(ComposeFooter);
        });
    }

    /// <summary>
    /// Composes the PDF header.
    /// </summary>
    /// <param name="container">The container.</param>
    /// <param name="report">The glycemic diary report.</param>
    private static void ComposeHeader(
        IContainer container,
        GlycemicDiaryReport report)
    {
        container
            .Background(BrandBlueSoft)
            .Border(1)
            .BorderColor(BrandBorder)
            .CornerRadius(12)
            .Padding(16)
            .Row(row =>
            {
                row.ConstantItem(150)
                    .Height(42)
                    .Element(ComposeLogo);

                row.RelativeItem()
                    .PaddingLeft(16)
                    .Column(column =>
                    {
                        column.Spacing(3);

                        column.Item().Text("Glycemic diary")
                            .FontSize(22)
                            .SemiBold()
                            .FontColor(BrandBlueDark);

                        column.Item().Text($"{FormatDate(report.PeriodStartsAt)} - {FormatDate(report.PeriodEndsAt)}")
                            .FontSize(10)
                            .FontColor(TextSecondary);

                        column.Item().Text("Local-first glucose summary")
                            .FontSize(9)
                            .FontColor(TextMuted);
                    });
            });
    }

    /// <summary>
    /// Composes the GlucoDesk logo area.
    /// </summary>
    /// <param name="container">The target container.</param>
    private static void ComposeLogo(IContainer container)
    {
        if (BrandLogoBytes is null)
        {
            container.Text("GlucoDesk")
                .FontSize(20)
                .SemiBold()
                .FontColor(BrandBlueDark);

            return;
        }

        container.Image(BrandLogoBytes).FitArea();
    }

    /// <summary>
    /// Applies the standard PDF card style.
    /// </summary>
    /// <param name="container">The target container.</param>
    /// <returns>The styled container.</returns>
    private static IContainer Card(IContainer container)
    {
        return container
            .Background(White)
            .Border(1)
            .BorderColor(BrandBorder)
            .CornerRadius(12)
            .Padding(14);
    }

    /// <summary>
    /// Composes the PDF content.
    /// </summary>
    /// <param name="container">The container.</param>
    /// <param name="report">The glycemic diary report.</param>
    /// <param name="preferredUnit">The preferred glucose display unit.</param>
    /// <param name="weeklyReview">The optional weekly review.</param>
    private void ComposeContent(
        IContainer container,
        GlycemicDiaryReport report,
        GlucoseUnit preferredUnit,
        GlycemicDiaryWeeklyReview? weeklyReview)
    {
        container.Column(column =>
        {
            column.Spacing(14);

            column.Item()
                .PaddingTop(8)
                .Element(content => ComposeOverview(content, report, preferredUnit));

            column.Item()
                .Element(content => ComposeStory(content, report));

            column.Item()
                .Element(content => ComposeWeeklyReview(content, weeklyReview));

            column.Item()
                .Element(content => ComposeLocalPatterns(content, report));

            column.Item()
                .Element(content => ComposeDailyDiaryTable(content, report, preferredUnit));

            column.Item()
                .Element(content => ComposeDataCompleteness(content, report));

            column.Item()
                .Element(content => ComposeExportMetadata(content, report, preferredUnit));

            column.Item()
                .Element(ComposeSafetyNotice);
        });
    }

    /// <summary>
    /// Composes the export metadata section.
    /// </summary>
    /// <param name="container">The container.</param>
    /// <param name="report">The glycemic diary report.</param>
    /// <param name="preferredUnit">The preferred glucose display unit.</param>
    private static void ComposeExportMetadata(
        IContainer container,
        GlycemicDiaryReport report,
        GlucoseUnit preferredUnit)
    {
        container.Element(Card).Column(column =>
        {
            column.Spacing(6);

            column.Item().Text("Export metadata")
                .FontSize(12)
                .SemiBold()
                .FontColor(BrandBlueDark);

            column.Item().Text(BuildExportMetadataText(report, preferredUnit))
                .FontSize(8)
                .FontColor(TextSecondary);
        });
    }

    /// <summary>
    /// Builds the export metadata text.
    /// </summary>
    /// <param name="report">The glycemic diary report.</param>
    /// <param name="preferredUnit">The preferred glucose display unit.</param>
    /// <returns>The export metadata text.</returns>
    private static string BuildExportMetadataText(
        GlycemicDiaryReport report,
        GlucoseUnit preferredUnit)
    {
        return $"Generated at: {FormatGeneratedAt(DateTimeOffset.Now)} · Unit: {FormatUnit(preferredUnit)} · Target range: {FormatTargetRange(preferredUnit)} · Source: Local glucose history · Report type: Local-first glycemic diary export · Period: {FormatDateRange(report.PeriodStartsAt, report.PeriodEndsAt)}";
    }

    /// <summary>
    /// Formats the export generation timestamp.
    /// </summary>
    /// <param name="generatedAt">The generation timestamp.</param>
    /// <returns>The formatted timestamp.</returns>
    private static string FormatGeneratedAt(
        DateTimeOffset generatedAt)
    {
        return generatedAt.ToString("yyyy-MM-dd HH:mm zzz", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Formats the glucose unit for export display.
    /// </summary>
    /// <param name="preferredUnit">The preferred glucose display unit.</param>
    /// <returns>The formatted unit.</returns>
    private static string FormatUnit(
        GlucoseUnit preferredUnit)
    {
        return preferredUnit.ToString().Contains("Mmol", StringComparison.OrdinalIgnoreCase)
            ? "mmol/L"
            : "mg/dL";
    }

    /// <summary>
    /// Formats the target range for export display.
    /// </summary>
    /// <param name="preferredUnit">The preferred glucose display unit.</param>
    /// <returns>The formatted target range.</returns>
    private static string FormatTargetRange(
        GlucoseUnit preferredUnit)
    {
        return preferredUnit.ToString().Contains("Mmol", StringComparison.OrdinalIgnoreCase)
            ? "3.9-10.0 mmol/L"
            : "70-180 mg/dL";
    }

    /// <summary>
    /// Composes the overview section.
    /// </summary>
    /// <param name="container">The container.</param>
    /// <param name="report">The glycemic diary report.</param>
    /// <param name="preferredUnit">The preferred glucose display unit.</param>
    private void ComposeOverview(
        IContainer container,
        GlycemicDiaryReport report,
        GlucoseUnit preferredUnit)
    {
        var completenessScore = _completenessScoringService.Calculate(report.OverallContinuity);

        container.Element(Card).Column(column =>
        {
            column.Spacing(10);

            column.Item().Row(row =>
            {
                row.RelativeItem().Column(title =>
                {
                    title.Item().Text("Overview")
                        .FontSize(15)
                        .SemiBold()
                        .FontColor(BrandBlueDark);

                    title.Item().Text($"Summary of the selected glucose history period. Values shown in {FormatGlucoseUnitLabel(preferredUnit)}.")
                        .FontSize(8)
                        .FontColor(TextMuted);
                });

                row.ConstantItem(150)
                    .AlignRight()
                    .AlignMiddle()
                    .Text($"{completenessScore.StatusText} · {completenessScore.CoverageText}")
                    .FontSize(9)
                    .SemiBold()
                    .FontColor(BrandBlueDark);
            });

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                WriteMetric(table, "Average", FormatGlucoseValue(report.AverageMgDl, preferredUnit));
                WriteMetric(table, "Range", FormatGlucoseRange(report.MinimumMgDl, report.MaximumMgDl, preferredUnit));
                WriteMetric(table, "Time in range", FormatPercentage(report.TimeInRangePercentage));
                WriteMetric(table, "History reliability", completenessScore.StatusText);
                WriteMetric(table, "Data coverage", completenessScore.CoverageText);
                WriteMetric(table, "Readings", report.ReadingsCount.ToString(CultureInfo.InvariantCulture));
                WriteMetric(table, "Detected gaps", completenessScore.DetectedGapCount.ToString(CultureInfo.InvariantCulture));
                WriteMetric(table, "Incomplete days", report.IncompleteDaysCount.ToString(CultureInfo.InvariantCulture));
                WriteMetric(table, "Empty days", report.EmptyDaysCount.ToString(CultureInfo.InvariantCulture));
            });
        });
    }

    /// <summary>
    /// Composes the glucose story section.
    /// </summary>
    /// <param name="container">The container.</param>
    /// <param name="report">The glycemic diary report.</param>
    private void ComposeStory(
        IContainer container,
        GlycemicDiaryReport report)
    {
        var story = _storyService.CreateStory(report);

        container.Element(Card).Column(column =>
        {
            column.Spacing(8);

            column.Item().Row(row =>
            {
                row.RelativeItem().Column(title =>
                {
                    title.Item().Text("Glucose story")
                        .FontSize(15)
                        .SemiBold()
                        .FontColor(BrandBlueDark);

                    title.Item().Text("Human-readable interpretation of the selected diary period.")
                        .FontSize(8)
                        .FontColor(TextMuted);
                });

                row.ConstantItem(110)
                    .AlignRight()
                    .AlignMiddle()
                    .Text(story.Level.ToString())
                    .FontSize(9)
                    .SemiBold()
                    .FontColor(BrandBlueDark);
            });

            column.Item().Text(story.Headline)
                .FontSize(12)
                .SemiBold()
                .FontColor(BrandBlueDark);

            column.Item().Text(story.SummaryText)
                .FontSize(9)
                .FontColor(TextSecondary);

            column.Item().Text(story.HistoryReliabilityText)
                .FontSize(8)
                .FontColor(TextMuted);
        });
    }

    /// <summary>
    /// Composes the weekly review section.
    /// </summary>
    /// <param name="container">The container.</param>
    /// <param name="weeklyReview">The optional weekly review.</param>
    private static void ComposeWeeklyReview(
        IContainer container,
        GlycemicDiaryWeeklyReview? weeklyReview)
    {
        container.Element(Card).Column(column =>
        {
            column.Spacing(8);

            column.Item().Row(row =>
            {
                row.RelativeItem().Column(title =>
                {
                    title.Item().Text("Weekly review")
                        .FontSize(15)
                        .SemiBold()
                        .FontColor(BrandBlueDark);

                    title.Item().Text("Comparison with the previous equivalent period.")
                        .FontSize(8)
                        .FontColor(TextMuted);
                });

                row.ConstantItem(90)
                    .AlignRight()
                    .AlignMiddle()
                    .Text(weeklyReview?.RequiresCaution == true ? "Caution" : "Info")
                    .FontSize(9)
                    .SemiBold()
                    .FontColor(weeklyReview?.RequiresCaution == true ? WarningText : SuccessText);
            });

            if (weeklyReview is null)
            {
                column.Item().Text("Weekly review unavailable")
                    .FontSize(11)
                    .SemiBold()
                    .FontColor(BrandBlueDark);

                column.Item().Text("The weekly comparison could not be generated for this export. The diary data below is still available.")
                    .FontSize(9)
                    .FontColor(TextSecondary);

                return;
            }

            column.Item().Text(weeklyReview.Headline)
                .FontSize(11)
                .SemiBold()
                .FontColor(BrandBlueDark);

            column.Item().Text(weeklyReview.SummaryText)
                .FontSize(9)
                .FontColor(TextSecondary);

            column.Item().Text(weeklyReview.CurrentHistoryReliabilityText)
                .FontSize(8)
                .FontColor(TextMuted);

            column.Item().Text(BuildComparisonPeriodText(weeklyReview))
                .FontSize(8)
                .FontColor(TextMuted);

            foreach (var change in weeklyReview.Changes.Take(4))
            {
                column.Item()
                    .PaddingTop(4)
                    .BorderTop(1)
                    .BorderColor(BrandBorder)
                    .PaddingTop(6)
                    .Row(row =>
                    {
                        row.RelativeItem(2).Text(change.DisplayName)
                            .FontSize(8)
                            .SemiBold()
                            .FontColor(BrandBlueDark);

                        row.RelativeItem().AlignRight().Text(change.PreviousValueText)
                            .FontSize(8)
                            .FontColor(TextSecondary);

                        row.RelativeItem().AlignRight().Text(change.CurrentValueText)
                            .FontSize(8)
                            .FontColor(TextSecondary);

                        row.RelativeItem().AlignRight().Text(change.DeltaText)
                            .FontSize(8)
                            .SemiBold()
                            .FontColor(GetWeeklyReviewSeverityColor(change.Severity));
                    });
            }
        });
    }

    /// <summary>
    /// Builds the comparison period text for a weekly review.
    /// </summary>
    /// <param name="weeklyReview">The weekly review.</param>
    /// <returns>The comparison period text.</returns>
    private static string BuildComparisonPeriodText(
        GlycemicDiaryWeeklyReview weeklyReview)
    {
        var comparisonPeriodText = $"Current period: {FormatDateRange(weeklyReview.CurrentPeriodStartsAt, weeklyReview.CurrentPeriodEndsAt)} · Previous period: {FormatDateRange(weeklyReview.PreviousPeriodStartsAt, weeklyReview.PreviousPeriodEndsAt)}";

        if (HasSharedBoundaryDate(weeklyReview))
        {
            comparisonPeriodText += " (previous period ends before the current period starts)";
        }

        return comparisonPeriodText;
    }

    /// <summary>
    /// Determines whether the previous period end and current period start share the same displayed date.
    /// </summary>
    /// <param name="weeklyReview">The weekly review.</param>
    /// <returns><c>true</c> when the displayed boundary date is shared; otherwise, <c>false</c>.</returns>
    private static bool HasSharedBoundaryDate(
        GlycemicDiaryWeeklyReview weeklyReview)
    {
        return weeklyReview.PreviousPeriodEndsAt.Date == weeklyReview.CurrentPeriodStartsAt.Date
            && weeklyReview.PreviousPeriodEndsAt <= weeklyReview.CurrentPeriodStartsAt;
    }

    /// <summary>
    /// Formats a date range for PDF display.
    /// </summary>
    /// <param name="startsAt">The period start.</param>
    /// <param name="endsAt">The period end.</param>
    /// <returns>The formatted date range.</returns>
    private static string FormatDateRange(
        DateTimeOffset startsAt,
        DateTimeOffset endsAt)
    {
        return $"{startsAt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)} - {endsAt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}";
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
    /// Gets the display color for a weekly review severity.
    /// </summary>
    /// <param name="severity">The signal severity.</param>
    /// <returns>The display color.</returns>
    private static string GetWeeklyReviewSeverityColor(
        GlycemicDiaryReviewSignalSeverity severity)
    {
        return severity switch
        {
            GlycemicDiaryReviewSignalSeverity.Important => WarningText,
            GlycemicDiaryReviewSignalSeverity.Caution => WarningText,
            GlycemicDiaryReviewSignalSeverity.Info => SuccessText,
            _ => TextSecondary
        };
    }

    /// <summary>
    /// Composes the local patterns section.
    /// </summary>
    /// <param name="container">The container.</param>
    /// <param name="report">The glycemic diary report.</param>
    private void ComposeLocalPatterns(
        IContainer container,
        GlycemicDiaryReport report)
    {
        var analysis = _patternAnalysisService.Analyze(report);
        var patterns = analysis.Patterns
            .OrderByDescending(GetPatternSeverityRank)
            .ThenBy(pattern => pattern.Kind)
            .Take(5)
            .ToArray();

        var patternSummaryText = analysis.Patterns.Count > patterns.Length
            ? $"Top {patterns.Length} of {analysis.Patterns.Count}"
            : analysis.HasPatterns
                ? $"{analysis.Patterns.Count} detected"
                : "None detected";

        container.Element(Card).Column(column =>
        {
            column.Spacing(8);

            column.Item().Row(row =>
            {
                row.RelativeItem().Column(title =>
                {
                    title.Item().Text("Local patterns")
                        .FontSize(15)
                        .SemiBold()
                        .FontColor(BrandBlueDark);

                    title.Item().Text("Recurring local glucose tendencies detected from diary time blocks.")
                        .FontSize(8)
                        .FontColor(TextMuted);
                });

                row.ConstantItem(120)
                    .AlignRight()
                    .AlignMiddle()
                    .Text(patternSummaryText)
                    .FontSize(9)
                    .SemiBold()
                    .FontColor(BrandBlueDark);
            });

            if (patterns.Length == 0)
            {
                column.Item().Text("No recurring local patterns were detected for the selected period.")
                    .FontSize(9)
                    .FontColor(TextSecondary);

                return;
            }

            foreach (var pattern in patterns)
            {
                column.Item()
                    .PaddingTop(4)
                    .BorderTop(1)
                    .BorderColor(BrandBorder)
                    .PaddingTop(6)
                    .Column(patternColumn =>
                    {
                        patternColumn.Spacing(2);

                        patternColumn.Item().Row(row =>
                        {
                            row.RelativeItem().Text(pattern.Title)
                                .FontSize(10)
                                .SemiBold()
                                .FontColor(BrandBlueDark);

                            row.ConstantItem(80)
                                .AlignRight()
                                .Text(pattern.Severity.ToString())
                                .FontSize(8)
                                .SemiBold()
                                .FontColor(GetPatternSeverityColor(pattern.Severity));
                        });

                        patternColumn.Item().Text(pattern.Description)
                            .FontSize(8)
                            .FontColor(TextSecondary);

                        patternColumn.Item().Text(BuildPatternSupportText(pattern))
                            .FontSize(7)
                            .FontColor(TextMuted);
                    });
            }
        });
    }

    /// <summary>
    /// Builds the support text for a detected pattern.
    /// </summary>
    /// <param name="pattern">The detected pattern.</param>
    /// <returns>The support text.</returns>
    private static string BuildPatternSupportText(GlycemicDiaryPattern pattern)
    {
        var scope = pattern.TimeBlockLabel is null
            ? "overall period"
            : pattern.TimeBlockLabel;

        return $"{pattern.SupportingDaysCount} supporting {Pluralize("day", pattern.SupportingDaysCount)} · {scope}";
    }

    /// <summary>
    /// Pluralizes a singular noun.
    /// </summary>
    /// <param name="singular">The singular noun.</param>
    /// <param name="count">The item count.</param>
    /// <returns>The pluralized noun.</returns>
    private static string Pluralize(string singular, int count)
    {
        return count == 1
            ? singular
            : $"{singular}s";
    }

    /// <summary>
    /// Gets a deterministic severity rank for pattern ordering.
    /// </summary>
    /// <param name="pattern">The detected pattern.</param>
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
    /// Gets the display color for a pattern severity.
    /// </summary>
    /// <param name="severity">The pattern severity.</param>
    /// <returns>The display color.</returns>
    private static string GetPatternSeverityColor(GlycemicDiaryPatternSeverity severity)
    {
        return severity switch
        {
            GlycemicDiaryPatternSeverity.Important => WarningText,
            GlycemicDiaryPatternSeverity.Caution => WarningText,
            GlycemicDiaryPatternSeverity.Info => SuccessText,
            _ => TextSecondary
        };
    }

    /// <summary>
    /// Composes the daily diary table.
    /// </summary>
    /// <param name="container">The container.</param>
    /// <param name="report">The glycemic diary report.</param>
    /// <param name="preferredUnit">The preferred glucose display unit.</param>
    private static void ComposeDailyDiaryTable(
        IContainer container,
        GlycemicDiaryReport report,
        GlucoseUnit preferredUnit)
    {
        container.Element(Card).Column(column =>
        {
            column.Spacing(10);

            column.Item().Column(title =>
            {
                title.Spacing(2);

                title.Item().Text("Daily diary")
                    .FontSize(15)
                    .SemiBold()
                    .FontColor(BrandBlueDark);

                title.Item().Text($"Daily glucose summaries and key time-block values shown in {FormatGlucoseUnitLabel(preferredUnit)}.")
                    .FontSize(8)
                    .FontColor(TextMuted);
            });

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(1.15f);
                    columns.RelativeColumn(0.75f);
                    columns.RelativeColumn(0.75f);
                    columns.RelativeColumn(0.75f);
                    columns.RelativeColumn(0.9f);
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    DailyHeaderCell(header.Cell()).Text("Date")
                        .FontColor(BrandBlueDark)
                        .SemiBold();

                    DailyHeaderCell(header.Cell()).Text("Avg")
                        .FontColor(BrandBlueDark)
                        .SemiBold();

                    DailyHeaderCell(header.Cell()).Text("Min")
                        .FontColor(BrandBlueDark)
                        .SemiBold();

                    DailyHeaderCell(header.Cell()).Text("Max")
                        .FontColor(BrandBlueDark)
                        .SemiBold();

                    DailyHeaderCell(header.Cell()).Text("TIR")
                        .FontColor(BrandBlueDark)
                        .SemiBold();

                    DailyHeaderCell(header.Cell()).Text("Breakfast")
                        .FontColor(BrandBlueDark)
                        .SemiBold();

                    DailyHeaderCell(header.Cell()).Text("Lunch")
                        .FontColor(BrandBlueDark)
                        .SemiBold();

                    DailyHeaderCell(header.Cell()).Text("Dinner")
                        .FontColor(BrandBlueDark)
                        .SemiBold();

                    DailyHeaderCell(header.Cell()).Text("Pre-night")
                        .FontColor(BrandBlueDark)
                        .SemiBold();
                });

                var rowIndex = 0;

                foreach (var day in report.DailyEntries.OrderBy(day => day.Date))
                {
                    var isAlternate = rowIndex % 2 != 0;

                    DailyBodyCell(table.Cell(), isAlternate).Text(day.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                    DailyBodyCell(table.Cell(), isAlternate).Text(FormatGlucoseAmount(day.AverageMgDl, preferredUnit));
                    DailyBodyCell(table.Cell(), isAlternate).Text(FormatGlucoseAmount(day.MinimumMgDl, preferredUnit));
                    DailyBodyCell(table.Cell(), isAlternate).Text(FormatGlucoseAmount(day.MaximumMgDl, preferredUnit));
                    DailyBodyCell(table.Cell(), isAlternate).Text(FormatPercentage(day.TimeInRangePercentage));
                    DailyBodyCell(table.Cell(), isAlternate).Text(FormatGlucoseAmount(GetBlockValue(day, "Breakfast"), preferredUnit));
                    DailyBodyCell(table.Cell(), isAlternate).Text(FormatGlucoseAmount(GetBlockValue(day, "Lunch"), preferredUnit));
                    DailyBodyCell(table.Cell(), isAlternate).Text(FormatGlucoseAmount(GetBlockValue(day, "Dinner"), preferredUnit));
                    DailyBodyCell(table.Cell(), isAlternate).Text(FormatGlucoseAmount(GetBlockValue(day, "Pre-night"), preferredUnit));

                    rowIndex++;
                }
            });
        });
    }

    /// <summary>
    /// Composes the data completeness section.
    /// </summary>
    /// <param name="container">The container.</param>
    /// <param name="report">The glycemic diary report.</param>
    private void ComposeDataCompleteness(
        IContainer container,
        GlycemicDiaryReport report)
    {
        var completenessScore = _completenessScoringService.Calculate(report.OverallContinuity);
        var hasGoodCoverage = !completenessScore.RequiresCaution;
        var backgroundColor = hasGoodCoverage ? SuccessSoft : WarningSoft;
        var borderColor = hasGoodCoverage ? SuccessBorder : WarningBorder;
        var statusColor = hasGoodCoverage ? SuccessText : WarningText;
        var statusText = $"{completenessScore.StatusText} · {completenessScore.CoverageText}";

        container
            .Background(backgroundColor)
            .Border(1)
            .BorderColor(borderColor)
            .CornerRadius(12)
            .Padding(14)
            .Column(column =>
            {
                column.Spacing(10);

                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(title =>
                    {
                        title.Spacing(2);

                        title.Item().Text("Data completeness")
                            .FontSize(15)
                            .SemiBold()
                            .FontColor(BrandBlueDark);

                        title.Item().Text($"{completenessScore.DetailText} Days marked as partial may contain missing CGM history and should be interpreted carefully.")
                            .FontSize(8)
                            .FontColor(TextSecondary);
                    });

                    row.ConstantItem(150)
                        .AlignRight()
                        .AlignMiddle()
                        .Text(statusText)
                        .FontSize(9)
                        .SemiBold()
                        .FontColor(statusColor);
                });

                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(1.4f);
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn(0.7f);
                    });

                    table.Header(header =>
                    {
                        CompletenessHeaderCell(header.Cell()).Text("Date")
                            .FontColor(BrandBlueDark)
                            .SemiBold();

                        CompletenessHeaderCell(header.Cell()).Text("Coverage")
                            .FontColor(BrandBlueDark)
                            .SemiBold();

                        CompletenessHeaderCell(header.Cell()).Text("Status")
                            .FontColor(BrandBlueDark)
                            .SemiBold();

                        CompletenessHeaderCell(header.Cell()).Text("Gaps")
                            .FontColor(BrandBlueDark)
                            .SemiBold();
                    });

                    var rowIndex = 0;

                    foreach (var day in report.DailyEntries.OrderBy(day => day.Date))
                    {
                        var isAlternate = rowIndex % 2 != 0;
                        var dayStatusColor = day.IsDataComplete ? SuccessText : WarningText;
                        var dayStatusText = day.IsDataComplete ? "Complete" : "Partial";

                        CompletenessBodyCell(table.Cell(), isAlternate).Text(day.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                        CompletenessBodyCell(table.Cell(), isAlternate).Text(FormatPercentage(day.DataCoveragePercentage));

                        CompletenessBodyCell(table.Cell(), isAlternate).Text(dayStatusText)
                            .SemiBold()
                            .FontColor(dayStatusColor);

                        CompletenessBodyCell(table.Cell(), isAlternate).Text(day.GapCount.ToString(CultureInfo.InvariantCulture));

                        rowIndex++;
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
            .Background(NeutralSoft)
            .Border(1)
            .BorderColor("#DDE6F2")
            .CornerRadius(10)
            .Padding(12)
            .Column(column =>
            {
                column.Spacing(5);

                column.Item().Row(row =>
                {
                    row.ConstantItem(4)
                        .Height(16)
                        .Background(BrandBlueDark);

                    row.RelativeItem()
                        .PaddingLeft(8)
                        .Text("Safety notice")
                        .FontSize(11)
                        .SemiBold()
                        .FontColor(BrandBlueDark);
                });

                column.Item().Text(_options.SafetyDisclaimer)
                    .FontSize(8)
                    .FontColor(TextSecondary);
            });
    }

    /// <summary>
    /// Composes the PDF footer.
    /// </summary>
    /// <param name="container">The container.</param>
    private static void ComposeFooter(IContainer container)
    {
        container
            .BorderTop(1)
            .BorderColor("#DDE6F2")
            .PaddingTop(8)
            .Row(row =>
            {
                row.RelativeItem()
                    .Text("Generated by GlucoDesk")
                    .FontSize(8)
                    .FontColor(TextMuted);

                row.ConstantItem(90)
                    .AlignRight()
                    .Text(text =>
                    {
                        text.Span("Page ")
                            .FontSize(8)
                            .FontColor(TextMuted);

                        text.CurrentPageNumber()
                            .FontSize(8)
                            .FontColor(TextMuted);

                        text.Span(" of ")
                            .FontSize(8)
                            .FontColor(TextMuted);

                        text.TotalPages()
                            .FontSize(8)
                            .FontColor(TextMuted);
                    });
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
        return timestamp.ToLocalTime().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Formats a nullable glucose value with unit.
    /// </summary>
    /// <param name="valueMgDl">The nullable glucose value expressed in mg/dL.</param>
    /// <param name="preferredUnit">The preferred glucose display unit.</param>
    /// <returns>The formatted glucose value.</returns>
    private static string FormatGlucoseValue(
        decimal? valueMgDl,
        GlucoseUnit preferredUnit)
    {
        return valueMgDl is null
            ? "—"
            : $"{FormatGlucoseAmount(valueMgDl, preferredUnit)} {FormatGlucoseUnitLabel(preferredUnit)}";
    }

    /// <summary>
    /// Formats a nullable glucose range with unit.
    /// </summary>
    /// <param name="minimumMgDl">The nullable minimum glucose value expressed in mg/dL.</param>
    /// <param name="maximumMgDl">The nullable maximum glucose value expressed in mg/dL.</param>
    /// <param name="preferredUnit">The preferred glucose display unit.</param>
    /// <returns>The formatted glucose range.</returns>
    private static string FormatGlucoseRange(
        decimal? minimumMgDl,
        decimal? maximumMgDl,
        GlucoseUnit preferredUnit)
    {
        if (minimumMgDl is null || maximumMgDl is null)
        {
            return "—";
        }

        return $"{FormatGlucoseAmount(minimumMgDl, preferredUnit)} - {FormatGlucoseAmount(maximumMgDl, preferredUnit)} {FormatGlucoseUnitLabel(preferredUnit)}";
    }

    /// <summary>
    /// Formats a nullable glucose value without unit.
    /// </summary>
    /// <param name="valueMgDl">The nullable glucose value expressed in mg/dL.</param>
    /// <param name="preferredUnit">The preferred glucose display unit.</param>
    /// <returns>The formatted glucose amount.</returns>
    private static string FormatGlucoseAmount(
        decimal? valueMgDl,
        GlucoseUnit preferredUnit)
    {
        var convertedValue = ConvertGlucoseAmount(valueMgDl, preferredUnit);

        if (convertedValue is null)
        {
            return "—";
        }

        return preferredUnit switch
        {
            GlucoseUnit.MgDl => convertedValue.Value.ToString("0", CultureInfo.InvariantCulture),
            GlucoseUnit.MmolL => convertedValue.Value.ToString("0.0", CultureInfo.InvariantCulture),
            _ => throw new ArgumentOutOfRangeException(nameof(preferredUnit), preferredUnit, "Unsupported glucose unit.")
        };
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
    /// Formats a nullable percentage value.
    /// </summary>
    /// <param name="value">The percentage value.</param>
    /// <returns>The formatted percentage.</returns>
    private static string FormatPercentage(decimal? value)
    {
        return value is null
            ? "—"
            : $"{value.Value.ToString("0.##", CultureInfo.InvariantCulture)}%";
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

    /// <summary>
    /// Loads the embedded GlucoDesk brand logo.
    /// </summary>
    /// <returns>The logo bytes, if available.</returns>
    private static byte[]? LoadBrandLogoBytes()
    {
        var assembly = Assembly.GetExecutingAssembly();

        const string resourceName = "GlucoDesk.Infrastructure.Assets.Brand.glucodesk-wordmark-removebg-preview.png";

        using var stream = assembly.GetManifestResourceStream(resourceName);

        if (stream is null)
        {
            return null;
        }

        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);

        return memoryStream.ToArray();
    }

    /// <summary>
    /// Applies the daily diary table header cell style.
    /// </summary>
    /// <param name="container">The target container.</param>
    /// <returns>The styled container.</returns>
    private static IContainer DailyHeaderCell(IContainer container)
    {
        return container
            .Background(BrandBlueSoft)
            .Border(0.5f)
            .BorderColor(BrandBorder)
            .PaddingVertical(5)
            .PaddingHorizontal(4);
    }

    /// <summary>
    /// Applies the daily diary table body cell style.
    /// </summary>
    /// <param name="container">The target container.</param>
    /// <param name="isAlternate">A value indicating whether the row is alternate.</param>
    /// <returns>The styled container.</returns>
    private static IContainer DailyBodyCell(
        IContainer container,
        bool isAlternate)
    {
        return container
            .Background(isAlternate ? NeutralSoft : White)
            .Border(0.5f)
            .BorderColor("#E3EAF3")
            .PaddingVertical(4)
            .PaddingHorizontal(4);
    }

    /// <summary>
    /// Applies the data completeness table header cell style.
    /// </summary>
    /// <param name="container">The target container.</param>
    /// <returns>The styled container.</returns>
    private static IContainer CompletenessHeaderCell(IContainer container)
    {
        return container
            .Background(White)
            .Border(0.5f)
            .BorderColor(BrandBorder)
            .PaddingVertical(5)
            .PaddingHorizontal(4);
    }

    /// <summary>
    /// Applies the data completeness table body cell style.
    /// </summary>
    /// <param name="container">The target container.</param>
    /// <param name="isAlternate">A value indicating whether the row is alternate.</param>
    /// <returns>The styled container.</returns>
    private static IContainer CompletenessBodyCell(
        IContainer container,
        bool isAlternate)
    {
        return container
            .Background(isAlternate ? "#FFFDF8" : White)
            .Border(0.5f)
            .BorderColor("#E8DDBF")
            .PaddingVertical(4)
            .PaddingHorizontal(4);
    }

    #endregion
}