namespace GlucoDesk.Desktop.DataBackup.Models;

/// <summary>
/// Represents a portable GlucoDesk local-data backup file.
/// </summary>
public sealed record LocalDataBackupFile(
    string FileName,
    string ContentType,
    byte[] Content);
