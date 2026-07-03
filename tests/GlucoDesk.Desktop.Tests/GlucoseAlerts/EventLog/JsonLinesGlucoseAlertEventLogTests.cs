using System.Text.Json;
using GlucoDesk.Desktop.GlucoseAlerts.EventLog;
using GlucoDesk.Desktop.GlucoseAlerts.Models;

namespace GlucoDesk.Desktop.Tests.GlucoseAlerts.EventLog;

public sealed class JsonLinesGlucoseAlertEventLogTests
{
    [Fact]
    public async Task WriteAsync_ShouldCreateJsonLine()
    {
        var filePath = Path.Combine(
            Path.GetTempPath(),
            "glucodesk-tests",
            $"{Guid.NewGuid():N}.jsonl");

        var eventLog = new JsonLinesGlucoseAlertEventLog(filePath);

        await eventLog.WriteAsync(
            new GlucoseAlertEvent(
                new DateTimeOffset(2026, 7, 3, 10, 0, 0, TimeSpan.Zero),
                GlucoseAlertEventKind.Presented,
                GlucoseAlertKind.High,
                "Glucose alert banner presented."),
            CancellationToken.None);

        var lines = await File.ReadAllLinesAsync(filePath);

        Assert.Single(lines);

        using var document = JsonDocument.Parse(lines[0]);

        Assert.Equal("Presented", document.RootElement.GetProperty("eventKind").GetString());
        Assert.Equal("High", document.RootElement.GetProperty("alertKind").GetString());
        Assert.Equal("Glucose alert banner presented.", document.RootElement.GetProperty("message").GetString());
    }

    [Fact]
    public async Task WriteAsync_ShouldAppendMultipleEvents()
    {
        var filePath = Path.Combine(
            Path.GetTempPath(),
            "glucodesk-tests",
            $"{Guid.NewGuid():N}.jsonl");

        var eventLog = new JsonLinesGlucoseAlertEventLog(filePath);

        await eventLog.WriteAsync(
            new GlucoseAlertEvent(
                DateTimeOffset.UtcNow,
                GlucoseAlertEventKind.Presented,
                GlucoseAlertKind.Low,
                "Presented."),
            CancellationToken.None);

        await eventLog.WriteAsync(
            new GlucoseAlertEvent(
                DateTimeOffset.UtcNow,
                GlucoseAlertEventKind.Dismissed,
                GlucoseAlertKind.Low,
                "Dismissed."),
            CancellationToken.None);

        var lines = await File.ReadAllLinesAsync(filePath);

        Assert.Equal(2, lines.Length);
    }

    [Fact]
    public async Task WriteAsync_ShouldTruncateLongMessages()
    {
        var filePath = Path.Combine(
            Path.GetTempPath(),
            "glucodesk-tests",
            $"{Guid.NewGuid():N}.jsonl");

        var eventLog = new JsonLinesGlucoseAlertEventLog(filePath);
        var longMessage = new string('A', 300);

        await eventLog.WriteAsync(
            new GlucoseAlertEvent(
                DateTimeOffset.UtcNow,
                GlucoseAlertEventKind.NativeNotificationRequested,
                GlucoseAlertKind.High,
                longMessage),
            CancellationToken.None);

        var line = (await File.ReadAllLinesAsync(filePath)).Single();

        using var document = JsonDocument.Parse(line);

        Assert.Equal(240, document.RootElement.GetProperty("message").GetString()?.Length);
    }
}
