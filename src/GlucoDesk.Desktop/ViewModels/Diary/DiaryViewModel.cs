using System.Globalization;
using CommunityToolkit.Mvvm.Input;
using GlucoDesk.Application.Cgm.Diary.Exports.Requests;
using GlucoDesk.Application.Cgm.Diary.Exports.Results;
using GlucoDesk.Application.Cgm.Diary.Exports.Services.Abstractions;
using GlucoDesk.Application.Cgm.Diary.Requests;
using GlucoDesk.Application.Cgm.Diary.Results;
using GlucoDesk.Application.Cgm.Diary.Services.Abstractions;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Desktop.Diary.Services.Abstractions;
using GlucoDesk.Desktop.ViewModels.Common;
using GlucoDesk.Desktop.ViewModels.Diary.Enums;
using GlucoDesk.Desktop.ViewModels.Diary.Options;

namespace GlucoDesk.Desktop.ViewModels.Diary;

/// <summary>
/// View model for glycemic diary export.
/// </summary>
public sealed class DiaryViewModel : ViewModelBase
{
    private readonly IGlycemicDiaryService _diaryService;
    private readonly IGlycemicDiaryExcelExportService _excelExportService;
    private readonly IGlycemicDiaryPdfExportService _pdfExportService;
    private readonly IDiaryExportFileSaveService _fileSaveService;
    private readonly TimeProvider _timeProvider;

    private DiaryExportPeriodPresetOption _selectedPeriodPreset;
    private DiaryExportFormatOption _selectedFormat;
    private bool _isExporting;
    private bool _isPreviewLoading;
    private bool _hasError;
    private bool _hasSuccess;
    private bool _hasWarning;
    private bool _hasPreview;
    private bool _hasPreviewWarning;
    private bool _hasPreviewError;
    private string _statusTitle;
    private string _statusText;
    private string _previewStatusTitle;
    private string _previewStatusText;
    private string _previewCoverageText;
    private string _previewDetailsText;

    /// <summary>
    /// Initializes a new instance of the <see cref="DiaryViewModel"/> class.
    /// </summary>
    /// <param name="diaryService">The glycemic diary service.</param>
    /// <param name="excelExportService">The Excel export service.</param>
    /// <param name="pdfExportService">The PDF export service.</param>
    /// <param name="fileSaveService">The file save service.</param>
    /// <param name="timeProvider">The time provider.</param>
    public DiaryViewModel(
        IGlycemicDiaryService diaryService,
        IGlycemicDiaryExcelExportService excelExportService,
        IGlycemicDiaryPdfExportService pdfExportService,
        IDiaryExportFileSaveService fileSaveService,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(diaryService);
        ArgumentNullException.ThrowIfNull(excelExportService);
        ArgumentNullException.ThrowIfNull(pdfExportService);
        ArgumentNullException.ThrowIfNull(fileSaveService);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _diaryService = diaryService;
        _excelExportService = excelExportService;
        _pdfExportService = pdfExportService;
        _fileSaveService = fileSaveService;
        _timeProvider = timeProvider;

        PeriodPresets =
        [
            new DiaryExportPeriodPresetOption(
                DiaryExportPeriodPresetKind.LastFourteenDays,
                "Last 14 days",
                "A compact recent diary for short-term review."),
            new DiaryExportPeriodPresetOption(
                DiaryExportPeriodPresetKind.LastThirtyDays,
                "Last 30 days",
                "A complete recent diary for monthly review."),
            new DiaryExportPeriodPresetOption(
                DiaryExportPeriodPresetKind.CurrentMonth,
                "Current month",
                "From the first day of this month to now."),
            new DiaryExportPeriodPresetOption(
                DiaryExportPeriodPresetKind.PreviousMonth,
                "Previous month",
                "The full previous calendar month.")
        ];

        Formats =
        [
            new DiaryExportFormatOption(
                DiaryExportFormatKind.Excel,
                "Excel workbook",
                "Best for analysis, filtering and sharing structured data."),
            new DiaryExportFormatOption(
                DiaryExportFormatKind.Pdf,
                "PDF document",
                "Best for a clean printable diary summary.")
        ];

        _selectedPeriodPreset = PeriodPresets[1];
        _selectedFormat = Formats[0];

        _statusTitle = "Ready to export";
        _statusText = "Choose a period and format, then export your diary.";

        _previewStatusTitle = "Data preview";
        _previewStatusText = "Refresh the preview to check local data completeness before exporting.";
        _previewCoverageText = "Coverage not checked yet";
        _previewDetailsText = "The exported diary will always include data-completeness indicators.";

        ExportCommand = new AsyncRelayCommand(
            ExportAsync,
            CanExport);

        RefreshPreviewCommand = new AsyncRelayCommand(
            RefreshPreviewAsync,
            CanRefreshPreview);
    }

