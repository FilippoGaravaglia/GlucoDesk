using System.Text.Json;

namespace GlucoDesk.Desktop.Onboarding;

/// <summary>
/// Stores the completion state of the first-run feature tour locally.
/// </summary>
public sealed class FeatureTourPreferenceStore
{
    /// <summary>
    /// Gets the current tour content version.
    /// Increment this value only when every user should see a substantially
    /// new onboarding experience.
    /// </summary>
    public const int CurrentTourVersion = 1;

    private static readonly JsonSerializerOptions SerializerOptions =
        new(JsonSerializerDefaults.Web)
        {
            WriteIndented = true
        };

    private readonly string _filePath;

    /// <summary>
    /// Initializes a new store using the provided file path.
    /// </summary>
    /// <param name="filePath">The full local state file path.</param>
    public FeatureTourPreferenceStore(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException(
                "A feature tour preference file path is required.",
                nameof(filePath));
        }

        _filePath = filePath;
    }

    /// <summary>
    /// Creates the production store.
    /// </summary>
    /// <returns>The default local feature tour store.</returns>
    public static FeatureTourPreferenceStore CreateDefault()
    {
        var applicationDataDirectory =
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData);

        if (string.IsNullOrWhiteSpace(applicationDataDirectory))
        {
            applicationDataDirectory = AppContext.BaseDirectory;
        }

        return new FeatureTourPreferenceStore(
            Path.Combine(
                applicationDataDirectory,
                "GlucoDesk",
                "onboarding",
                "feature-tour.json"));
    }

    /// <summary>
    /// Returns whether the current feature tour has already been completed.
    /// Invalid or unreadable files are treated as incomplete.
    /// </summary>
    public bool HasCompletedCurrentTour()
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                return false;
            }

            var json = File.ReadAllText(_filePath);

            var document =
                JsonSerializer.Deserialize<FeatureTourPreferenceDocument>(
                    json,
                    SerializerOptions);

            return document is not null
                && document.Completed
                && document.TourVersion >= CurrentTourVersion;
        }
        catch (Exception exception)
            when (exception is IOException
                or UnauthorizedAccessException
                or JsonException
                or NotSupportedException)
        {
            return false;
        }
    }

    /// <summary>
    /// Persists completion of the current feature tour using an atomic write.
    /// </summary>
    public void MarkCurrentTourCompleted()
    {
        var directoryPath = Path.GetDirectoryName(_filePath);

        if (!string.IsNullOrWhiteSpace(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        var temporaryFilePath =
            $"{_filePath}.{Guid.NewGuid():N}.tmp";

        try
        {
            var document = new FeatureTourPreferenceDocument
            {
                Completed = true,
                TourVersion = CurrentTourVersion,
                CompletedAtUtc = DateTimeOffset.UtcNow
            };

            var json = JsonSerializer.Serialize(
                document,
                SerializerOptions);

            File.WriteAllText(
                temporaryFilePath,
                json);

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

    private sealed record FeatureTourPreferenceDocument
    {
        public bool Completed { get; init; }

        public int TourVersion { get; init; }

        public DateTimeOffset? CompletedAtUtc { get; init; }
    }
}
