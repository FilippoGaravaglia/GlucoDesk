using GlucoDesk.Desktop.DesktopPresence.Services.Abstractions;

namespace GlucoDesk.Desktop.DesktopPresence.Services;

/// <summary>
/// File-backed privacy mode store for desktop presence.
/// </summary>
public sealed class FileDesktopPresencePrivacyModeStore : IDesktopPresencePrivacyModeStore
{
    private const string EnabledValue = "true";
    private const string DisabledValue = "false";

    private readonly string _filePath;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileDesktopPresencePrivacyModeStore"/> class.
    /// </summary>
    public FileDesktopPresencePrivacyModeStore()
        : this(CreateDefaultFilePath())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileDesktopPresencePrivacyModeStore"/> class.
    /// </summary>
    /// <param name="filePath">The privacy mode state file path.</param>
    public FileDesktopPresencePrivacyModeStore(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("Privacy mode state file path cannot be empty.", nameof(filePath));
        }

        _filePath = filePath;
    }

    /// <inheritdoc />
    public bool Load()
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                return false;
            }

            var value = File.ReadAllText(_filePath).Trim();

            return string.Equals(value, EnabledValue, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc />
    public void Save(bool isEnabled)
    {
        try
        {
            var directoryPath = Path.GetDirectoryName(_filePath);

            if (!string.IsNullOrWhiteSpace(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var temporaryFilePath = $"{_filePath}.{Guid.NewGuid():N}.tmp";
            var value = isEnabled ? EnabledValue : DisabledValue;

            File.WriteAllText(temporaryFilePath, value);
            File.Move(temporaryFilePath, _filePath, overwrite: true);
        }
        catch
        {
            // Privacy persistence is best-effort and must never break the desktop companion.
        }
    }

    /// <summary>
    /// Creates the default privacy mode state file path.
    /// </summary>
    /// <returns>The default privacy mode state file path.</returns>
    private static string CreateDefaultFilePath()
    {
        var applicationDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        if (string.IsNullOrWhiteSpace(applicationDataPath))
        {
            applicationDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        }

        if (string.IsNullOrWhiteSpace(applicationDataPath))
        {
            applicationDataPath = AppContext.BaseDirectory;
        }

        return Path.Combine(
            applicationDataPath,
            "GlucoDesk",
            "desktop-presence-privacy-mode.txt");
    }
}
