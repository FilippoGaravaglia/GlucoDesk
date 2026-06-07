using GlucoDesk.Application.Cgm.Dashboard.Requests;
using GlucoDesk.Application.Cgm.Providers.Abstractions;
using GlucoDesk.Application.Cgm.Providers.Metadata;
using GlucoDesk.Application.Cgm.Readings.Requests;
using GlucoDesk.Application.Cgm.Readings.Results;
using GlucoDesk.Application.Cgm.Services;
using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Core.Glucose.Readings;
using GlucoDesk.Core.Glucose.ValueObjects;

namespace GlucoDesk.Application.Tests.Cgm.Services;

public sealed class GlucoseDataServiceTests
{
    [Fact]
    public async Task GetProviderMetadataAsync_ShouldReturnProviderMetadata()
    {
        var service = CreateService();

        var result = await service.GetProviderMetadataAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(CgmProviderKind.Mock, result.Value.ProviderKind);
    }

    [Fact]
    public async Task GetLatestReadingAsync_ShouldReturnLatestReading()
    {
        var service = CreateService();

        var result = await service.GetLatestReadingAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.HasReading);
    }

    [Fact]
    public async Task GetRecentReadingsAsync_ShouldForwardRequestToLiveProvider()
    {
        var liveProvider = new FakeLiveProvider();
        var service = CreateService(liveProvider: liveProvider);

        var request = new GlucoseReadingsRequest(
            FixedNow.AddHours(-1),
            FixedNow,
            limit: 5);

        var result = await service.GetRecentReadingsAsync(request, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Same(request, liveProvider.CapturedRecentReadingsRequest);
    }

    [Fact]
    public async Task GetHistoricalReadingsAsync_ShouldForwardRequestToHistoricalProvider()
    {
        var historicalProvider = new FakeHistoricalProvider();
        var service = CreateService(historicalProvider: historicalProvider);

        var request = new GlucoseReadingsRequest(
            FixedNow.AddDays(-1),
            FixedNow,
            limit: 10);

        var result = await service.GetHistoricalReadingsAsync(request, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Same(request, historicalProvider.CapturedRequest);
    }

    [Fact]
    public async Task GetDashboardSnapshotAsync_ShouldBuildSnapshot()
    {
        var liveProvider = new FakeLiveProvider();
        var service = CreateService(liveProvider: liveProvider);

        var request = new GlucoseDashboardRequest(
            TimeSpan.FromHours(3),
            TimeSpan.FromMinutes(15),
            maxReadings: 36);

        var result = await service.GetDashboardSnapshotAsync(request, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.HasLatestReading);
        Assert.True(result.Value.HasRecentReadings);
        Assert.Equal(FixedNow, result.Value.SnapshotCreatedAt);
        Assert.NotNull(liveProvider.CapturedRecentReadingsRequest);
        Assert.Equal(FixedNow.AddHours(-3), liveProvider.CapturedRecentReadingsRequest.From);
        Assert.Equal(FixedNow, liveProvider.CapturedRecentReadingsRequest.To);
        Assert.Equal(36, liveProvider.CapturedRecentReadingsRequest.Limit);
    }

    [Fact]
    public async Task GetDashboardSnapshotAsync_ShouldReturnFailure_WhenMetadataProviderFails()
    {
        var expectedError = new Error("Metadata.Failed", "Metadata provider failed.");

        var service = CreateService(
            metadataProvider: new FakeMetadataProvider(Result<CgmProviderMetadata>.Failure(expectedError)));

        var result = await service.GetDashboardSnapshotAsync(
            GlucoseDashboardRequest.Default,
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(expectedError, result.Error);
    }

    [Fact]
    public async Task GetDashboardSnapshotAsync_ShouldReturnFailure_WhenLatestReadingProviderFails()
    {
        var expectedError = new Error("Latest.Failed", "Latest reading provider failed.");

        var service = CreateService(
            liveProvider: new FakeLiveProvider(
                latestResult: Result<LatestGlucoseReadingResult>.Failure(expectedError)));

        var result = await service.GetDashboardSnapshotAsync(
            GlucoseDashboardRequest.Default,
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(expectedError, result.Error);
    }

    #region Helpers

    private static readonly DateTimeOffset FixedNow = new(2026, 6, 7, 10, 0, 0, TimeSpan.Zero);

    /// <summary>
    /// Creates a glucose data service with fake providers.
    /// </summary>
    /// <param name="liveProvider">The optional fake live provider.</param>
    /// <param name="historicalProvider">The optional fake historical provider.</param>
    /// <param name="metadataProvider">The optional fake metadata provider.</param>
    /// <returns>The glucose data service.</returns>
    private static GlucoseDataService CreateService(
        ICgmLiveProvider? liveProvider = null,
        ICgmHistoricalProvider? historicalProvider = null,
        ICgmMetadataProvider? metadataProvider = null)
    {
        return new GlucoseDataService(
            liveProvider ?? new FakeLiveProvider(),
            historicalProvider ?? new FakeHistoricalProvider(),
            metadataProvider ?? new FakeMetadataProvider(),
            new TestTimeProvider(FixedNow));
    }

    /// <summary>
    /// Creates a valid glucose reading for tests.
    /// </summary>
    /// <param name="timestamp">The optional reading timestamp.</param>
    /// <returns>A valid glucose reading.</returns>
    private static GlucoseReading CreateReading(DateTimeOffset? timestamp = null)
    {
        return new GlucoseReading(
            timestamp ?? FixedNow,
            new GlucoseValue(120, GlucoseUnit.MgDl),
            TrendDirection.Flat,
            CgmProviderKind.Mock,
            GlucoseDataFreshness.NearRealTime);
    }

    /// <summary>
    /// Creates valid provider metadata for tests.
    /// </summary>
    /// <returns>Valid provider metadata.</returns>
    private static CgmProviderMetadata CreateMetadata()
    {
        return new CgmProviderMetadata(
            CgmProviderKind.Mock,
            "Mock",
            GlucoseDataFreshness.NearRealTime,
            supportsLiveReadings: true,
            supportsHistoricalReadings: true);
    }

    private sealed class FakeLiveProvider : ICgmLiveProvider
    {
        private readonly Result<LatestGlucoseReadingResult> _latestResult;
        private readonly Result<GlucoseReadingsResult> _recentResult;

        public FakeLiveProvider(
            Result<LatestGlucoseReadingResult>? latestResult = null,
            Result<GlucoseReadingsResult>? recentResult = null)
        {
            _latestResult = latestResult
                ?? Result<LatestGlucoseReadingResult>.Success(
                    new LatestGlucoseReadingResult(CreateReading(), FixedNow));

            _recentResult = recentResult
                ?? Result<GlucoseReadingsResult>.Success(
                    new GlucoseReadingsResult(
                        [
                            CreateReading(FixedNow.AddMinutes(-10)),
                            CreateReading(FixedNow.AddMinutes(-5))
                        ],
                        FixedNow));
        }

        public GlucoseReadingsRequest? CapturedRecentReadingsRequest { get; private set; }

        /// <inheritdoc />
        public Task<Result<LatestGlucoseReadingResult>> GetLatestReadingAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_latestResult);
        }

        /// <inheritdoc />
        public Task<Result<GlucoseReadingsResult>> GetRecentReadingsAsync(
            GlucoseReadingsRequest request,
            CancellationToken cancellationToken)
        {
            CapturedRecentReadingsRequest = request;

            return Task.FromResult(_recentResult);
        }
    }

    private sealed class FakeHistoricalProvider : ICgmHistoricalProvider
    {
        public GlucoseReadingsRequest? CapturedRequest { get; private set; }

        /// <inheritdoc />
        public Task<Result<GlucoseReadingsResult>> GetReadingsAsync(
            GlucoseReadingsRequest request,
            CancellationToken cancellationToken)
        {
            CapturedRequest = request;

            return Task.FromResult(Result<GlucoseReadingsResult>.Success(
                new GlucoseReadingsResult([CreateReading()], FixedNow)));
        }
    }

    private sealed class FakeMetadataProvider : ICgmMetadataProvider
    {
        private readonly Result<CgmProviderMetadata> _result;

        public FakeMetadataProvider(Result<CgmProviderMetadata>? result = null)
        {
            _result = result ?? Result<CgmProviderMetadata>.Success(CreateMetadata());
        }

        /// <inheritdoc />
        public Task<Result<CgmProviderMetadata>> GetMetadataAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_result);
        }
    }

    private sealed class TestTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestTimeProvider"/> class.
        /// </summary>
        /// <param name="utcNow">The fixed UTC timestamp.</param>
        public TestTimeProvider(DateTimeOffset utcNow)
        {
            _utcNow = utcNow;
        }

        /// <summary>
        /// Gets the fixed UTC timestamp.
        /// </summary>
        /// <returns>The fixed UTC timestamp.</returns>
        public override DateTimeOffset GetUtcNow()
        {
            return _utcNow;
        }
    }

    #endregion
}