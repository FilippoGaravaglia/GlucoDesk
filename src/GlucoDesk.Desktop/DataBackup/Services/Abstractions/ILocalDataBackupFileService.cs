using GlucoDesk.Application.Common.Results;
using GlucoDesk.Desktop.DataBackup.Models;
using GlucoDesk.Desktop.DataBackup.Results;

namespace GlucoDesk.Desktop.DataBackup.Services.Abstractions;

/// <summary>
/// Opens native desktop file dialogs for portable GlucoDesk backups.
/// </summary>
public interface ILocalDataBackupFileService
{
    /// <summary>
    /// Lets the user choose where to save a generated backup.
    /// </summary>
    Task<Result<LocalDataBackupSaveResult>> SaveAsync(
        LocalDataBackupFile backupFile,
        CancellationToken cancellationToken);

    /// <summary>
    /// Lets the user select an existing portable backup.
    /// </summary>
    Task<Result<LocalDataBackupOpenResult>> OpenAsync(
        CancellationToken cancellationToken);
}
