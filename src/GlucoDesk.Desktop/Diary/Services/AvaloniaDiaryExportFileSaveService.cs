using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using GlucoDesk.Application.Cgm.Diary.Exports.Results;
using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Desktop.Diary.Results;
using GlucoDesk.Desktop.Diary.Services.Abstractions;
using AvaloniaApplication = Avalonia.Application;
using GlucoDesk.Desktop.Localization;

namespace GlucoDesk.Desktop.Diary.Services;

/// <summary>
/// Saves exported glycemic diary files through the Avalonia storage provider.
/// </summary>
public sealed class AvaloniaDiaryExportFileSaveService : IDiaryExportFileSaveService
{
    /// <inheritdoc />
    public async Task<Result<DiaryExportSaveResult>> SaveAsync(
        GlycemicDiaryExportFile file,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(file);
        cancellationToken.ThrowIfCancellationRequested();

        var selectedFileResult = await PickSaveFileOnUiThreadAsync(file);

        if (selectedFileResult.IsFailure)
        {
            return Result<DiaryExportSaveResult>.Failure(selectedFileResult.Error);
        }

        if (selectedFileResult.Value.WasCanceled)
        {
            return Result<DiaryExportSaveResult>.Success(
                DiaryExportSaveResult.Canceled());
        }

        var selectedFile = selectedFileResult.Value.SelectedFile;

        await using var stream = await selectedFile.OpenWriteAsync();

        await stream.WriteAsync(file.Content, cancellationToken);

        return Result<DiaryExportSaveResult>.Success(
            DiaryExportSaveResult.Saved(selectedFile.Name));
    }

    #region Helpers

    /// <summary>
    /// Opens the save file picker on the Avalonia UI thread.
    /// </summary>
    /// <param name="file">The exported diary file.</param>
    /// <returns>The selected save file result.</returns>
    private static Task<Result<SaveFilePickerSelection>> PickSaveFileOnUiThreadAsync(
        GlycemicDiaryExportFile file)
    {
        var completion = new TaskCompletionSource<Result<SaveFilePickerSelection>>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        Dispatcher.UIThread.Post(async () =>
        {
            try
            {
                var storageProvider = GetStorageProvider();

                if (storageProvider is null)
                {
                    completion.SetResult(Result<SaveFilePickerSelection>.Failure(
                        new Error(
                            "DiaryExport.StorageUnavailable",
                            T("DiaryStorageUnavailable"))));

                    return;
                }

                var selectedFile = await storageProvider.SaveFilePickerAsync(
                    CreateSaveOptions(file));

                completion.SetResult(Result<SaveFilePickerSelection>.Success(
                    SaveFilePickerSelection.From(selectedFile)));
            }
            catch (Exception exception)
            {
                completion.SetResult(Result<SaveFilePickerSelection>.Failure(
                    new Error(
                        "DiaryExport.SaveDialogFailed",
                        exception.Message)));
            }
        });

        return completion.Task;
    }

    /// <summary>
    /// Gets the current desktop storage provider.
    /// </summary>
    /// <returns>The storage provider, if available.</returns>
    private static IStorageProvider? GetStorageProvider()
    {
        if (AvaloniaApplication.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            return null;
        }

        if (desktop.MainWindow is null)
        {
            return null;
        }

        return TopLevel
            .GetTopLevel(desktop.MainWindow)
            ?.StorageProvider;
    }

    /// <summary>
    /// Creates save picker options for an exported diary file.
    /// </summary>
    /// <param name="file">The exported diary file.</param>
    /// <returns>The save picker options.</returns>
    private static FilePickerSaveOptions CreateSaveOptions(
        GlycemicDiaryExportFile file)
    {
        return new FilePickerSaveOptions
        {
            Title = T("DiarySaveDialogTitle"),
            SuggestedFileName = file.FileName,
            FileTypeChoices =
            [
                CreateFileType(file)
            ]
        };
    }

    /// <summary>
    /// Creates a file picker type for an exported diary file.
    /// </summary>
    /// <param name="file">The exported diary file.</param>
    /// <returns>The file picker type.</returns>
    private static FilePickerFileType CreateFileType(
        GlycemicDiaryExportFile file)
    {
        if (file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            return new FilePickerFileType(T("DiaryFormatPdfDocument"))
            {
                Patterns = ["*.pdf"],
                MimeTypes = [file.ContentType]
            };
        }

        return new FilePickerFileType(T("DiaryFormatExcelWorkbook"))
        {
            Patterns = ["*.xlsx"],
            MimeTypes = [file.ContentType]
        };
    }

    private static string T(string key)
    {
        return LocalizationManager.GetString(key);
    }

    #endregion

    /// <summary>
    /// Represents the save file picker selection.
    /// </summary>
    private sealed record SaveFilePickerSelection
    {
        private SaveFilePickerSelection(
            IStorageFile? selectedFile)
        {
            SelectedFileOrNull = selectedFile;
        }

        /// <summary>
        /// Gets a value indicating whether the save operation was canceled.
        /// </summary>
        public bool WasCanceled => SelectedFileOrNull is null;

        /// <summary>
        /// Gets the selected storage file.
        /// </summary>
        public IStorageFile SelectedFile =>
            SelectedFileOrNull ??
            throw new InvalidOperationException("No selected file is available because the save operation was canceled.");

        private IStorageFile? SelectedFileOrNull { get; }

        /// <summary>
        /// Creates a save file picker selection.
        /// </summary>
        /// <param name="selectedFile">The selected storage file, or null when canceled.</param>
        /// <returns>The save file picker selection.</returns>
        public static SaveFilePickerSelection From(
            IStorageFile? selectedFile)
        {
            return new SaveFilePickerSelection(selectedFile);
        }
    }
}
