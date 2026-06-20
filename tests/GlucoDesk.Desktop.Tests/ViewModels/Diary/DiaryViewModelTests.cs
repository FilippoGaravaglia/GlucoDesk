using GlucoDesk.Application.Cgm.Diary.Exports.Requests;
using GlucoDesk.Application.Cgm.Diary.Exports.Results;
using GlucoDesk.Application.Cgm.Diary.Exports.Services.Abstractions;
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
            new FakeExcelExportService(),
            new FakePdfExportService(),
            saveService,
            TimeProvider.System);

        // Act
        await viewModel.ExportCommand.ExecuteAsync(null);

        // Assert
        Assert.False(viewModel.HasError);
        Assert.False(viewModel.HasSuccess);
        Assert.Equal("Export cancelled.", viewModel.StatusText);
    }

    #region Helpers

    private sealed class FakeExcelExportService : IGlycemicDiaryExcelExportService
    {
        public int ExportCount { get; private set; }

        public bool ShouldFail { get; init; }

        /// <inheritdoc />
        public Task<Result<GlycemicDiaryExportFile>> ExportAsync(
            GlycemicDiaryExcelExportRequest request,
            CancellationToken cancellationToken)
        {
            ExportCount++;

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

        /// <inheritdoc />
        public Task<Result<DiaryExportSaveResult>> SaveAsync(
            GlycemicDiaryExportFile file,
            CancellationToken cancellationToken)
        {
            SaveCount++;

            return Task.FromResult(Result<DiaryExportSaveResult>.Success(
                ShouldCancel
                    ? DiaryExportSaveResult.Canceled()
                    : DiaryExportSaveResult.Saved(file.FileName)));
        }
    }

    #endregion
}