using System.Text.Json;

namespace GlucoDesk.Desktop.Localization;

/// <summary>
/// Stores the local UI language preference.
/// </summary>
public static class LanguagePreferenceStore
{
    private const string DirectoryName = "GlucoDesk";
    private const string FileName = "language-preferences.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    /// <summary>
    /// Loads the selected language code from local storage.
    /// </summary>
    /// <returns>The stored language code, or the default language code.</returns>
    public static string LoadLanguageCode()
    {
        try
        {
            var filePath = GetPreferenceFilePath();

            if (!File.Exists(filePath))
            {
                return TranslationCatalog.DefaultLanguageCode;
            }

            var json = File.ReadAllText(filePath);
            var file = JsonSerializer.Deserialize<LanguagePreferenceFile>(json, JsonOptions);

            return TranslationCatalog.NormalizeLanguageCode(file?.LanguageCode);
        }
        catch
        {
            return TranslationCatalog.DefaultLanguageCode;
        }
    }

    /// <summary>
    /// Saves the selected language code to local storage.
    /// </summary>
    /// <param name="languageCode">The selected language code.</param>
    public static void SaveLanguageCode(string languageCode)
    {
        var normalizedLanguageCode = TranslationCatalog.NormalizeLanguageCode(languageCode);
        var filePath = GetPreferenceFilePath();
        var directoryPath = Path.GetDirectoryName(filePath);

        if (!string.IsNullOrWhiteSpace(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        var file = new LanguagePreferenceFile(normalizedLanguageCode);
        var json = JsonSerializer.Serialize(file, JsonOptions);

        File.WriteAllText(filePath, json);
    }

    private static string GetPreferenceFilePath()
    {
        var baseDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        if (string.IsNullOrWhiteSpace(baseDirectory))
        {
            baseDirectory = AppContext.BaseDirectory;
        }

        return Path.Combine(baseDirectory, DirectoryName, FileName);
    }

    private sealed record LanguagePreferenceFile(string LanguageCode);
}
