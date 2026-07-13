using System.Globalization;
using CommunityToolkit.Mvvm.Input;
using GlucoDesk.Application.Cgm.Diary.Exports.Requests;
using GlucoDesk.Application.Cgm.Diary.Exports.Results;
using GlucoDesk.Application.Cgm.Diary.Exports.Services.Abstractions;
using GlucoDesk.Application.Cgm.Diary.Requests;
using GlucoDesk.Application.Cgm.Diary.Results;
using GlucoDesk.Application.Cgm.Diary.Services.Abstractions;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Application.Settings.Abstractions;
using GlucoDesk.Desktop.Diary.Services.Abstractions;
using GlucoDesk.Desktop.ViewModels.Common;
using GlucoDesk.Desktop.ViewModels.Diary.Enums;
using GlucoDesk.Desktop.ViewModels.Diary.Options;
using GlucoDesk.Desktop.Localization;

namespace GlucoDesk.Desktop.ViewModels.Diary;

/// <summary>
/// View model for glycemic diary export.
/// </summary>
public sealed class DiaryViewModel : ViewModelBase, IDisposable
{
    private readonly IGlycemicDiaryService _diaryService;
    private readonly IGlycemicDiaryExcelExportService _excelExportService;
    private readonly IGlycemicDiaryPdfExportService _pdfExportService;
    private readonly IDiaryExportFileSaveService _fileSaveService;
    private readonly IApplicationSettingsService _settingsService;
    private readonly TimeProvider _timeProvider;

    private IReadOnlyList<DiaryExportPeriodPresetOption> _periodPresets = [];
    private IReadOnlyList<DiaryExportFormatOption> _formats = [];
    private bool _isDisposed;

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
    /// <param name="settingsService">The application settings service.</param>
    /// <param name="timeProvider">The time provider.</param>
    public DiaryViewModel(
        IGlycemicDiaryService diaryService,
        IGlycemicDiaryExcelExportService excelExportService,
        IGlycemicDiaryPdfExportService pdfExportService,
        IDiaryExportFileSaveService fileSaveService,
        IApplicationSettingsService settingsService,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(diaryService);
        ArgumentNullException.ThrowIfNull(excelExportService);
        ArgumentNullException.ThrowIfNull(pdfExportService);
        ArgumentNullException.ThrowIfNull(fileSaveService);
        ArgumentNullException.ThrowIfNull(settingsService);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _diaryService = diaryService;
        _excelExportService = excelExportService;
        _pdfExportService = pdfExportService;
        _fileSaveService = fileSaveService;
        _settingsService = settingsService;
        _timeProvider = timeProvider;

        PeriodPresets = BuildPeriodPresets();
        Formats = BuildFormats();

        _selectedPeriodPreset = PeriodPresets.Single(
            option => option.Kind == DiaryExportPeriodPresetKind.LastThirtyDays);

        _selectedFormat = Formats.Single(
            option => option.Kind == DiaryExportFormatKind.Excel);

        _statusTitle = T("DiaryReadyToExportTitle");
        _statusText = T("DiaryReadyToExportDescription");

        _previewStatusTitle = T("DiaryDataPreviewTitle");
        _previewStatusText = T("DiaryDataPreviewDescription");
        _previewCoverageText = T("DiaryCoverageNotCheckedYet");
        _previewDetailsText = T("DiaryCompletenessIndicatorsDescription");

        LocalizationManager.LanguageChanged += OnLanguageChanged;

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
    public IReadOnlyList<DiaryExportPeriodPresetOption> PeriodPresets
    {
        get => _periodPresets;
        private set => SetProperty(ref _periodPresets, value);
    }

    /// <summary>
    /// Gets the available export formats.
    /// </summary>
    public IReadOnlyList<DiaryExportFormatOption> Formats
    {
        get => _formats;
        private set => SetProperty(ref _formats, value);
    }

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
    public string ExportButtonText => IsExporting ? T("DiaryExporting") : T("DiaryExportDiary");

    /// <summary>
    /// Gets the refresh preview button text.
    /// </summary>
    public string RefreshPreviewButtonText => IsPreviewLoading ? T("DiaryChecking") : T("DiaryRefreshPreview");

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

    /// <inheritdoc />
    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        LocalizationManager.LanguageChanged -= OnLanguageChanged;
        _isDisposed = true;
    }

