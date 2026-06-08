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

    public void Dispose()
    {
        if (Directory.Exists(_temporaryDirectoryPath))
        {
            Directory.Delete(_temporaryDirectoryPath, recursive: true);
        }
    }

    #region Helpers

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