    /// <summary>
    /// Gets the available period presets.
    /// </summary>
    public IReadOnlyList<DiaryExportPeriodPresetOption> PeriodPresets { get; }

    /// <summary>
    /// Gets the available export formats.
    /// </summary>
    public IReadOnlyList<DiaryExportFormatOption> Formats { get; }

    /// <summary>
    /// Gets the export command.
    /// </summary>
    public IAsyncRelayCommand ExportCommand { get; }

    /// <summary>
    /// Gets the refresh preview command.
    /// </summary>
    public IAsyncRelayCommand RefreshPreviewCommand { get; }

    /// <summary>
    /// Gets or sets the selected period preset.
    /// </summary>
    public DiaryExportPeriodPresetOption SelectedPeriodPreset
    {
        get => _selectedPeriodPreset;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            if (SetProperty(ref _selectedPeriodPreset, value))
            {
                ResetReadyStatus();
                ResetPreviewStatus();
                ExportCommand.NotifyCanExecuteChanged();
                RefreshPreviewCommand.NotifyCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the selected export format.
    /// </summary>
    public DiaryExportFormatOption SelectedFormat
    {
        get => _selectedFormat;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            if (SetProperty(ref _selectedFormat, value))
            {
                ResetReadyStatus();
                ExportCommand.NotifyCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether an export is currently running.
    /// </summary>
    public bool IsExporting
    {
        get => _isExporting;
        private set
        {
            if (SetProperty(ref _isExporting, value))
            {
                OnPropertyChanged(nameof(CanEditSelection));
                OnPropertyChanged(nameof(ExportButtonText));
                ExportCommand.NotifyCanExecuteChanged();
                RefreshPreviewCommand.NotifyCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether the data preview is currently loading.
    /// </summary>
    public bool IsPreviewLoading
    {
        get => _isPreviewLoading;
        private set
        {
            if (SetProperty(ref _isPreviewLoading, value))
            {
                OnPropertyChanged(nameof(CanEditSelection));
                OnPropertyChanged(nameof(RefreshPreviewButtonText));
                ExportCommand.NotifyCanExecuteChanged();
                RefreshPreviewCommand.NotifyCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether the export controls can be edited.
    /// </summary>
    public bool CanEditSelection => !IsExporting && !IsPreviewLoading;

    /// <summary>
    /// Gets the export button text.
    /// </summary>
    public string ExportButtonText => IsExporting ? "Exporting..." : "Export diary";

    /// <summary>
    /// Gets the refresh preview button text.
    /// </summary>
    public string RefreshPreviewButtonText => IsPreviewLoading ? "Checking..." : "Refresh preview";

    /// <summary>
    /// Gets a value indicating whether the last export failed.
    /// </summary>
    public bool HasError
    {
        get => _hasError;
        private set => SetProperty(ref _hasError, value);
    }

    /// <summary>
    /// Gets a value indicating whether the last export completed successfully.
    /// </summary>
    public bool HasSuccess
    {
        get => _hasSuccess;
        private set => SetProperty(ref _hasSuccess, value);
    }

    /// <summary>
    /// Gets a value indicating whether the last export completed with a user-facing warning.
    /// </summary>
    public bool HasWarning
    {
        get => _hasWarning;
        private set => SetProperty(ref _hasWarning, value);
    }

    /// <summary>
    /// Gets a value indicating whether a preview is available.
    /// </summary>
    public bool HasPreview
    {
        get => _hasPreview;
        private set => SetProperty(ref _hasPreview, value);
    }

    /// <summary>
    /// Gets a value indicating whether the preview contains a warning.
    /// </summary>
    public bool HasPreviewWarning
    {
        get => _hasPreviewWarning;
        private set => SetProperty(ref _hasPreviewWarning, value);
    }

    /// <summary>
    /// Gets a value indicating whether the preview failed.
    /// </summary>
    public bool HasPreviewError
    {
        get => _hasPreviewError;
        private set => SetProperty(ref _hasPreviewError, value);
    }

    /// <summary>
    /// Gets the export status title.
    /// </summary>
    public string StatusTitle
    {
        get => _statusTitle;
        private set => SetProperty(ref _statusTitle, value);
    }

    /// <summary>
    /// Gets the export status text.
    /// </summary>
    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    /// <summary>
    /// Gets the data preview status title.
    /// </summary>
    public string PreviewStatusTitle
    {
        get => _previewStatusTitle;
        private set => SetProperty(ref _previewStatusTitle, value);
    }

    /// <summary>
    /// Gets the data preview status text.
    /// </summary>
    public string PreviewStatusText
    {
        get => _previewStatusText;
        private set => SetProperty(ref _previewStatusText, value);
    }

    /// <summary>
    /// Gets the data preview coverage text.
    /// </summary>
    public string PreviewCoverageText
    {
        get => _previewCoverageText;
        private set => SetProperty(ref _previewCoverageText, value);
    }

    /// <summary>
    /// Gets the data preview details text.
    /// </summary>
    public string PreviewDetailsText
    {
        get => _previewDetailsText;
        private set => SetProperty(ref _previewDetailsText, value);
    }

    #region Helpers

    /// <summary>
    /// Determines whether the diary can be exported.
    /// </summary>
    /// <returns>A value indicating whether export is available.</returns>
    private bool CanExport()
    {
        return !IsExporting &&
               !IsPreviewLoading &&
               SelectedPeriodPreset is not null &&
               SelectedFormat is not null;
    }

    /// <summary>
    /// Determines whether the diary preview can be refreshed.
    /// </summary>
    /// <returns>A value indicating whether preview refresh is available.</returns>
    private bool CanRefreshPreview()
    {
        return !IsExporting &&
               !IsPreviewLoading &&
               SelectedPeriodPreset is not null;
    }

    /// <summary>
    /// Refreshes the local data completeness preview for the selected period.
    /// </summary>
    private async Task RefreshPreviewAsync()
    {
        if (IsPreviewLoading || IsExporting)
        {
            return;
        }

        IsPreviewLoading = true;
        HasPreview = false;
        HasPreviewWarning = false;
        HasPreviewError = false;
        PreviewStatusTitle = "Checking local history";
        PreviewStatusText = "GlucoDesk is analyzing the selected period.";
        PreviewCoverageText = "Checking coverage...";
        PreviewDetailsText = "Please wait while the local history is scanned.";

        try
        {
            var previewResult = await _diaryService.CreateDiaryAsync(
                CreateDiaryRequest(),
                CancellationToken.None);

            if (previewResult.IsFailure)
            {
                SetPreviewError(previewResult.Error.Message);
                return;
            }

            SetPreviewStatus(previewResult.Value);
        }
        catch (OperationCanceledException)
        {
            SetPreviewWarning(
                "Preview cancelled",
                "The data completeness preview was cancelled.");
        }
        catch (Exception exception)
        {
            SetPreviewError($"Unexpected preview error: {exception.Message}");
        }
        finally
        {
            IsPreviewLoading = false;
        }
    }

    /// <summary>
    /// Exports the glycemic diary using the selected format and period.
    /// </summary>
    private async Task ExportAsync()
    {
        if (IsExporting)
        {
            return;
        }

        IsExporting = true;
        SetBusyStatus();

        try
        {
            var exportResult = await CreateExportFileAsync(CancellationToken.None);

            if (exportResult.IsFailure)
            {
                SetErrorStatus(exportResult.Error.Message);
                return;
            }

            StatusTitle = "Choose save location";
            StatusText = "Select where to save the generated diary file.";

            var saveResult = await _fileSaveService.SaveAsync(
                exportResult.Value,
                CancellationToken.None);

            if (saveResult.IsFailure)
            {
                SetErrorStatus(saveResult.Error.Message);
                return;
            }

            if (saveResult.Value.WasCanceled)
            {
                SetWarningStatus(
                    "Export cancelled",
                    "No file was saved.");
                return;
            }

            SetSuccessStatus(saveResult.Value.SavedFileName);
        }
        catch (OperationCanceledException)
        {
            SetWarningStatus(
                "Export cancelled",
                "The export operation was cancelled.");
        }
        catch (Exception exception)
        {
            SetErrorStatus($"Unexpected export error: {exception.Message}");
        }
        finally
        {
            IsExporting = false;
        }
    }

    /// <summary>
    /// Creates the export file using the selected export format.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The exported diary file.</returns>
    private Task<Result<GlycemicDiaryExportFile>> CreateExportFileAsync(
        CancellationToken cancellationToken)
    {
        var diaryRequest = CreateDiaryRequest();

        return SelectedFormat.Kind switch
        {
            DiaryExportFormatKind.Excel => _excelExportService.ExportAsync(
                new GlycemicDiaryExcelExportRequest(diaryRequest),
                cancellationToken),

            DiaryExportFormatKind.Pdf => _pdfExportService.ExportAsync(
                new GlycemicDiaryPdfExportRequest(diaryRequest),
                cancellationToken),

            _ => throw new InvalidOperationException("Unsupported diary export format.")
        };
    }

    /// <summary>
    /// Creates the diary request from the selected period preset.
    /// </summary>
    /// <returns>The diary request.</returns>
    private GlycemicDiaryRequest CreateDiaryRequest()
    {
        var now = _timeProvider.GetLocalNow();

        var (startsAt, endsAt) = SelectedPeriodPreset.Kind switch
        {
            DiaryExportPeriodPresetKind.LastFourteenDays => (
                now.AddDays(-14),
                now),

            DiaryExportPeriodPresetKind.LastThirtyDays => (
                now.AddDays(-30),
                now),

            DiaryExportPeriodPresetKind.CurrentMonth => (
                new DateTimeOffset(
                    now.Year,
                    now.Month,
                    1,
                    0,
                    0,
                    0,
                    now.Offset),
                now),

            DiaryExportPeriodPresetKind.PreviousMonth => CreatePreviousMonthRange(now),

            _ => throw new InvalidOperationException("Unsupported diary export period.")
        };

        return new GlycemicDiaryRequest(startsAt, endsAt);
    }

    /// <summary>
    /// Creates the previous calendar month range.
    /// </summary>
    /// <param name="now">The current local timestamp.</param>
    /// <returns>The previous month range.</returns>
    private static (DateTimeOffset StartsAt, DateTimeOffset EndsAt) CreatePreviousMonthRange(
        DateTimeOffset now)
    {
        var currentMonthStart = new DateTimeOffset(
            now.Year,
            now.Month,
            1,
            0,
            0,
            0,
            now.Offset);

        var previousMonthStart = currentMonthStart.AddMonths(-1);
        var previousMonthEnd = currentMonthStart.AddTicks(-1);

        return (previousMonthStart, previousMonthEnd);
    }

    /// <summary>
    /// Sets the preview status from a generated diary report.
    /// </summary>
    /// <param name="report">The glycemic diary report.</param>
    private void SetPreviewStatus(GlycemicDiaryReport report)
    {
        var coverage = report.OverallContinuity.DataCoveragePercentage;
        var hasReadings = report.ReadingsCount > 0;
        var hasGoodCoverage = coverage >= 90;

        HasPreview = true;
        HasPreviewError = false;
        HasPreviewWarning = !hasReadings || !hasGoodCoverage;

        PreviewCoverageText = $"Data coverage {FormatPreviewPercentage(coverage)}";

        if (!hasReadings)
        {
            PreviewStatusTitle = "No local data found";
            PreviewStatusText = "The selected period does not contain local glucose readings yet.";
        }
        else if (hasGoodCoverage)
        {
            PreviewStatusTitle = "History coverage looks good";
            PreviewStatusText = "The selected period has strong local history coverage.";
        }
        else
        {
            PreviewStatusTitle = "Partial local history";
            PreviewStatusText = "The selected period contains incomplete local history. The export will include completeness indicators.";
        }

        PreviewDetailsText =
            $"{report.ReadingsCount} readings · " +
            $"{report.IncompleteDaysCount} incomplete days · " +
            $"{report.EmptyDaysCount} empty days · " +
            $"{report.OverallContinuity.Gaps.Count} detected gaps";
    }

    /// <summary>
    /// Resets the diary export status to its ready state.
    /// </summary>
    private void ResetReadyStatus()
    {
        if (IsExporting)
        {
            return;
        }

        HasError = false;
        HasSuccess = false;
        HasWarning = false;
        StatusTitle = "Ready to export";
        StatusText = "Choose a period and format, then export your diary.";
    }

    /// <summary>
    /// Resets the data completeness preview status.
    /// </summary>
    private void ResetPreviewStatus()
    {
        if (IsPreviewLoading)
        {
            return;
        }

        HasPreview = false;
        HasPreviewWarning = false;
        HasPreviewError = false;
        PreviewStatusTitle = "Data preview";
        PreviewStatusText = "Refresh the preview to check local data completeness before exporting.";
        PreviewCoverageText = "Coverage not checked yet";
        PreviewDetailsText = "The exported diary will always include data-completeness indicators.";
    }

    /// <summary>
    /// Sets the diary export status to busy.
    /// </summary>
    private void SetBusyStatus()
    {
        HasError = false;
        HasSuccess = false;
        HasWarning = false;
        StatusTitle = "Generating diary";
        StatusText = "GlucoDesk is preparing your export file.";
    }

    /// <summary>
    /// Sets the diary export status to success.
    /// </summary>
    /// <param name="savedFileName">The saved file name.</param>
    private void SetSuccessStatus(string? savedFileName)
    {
        HasError = false;
        HasWarning = false;
        HasSuccess = true;
        StatusTitle = "Diary exported";
        StatusText = string.IsNullOrWhiteSpace(savedFileName)
            ? "The diary file was saved successfully."
            : $"The diary was saved as {savedFileName}.";
    }

    /// <summary>
    /// Sets the diary export status to warning.
    /// </summary>
    /// <param name="title">The status title.</param>
    /// <param name="message">The status message.</param>
    private void SetWarningStatus(
        string title,
        string message)
    {
        HasError = false;
        HasSuccess = false;
        HasWarning = true;
        StatusTitle = title;
        StatusText = message;
    }

    /// <summary>
    /// Sets the diary export status to error.
    /// </summary>
    /// <param name="message">The error message.</param>
    private void SetErrorStatus(string message)
    {
        HasSuccess = false;
        HasWarning = false;
        HasError = true;
        StatusTitle = "Export failed";
        StatusText = string.IsNullOrWhiteSpace(message)
            ? "Unable to export the diary."
            : message;
    }

    /// <summary>
    /// Sets the data preview status to warning.
    /// </summary>
    /// <param name="title">The warning title.</param>
    /// <param name="message">The warning message.</param>
    private void SetPreviewWarning(
        string title,
        string message)
    {
        HasPreview = false;
        HasPreviewError = false;
        HasPreviewWarning = true;
        PreviewStatusTitle = title;
        PreviewStatusText = message;
        PreviewCoverageText = "Coverage unavailable";
        PreviewDetailsText = "Try refreshing the preview again.";
    }

    /// <summary>
    /// Sets the data preview status to error.
    /// </summary>
    /// <param name="message">The error message.</param>
    private void SetPreviewError(string message)
    {
        HasPreview = false;
        HasPreviewWarning = false;
        HasPreviewError = true;
        PreviewStatusTitle = "Preview failed";
        PreviewStatusText = string.IsNullOrWhiteSpace(message)
            ? "Unable to analyze local history for the selected period."
            : message;
        PreviewCoverageText = "Coverage unavailable";
        PreviewDetailsText = "The export can still be generated, but completeness should be checked in the file.";
    }

    /// <summary>
    /// Formats a percentage value for preview labels.
    /// </summary>
    /// <param name="value">The percentage value.</param>
    /// <returns>The formatted percentage.</returns>
    private static string FormatPreviewPercentage(decimal value)
    {
        return $"{value.ToString("0.##", CultureInfo.InvariantCulture)}%";
    }

    #endregion
}