    private static IReadOnlyList<DiaryExportPeriodPresetOption> BuildPeriodPresets()
    {
        return
        [
            new DiaryExportPeriodPresetOption(
                DiaryExportPeriodPresetKind.LastFourteenDays,
                T("DiaryPeriodLast14Days"),
                T("DiaryPeriodLast14DaysDescription")),

            new DiaryExportPeriodPresetOption(
                DiaryExportPeriodPresetKind.LastThirtyDays,
                T("DiaryPeriodLast30Days"),
                T("DiaryPeriodLast30DaysDescription")),

            new DiaryExportPeriodPresetOption(
                DiaryExportPeriodPresetKind.CurrentMonth,
                T("DiaryPeriodCurrentMonth"),
                T("DiaryPeriodCurrentMonthDescription")),

            new DiaryExportPeriodPresetOption(
                DiaryExportPeriodPresetKind.PreviousMonth,
                T("DiaryPeriodPreviousMonth"),
                T("DiaryPeriodPreviousMonthDescription"))
        ];
    }

    private static IReadOnlyList<DiaryExportFormatOption> BuildFormats()
    {
        return
        [
            new DiaryExportFormatOption(
                DiaryExportFormatKind.Excel,
                T("DiaryFormatExcelWorkbook"),
                T("DiaryFormatExcelWorkbookDescription")),

            new DiaryExportFormatOption(
                DiaryExportFormatKind.Pdf,
                T("DiaryFormatPdfDocument"),
                T("DiaryFormatPdfDocumentDescription"))
        ];
    }

    private void OnLanguageChanged(object? sender, EventArgs eventArgs)
    {
        _ = sender;
        _ = eventArgs;

        var selectedPeriodKind = SelectedPeriodPreset.Kind;
        var selectedFormatKind = SelectedFormat.Kind;

        PeriodPresets = BuildPeriodPresets();
        Formats = BuildFormats();

        _selectedPeriodPreset = PeriodPresets.Single(
            option => option.Kind == selectedPeriodKind);

        _selectedFormat = Formats.Single(
            option => option.Kind == selectedFormatKind);

        OnPropertyChanged(nameof(SelectedPeriodPreset));
        OnPropertyChanged(nameof(SelectedFormat));
        OnPropertyChanged(nameof(ExportButtonText));
        OnPropertyChanged(nameof(RefreshPreviewButtonText));

        ResetReadyStatus();
        ResetPreviewStatus();
    }

    private static string T(string key)
    {
        return LocalizationManager.GetString(key);
    }

    private static string TF(string key, params object?[] arguments)
    {
        return string.Format(
            CultureInfo.CurrentCulture,
            T(key),
            arguments);
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
        PreviewStatusTitle = T("DiaryCheckingLocalHistoryTitle");
        PreviewStatusText = T("DiaryAnalyzingSelectedPeriod");
        PreviewCoverageText = T("DiaryCheckingCoverage");
        PreviewDetailsText = T("DiaryScanningLocalHistory");

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
        catch (Exception)
        {
            SetPreviewError(string.Empty);
            PreviewStatusText = T("DiaryUnexpectedPreviewError");
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
            StatusText = T("DiarySelectSaveLocation");

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
                    T("DiaryExportCancelledTitle"),
                    T("DiaryNoFileSaved"));
                return;
            }

