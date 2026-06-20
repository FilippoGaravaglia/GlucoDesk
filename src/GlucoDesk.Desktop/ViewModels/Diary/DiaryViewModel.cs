using CommunityToolkit.Mvvm.Input;
using GlucoDesk.Application.Cgm.Diary.Exports.Requests;
using GlucoDesk.Application.Cgm.Diary.Exports.Results;
using GlucoDesk.Application.Cgm.Diary.Exports.Services.Abstractions;
using GlucoDesk.Application.Cgm.Diary.Requests;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Desktop.Diary.Services.Abstractions;
using GlucoDesk.Desktop.ViewModels.Diary.Enums;
using GlucoDesk.Desktop.ViewModels.Diary.Options;
using GlucoDesk.Desktop.ViewModels.Common;

namespace GlucoDesk.Desktop.ViewModels.Diary;

/// <summary>
/// View model for glycemic diary export.
/// </summary>
public sealed class DiaryViewModel : ViewModelBase
{
    private readonly IGlycemicDiaryExcelExportService _excelExportService;
    private readonly IGlycemicDiaryPdfExportService _pdfExportService;
    private readonly IDiaryExportFileSaveService _fileSaveService;
    private readonly TimeProvider _timeProvider;

    private DiaryExportPeriodPresetOption _selectedPeriodPreset;
    private DiaryExportFormatOption _selectedFormat;
    private bool _isExporting;
    private bool _hasError;
    private bool _hasSuccess;
    private string _statusText;

    /// <summary>
    /// Initializes a new instance of the <see cref="DiaryViewModel"/> class.
    /// </summary>
    /// <param name="excelExportService">The Excel export service.</param>
    /// <param name="pdfExportService">The PDF export service.</param>
    /// <param name="fileSaveService">The file save service.</param>
    /// <param name="timeProvider">The time provider.</param>
    public DiaryViewModel(
        IGlycemicDiaryExcelExportService excelExportService,
        IGlycemicDiaryPdfExportService pdfExportService,
        IDiaryExportFileSaveService fileSaveService,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(excelExportService);
        ArgumentNullException.ThrowIfNull(pdfExportService);
        ArgumentNullException.ThrowIfNull(fileSaveService);
        ArgumentNullException.ThrowIfNull(timeProvider);

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
        _statusText = "Choose a period and format, then export your diary.";

        ExportCommand = new AsyncRelayCommand(
            ExportAsync,
            CanExport);
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
    /// Gets or sets the selected period preset.
    /// </summary>
    public DiaryExportPeriodPresetOption SelectedPeriodPreset
    {
        get => _selectedPeriodPreset;
        set
        {
            if (SetProperty(ref _selectedPeriodPreset, value))
            {
                ExportCommand.NotifyCanExecuteChanged();
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
            if (SetProperty(ref _selectedFormat, value))
            {
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
                ExportCommand.NotifyCanExecuteChanged();
            }
        }
    }

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
    /// Gets the export status text.
    /// </summary>
    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    #region Helpers

    /// <summary>
    /// Determines whether the diary can be exported.
    /// </summary>
    /// <returns>A value indicating whether export is available.</returns>
    private bool CanExport()
    {
        return !IsExporting &&
               SelectedPeriodPreset is not null &&
               SelectedFormat is not null;
    }

    /// <summary>
    /// Exports the glycemicsummary>
    /// </summary>
    private async Task ExportAsync()
    {
        IsExporting = true;
        HasError = false;
        HasSuccess = false;
        StatusText = "Generating diary export...";

        try
        {
            var exportResult = await CreateExportFileAsync(CancellationToken.None);

            if (exportResult.IsFailure)
            {
                HasError = true;
                StatusText = exportResult.Error.Message;
                return;
            }

            StatusText = "Choose where to save your diary...";

            var saveResult = await _fileSaveService
                .SaveAsync(exportResult.Value, CancellationToken.None);

            if (saveResult.IsFailure)
            {
                HasError = true;
                StatusText = saveResult.Error.Message;
                return;
            }

            if (saveResult.Value.WasCanceled)
            {
                StatusText = "Export cancelled.";
                return;
            }

            HasSuccess = true;
            StatusText = $"Diary saved as {saveResult.Value.SavedFileName}.";
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

    #endregion
}