using System.Text.Json;

namespace GlucoDesk.Desktop.Localization;

/// <summary>
/// Describes the persisted application language preference.
/// </summary>
/// <param name="HasExplicitPreference">
/// Whether the user explicitly selected and persisted a supported language.
/// </param>
/// <param name="LanguageCode">
/// The normalized supported language code, or the application fallback language.
/// </param>
public sealed record LanguagePreferenceReadResult(
    bool HasExplicitPreference,
    string LanguageCode);

/// <summary>
/// Reads and writes a language preference file.
/// </summary>
public sealed class LanguagePreferenceFileStore
{
    private const int CurrentSchemaVersion = 1;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _filePath;

    /// <summary>
    /// Initializes the file store.
    /// </summary>
    /// <param name="filePath">The absolute preference file path.</param>
    public LanguagePreferenceFileStore(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException(
                "Language preference file path cannot be empty.",
                nameof(filePath));
        }

        _filePath = Path.GetFullPath(filePath);
    }

    /// <summary>
    /// Reads the persisted language preference.
    /// </summary>
    /// <returns>
    /// An explicit supported preference when available; otherwise the
    /// non-explicit default language fallback.
    /// </returns>
    public LanguagePreferenceReadResult Read()
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                return CreateFallbackResult();
            }

            var json = File.ReadAllText(_filePath);
            var preferenceFile =
                JsonSerializer.Deserialize<LanguagePreferenceFile>(
                    json,
                    JsonOptions);

            if (!TryNormalizeSupportedLanguageCode(
                    preferenceFile?.LanguageCode,
                    out var languageCode))
            {
                return CreateFallbackResult();
            }

            return new LanguagePreferenceReadResult(
                HasExplicitPreference: true,
                LanguageCode: languageCode);
        }
        catch (JsonException)
        {
            return CreateFallbackResult();
        }
        catch (IOException)
        {
            return CreateFallbackResult();
        }
        catch (UnauthorizedAccessException)
        {
            return CreateFallbackResult();
        }
    }

    /// <summary>
    /// Atomically saves a supported language preference.
    /// </summary>
    /// <param name="languageCode">The selected language code.</param>
    public void Save(string languageCode)
    {
        if (!TryNormalizeSupportedLanguageCode(
                languageCode,
                out var normalizedLanguageCode))
        {
            throw new ArgumentException(
                $"Language '{languageCode}' is not supported.",
                nameof(languageCode));
        }

        var directoryPath = Path.GetDirectoryName(_filePath);

        if (!string.IsNullOrWhiteSpace(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        var temporaryFilePath =
            $"{_filePath}.{Guid.NewGuid():N}.tmp";

        try
        {
            var preferenceFile = new LanguagePreferenceFile
            {
                SchemaVersion = CurrentSchemaVersion,
                LanguageCode = normalizedLanguageCode
            };

            var json = JsonSerializer.Serialize(
                preferenceFile,
                JsonOptions);

            File.WriteAllText(temporaryFilePath, json);
            File.Move(
                temporaryFilePath,
                _filePath,
                overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryFilePath))
            {
                File.Delete(temporaryFilePath);
            }
        }
    }

    private static LanguagePreferenceReadResult CreateFallbackResult()
    {
        return new LanguagePreferenceReadResult(
            HasExplicitPreference: false,
            LanguageCode: TranslationCatalog.DefaultLanguageCode);
    }

    private static bool TryNormalizeSupportedLanguageCode(
        string? languageCode,
        out string normalizedLanguageCode)
    {
        normalizedLanguageCode =
            TranslationCatalog.DefaultLanguageCode;

        if (string.IsNullOrWhiteSpace(languageCode))
        {
            return false;
        }

        var neutralLanguageCode = languageCode
            .Trim()
            .Replace('_', '-')
            .Split(
                '-',
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries)
            .FirstOrDefault();

        if (string.IsNullOrWhiteSpace(neutralLanguageCode))
        {
            return false;
        }

        var supportedLanguage =
            TranslationCatalog.SupportedLanguages.FirstOrDefault(
                option => string.Equals(
                    option.Code,
                    neutralLanguageCode,
                    StringComparison.OrdinalIgnoreCase));

        if (supportedLanguage is null)
        {
            return false;
        }

        normalizedLanguageCode = supportedLanguage.Code;
        return true;
    }

    private sealed class LanguagePreferenceFile
    {
        public int SchemaVersion { get; init; }

        public string? LanguageCode { get; init; }
    }
}

/// <summary>
/// Provides access to the application language preference stored locally.
/// </summary>
public static class LanguagePreferenceStore
{
    private const string DirectoryName = "GlucoDesk";
    private const string FileName = "language-preferences.json";

    /// <summary>
    /// Loads the selected language code or the default fallback.
    /// </summary>
    public static string LoadLanguageCode()
    {
        return CreateDefaultStore()
            .Read()
            .LanguageCode;
    }

    /// <summary>
    /// Determines whether the user has explicitly chosen a language.
    /// </summary>
    public static bool HasExplicitLanguagePreference()
    {
        return CreateDefaultStore()
            .Read()
            .HasExplicitPreference;
    }

    /// <summary>
    /// Reads both the selected language and explicit-selection state.
    /// </summary>
    public static LanguagePreferenceReadResult ReadPreference()
    {
        return CreateDefaultStore().Read();
    }

    /// <summary>
    /// Persists the selected language code.
    /// </summary>
    public static void SaveLanguageCode(string languageCode)
    {
        CreateDefaultStore().Save(languageCode);
    }

    private static LanguagePreferenceFileStore CreateDefaultStore()
    {
        return new LanguagePreferenceFileStore(
            GetPreferenceFilePath());
    }

    private static string GetPreferenceFilePath()
    {
        var baseDirectory = Environment.GetFolderPath(
            Environment.SpecialFolder.ApplicationData);

        if (string.IsNullOrWhiteSpace(baseDirectory))
        {
            baseDirectory = AppContext.BaseDirectory;
        }

        return Path.Combine(
            baseDirectory,
            DirectoryName,
            FileName);
    }
}
