using GlucoDesk.Infrastructure.Storage;

namespace GlucoDesk.Infrastructure.Settings.Options;

/// <summary>
/// Represents local file-system storage options for GlucoDesk settings.
/// </summary>
public sealed record LocalSettingsStorageOptions
{
    private const string DefaultSettingsFileName = "settings.json";

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalSettingsStorageOptions"/> class.
    /// </summary>
    /// <param name="settingsFilePath">The settings file path.</param>
    /// <exception cref="ArgumentException">Thrown when the settings file path is invalid.</exception>
    public LocalSettingsStorageOptions(string settingsFilePath)
    {
        if (string.IsNullOrWhiteSpace(settingsFilePath))
        {
            throw new ArgumentException(
                "Settings file path must be specified.",
                nameof(settingsFilePath));
        }

        SettingsFilePath = settingsFilePath.Trim();
    }

    /// <summary>
    /// Gets the default local settings storage options.
    /// </summary>
    public static LocalSettingsStorageOptions Default { get; } = new(
        LocalApplicationDataDirectory.GetFilePath(DefaultSettingsFileName));

    /// <summary>
    /// Gets the settings file path.
    /// </summary>
    public string SettingsFilePath { get; }
}