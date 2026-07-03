using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using GlucoDesk.Desktop.GlucoseAlerts.Models;

namespace GlucoDesk.Desktop.GlucoseAlerts.EventLog;

/// <summary>
/// Writes privacy-safe glucose alert events to a local JSON Lines file with bounded retention.
/// </summary>
public sealed class JsonLinesGlucoseAlertEventLog : IGlucoseAlertEventLog
{
    private const long DefaultMaxLogFileSizeBytes = 1_048_576;
    private const int DefaultRetainedFileCount = 3;

    private static readonly JsonSerializerOptions SerializerOptions = CreateSerializerOptions();

    private readonly string _filePath;
    private readonly long _maxLogFileSizeBytes;
    private readonly int _retainedFileCount;
    private readonly SemaphoreSlim _writeLock = new(1, 1);

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonLinesGlucoseAlertEventLog"/> class.
    /// </summary>
    /// <param name="filePath">The JSON Lines file path.</param>
    /// <param name="maxLogFileSizeBytes">The maximum active log file size in bytes before rotation.</param>
    /// <param name="retainedFileCount">The number of rotated log files to retain.</param>
    public JsonLinesGlucoseAlertEventLog(
        string filePath,
        long maxLogFileSizeBytes = DefaultMaxLogFileSizeBytes,
        int retainedFileCount = DefaultRetainedFileCount)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("Event log file path is required.", nameof(filePath));
        }

        if (maxLogFileSizeBytes <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxLogFileSizeBytes),
                maxLogFileSizeBytes,
                "Maximum log file size must be greater than zero.");
        }

        if (retainedFileCount < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(retainedFileCount),
                retainedFileCount,
                "At least one rotated log file must be retained.");
        }

        _filePath = filePath;
        _maxLogFileSizeBytes = maxLogFileSizeBytes;
        _retainedFileCount = retainedFileCount;
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
            RotateIfNeeded(line);

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

    /// <summary>
    /// Rotates the active log file when appending the next line would exceed the configured size limit.
    /// </summary>
    /// <param name="nextLine">The next line to append.</param>
    private void RotateIfNeeded(string nextLine)
    {
        if (!File.Exists(_filePath))
        {
            return;
        }

        var currentSize = new FileInfo(_filePath).Length;
        var nextLineSize = Encoding.UTF8.GetByteCount(nextLine + Environment.NewLine);

        if (currentSize + nextLineSize <= _maxLogFileSizeBytes)
        {
            return;
        }

        RotateFiles();
    }

    /// <summary>
    /// Rotates local log files while keeping only the configured number of retained files.
    /// </summary>
    private void RotateFiles()
    {
        var oldestFilePath = BuildRotatedFilePath(_retainedFileCount);

        if (File.Exists(oldestFilePath))
        {
            File.Delete(oldestFilePath);
        }

        for (var index = _retainedFileCount - 1; index >= 1; index--)
        {
            var sourcePath = BuildRotatedFilePath(index);
            var destinationPath = BuildRotatedFilePath(index + 1);

            if (File.Exists(sourcePath))
            {
                File.Move(sourcePath, destinationPath, overwrite: true);
            }
        }

        if (File.Exists(_filePath))
        {
            File.Move(_filePath, BuildRotatedFilePath(1), overwrite: true);
        }
    }

    /// <summary>
    /// Builds a rotated log file path.
    /// </summary>
    /// <param name="index">The rotated log index.</param>
    /// <returns>The rotated log file path.</returns>
    private string BuildRotatedFilePath(int index)
    {
        var directoryPath = Path.GetDirectoryName(_filePath);
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(_filePath);
        var extension = Path.GetExtension(_filePath);
        var rotatedFileName = $"{fileNameWithoutExtension}.{index}{extension}";

        return string.IsNullOrWhiteSpace(directoryPath)
            ? rotatedFileName
            : Path.Combine(directoryPath, rotatedFileName);
    }

    #endregion

    private sealed record GlucoseAlertEventDocument(
        DateTimeOffset Timestamp,
        GlucoseAlertEventKind EventKind,
        GlucoseAlertKind AlertKind,
        string Message);
}
