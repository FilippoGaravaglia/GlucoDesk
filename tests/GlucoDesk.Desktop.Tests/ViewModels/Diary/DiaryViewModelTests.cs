using GlucoDesk.Application.Cgm.Diary.Exports.Requests;
using GlucoDesk.Application.Cgm.Diary.Exports.Results;
using GlucoDesk.Application.Cgm.Diary.Exports.Services.Abstractions;
using GlucoDesk.Application.Cgm.Diary.Requests;
using GlucoDesk.Application.Cgm.Diary.Results;
using GlucoDesk.Application.Cgm.Diary.Services.Abstractions;
using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Application.Settings.Abstractions;
using GlucoDesk.Application.Settings.Models;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Desktop.Diary.Results;
using GlucoDesk.Desktop.Diary.Services.Abstractions;
using GlucoDesk.Desktop.ViewModels.Diary;
using GlucoDesk.Desktop.ViewModels.Diary.Enums;

namespace GlucoDesk.Desktop.Tests.ViewModels.Diary;

public sealed class DiaryViewModelTests
{
    [Fact]
    public async Task ExportCommand_ShouldExportExcelAndSaveFile_WhenExcelIsSelected()
    {
        // Arrange
        var excelService = new FakeExcelExportService();
        var pdfService = new FakePdfExportService();
        var saveService = new FakeFileSaveService();

        var viewModel = CreateViewModel(
            excelService: excelService,
            pdfService: pdfService,
            saveService: saveService);

        viewModel.SelectedFormat = viewModel.Formats.Single(format =>
            format.Kind == DiaryExportFormatKind.Excel);

        // Act
        await viewModel.ExportCommand.ExecuteAsync(null);

        // Assert
        Assert.Equal(1, excelService.ExportCount);
        Assert.Equal(0, pdfService.ExportCount);
        Assert.Equal(1, saveService.SaveCount);
        Assert.True(viewModel.HasSuccess);
        Assert.False(viewModel.HasError);
    }

    [Fact]
    public async Task ExportCommand_ShouldExportPdfAndSaveFile_WhenPdfIsSelected()
    {
        // Arrange
        var excelService = new FakeExcelExportService();
        var pdfService = new FakePdfExportService();
        var saveService = new FakeFileSaveService();

        var viewModel = CreateViewModel(
            excelService: excelService,
            pdfService: pdfService,
            saveService: saveService);

        viewModel.SelectedFormat = viewModel.Formats.Single(format =>
            format.Kind == DiaryExportFormatKind.Pdf);

        // Act
        await viewModel.ExportCommand.ExecuteAsync(null);

        // Assert
        Assert.Equal(0, excelService.ExportCount);
        Assert.Equal(1, pdfService.ExportCount);
        Assert.Equal(1, saveService.SaveCount);
        Assert.True(viewModel.HasSuccess);
        Assert.False(viewModel.HasError);
    }

    [Fact]
    public async Task ExportCommand_ShouldPassPreferredUnitToExcelExport_WhenSettingsUseMmolL()
    {
        // Arrange
        var excelService = new FakeExcelExportService();

        var settingsService = new FakeApplicationSettingsService
        {
            Settings = new ApplicationSettings(preferredUnit: GlucoseUnit.MmolL)
        };

        var viewModel = CreateViewModel(
            excelService: excelService,
            settingsService: settingsService);

        viewModel.SelectedFormat = viewModel.Formats.Single(format =>
            format.Kind == DiaryExportFormatKind.Excel);

        // Act
        await viewModel.ExportCommand.ExecuteAsync(null);

        // Assert
        Assert.NotNull(excelService.LastRequest);
        Assert.Equal(GlucoseUnit.MmolL, excelService.LastRequest.PreferredUnit);
    }

    [Fact]
    public async Task ExportCommand_ShouldPassPreferredUnitToPdfExport_WhenSettingsUseMmolL()
    {
        // Arrange
        var pdfService = new FakePdfExportService();

        var settingsService = new FakeApplicationSettingsService
        {
            Settings = new ApplicationSettings(preferredUnit: GlucoseUnit.MmolL)
        };

        var viewModel = CreateViewModel(
            pdfService: pdfService,
            settingsService: settingsService);

        viewModel.SelectedFormat = viewModel.Formats.Single(format =>
            format.Kind == DiaryExportFormatKind.Pdf);

        // Act
        await viewModel.ExportCommand.ExecuteAsync(null);

        // Assert
        Assert.NotNull(pdfService.LastRequest);
        Assert.Equal(GlucoseUnit.MmolL, pdfService.LastRequest.PreferredUnit);
    }

