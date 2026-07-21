using GlucoDesk.Application.Common.Results;
using GlucoDesk.Desktop.DataBackup.Models;
using GlucoDesk.Desktop.DataBackup.Results;

namespace GlucoDesk.Desktop.DataBackup.Services.Abstractions;

/// <summary>
/// Exports and imports portable GlucoDesk local-data backups.
/// </summary>
public interface ILocalDataBackupService
{
    /// <summary>
    /// Builds a portable backup containing local glucose history and
    /// non-secret application preferences.
    /// </summary>
    Task<Result<LocalDataBackupFile>> ExportAsync(
        CancellationToken cancellationToken);

    /// <summary>
    /// Imports a portable backup, merging glucose history and restoring
    /// non-secret preferences.
    /// </summary>
    Task<Result<LocalDataImportResult>> ImportAsync(
        Stream backupStream,
        CancellationToken cancellationToken);
}
