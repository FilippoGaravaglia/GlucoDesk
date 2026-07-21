namespace GlucoDesk.Desktop.DataBackup.Results;

/// <summary>
/// Describes a portable backup selected through the desktop file picker.
/// </summary>
public sealed record LocalDataBackupOpenResult(
    bool WasCanceled,
    string? FileName,
    byte[]? Content)
{
    /// <summary>
    /// Creates a canceled open result.
    /// </summary>
    public static LocalDataBackupOpenResult Canceled()
    {
        return new LocalDataBackupOpenResult(
            WasCanceled: true,
            FileName: null,
            Content: null);
    }

    /// <summary>
    /// Creates a successful open result.
    /// </summary>
    public static LocalDataBackupOpenResult Opened(
        string fileName,
        byte[] content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentNullException.ThrowIfNull(content);

        return new LocalDataBackupOpenResult(
            WasCanceled: false,
            FileName: fileName,
            Content: content);
    }
}
