using GlucoDesk.Desktop.DataBackup.Services.Abstractions;
using GlucoDesk.Desktop.Localization;

namespace GlucoDesk.Desktop.DataBackup.Services;

/// <summary>
/// Reads and restores the desktop language preference for portable backups.
/// </summary>
public sealed class LocalBackupLanguagePreferenceService :
    ILocalBackupLanguagePreferenceService
{
    /// <inheritdoc />
    public string CurrentLanguageCode =>
        LocalizationManager.CurrentLanguageCode;

    /// <inheritdoc />
    public bool TryApplyAndPersist(string? languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            return false;
        }

        var supportedLanguage =
            TranslationCatalog.SupportedLanguages.FirstOrDefault(
                language => string.Equals(
                    language.Code,
                    languageCode,
                    StringComparison.OrdinalIgnoreCase));

        if (supportedLanguage is null)
        {
            return false;
        }

        LocalizationManager.SetLanguage(
            supportedLanguage.Code);

        return true;
    }
}
