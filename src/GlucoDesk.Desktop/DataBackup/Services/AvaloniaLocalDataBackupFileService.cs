using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Desktop.DataBackup.Models;
using GlucoDesk.Desktop.DataBackup.Results;
using GlucoDesk.Desktop.DataBackup.Services.Abstractions;
using GlucoDesk.Desktop.Localization;
using AvaloniaApplication = Avalonia.Application;

namespace GlucoDesk.Desktop.DataBackup.Services;

/// <summary>
/// Saves and opens portable backups through Avalonia native file dialogs.
/// </summary>
public sealed class AvaloniaLocalDataBackupFileService :
    ILocalDataBackupFileService
{
    private const int MaximumBackupFileSizeBytes =
        100 * 1024 * 1024;

    /// <inheritdoc />
    public async Task<Result<LocalDataBackupSaveResult>> SaveAsync(
        LocalDataBackupFile backupFile,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(backupFile);
        cancellationToken.ThrowIfCancellationRequested();

        var selectionResult =
            await PickSaveFileOnUiThreadAsync(backupFile);

        if (selectionResult.IsFailure)
        {
            return Result<LocalDataBackupSaveResult>.Failure(
                selectionResult.Error);
        }

        if (selectionResult.Value.WasCanceled)
        {
            return Result<LocalDataBackupSaveResult>.Success(
                LocalDataBackupSaveResult.Canceled());
        }

        var selectedFile =
            selectionResult.Value.SelectedFile;

        try
        {
            await using var stream =
                await selectedFile.OpenWriteAsync();

            if (stream.CanSeek)
            {
                stream.SetLength(0);
            }

            await stream.WriteAsync(
                backupFile.Content,
                cancellationToken);

            await stream.FlushAsync(
                cancellationToken);

            return Result<LocalDataBackupSaveResult>.Success(
                LocalDataBackupSaveResult.Saved(
                    selectedFile.Name));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
            when (IsStorageException(exception))
        {
            return Result<LocalDataBackupSaveResult>.Failure(
                new Error(
                    "LocalBackup.FileSaveFailed",
                    exception.Message));
        }
    }

    /// <inheritdoc />
    public async Task<Result<LocalDataBackupOpenResult>> OpenAsync(
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var selectionResult =
            await PickOpenFileOnUiThreadAsync();

        if (selectionResult.IsFailure)
        {
            return Result<LocalDataBackupOpenResult>.Failure(
                selectionResult.Error);
        }

        if (selectionResult.Value.WasCanceled)
        {
            return Result<LocalDataBackupOpenResult>.Success(
                LocalDataBackupOpenResult.Canceled());
        }

        var selectedFile =
            selectionResult.Value.SelectedFile;

        try
        {
            await using var stream =
                await selectedFile.OpenReadAsync();

            if (stream.CanSeek &&
                stream.Length > MaximumBackupFileSizeBytes)
            {
                return Result<LocalDataBackupOpenResult>.Failure(
                    new Error(
                        "LocalBackup.FileTooLarge",
                        T("LocalBackupFileTooLarge")));
            }

            await using var memoryStream =
                new MemoryStream();

            var buffer = new byte[81920];
            var totalBytes = 0;

            while (true)
            {
                var bytesRead = await stream.ReadAsync(
                    buffer.AsMemory(),
                    cancellationToken);

                if (bytesRead == 0)
                {
                    break;
                }

                totalBytes += bytesRead;

                if (totalBytes > MaximumBackupFileSizeBytes)
                {
                    return Result<LocalDataBackupOpenResult>.Failure(
                        new Error(
                            "LocalBackup.FileTooLarge",
                            T("LocalBackupFileTooLarge")));
                }

                await memoryStream.WriteAsync(
                    buffer.AsMemory(0, bytesRead),
                    cancellationToken);
            }

            return Result<LocalDataBackupOpenResult>.Success(
                LocalDataBackupOpenResult.Opened(
                    selectedFile.Name,
                    memoryStream.ToArray()));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
            when (IsStorageException(exception))
        {
            return Result<LocalDataBackupOpenResult>.Failure(
                new Error(
                    "LocalBackup.FileOpenFailed",
                    exception.Message));
        }
    }

    private static Task<Result<StorageFileSelection>>
        PickSaveFileOnUiThreadAsync(
            LocalDataBackupFile backupFile)
    {
        var completion =
            new TaskCompletionSource<
                Result<StorageFileSelection>>(
                TaskCreationOptions
                    .RunContinuationsAsynchronously);

        Dispatcher.UIThread.Post(async () =>
        {
            try
            {
                var storageProvider =
                    GetStorageProvider();

                if (storageProvider is null)
                {
                    completion.SetResult(
                        Result<StorageFileSelection>.Failure(
                            new Error(
                                "LocalBackup.StorageUnavailable",
                                T("LocalBackupStorageUnavailable"))));

                    return;
                }

                var selectedFile =
                    await storageProvider.SaveFilePickerAsync(
                        new FilePickerSaveOptions
                        {
                            Title =
                                T("LocalBackupSaveDialogTitle"),
                            SuggestedFileName =
                                backupFile.FileName,
                            DefaultExtension =
                                "glucodesk-backup",
                            FileTypeChoices =
                            [
                                CreateBackupFileType()
                            ]
                        });

                completion.SetResult(
                    Result<StorageFileSelection>.Success(
                        StorageFileSelection.From(
                            selectedFile)));
            }
            catch (Exception exception)
            {
                completion.SetResult(
                    Result<StorageFileSelection>.Failure(
                        new Error(
                            "LocalBackup.SaveDialogFailed",
                            exception.Message)));
            }
        });

        return completion.Task;
    }

    private static Task<Result<StorageFileSelection>>
        PickOpenFileOnUiThreadAsync()
    {
        var completion =
            new TaskCompletionSource<
                Result<StorageFileSelection>>(
                TaskCreationOptions
                    .RunContinuationsAsynchronously);

        Dispatcher.UIThread.Post(async () =>
        {
            try
            {
                var storageProvider =
                    GetStorageProvider();

                if (storageProvider is null)
                {
                    completion.SetResult(
                        Result<StorageFileSelection>.Failure(
                            new Error(
                                "LocalBackup.StorageUnavailable",
                                T("LocalBackupStorageUnavailable"))));

                    return;
                }

                var selectedFiles =
                    await storageProvider.OpenFilePickerAsync(
                        new FilePickerOpenOptions
                        {
                            Title =
                                T("LocalBackupOpenDialogTitle"),
                            AllowMultiple = false,
                            FileTypeFilter =
                            [
                                CreateBackupFileType()
                            ]
                        });

                completion.SetResult(
                    Result<StorageFileSelection>.Success(
                        StorageFileSelection.From(
                            selectedFiles.FirstOrDefault())));
            }
            catch (Exception exception)
            {
                completion.SetResult(
                    Result<StorageFileSelection>.Failure(
                        new Error(
                            "LocalBackup.OpenDialogFailed",
                            exception.Message)));
            }
        });

        return completion.Task;
    }

    private static IStorageProvider? GetStorageProvider()
    {
        if (AvaloniaApplication.Current?.ApplicationLifetime
            is not IClassicDesktopStyleApplicationLifetime desktop)
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

    private static FilePickerFileType CreateBackupFileType()
    {
        return new FilePickerFileType(
            T("LocalBackupFileType"))
        {
            Patterns =
            [
                "*.glucodesk-backup"
            ],
            MimeTypes =
            [
                "application/vnd.glucodesk.backup",
                "application/zip"
            ]
        };
    }

    private static bool IsStorageException(
        Exception exception)
    {
        return exception is
            IOException
            or UnauthorizedAccessException
            or NotSupportedException;
    }

    private static string T(string key)
    {
        return LocalizationManager.GetString(key);
    }

    /// <summary>
    /// Represents the result of a native file picker operation.
    /// </summary>
    private sealed record StorageFileSelection
    {
        private StorageFileSelection(
            IStorageFile? selectedFile)
        {
            SelectedFileOrNull = selectedFile;
        }

        /// <summary>
        /// Gets whether the user canceled the picker.
        /// </summary>
        public bool WasCanceled =>
            SelectedFileOrNull is null;

        /// <summary>
        /// Gets the selected file.
        /// </summary>
        public IStorageFile SelectedFile =>
            SelectedFileOrNull
            ?? throw new InvalidOperationException(
                "No selected file is available because "
                + "the file picker was canceled.");

        private IStorageFile? SelectedFileOrNull
        {
            get;
        }

        /// <summary>
        /// Creates a picker selection from an optional storage file.
        /// </summary>
        public static StorageFileSelection From(
            IStorageFile? selectedFile)
        {
            return new StorageFileSelection(
                selectedFile);
        }
    }
}
