namespace GlucoDesk.Infrastructure.Storage;

/// <summary>
/// Resolves GlucoDesk local application data paths in a platform-aware way.
/// </summary>
public static class LocalApplicationDataDirectory
{
    private const string ApplicationDirectoryName = "GlucoDesk";

    /// <summary>
    /// Gets the GlucoDesk local application data directory path.
    /// </summary>
    /// <returns>The platform-specific local application data directory path.</returns>
    public static string GetApplicationDirectoryPath()
    {
        return Path.Combine(
            GetBaseDirectoryPath(),
            ApplicationDirectoryName);
    }

    /// <summary>
    /// Gets a file path inside the GlucoDesk local application data directory.
    /// </summary>
    /// <param name="fileName">The local data file name.</param>
    /// <returns>The platform-specific file path.</returns>
    public static string GetFilePath(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException(
                "Local data file name cannot be empty.",
                nameof(fileName));
        }

        return Path.Combine(
            GetApplicationDirectoryPath(),
            fileName.Trim());
    }

    #region Helpers

    /// <summary>
    /// Gets the platform-specific base data directory.
    /// </summary>
    /// <returns>The base data directory.</returns>
    private static string GetBaseDirectoryPath()
    {
        if (OperatingSystem.IsWindows())
        {
            return GetKnownFolderOrFallback(
                Environment.SpecialFolder.LocalApplicationData,
                "LOCALAPPDATA");
        }

        return GetKnownFolderOrFallback(
            Environment.SpecialFolder.ApplicationData,
            "HOME");
    }

    /// <summary>
    /// Gets a known folder path or falls back to an environment variable or temporary directory.
    /// </summary>
    /// <param name="folder">The known folder.</param>
    /// <param name="fallbackEnvironmentVariableName">The fallback environment variable name.</param>
    /// <returns>The resolved directory path.</returns>
    private static string GetKnownFolderOrFallback(
        Environment.SpecialFolder folder,
        string fallbackEnvironmentVariableName)
    {
        var knownFolderPath = Environment.GetFolderPath(folder);

        if (!string.IsNullOrWhiteSpace(knownFolderPath))
        {
            return knownFolderPath;
        }

        var environmentPath = Environment.GetEnvironmentVariable(fallbackEnvironmentVariableName);

        if (!string.IsNullOrWhiteSpace(environmentPath))
        {
            return environmentPath;
        }

        return Path.GetTempPath();
    }

    #endregion
}