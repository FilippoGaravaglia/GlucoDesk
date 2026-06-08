namespace GlucoDesk.Infrastructure.Cgm.History.Options;

/// <summary>
/// Represents local file-system storage options for glucose history.
/// </summary>
public sealed record LocalGlucoseHistoryStorageOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LocalGlucoseHistoryStorageOptions"/> class.
    /// </summary>
    /// <param name="historyFilePath">The glucose history file path.</param>
    /// <exception cref="ArgumentException">Thrown when the history file path is invalid.</exception>
    public LocalGlucoseHistoryStorageOptions(string historyFilePath)
    {
        if (string.IsNullOrWhiteSpace(historyFilePath))
        {
            throw new ArgumentException("History file path must be specified.", nameof(historyFilePath));
        }

        HistoryFilePath = historyFilePath.Trim();
    }

    /// <summary>
    /// Gets the default local glucose history storage options.
    /// </summary>
    public static LocalGlucoseHistoryStorageOptions Default { get; } = new(BuildDefaultHistoryFilePath());

    /// <summary>
    /// Gets the glucose history file path.
    /// </summary>
    public string HistoryFilePath { get; }

    #region Helpers

    /// <summary>
    /// Builds the default cross-platform glucose history file path.
    /// </summary>
    /// <returns>The default glucose history file path.</returns>
    private static string BuildDefaultHistoryFilePath()
    {
        var localApplicationDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        if (string.IsNullOrWhiteSpace(localApplicationDataPath))
        {
            localApplicationDataPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }

        return Path.Combine(localApplicationDataPath, "GlucoDesk", "history", "glucose-readings.json");
    }

    #endregion
}