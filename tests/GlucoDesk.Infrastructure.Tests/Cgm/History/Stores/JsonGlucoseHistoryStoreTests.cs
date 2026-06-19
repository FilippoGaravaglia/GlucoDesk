using GlucoDesk.Application.Cgm.History.Requests;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Core.Glucose.Readings;
using GlucoDesk.Core.Glucose.ValueObjects;
using GlucoDesk.Infrastructure.Cgm.History.Options;
using GlucoDesk.Infrastructure.Cgm.History.Stores;

namespace GlucoDesk.Infrastructure.Tests.Cgm.History.Stores;

public sealed class JsonGlucoseHistoryStoreTests : IDisposable
{
    private readonly string _temporaryDirectoryPath;

    public JsonGlucoseHistoryStoreTests()
    {
        _temporaryDirectoryPath = Path.Combine(
            Path.GetTempPath(),
            "GlucoDesk.Tests",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(_temporaryDirectoryPath);
    }

    [Fact]
    public async Task GetReadingsAsync_ShouldReturnEmptyResult_WhenFileDoesNotExist()
    {
        // Arrange
        var store = CreateStore();
        var from = new DateTimeOffset(2026, 6, 8, 8, 0, 0, TimeSpan.Zero);
        var request = new GlucoseHistoryRequest(from, from.AddHours(1));

        // Act
        var result = await store.GetReadingsAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Readings);
    }

