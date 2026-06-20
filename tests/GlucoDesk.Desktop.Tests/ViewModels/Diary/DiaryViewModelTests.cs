using GlucoDesk.Application.Cgm.Diary.Exports.Requests;
using GlucoDesk.Application.Cgm.Diary.Exports.Results;
using GlucoDesk.Application.Cgm.Diary.Exports.Services.Abstractions;
using GlucoDesk.Application.Cgm.Diary.Requests;
using GlucoDesk.Application.Cgm.Diary.Results;
using GlucoDesk.Application.Cgm.Diary.Services.Abstractions;
using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
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

        var viewModel = new DiaryViewModel(
            new FakeGlycemicDiaryService(),
            excelService,
            pdfService,
            saveService,
            TimeProvider.System);

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

        var viewModel = new DiaryViewModel(
            new FakeGlycemicDiaryService(),
            excelService,
            pdfService,
            saveService,
            TimeProvider.System);

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
    public async Task ExportCommand_ShouldShowError_WhenExportFails()
    {
        // Arrange
        var excelService = new FakeExcelExportService
        {
            ShouldFail = true
        };

        var viewModel = new DiaryViewModel(
            new FakeGlycemicDiaryService(),
            excelService,
            new FakePdfExportService(),
            new FakeFileSaveService(),
            TimeProvider.System);

        // Act
        await viewModel.ExportCommand.ExecuteAsync(null);

        // Assert
        Assert.True(viewModel.HasError);
        Assert.False(viewModel.HasSuccess);
        Assert.Contains("Unable to export diary", viewModel.StatusText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExportCommand_ShouldNotShowSuccess_WhenSaveIsCanceled()
    {
        // Arrange
        var saveService = new FakeFileSaveService
        {
            ShouldCancel = true
        };

        var viewModel = new DiaryViewModel(
            new FakeGlycemicDiaryService(),
            new FakeExcelExportService(),
            new FakePdfExportService(),
            saveService,
            TimeProvider.System);

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
        var viewModel = new DiaryViewModel(
            new FakeGlycemicDiaryService(),
            new FakeExcelExportService(),
            new FakePdfExportService(),
            new FakeFileSaveService(),
            TimeProvider.System);

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

        var viewModel = new DiaryViewModel(
            new FakeGlycemicDiaryService(),
            new FakeExcelExportService(),
            new FakePdfExportService(),
            saveService,
            TimeProvider.System);

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

        var viewModel = new DiaryViewModel(
            new FakeGlycemicDiaryService(),
            excelService,
            new FakePdfExportService(),
            new FakeFileSaveService(),
            TimeProvider.System);

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

        var viewModel = new DiaryViewModel(
            diaryService,
            new FakeExcelExportService(),
            new FakePdfExportService(),
            new FakeFileSaveService(),
            TimeProvider.System);

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

        public bool ShouldFail { get; init; }

        public bool ShouldThrow { get; init; }

        /// <inheritdoc />
        public Task<Result<GlycemicDiaryExportFile>> ExportAsync(
            GlycemicDiaryExcelExportRequest request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            cancellationToken.ThrowIfCancellationRequested();

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

        /// <inheritdoc />
        public Task<Result<GlycemicDiaryExportFile>> ExportAsync(
            GlycemicDiaryPdfExportRequest request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            cancellationToken.ThrowIfCancellationRequested();

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

    #endregion
}