    [Fact]
    public async Task ExportCommand_ShouldShowError_WhenExportFails()
    {
        // Arrange
        var excelService = new FakeExcelExportService
        {
            ShouldFail = true
        };

        var viewModel = CreateViewModel(
            excelService: excelService);

        // Act
        await viewModel.ExportCommand.ExecuteAsync(null);

        // Assert
        Assert.True(viewModel.HasError);
        Assert.False(viewModel.HasSuccess);
        Assert.Contains("Unable to export diary", viewModel.StatusText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExportCommand_ShouldShowError_WhenSettingsCannotBeLoaded()
    {
        // Arrange
        var excelService = new FakeExcelExportService();
        var saveService = new FakeFileSaveService();

        var settingsService = new FakeApplicationSettingsService
        {
            ShouldFail = true
        };

        var viewModel = CreateViewModel(
            excelService: excelService,
            saveService: saveService,
            settingsService: settingsService);

        // Act
        await viewModel.ExportCommand.ExecuteAsync(null);

        // Assert
        Assert.True(viewModel.HasError);
        Assert.False(viewModel.HasSuccess);
        Assert.Equal(0, excelService.ExportCount);
        Assert.Equal(0, saveService.SaveCount);
    }

    [Fact]
    public async Task ExportCommand_ShouldNotShowSuccess_WhenSaveIsCanceled()
    {
        // Arrange
        var saveService = new FakeFileSaveService
        {
            ShouldCancel = true
        };

        var viewModel = CreateViewModel(
            saveService: saveService);

        // Act
        await viewModel.ExportCommand.ExecuteAsync(null);

        // Assert
        Assert.False(viewModel.HasError);
        Assert.False(viewModel.HasSuccess);
        Assert.True(viewModel.HasWarning);
        Assert.Equal("Export cancelled", viewModel.StatusTitle);
        Assert.Equal("No file was saved.", viewModel.StatusText);
    }

    [Fact]
    public void Constructor_ShouldExposeReadyStatus()
    {
        // Arrange & Act
        var viewModel = CreateViewModel();

        // Assert
        Assert.False(viewModel.IsExporting);
        Assert.False(viewModel.IsPreviewLoading);
        Assert.True(viewModel.CanEditSelection);

        Assert.False(viewModel.HasError);
        Assert.False(viewModel.HasSuccess);
        Assert.False(viewModel.HasWarning);

        Assert.False(viewModel.HasPreview);
        Assert.False(viewModel.HasPreviewWarning);
        Assert.False(viewModel.HasPreviewError);

        Assert.Equal("Ready to export", viewModel.StatusTitle);
        Assert.Equal("Export diary", viewModel.ExportButtonText);

        Assert.Equal("Data preview", viewModel.PreviewStatusTitle);
        Assert.Equal("Coverage not checked yet", viewModel.PreviewCoverageText);
        Assert.Equal("Refresh preview", viewModel.RefreshPreviewButtonText);
    }

    [Fact]
    public async Task ExportCommand_ShouldShowError_WhenSaveFails()
    {
        // Arrange
        var saveService = new FakeFileSaveService
        {
            ShouldFail = true
        };

        var viewModel = CreateViewModel(
            saveService: saveService);

        // Act
        await viewModel.ExportCommand.ExecuteAsync(null);

        // Assert
        Assert.True(viewModel.HasError);
        Assert.False(viewModel.HasSuccess);
        Assert.False(viewModel.HasWarning);
        Assert.Equal("Export failed", viewModel.StatusTitle);
    }

    [Fact]
    public async Task ExportCommand_ShouldHandleUnexpectedExportException()
    {
        // Arrange
        var excelService = new FakeExcelExportService
        {
            ShouldThrow = true
        };

        var viewModel = CreateViewModel(
            excelService: excelService);

        // Act
        await viewModel.ExportCommand.ExecuteAsync(null);

        // Assert
        Assert.True(viewModel.HasError);
        Assert.False(viewModel.HasSuccess);
        Assert.Contains("Unexpected export error", viewModel.StatusText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RefreshPreviewCommand_ShouldShowError_WhenPreviewFails()
    {
        // Arrange
        var diaryService = new FakeGlycemicDiaryService
        {
            ShouldFail = true
        };

        var viewModel = CreateViewModel(
            diaryService: diaryService);

        // Act
        await viewModel.RefreshPreviewCommand.ExecuteAsync(null);

        // Assert
        Assert.True(viewModel.HasPreviewError);
        Assert.False(viewModel.HasPreview);
        Assert.False(viewModel.HasPreviewWarning);
        Assert.Equal("Preview failed", viewModel.PreviewStatusTitle);
        Assert.Equal("Coverage unavailable", viewModel.PreviewCoverageText);
    }

    #region Helpers

    /// <summary>
    /// Creates a diary view model for tests.
    /// </summary>
    /// <param name="diaryService">The optional diary service.</param>
    /// <param name="excelService">The optional Excel export service.</param>
    /// <param name="pdfService">The optional PDF export service.</param>
    /// <param name="saveService">The optional file save service.</param>
    /// <param name="settingsService">The optional settings service.</param>
    /// <returns>The diary view model.</returns>
    private static DiaryViewModel CreateViewModel(
        IGlycemicDiaryService? diaryService = null,
        FakeExcelExportService? excelService = null,
        FakePdfExportService? pdfService = null,
        FakeFileSaveService? saveService = null,
        IApplicationSettingsService? settingsService = null)
    {
        return new DiaryViewModel(
            diaryService ?? new FakeGlycemicDiaryService(),
            excelService ?? new FakeExcelExportService(),
            pdfService ?? new FakePdfExportService(),
            saveService ?? new FakeFileSaveService(),
            settingsService ?? new FakeApplicationSettingsService(),
            TimeProvider.System);
    }

    private sealed class FakeGlycemicDiaryService : IGlycemicDiaryService
    {
        public bool ShouldFail { get; init; }

        /// <inheritdoc />
        public Task<Result<GlycemicDiaryReport>> CreateDiaryAsync(
            GlycemicDiaryRequest request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            cancellationToken.ThrowIfCancellationRequested();

            if (ShouldFail)
            {
                return Task.FromResult(Result<GlycemicDiaryReport>.Failure(
                    new Error(
                        "Diary.PreviewFailed",
                        "Unable to preview diary completeness.")));
            }

            throw new NotSupportedException("Preview generation is not used by this test.");
        }
    }

    private sealed class FakeExcelExportService : IGlycemicDiaryExcelExportService
    {
        public int ExportCount { get; private set; }

        public GlycemicDiaryExcelExportRequest? LastRequest { get; private set; }

        public bool ShouldFail { get; init; }

        public bool ShouldThrow { get; init; }

        /// <inheritdoc />
        public Task<Result<GlycemicDiaryExportFile>> ExportAsync(
            GlycemicDiaryExcelExportRequest request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            cancellationToken.ThrowIfCancellationRequested();

            LastRequest = request;
            ExportCount++;

            if (ShouldThrow)
            {
                throw new InvalidOperationException("Simulated export exception.");
            }

            if (ShouldFail)
            {
                return Task.FromResult(Result<GlycemicDiaryExportFile>.Failure(
                    new Error(
                        "Diary.ExportFailed",
                        "Unable to export diary.")));
            }

            return Task.FromResult(Result<GlycemicDiaryExportFile>.Success(
                new GlycemicDiaryExportFile(
                    "diary.xlsx",
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    [1, 2, 3])));
        }
    }

    private sealed class FakePdfExportService : IGlycemicDiaryPdfExportService
    {
        public int ExportCount { get; private set; }

        public GlycemicDiaryPdfExportRequest? LastRequest { get; private set; }

        /// <inheritdoc />
        public Task<Result<GlycemicDiaryExportFile>> ExportAsync(
            GlycemicDiaryPdfExportRequest request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            cancellationToken.ThrowIfCancellationRequested();

            LastRequest = request;
            ExportCount++;

            return Task.FromResult(Result<GlycemicDiaryExportFile>.Success(
                new GlycemicDiaryExportFile(
                    "diary.pdf",
                    "application/pdf",
                    [1, 2, 3])));
        }
    }

    private sealed class FakeFileSaveService : IDiaryExportFileSaveService
    {
        public int SaveCount { get; private set; }

        public bool ShouldCancel { get; init; }

        public bool ShouldFail { get; init; }

        /// <inheritdoc />
        public Task<Result<DiaryExportSaveResult>> SaveAsync(
            GlycemicDiaryExportFile file,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(file);
            cancellationToken.ThrowIfCancellationRequested();

            SaveCount++;

            if (ShouldFail)
            {
                return Task.FromResult(Result<DiaryExportSaveResult>.Failure(
                    new Error(
                        "Diary.SaveFailed",
                        "Unable to save diary file.")));
            }

            return Task.FromResult(Result<DiaryExportSaveResult>.Success(
                ShouldCancel
                    ? DiaryExportSaveResult.Canceled()
                    : DiaryExportSaveResult.Saved(file.FileName)));
        }
    }

    private sealed class FakeApplicationSettingsService : IApplicationSettingsService
    {
        public bool ShouldFail { get; init; }

        public ApplicationSettings Settings { get; init; } = ApplicationSettings.Default;

        /// <inheritdoc />
        public Task<Result<ApplicationSettings>> GetSettingsAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (ShouldFail)
            {
                return Task.FromResult(Result<ApplicationSettings>.Failure(
                    new Error(
                        "Settings.LoadFailed",
                        "Unable to load settings.")));
            }

            return Task.FromResult(Result<ApplicationSettings>.Success(Settings));
        }

        /// <inheritdoc />
        public Task<Result> SaveSettingsAsync(
            ApplicationSettings settings,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(settings);
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(Result.Success());
        }
    }

    #endregion
}