    [Fact]
    public async Task SaveReadingsAsync_ShouldCreateHistoryFile()
    {
        // Arrange
        var store = CreateStore();
        var reading = CreateReading(
            new DateTimeOffset(2026, 6, 8, 8, 0, 0, TimeSpan.Zero),
            110m);

        // Act
        var result = await store.SaveReadingsAsync([reading], CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(File.Exists(BuildHistoryFilePath()));
    }

    [Fact]
    public async Task GetReadingsAsync_ShouldReturnSavedReadingsWithinRange()
    {
        // Arrange
        var store = CreateStore();
        var from = new DateTimeOffset(2026, 6, 8, 8, 0, 0, TimeSpan.Zero);
        var firstReading = CreateReading(from.AddMinutes(5), 110m);
        var secondReading = CreateReading(from.AddMinutes(10), 120m);
        var outOfRangeReading = CreateReading(from.AddHours(2), 130m);

        await store.SaveReadingsAsync(
            [
                outOfRangeReading,
                secondReading,
                firstReading
            ],
            CancellationToken.None);

        // Act
        var result = await store.GetReadingsAsync(
            new GlucoseHistoryRequest(from, from.AddHours(1)),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
        Assert.Equal(110m, result.Value.Readings.First().Value.Amount);
        Assert.Equal(120m, result.Value.Readings.Last().Value.Amount);
    }

    [Fact]
    public async Task SaveReadingsAsync_ShouldDeduplicateReadingsByTimestampAndProvider()
    {
        // Arrange
        var store = CreateStore();
        var timestamp = new DateTimeOffset(2026, 6, 8, 8, 0, 0, TimeSpan.Zero);

        await store.SaveReadingsAsync([CreateReading(timestamp, 110m)], CancellationToken.None);
        await store.SaveReadingsAsync([CreateReading(timestamp, 115m)], CancellationToken.None);

        // Act
        var result = await store.GetReadingsAsync(
            new GlucoseHistoryRequest(timestamp.AddMinutes(-1), timestamp.AddMinutes(1)),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Readings);
        Assert.Equal(115m, result.Value.Readings.Single().Value.Amount);
    }

    [Fact]
    public async Task GetReadingsAsync_ShouldReturnEmptyHistory_AndQuarantineFile_WhenJsonIsInvalid()
    {
        // Arrange
        var historyFilePath = BuildHistoryFilePath();

        await File.WriteAllTextAsync(
            historyFilePath,
            "{ invalid-json",
            CancellationToken.None);

        var store = CreateStore();

        var request = new GlucoseHistoryRequest(
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow);

        // Act
        var result = await store.GetReadingsAsync(
            request,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Readings);

        Assert.False(File.Exists(historyFilePath));

        var quarantinedFiles = Directory.GetFiles(
            _temporaryDirectoryPath,
            "glucose-history.json.corrupt.*");

        Assert.Single(quarantinedFiles);
    }

    [Fact]
    public async Task GetReadingsAsync_ShouldRecoverFromBackup_WhenPrimaryJsonIsInvalid()
    {
        // Arrange
        var historyFilePath = BuildHistoryFilePath();
        var backupFilePath = $"{historyFilePath}.bak";

        var timestamp = new DateTimeOffset(
            2026,
            6,
            18,
            10,
            0,
            0,
            TimeSpan.Zero);

        var backupJson = """
        [
          {
            "timestamp": "2026-06-18T10:00:00+00:00",
            "amount": 120,
            "unit": "MgDl",
            "trend": "Flat",
            "providerKind": "DexcomShare",
            "freshness": "NearRealTime"
          }
        ]
        """;

        await File.WriteAllTextAsync(
            historyFilePath,
            "{ invalid-json",
            CancellationToken.None);

        await File.WriteAllTextAsync(
            backupFilePath,
            backupJson,
            CancellationToken.None);

        var store = CreateStore();

        var request = new GlucoseHistoryRequest(
            timestamp.AddMinutes(-5),
            timestamp.AddMinutes(5));

        // Act
        var result = await store.GetReadingsAsync(
            request,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        var reading = Assert.Single(result.Value.Readings);

        Assert.Equal(timestamp, reading.Timestamp);
        Assert.Equal(120m, reading.Value.Amount);
        Assert.Equal(GlucoseUnit.MgDl, reading.Value.Unit);
        Assert.Equal(TrendDirection.Flat, reading.Trend);
        Assert.Equal(CgmProviderKind.DexcomShare, reading.Provider);
        Assert.Equal(GlucoseDataFreshness.NearRealTime, reading.Freshness);

        Assert.True(File.Exists(historyFilePath));
    }

    [Fact]
    public async Task SaveReadingsWithSummaryAsync_ShouldReturnDetailedSummary_WhenNewReadingsAreSaved()
    {
        // Arrange
        var store = CreateStore();

        var firstReading = CreateReading(
            new DateTimeOffset(2026, 6, 8, 8, 0, 0, TimeSpan.Zero),
            110m);

        var secondReading = CreateReading(
            new DateTimeOffset(2026, 6, 8, 8, 5, 0, TimeSpan.Zero),
            115m);

        // Act
        var result = await store.SaveReadingsWithSummaryAsync(
            [firstReading, secondReading],
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(CgmProviderKind.DexcomShare, result.Value.ProviderKind);
        Assert.Equal(2, result.Value.IncomingReadingsCount);
        Assert.Equal(2, result.Value.AddedReadingsCount);
        Assert.Equal(0, result.Value.DuplicateReadingsCount);
        Assert.Equal(2, result.Value.StoredReadingsCount);
        Assert.True(result.Value.HasNewReadings);
    }

    [Fact]
    public async Task SaveReadingsWithSummaryAsync_ShouldCountDuplicates_WhenReadingsAlreadyExist()
    {
        // Arrange
        var store = CreateStore();
        var timestamp = new DateTimeOffset(2026, 6, 8, 8, 0, 0, TimeSpan.Zero);

        var firstReading = CreateReading(timestamp, 110m);
        var duplicateReading = CreateReading(timestamp, 115m);

        await store.SaveReadingsWithSummaryAsync(
            [firstReading],
            CancellationToken.None);

        // Act
        var result = await store.SaveReadingsWithSummaryAsync(
            [duplicateReading],
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(CgmProviderKind.DexcomShare, result.Value.ProviderKind);
        Assert.Equal(1, result.Value.IncomingReadingsCount);
        Assert.Equal(0, result.Value.AddedReadingsCount);
        Assert.Equal(1, result.Value.DuplicateReadingsCount);
        Assert.Equal(1, result.Value.StoredReadingsCount);
        Assert.False(result.Value.HasNewReadings);

        var readingsResult = await store.GetReadingsAsync(
            new GlucoseHistoryRequest(timestamp.AddMinutes(-1), timestamp.AddMinutes(1)),
            CancellationToken.None);

        Assert.True(readingsResult.IsSuccess);
        Assert.Single(readingsResult.Value.Readings);
        Assert.Equal(115m, readingsResult.Value.Readings.Single().Value.Amount);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        DeleteDirectorySafely(_temporaryDirectoryPath);
    }

    #region Helpers

    /// <summary>
    /// Creates a JSON glucose history store for the current test.
    /// </summary>
    /// <returns>The JSON glucose history store.</returns>
    private JsonGlucoseHistoryStore CreateStore()
    {
        return new JsonGlucoseHistoryStore(
            new LocalGlucoseHistoryStorageOptions(BuildHistoryFilePath()));
    }

    /// <summary>
    /// Builds the history file path for the current test.
    /// </summary>
    /// <returns>The history file path.</returns>
    private string BuildHistoryFilePath()
    {
        return Path.Combine(_temporaryDirectoryPath, "glucose-history.json");
    }

    /// <summary>
    /// Creates a glucose reading for tests.
    /// </summary>
    /// <param name="timestamp">The reading timestamp.</param>
    /// <param name="valueMgDl">The glucose value in mg/dL.</param>
    /// <returns>The glucose reading.</returns>
    private static GlucoseReading CreateReading(
        DateTimeOffset timestamp,
        decimal valueMgDl)
    {
        return new GlucoseReading(
            timestamp,
            new GlucoseValue(valueMgDl, GlucoseUnit.MgDl),
            TrendDirection.Flat,
            CgmProviderKind.DexcomShare,
            GlucoseDataFreshness.NearRealTime);
    }

    /// <summary>
    /// Deletes a directory without failing the test when cleanup is not possible.
    /// </summary>
    /// <param name="directoryPath">The directory path.</param>
    private static void DeleteDirectorySafely(string directoryPath)
    {
        try
        {
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, recursive: true);
            }
        }
        catch
        {
            // Test cleanup is best-effort.
        }
    }

    #endregion
}