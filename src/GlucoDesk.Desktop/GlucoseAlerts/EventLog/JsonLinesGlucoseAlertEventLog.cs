using System.Text.Json;
using System.Text.Json.Serialization;
using GlucoDesk.Desktop.GlucoseAlerts.Models;

namespace GlucoDesk.Desktop.GlucoseAlerts.EventLog;

/// <summary>
/// Writes privacy-safe glucose alert events to a local JSON Lines file.
/// </summary>
public sealed class JsonLinesGlucoseAlertEventLog : IGlucoseAlertEventLog
{
    private static readonly JsonSerializerOptions SerializerOptions = CreateSerializerOptions();

    private readonly string _filePath;
    private readonly SemaphoreSlim _writeLock = new(1, 1);

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonLinesGlucoseAlertEventLog"/> class.
    /// </summary>
    /// <param name="filePath">The JSON Lines file path.</param>
    public JsonLinesGlucoseAlertEventLog(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("Event log file path is required.", nameof(filePath));
        }

        _filePath = filePath;
    }

    /// <summary>
    /// Creates a local event log using the default user application data path.
    /// </summary>
    /// <returns>The local glucose alert event log.</returns>
    public static JsonLinesGlucoseAlertEventLog CreateDefault()
    {
        var applicationDataPath = Environment.GetFolderPath(
            Environment.SpecialFolder.ApplicationData);

        var directoryPath = Path.Combine(
            applicationDataPath,
            "GlucoDesk",
            "logs");

        return new JsonLinesGlucoseAlertEventLog(
            Path.Combine(directoryPath, "glucose-alert-events.jsonl"));
    }

    /// <inheritdoc />
    public async Task WriteAsync(
        GlucoseAlertEvent eventEntry,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(eventEntry);

        var directoryPath = Path.GetDirectoryName(_filePath);

        if (!string.IsNullOrWhiteSpace(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        var line = JsonSerializer.Serialize(
            ToDocument(eventEntry),
            SerializerOptions);

        await _writeLock
            .WaitAsync(cancellationToken)
            .ConfigureAwait(false);

        try
        {
            await File.AppendAllTextAsync(
                    _filePath,
                    line + Environment.NewLine,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        finally
        {
            _writeLock.Release();
        }
    }

    #region Helpers

    /// <summary>
    /// Creates JSON serializer options.
    /// </summary>
    /// <returns>The JSON serializer options.</returns>
    private static JsonSerializerOptions CreateSerializerOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        options.Converters.Add(new JsonStringEnumConverter<GlucoseAlertEventKind>());
        options.Converters.Add(new JsonStringEnumConverter<GlucoseAlertKind>());

        return options;
    }

    /// <summary>
    /// Converts the event to a persistable document.
    /// </summary>
    /// <param name="eventEntry">The event entry.</param>
    /// <returns>The event document.</returns>
    private static GlucoseAlertEventDocument ToDocument(GlucoseAlertEvent eventEntry)
    {
        return new GlucoseAlertEventDocument(
            eventEntry.Timestamp,
            eventEntry.EventKind,
            eventEntry.AlertKind,
            SanitizeMessage(eventEntry.Message));
    }

    /// <summary>
    /// Sanitizes an event message before persistence.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <returns>The sanitized message.</returns>
    private static string SanitizeMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return "Glucose alert event.";
        }

        return message.Length <= 240
            ? message
            : message[..240];
    }

    #endregion

    private sealed record GlucoseAlertEventDocument(
        DateTimeOffset Timestamp,
        GlucoseAlertEventKind EventKind,
        GlucoseAlertKind AlertKind,
        string Message);
}
