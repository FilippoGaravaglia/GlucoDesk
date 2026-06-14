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
    }

    [Fact]
    public async Task GetReadingsAsync_ShouldReturnEmptyResult_WhenFileDoesNotExist()
    {
        var store = CreateStore();
        var from = new DateTimeOffset(2026, 6, 8, 8, 0, 0, TimeSpan.Zero);
        var request = new GlucoseHistoryRequest(from, from.AddHours(1));

        var result = await store.GetReadingsAsync(request, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Readings);
    }

    [Fact]
    public async Task SaveReadingsAsync_ShouldCreateHistoryFile()
    {
        var store = CreateStore();
        var reading = CreateReading(new DateTimeOffset(2026, 6, 8, 8, 0, 0, TimeSpan.Zero), 110);

        var result = await store.SaveReadingsAsync([reading], CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(File.Exists(BuildHistoryFilePath()));
    }

    [Fact]
    public async Task GetReadingsAsync_ShouldReturnSavedReadingsWithinRange()
    {
        var store = CreateStore();
        var from = new DateTimeOffset(2026, 6, 8, 8, 0, 0, TimeSpan.Zero);
        var firstReading = CreateReading(from.AddMinutes(5), 110);
        var secondReading = CreateReading(from.AddMinutes(10), 120);
        var outOfRangeReading = CreateReading(from.AddHours(2), 130);

        await store.SaveReadingsAsync(
            [
                outOfRangeReading,
                secondReading,
                firstReading
            ],
            CancellationToken.None);

        var result = await store.GetReadingsAsync(
            new GlucoseHistoryRequest(from, from.AddHours(1)),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
        Assert.Equal(110, result.Value.Readings.First().Value.Amount);
        Assert.Equal(120, result.Value.Readings.Last().Value.Amount);
    }

    [Fact]
    public async Task SaveReadingsAsync_ShouldDeduplicateReadingsByTimestampAndProvider()
    {
        var store = CreateStore();
        var timestamp = new DateTimeOffset(2026, 6, 8, 8, 0, 0, TimeSpan.Zero);

        await store.SaveReadingsAsync([CreateReading(timestamp, 110)], CancellationToken.None);
        await store.SaveReadingsAsync([CreateReading(timestamp, 115)], CancellationToken.None);

        var result = await store.GetReadingsAsync(
            new GlucoseHistoryRequest(timestamp.AddMinutes(-1), timestamp.AddMinutes(1)),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Readings);
        Assert.Equal(115, result.Value.Readings.Single().Value.Amount);
    }

    [Fact]
    public async Task GetReadingsAsync_ShouldReturnFailure_WhenJsonIsInvalid()
    {
        Directory.CreateDirectory(_temporaryDirectoryPath);
        await File.WriteAllTextAsync(BuildHistoryFilePath(), "{ invalid json");

        var store = CreateStore();
        var from = new DateTimeOffset(2026, 6, 8, 8, 0, 0, TimeSpan.Zero);

        var result = await store.GetReadingsAsync(
            new GlucoseHistoryRequest(from, from.AddHours(1)),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("History.InvalidFormat", result.Error.Code);
    }

    [Fact]
    public async Task SaveReadingsWithSummaryAsync_ShouldReturnAddedCount_WhenReadingsAreNew()
    {
        var options = CreateOptions();
        var store = new JsonGlucoseHistoryStore(options);

        var readings = new[]
        {
            CreateReading(new DateTimeOffset(2026, 6, 14, 10, 0, 0, TimeSpan.Zero), CgmProviderKind.Nightscout),
            CreateReading(new DateTimeOffset(2026, 6, 14, 10, 5, 0, TimeSpan.Zero), CgmProviderKind.Nightscout)
        };

        var result = await store.SaveReadingsWithSummaryAsync(readings, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(CgmProviderKind.Nightscout, result.Value.ProviderKind);
        Assert.Equal(2, result.Value.IncomingReadingsCount);
        Assert.Equal(2, result.Value.AddedReadingsCount);
        Assert.Equal(0, result.Value.DuplicateReadingsCount);
        Assert.Equal(2, result.Value.StoredReadingsCount);
    }

    [Fact]
    public async Task SaveReadingsWithSummaryAsync_ShouldReturnDuplicateCount_WhenReadingsAlreadyExist()
    {
        var options = CreateOptions();
        var store = new JsonGlucoseHistoryStore(options);

        var readings = new[]
        {
            CreateReading(new DateTimeOffset(2026, 6, 14, 10, 0, 0, TimeSpan.Zero), CgmProviderKind.Nightscout)
        };

        var firstResult = await store.SaveReadingsWithSummaryAsync(readings, CancellationToken.None);
        var secondResult = await store.SaveReadingsWithSummaryAsync(readings, CancellationToken.None);

        Assert.True(firstResult.IsSuccess);
        Assert.True(secondResult.IsSuccess);
        Assert.Equal(1, secondResult.Value.IncomingReadingsCount);
        Assert.Equal(0, secondResult.Value.AddedReadingsCount);
        Assert.Equal(1, secondResult.Value.DuplicateReadingsCount);
        Assert.Equal(1, secondResult.Value.StoredReadingsCount);
    }

    [Fact]
    public async Task SaveReadingsWithSummaryAsync_ShouldKeepMockAndNightscoutReadingsSeparate()
    {
        var options = CreateOptions();
        var store = new JsonGlucoseHistoryStore(options);

        var timestamp = new DateTimeOffset(2026, 6, 14, 10, 0, 0, TimeSpan.Zero);

        var readings = new[]
        {
            CreateReading(timestamp, CgmProviderKind.Mock),
            CreateReading(timestamp, CgmProviderKind.Nightscout)
        };

        var result = await store.SaveReadingsWithSummaryAsync(readings, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(CgmProviderKind.Unknown, result.Value.ProviderKind);
        Assert.Equal(2, result.Value.IncomingReadingsCount);
        Assert.Equal(2, result.Value.AddedReadingsCount);
        Assert.Equal(0, result.Value.DuplicateReadingsCount);
        Assert.Equal(2, result.Value.StoredReadingsCount);
    }

    public void Dispose()
    {
        if (Directory.Exists(_temporaryDirectoryPath))
        {
            Directory.Delete(_temporaryDirectoryPath, recursive: true);
        }
    }

    #region Helpers

    /// <summary>
    /// Creates local glucose history storage options for tests.
    /// </summary>
    /// <returns>The local glucose history storage options.</returns>
    private static LocalGlucoseHistoryStorageOptions CreateOptions()
    {
        var historyFilePath = Path.Combine(
            Path.GetTempPath(),
            "GlucoDesk.Tests",
            $"{Guid.NewGuid():N}.glucose-history.json");
    
        return new LocalGlucoseHistoryStorageOptions(historyFilePath);
    }

    /// <summary>
    /// Creates a glucose reading for history store tests.
    /// </summary>
    /// <param name="timestamp">The reading timestamp.</param>
    /// <param name="providerKind">The provider kind.</param>
    /// <returns>The glucose reading.</returns>
    private static GlucoseReading CreateReading(
        DateTimeOffset timestamp,
        CgmProviderKind providerKind)
    {
        return new GlucoseReading(
            timestamp,
            new GlucoseValue(120, GlucoseUnit.MgDl),
            TrendDirection.Flat,
            providerKind,
            GlucoseDataFreshness.NearRealTime);
    }

    /// <summary>
    /// Creates a JSON glucose history store using a test file path.
    /// </summary>
    /// <returns>The JSON glucose history store.</returns>
    private JsonGlucoseHistoryStore CreateStore()
    {
        return new JsonGlucoseHistoryStore(
            new LocalGlucoseHistoryStorageOptions(BuildHistoryFilePath()));
    }

    /// <summary>
    /// Builds the test history file path.
    /// </summary>
    /// <returns>The test history file path.</returns>
    private string BuildHistoryFilePath()
    {
        return Path.Combine(_temporaryDirectoryPath, "glucose-history.json");
    }

    /// <summary>
    /// Creates a glucose reading for store tests.
    /// </summary>
    /// <param name="timestamp">The reading timestamp.</param>
    /// <param name="amount">The glucose amount.</param>
    /// <returns>The glucose reading.</returns>
    private static GlucoseReading CreateReading(
        DateTimeOffset timestamp,
        decimal amount)
    {
        return new GlucoseReading(
            timestamp,
            new GlucoseValue(amount, GlucoseUnit.MgDl),
            TrendDirection.Flat,
            CgmProviderKind.Mock,
            GlucoseDataFreshness.NearRealTime);
    }

    #endregion
}