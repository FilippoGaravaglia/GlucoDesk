namespace GlucoDesk.Infrastructure.Settings.Options;

/// <summary>
/// Represents local file-system storage options for GlucoDesk settings.
/// </summary>
public sealed record LocalSettingsStorageOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LocalSettingsStorageOptions"/> class.
    /// </summary>
    /// <param name="settingsFilePath">The settings file path.</param>
    /// <exception cref="ArgumentException">Thrown when the settings file path is invalid.</exception>
    public LocalSettingsStorageOptions(string settingsFilePath)
    {
        if (string.IsNullOrWhiteSpace(settingsFilePath))
        {
            throw new ArgumentException("Settings file path must be specified.", nameof(settingsFilePath));
        }

        SettingsFilePath = settingsFilePath.Trim();
    }

    /// <summary>
    /// Gets the default local settings storage options.
    /// </summary>
    public static LocalSettingsStorageOptions Default { get; } = new(BuildDefaultSettingsFilePath());

    /// <summary>
    /// Gets the settings file path.
    /// </summary>
    public string SettingsFilePath { get; }

    #region Helpers

    /// <summary>
    /// Builds the default cross-platform settings file path.
    /// </summary>
    /// <returns>The default settings file path.</returns>
    private static string BuildDefaultSettingsFilePath()
    {
        var applicationDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        if (string.IsNullOrWhiteSpace(applicationDataPath))
        {
            applicationDataPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }

        return Path.Combine(applicationDataPath, "GlucoDesk", "settings.json");
    }

    #endregion
}