namespace GlucoDesk.Desktop.Localization;

/// <summary>
/// Applies localized strings to Avalonia dynamic resources and persists the selected language.
/// </summary>
public static class LocalizationManager
{
    private static bool isInitialized;
    private static string currentLanguageCode = TranslationCatalog.DefaultLanguageCode;

    /// <summary>
    /// Raised when the application language changes.
    /// </summary>
    public static event EventHandler? LanguageChanged;

    /// <summary>
    /// Gets the current language code.
    /// </summary>
    public static string CurrentLanguageCode => currentLanguageCode;

    /// <summary>
    /// Gets the supported languages.
    /// </summary>
    public static IReadOnlyList<AppLanguageOption> AvailableLanguages => TranslationCatalog.SupportedLanguages;

    /// <summary>
    /// Initializes localization once using the persisted preference.
    /// </summary>
    public static void InitializeIfNeeded()
    {
        if (isInitialized)
        {
            return;
        }

        currentLanguageCode = LanguagePreferenceStore.LoadLanguageCode();
        ApplyLanguageResources(currentLanguageCode);
        isInitialized = true;
    }

    /// <summary>
    /// Changes the current language and persists the preference.
    /// </summary>
    /// <param name="languageCode">The requested language code.</param>
    public static void SetLanguage(string languageCode)
    {
        InitializeIfNeeded();

        var normalizedLanguageCode = TranslationCatalog.NormalizeLanguageCode(languageCode);

        if (string.Equals(
                currentLanguageCode,
                normalizedLanguageCode,
                StringComparison.Ordinal))
        {
            ApplyLanguageResources(currentLanguageCode);
            return;
        }

        currentLanguageCode = normalizedLanguageCode;
        LanguagePreferenceStore.SaveLanguageCode(currentLanguageCode);
        ApplyLanguageResources(currentLanguageCode);
        LanguageChanged?.Invoke(null, EventArgs.Empty);
    }

    /// <summary>
    /// Changes the application language for the current process without
    /// persisting the preference to disk.
    /// </summary>
    /// <param name="languageCode">The requested language code.</param>
    public static void SetLanguageForCurrentProcess(string languageCode)
    {
        var normalizedLanguageCode =
            TranslationCatalog.NormalizeLanguageCode(languageCode);

        var hasLanguageChanged = !string.Equals(
            currentLanguageCode,
            normalizedLanguageCode,
            StringComparison.Ordinal);

        currentLanguageCode = normalizedLanguageCode;
        isInitialized = true;

        ApplyLanguageResources(currentLanguageCode);

        if (hasLanguageChanged)
        {
            LanguageChanged?.Invoke(null, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Gets the supported language option for the given code.
    /// </summary>
    /// <param name="languageCode">The requested language code.</param>
    /// <returns>The matching language option.</returns>
    public static AppLanguageOption GetLanguageOption(string? languageCode)
    {
        var normalizedLanguageCode = TranslationCatalog.NormalizeLanguageCode(languageCode);

        return AvailableLanguages.FirstOrDefault(language =>
                string.Equals(
                    language.Code,
                    normalizedLanguageCode,
                    StringComparison.Ordinal))
            ?? AvailableLanguages[0];
    }

    /// <summary>
    /// Translates a key using the current language.
    /// </summary>
    /// <param name="key">The translation key.</param>
    /// <returns>The localized text.</returns>
    public static string GetString(string key)
    {
        InitializeIfNeeded();

        return TranslationCatalog.Translate(currentLanguageCode, key);
    }

    private static void ApplyLanguageResources(string languageCode)
    {
        if (global::Avalonia.Application.Current is null)
        {
            return;
        }

        var translations = TranslationCatalog.GetTranslations(languageCode);

        foreach (var translation in translations)
        {
            global::Avalonia.Application.Current.Resources[translation.Key] = translation.Value;
        }
    }
}
