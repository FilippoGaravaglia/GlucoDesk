using System.Text.Json;
using GlucoDesk.Desktop.GlucoseAlerts.EventLog;
using GlucoDesk.Desktop.GlucoseAlerts.Models;

namespace GlucoDesk.Desktop.Tests.GlucoseAlerts.EventLog;

public sealed class JsonLinesGlucoseAlertEventLogRotationTests
{
    [Fact]
    public async Task WriteAsync_ShouldRotateLogFile_WhenSizeLimitIsExceeded()
    {
        var filePath = BuildTempLogFilePath();
        var eventLog = new JsonLinesGlucoseAlertEventLog(
            filePath,
            maxLogFileSizeBytes: 350,
            retainedFileCount: 2);

        await WriteHighAlertAsync(eventLog, "First " + new string('A', 180));
        await WriteHighAlertAsync(eventLog, "Second " + new string('B', 180));

        Assert.True(File.Exists(filePath));
        Assert.True(File.Exists(BuildRotatedFilePath(filePath, 1)));

        var currentLines = await File.ReadAllLinesAsync(filePath);
        var rotatedLines = await File.ReadAllLinesAsync(BuildRotatedFilePath(filePath, 1));

        Assert.Single(currentLines);
        Assert.Single(rotatedLines);

        using var currentDocument = JsonDocument.Parse(currentLines[0]);

        Assert.Equal(
            "Presented",
            currentDocument.RootElement.GetProperty("eventKind").GetString());
    }

    [Fact]
    public async Task WriteAsync_ShouldKeepOnlyConfiguredRotatedFileCount()
    {
        var filePath = BuildTempLogFilePath();
        var eventLog = new JsonLinesGlucoseAlertEventLog(
            filePath,
            maxLogFileSizeBytes: 350,
            retainedFileCount: 2);

        for (var index = 0; index < 5; index++)
        {
            await WriteHighAlertAsync(
                eventLog,
                $"Event {index} " + new string('A', 180));
        }

        Assert.True(File.Exists(filePath));
        Assert.True(File.Exists(BuildRotatedFilePath(filePath, 1)));
        Assert.True(File.Exists(BuildRotatedFilePath(filePath, 2)));
        Assert.False(File.Exists(BuildRotatedFilePath(filePath, 3)));
    }

    [Fact]
    public void Constructor_ShouldRejectInvalidRotationSettings()
    {
        Assert.Throws<ArgumentException>(() => new JsonLinesGlucoseAlertEventLog(string.Empty));

        Assert.Throws<ArgumentOutOfRangeException>(() => new JsonLinesGlucoseAlertEventLog(
            BuildTempLogFilePath(),
            maxLogFileSizeBytes: 0));

        Assert.Throws<ArgumentOutOfRangeException>(() => new JsonLinesGlucoseAlertEventLog(
            BuildTempLogFilePath(),
            retainedFileCount: 0));
    }

    #region Helpers

    /// <summary>
    /// Writes a high glucose alert event.
    /// </summary>
    /// <param name="eventLog">The event log.</param>
    /// <param name="message">The event message.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static Task WriteHighAlertAsync(
        JsonLinesGlucoseAlertEventLog eventLog,
        string message)
    {
        return eventLog.WriteAsync(
            new GlucoseAlertEvent(
                DateTimeOffset.UtcNow,
                GlucoseAlertEventKind.Presented,
                GlucoseAlertKind.High,
                message),
            CancellationToken.None);
    }

    /// <summary>
    /// Builds a temporary log file path.
    /// </summary>
    /// <returns>The temporary log file path.</returns>
    private static string BuildTempLogFilePath()
    {
        var directoryPath = Path.Combine(
            Path.GetTempPath(),
            "glucodesk-tests",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(directoryPath);

        return Path.Combine(directoryPath, "glucose-alert-events.jsonl");
    }

    /// <summary>
    /// Builds the rotated log file path.
    /// </summary>
    /// <param name="filePath">The active file path.</param>
    /// <param name="index">The rotated file index.</param>
    /// <returns>The rotated file path.</returns>
    private static string BuildRotatedFilePath(
        string filePath,
        int index)
    {
        var directoryPath = Path.GetDirectoryName(filePath);
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
        var extension = Path.GetExtension(filePath);

        return Path.Combine(
            directoryPath!,
            $"{fileNameWithoutExtension}.{index}{extension}");
    }

    #endregion
}
