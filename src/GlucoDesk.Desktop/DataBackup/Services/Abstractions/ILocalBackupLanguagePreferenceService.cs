namespace GlucoDesk.Desktop.DataBackup.Services.Abstractions;

/// <summary>
/// Provides access to the application language included in portable backups.
/// </summary>
public interface ILocalBackupLanguagePreferenceService
{
    /// <summary>
    /// Gets the language currently active in the desktop application.
    /// </summary>
    string CurrentLanguageCode { get; }

    /// <summary>
    /// Applies and persists a supported language imported from a backup.
    /// </summary>
    /// <param name="languageCode">
    /// The language code stored in the backup.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the language is valid and was imported;
    /// otherwise <see langword="false"/>.
    /// </returns>
    bool TryApplyAndPersist(string? languageCode);
}