            SetSuccessStatus(saveResult.Value.SavedFileName);
        }
        catch (OperationCanceledException)
        {
            SetWarningStatus(
                T("DiaryExportCancelledTitle"),
                "The export operation was cancelled.");
        }
        catch (Exception)
        {
            SetErrorStatus(string.Empty);
            StatusText = T("DiaryUnexpectedExportError");
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
    private async Task<Result<GlycemicDiaryExportFile>> CreateExportFileAsync(
        CancellationToken cancellationToken)
    {
        var settingsResult = await _settingsService
            .GetSettingsAsync(cancellationToken)
            .ConfigureAwait(false);

        if (settingsResult.IsFailure)
        {
            return Result<GlycemicDiaryExportFile>.Failure(settingsResult.Error);
        }

        var diaryRequest = CreateDiaryRequest();
        var preferredUnit = settingsResult.Value.PreferredUnit;

        return SelectedFormat.Kind switch
        {
            DiaryExportFormatKind.Excel => await _excelExportService
                .ExportAsync(
                    new GlycemicDiaryExcelExportRequest(
                        diaryRequest,
                        preferredUnit: preferredUnit),
                    cancellationToken)
                .ConfigureAwait(false),

            DiaryExportFormatKind.Pdf => await _pdfExportService
                .ExportAsync(
                    new GlycemicDiaryPdfExportRequest(
                        diaryRequest,
                        preferredUnit: preferredUnit),
                    cancellationToken)
                .ConfigureAwait(false),

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
            PreviewStatusText = T("DiaryNoLocalReadingsDescription");
        }
        else if (hasGoodCoverage)
        {
            PreviewStatusTitle = T("DiaryHistoryCoverageGoodTitle");
            PreviewStatusText = T("DiaryHistoryCoverageGoodDescription");
        }
        else
        {
            PreviewStatusTitle = T("DiaryPartialHistoryTitle");
            PreviewStatusText = T("DiaryPartialHistoryDescription");
        }

        PreviewDetailsText = TF(
            "DiaryPreviewDetailsFormat",
            report.ReadingsCount,
            report.IncompleteDaysCount,
            report.EmptyDaysCount,
            report.OverallContinuity.Gaps.Count);
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
        StatusTitle = T("DiaryReadyToExportTitle");
        StatusText = T("DiaryReadyToExportDescription");
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
        PreviewStatusTitle = T("DiaryDataPreviewTitle");
        PreviewStatusText = T("DiaryDataPreviewDescription");
        PreviewCoverageText = T("DiaryCoverageNotCheckedYet");
        PreviewDetailsText = T("DiaryCompletenessIndicatorsDescription");
    }

    /// <summary>
    /// Sets the diary export status to busy.
    /// </summary>
    private void SetBusyStatus()
    {
        HasError = false;
        HasSuccess = false;
        HasWarning = false;
        StatusTitle = T("DiaryGeneratingTitle");
        StatusText = T("DiaryGeneratingDescription");
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
        StatusTitle = T("DiaryExportedTitle");
        StatusText = string.IsNullOrWhiteSpace(savedFileName)
            ? T("DiarySavedSuccessfully")
            : TF("DiarySavedAsFormat", savedFileName);
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
        _ = message;

        HasSuccess = false;
        HasWarning = false;
        HasError = true;
        StatusTitle = T("DiaryExportFailedTitle");
        StatusText = T("DiaryUnableToExport");
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
        _ = title;
        _ = message;

        HasPreview = false;
        HasPreviewError = false;
        HasPreviewWarning = true;
        PreviewStatusTitle = T("DiaryPreviewUnavailableTitle");
        PreviewStatusText = T("DiaryPreviewUnavailableDescription");
        PreviewCoverageText = T("DiaryCoverageUnavailable");
        PreviewDetailsText = T("DiaryTryRefreshAgain");
    }

    /// <summary>
    /// Sets the data preview status to error.
    /// </summary>
    /// <param name="message">The error message.</param>
    private void SetPreviewError(string message)
    {
        _ = message;

        HasPreview = false;
        HasPreviewWarning = false;
        HasPreviewError = true;
        PreviewStatusTitle = T("DiaryPreviewFailedTitle");
        PreviewStatusText = T("DiaryUnableAnalyzePeriod");
        PreviewCoverageText = T("DiaryCoverageUnavailable");
        PreviewDetailsText = T("DiaryPreviewErrorDetails");
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