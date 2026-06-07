using GlucoDesk.Application.Cgm.Readings.Requests;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Infrastructure.Cgm.Mock.Options;
using GlucoDesk.Infrastructure.Cgm.Mock.Providers;

namespace GlucoDesk.Infrastructure.Tests.Cgm.Mock.Providers;

public sealed class MockCgmProviderTests
{
    [Fact]
    public async Task GetMetadataAsync_ShouldReturnMockProviderMetadata()
    {
        var provider = CreateProvider();

        var result = await provider.GetMetadataAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(CgmProviderKind.Mock, result.Value.ProviderKind);
        Assert.Equal("Mock CGM Provider", result.Value.DisplayName);
        Assert.Equal(GlucoseDataFreshness.NearRealTime, result.Value.ExpectedFreshness);
        Assert.True(result.Value.SupportsLiveReadings);
        Assert.True(result.Value.SupportsHistoricalReadings);
    }

    [Fact]
    public async Task GetLatestReadingAsync_ShouldReturnNearRealTimeMockReading()
    {
        var provider = CreateProvider();

        var result = await provider.GetLatestReadingAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.HasReading);
        Assert.NotNull(result.Value.Reading);
        Assert.Equal(CgmProviderKind.Mock, result.Value.Reading.Provider);
        Assert.Equal(GlucoseDataFreshness.NearRealTime, result.Value.Reading.Freshness);
        Assert.Equal("Test Mock CGM", result.Value.Reading.Device);
    }

    [Fact]
    public async Task GetRecentReadingsAsync_ShouldReturnOrderedNearRealTimeReadingsInsideRange()
    {
        var now = new DateTimeOffset(2026, 6, 7, 10, 0, 0, TimeSpan.Zero);
        var provider = CreateProvider(now);

        var request = new GlucoseReadingsRequest(
            now.AddMinutes(-30),
            now,
            limit: null);

        var result = await provider.GetRecentReadingsAsync(request, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.HasReadings);
        Assert.All(result.Value.Readings, reading =>
        {
            Assert.Equal(CgmProviderKind.Mock, reading.Provider);
            Assert.Equal(GlucoseDataFreshness.NearRealTime, reading.Freshness);
            Assert.True(reading.Timestamp >= request.From);
            Assert.True(reading.Timestamp < request.To);
        });

        var orderedTimestamps = result.Value.Readings
            .Select(reading => reading.Timestamp)
            .ToArray();

        Assert.Equal(orderedTimestamps.OrderBy(timestamp => timestamp), orderedTimestamps);
    }

    [Fact]
    public async Task GetRecentReadingsAsync_ShouldRespectLimit()
    {
        var now = new DateTimeOffset(2026, 6, 7, 10, 0, 0, TimeSpan.Zero);
        var provider = CreateProvider(now);

        var request = new GlucoseReadingsRequest(
            now.AddHours(-1),
            now,
            limit: 3);

        var result = await provider.GetRecentReadingsAsync(request, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.Readings.Count);
    }

    [Fact]
    public async Task GetReadingsAsync_ShouldReturnHistoricalReadings()
    {
        var now = new DateTimeOffset(2026, 6, 7, 10, 0, 0, TimeSpan.Zero);
        var provider = CreateProvider(now);

        var request = new GlucoseReadingsRequest(
            now.AddMinutes(-30),
            now);

        var result = await provider.GetReadingsAsync(request, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.HasReadings);
        Assert.All(result.Value.Readings, reading =>
            Assert.Equal(GlucoseDataFreshness.Historical, reading.Freshness));
    }

    [Fact]
    public async Task GetRecentReadingsAsync_ShouldThrow_WhenRequestIsNull()
    {
        var provider = CreateProvider();

        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => provider.GetRecentReadingsAsync(null!, CancellationToken.None));

        Assert.Equal("request", exception.ParamName);
    }

    #region Helpers

    /// <summary>
    /// Creates a mock CGM provider using a deterministic timestamp.
    /// </summary>
    /// <param name="timestamp">The optional timestamp returned by the test time provider.</param>
    /// <returns>A mock CGM provider.</returns>
    private static MockCgmProvider CreateProvider(DateTimeOffset? timestamp = null)
    {
        var options = new MockCgmProviderOptions(deviceName: "Test Mock CGM");
        var timeProvider = new TestTimeProvider(timestamp ?? new DateTimeOffset(2026, 6, 7, 10, 0, 0, TimeSpan.Zero));

        return new MockCgmProvider(options, timeProvider);
    }

    private sealed class TestTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        public TestTimeProvider(DateTimeOffset utcNow)
        {
            _utcNow = utcNow;
        }

        public override DateTimeOffset GetUtcNow()
        {
            return _utcNow;
        }
    }

    #endregion
}