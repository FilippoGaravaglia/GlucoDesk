namespace GlucoDesk.Desktop.DataBackup.Results;

/// <summary>
/// Describes the result of saving a portable local-data backup.
/// </summary>
public sealed record LocalDataBackupSaveResult(
    bool WasCanceled,
    string? FileName)
{
    /// <summary>
    /// Creates a canceled save result.
    /// </summary>
    public static LocalDataBackupSaveResult Canceled()
    {
        return new LocalDataBackupSaveResult(
            WasCanceled: true,
            FileName: null);
    }

    /// <summary>
    /// Creates a successful save result.
    /// </summary>
    public static LocalDataBackupSaveResult Saved(
        string fileName)
    {
        return new LocalDataBackupSaveResult(
            WasCanceled: false,
            FileName: fileName);
    }
}
