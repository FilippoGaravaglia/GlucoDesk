using System.Text.Json;
using GlucoDesk.Application.Updates;
using GlucoDesk.Core.Updates;
using GlucoDesk.Infrastructure.Storage;

namespace GlucoDesk.Infrastructure.Updates;

/// <summary>
/// Stores update preferences in the local GlucoDesk application data directory.
/// </summary>
public sealed class JsonUpdatePreferencesStore :
    IUpdatePreferencesStore
{
    private const string FileName = "update-settings.json";

    private static readonly JsonSerializerOptions SerializerOptions =
        new(JsonSerializerDefaults.Web)
        {
            WriteIndented = true
        };

    private readonly string _filePath;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="JsonUpdatePreferencesStore"/> class.
    /// </summary>
    public JsonUpdatePreferencesStore()
        : this(LocalApplicationDataDirectory.GetFilePath(FileName))
    {
    }

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="JsonUpdatePreferencesStore"/> class.
    /// </summary>
    /// <param name="filePath">
    /// File used to persist update preferences.
    /// </param>
    public JsonUpdatePreferencesStore(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException(
                "Update preferences file path must be specified.",
                nameof(filePath));
        }

        _filePath = filePath;
    }

    /// <inheritdoc />
    public async Task<UpdatePreferences> LoadAsync(
        CancellationToken cancellationToken)
    {
        if (!File.Exists(_filePath))
        {
            return new UpdatePreferences();
        }

        try
        {
            await using var stream = File.OpenRead(_filePath);

            return await JsonSerializer.DeserializeAsync<UpdatePreferences>(
                       stream,
                       SerializerOptions,
                       cancellationToken)
                   .ConfigureAwait(false)
                   ?? new UpdatePreferences();
        }
        catch (JsonException)
        {
            return new UpdatePreferences();
        }
        catch (IOException)
        {
            return new UpdatePreferences();
        }
        catch (UnauthorizedAccessException)
        {
            return new UpdatePreferences();
        }
    }

    /// <inheritdoc />
    public async Task SaveAsync(
        UpdatePreferences preferences,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(preferences);

        var directory = Path.GetDirectoryName(_filePath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var temporaryPath = $"{_filePath}.tmp";

        await using (var stream = File.Create(temporaryPath))
        {
            await JsonSerializer.SerializeAsync(
                    stream,
                    preferences,
                    SerializerOptions,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        File.Move(
            temporaryPath,
            _filePath,
            overwrite: true);
    }
}
