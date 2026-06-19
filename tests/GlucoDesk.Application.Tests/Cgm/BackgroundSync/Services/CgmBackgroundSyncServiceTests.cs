using GlucoDesk.Application.Cgm.BackgroundSync.Enums;
using GlucoDesk.Application.Cgm.BackgroundSync.Services;
using GlucoDesk.Application.Cgm.Dashboard.Requests;
using GlucoDesk.Application.Cgm.Dashboard.Results;
using GlucoDesk.Application.Cgm.History.Requests;
using GlucoDesk.Application.Cgm.History.Results;
using GlucoDesk.Application.Cgm.History.Services.Abstractions;
using GlucoDesk.Application.Cgm.Providers.Metadata;
using GlucoDesk.Application.Cgm.Readings.Requests;
using GlucoDesk.Application.Cgm.Readings.Results;
using GlucoDesk.Application.Cgm.Services.Abstractions;
using GlucoDesk.Application.Cgm.WidgetState.Services.Abstractions;
using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Core.Glucose.Readings;
using GlucoDesk.Core.Glucose.ValueObjects;

namespace GlucoDesk.Application.Tests.Cgm.BackgroundSync.Services;

public sealed class CgmBackgroundSyncServiceTests
{
    [Fact]
    public async Task RunOnceAsync_ShouldPersistReadingsAndPublishWidgetState_WhenSnapshotContainsReadings()
    {
        // Arrange
        var syncedAt = new DateTimeOffset(2026, 6, 19, 10, 0, 0, TimeSpan.Zero);
        var latestReading = CreateReading(syncedAt.AddMinutes(-1), 120m);
        var olderReading = CreateReading(syncedAt.AddMinutes(-6), 115m);

        var historyService = new FakeGlucoseHistoryService();
        var widgetStatePublisher = new FakeWidgetStatePublisher();

        var service = new CgmBackgroundSyncService(
            new FakeGlucoseDataService(
                Result<GlucoseDashboardSnapshot>.Success(
                    CreateSnapshot(latestReading, [olderReading, latestReading], syncedAt))),
            new FixedTimeProvider(syncedAt),
            historyService,
            widgetStatePublisher);

        // Act
        var result = await service.RunOnceAsync(CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(BackgroundSyncStatus.Succeeded, result.Value.Status);
        Assert.Equal(CgmProviderKind.DexcomShare, result.Value.ProviderKind);
        Assert.Equal(2, result.Value.ReadingsCount);
        Assert.Equal(2, historyService.SavedReadings.Count);
        Assert.Same(latestReading, widgetStatePublisher.PublishedReading);
    }

    [Fact]
    public async Task RunOnceAsync_ShouldPublishUnavailable_WhenSnapshotContainsNoReadings()
    {
        // Arrange
        var syncedAt = new DateTimeOffset(2026, 6, 19, 10, 0, 0, TimeSpan.Zero);

        var widgetStatePublisher = new FakeWidgetStatePublisher();

        var service = new CgmBackgroundSyncService(
            new FakeGlucoseDataService(
                Result<GlucoseDashboardSnapshot>.Success(
                    CreateSnapshot(null, [], syncedAt))),
            new FixedTimeProvider(syncedAt),
            new FakeGlucoseHistoryService(),
            widgetStatePublisher);

        // Act
        var result = await service.RunOnceAsync(CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(BackgroundSyncStatus.NoData, result.Value.Status);
        Assert.True(widgetStatePublisher.PublishedUnavailable);
        Assert.Equal(CgmProviderKind.DexcomShare, widgetStatePublisher.UnavailableProviderKind);
    }

    [Fact]
    public async Task RunOnceAsync_ShouldPublishUnavailable_WhenProviderFails()
    {
        // Arrange
        var syncedAt = new DateTimeOffset(2026, 6, 19, 10, 0, 0, TimeSpan.Zero);

        var widgetStatePublisher = new FakeWidgetStatePublisher();

        var service = new CgmBackgroundSyncService(
            new FakeGlucoseDataService(
                Result<GlucoseDashboardSnapshot>.Failure(
                    new Error("Provider.Failed", "Provider failed."))),
            new FixedTimeProvider(syncedAt),
            new FakeGlucoseHistoryService(),
            widgetStatePublisher);

        // Act
        var result = await service.RunOnceAsync(CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(BackgroundSyncStatus.ProviderFailed, result.Value.Status);
        Assert.True(widgetStatePublisher.PublishedUnavailable);
        Assert.Equal(CgmProviderKind.Unknown, widgetStatePublisher.UnavailableProviderKind);
    }

    [Fact]
    public async Task RunOnceAsync_ShouldSucceed_WhenHistoryPersistenceFails()
    {
        // Arrange
        var syncedAt = new DateTimeOffset(2026, 6, 19, 10, 0, 0, TimeSpan.Zero);
        var latestReading = CreateReading(syncedAt.AddMinutes(-1), 120m);

        var service = new CgmBackgroundSyncService(
            new FakeGlucoseDataService(
                Result<GlucoseDashboardSnapshot>.Success(
                    CreateSnapshot(latestReading, [latestReading], syncedAt))),
            new FixedTimeProvider(syncedAt),
            new FailingGlucoseHistoryService(),
            new FakeWidgetStatePublisher());

        // Act
        var result = await service.RunOnceAsync(CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(BackgroundSyncStatus.Succeeded, result.Value.Status);
    }

    [Fact]
    public async Task RunOnceAsync_ShouldSucceed_WhenWidgetStatePublishingFails()
    {
        // Arrange
        var syncedAt = new DateTimeOffset(2026, 6, 19, 10, 0, 0, TimeSpan.Zero);
        var latestReading = CreateReading(syncedAt.AddMinutes(-1), 120m);

        var service = new CgmBackgroundSyncService(
            new FakeGlucoseDataService(
                Result<GlucoseDashboardSnapshot>.Success(
                    CreateSnapshot(latestReading, [latestReading], syncedAt))),
            new FixedTimeProvider(syncedAt),
            new FakeGlucoseHistoryService(),
            new FailingWidgetStatePublisher());

        // Act
        var result = await service.RunOnceAsync(CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(BackgroundSyncStatus.Succeeded, result.Value.Status);
    }

    #region Helpers

    /// <summary>
    /// Creates a dashboard snapshot for background sync tests.
    /// </summary>
    /// <param name="latestReading">The latest glucose reading.</param>
    /// <param name="recentReadings">The recent glucose readings.</param>
    /// <param name="snapshotCreatedAt">The snapshot creation timestamp.</param>
    /// <returns>The dashboard snapshot.</returns>
    private static GlucoseDashboardSnapshot CreateSnapshot(
        GlucoseReading? latestReading,
        IReadOnlyCollection<GlucoseReading> recentReadings,
        DateTimeOffset snapshotCreatedAt)
    {
        return new GlucoseDashboardSnapshot(
            new CgmProviderMetadata(
                CgmProviderKind.DexcomShare,
                "Dexcom Share",
                GlucoseDataFreshness.NearRealTime,
                true,
                true),
            latestReading,
            recentReadings,
            snapshotCreatedAt,
            snapshotCreatedAt,
            snapshotCreatedAt,
            TimeSpan.FromMinutes(15));
    }

    /// <summary>
    /// Creates a glucose reading for background sync tests.
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

    #endregion

    private sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        public FixedTimeProvider(DateTimeOffset utcNow)
        {
            _utcNow = utcNow;
        }

        public override DateTimeOffset GetUtcNow()
        {
            return _utcNow;
        }
    }

    private sealed class FakeGlucoseDataService : IGlucoseDataService
    {
        private readonly Result<GlucoseDashboardSnapshot> _result;

        public FakeGlucoseDataService(Result<GlucoseDashboardSnapshot> result)
        {
            _result = result;
        }

        public Task<Result<CgmProviderMetadata>> GetProviderMetadataAsync(
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_result.IsFailure)
            {
                return Task.FromResult(Result<CgmProviderMetadata>.Failure(_result.Error));
            }

            return Task.FromResult(Result<CgmProviderMetadata>.Success(_result.Value.Metadata));
        }

        public Task<Result<LatestGlucoseReadingResult>> GetLatestReadingAsync(
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_result.IsFailure)
            {
                return Task.FromResult(Result<LatestGlucoseReadingResult>.Failure(_result.Error));
            }

            return Task.FromResult(Result<LatestGlucoseReadingResult>.Success(
                new LatestGlucoseReadingResult(
                    _result.Value.LatestReading,
                    _result.Value.LatestReadingRetrievedAt)));
        }

        public Task<Result<GlucoseReadingsResult>> GetRecentReadingsAsync(
            GlucoseReadingsRequest request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            cancellationToken.ThrowIfCancellationRequested();

            if (_result.IsFailure)
            {
                return Task.FromResult(Result<GlucoseReadingsResult>.Failure(_result.Error));
            }

            return Task.FromResult(Result<GlucoseReadingsResult>.Success(
                new GlucoseReadingsResult(
                    _result.Value.RecentReadings,
                    _result.Value.RecentReadingsRetrievedAt)));
        }

        public Task<Result<GlucoseReadingsResult>> GetHistoricalReadingsAsync(
            GlucoseReadingsRequest request,
            CancellationToken cancellationToken)
        {
            return GetRecentReadingsAsync(request, cancellationToken);
        }

        public Task<Result<GlucoseDashboardSnapshot>> GetDashboardSnapshotAsync(
            GlucoseDashboardRequest request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(_result);
        }
    }

    private sealed class FakeGlucoseHistoryService : IGlucoseHistoryService
    {
        public IReadOnlyCollection<GlucoseReading> SavedReadings { get; private set; } =
            Array.Empty<GlucoseReading>();

        public Task<Result> SaveReadingsAsync(
            IReadOnlyCollection<GlucoseReading> readings,
            CancellationToken cancellationToken)
        {
            SavedReadings = readings;
            return Task.FromResult(Result.Success());
        }

        public Task<Result<GlucoseHistorySaveResult>> SaveReadingsWithSummaryAsync(
            IReadOnlyCollection<GlucoseReading> readings,
            CancellationToken cancellationToken)
        {
            SavedReadings = readings;

            return Task.FromResult(Result<GlucoseHistorySaveResult>.Success(
                new GlucoseHistorySaveResult(
                    CgmProviderKind.DexcomShare,
                    readings.Count,
                    readings.Count,
                    0,
                    readings.Count)));
        }

        public Task<Result<GlucoseHistoryResult>> GetReadingsAsync(
            GlucoseHistoryRequest request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(Result<GlucoseHistoryResult>.Success(
                new GlucoseHistoryResult(SavedReadings)));
        }
    }

    private sealed class FailingGlucoseHistoryService : IGlucoseHistoryService
    {
        public Task<Result> SaveReadingsAsync(
            IReadOnlyCollection<GlucoseReading> readings,
            CancellationToken cancellationToken)
        {
            throw new IOException("History storage unavailable.");
        }

        public Task<Result<GlucoseHistorySaveResult>> SaveReadingsWithSummaryAsync(
            IReadOnlyCollection<GlucoseReading> readings,
            CancellationToken cancellationToken)
        {
            throw new IOException("History storage unavailable.");
        }

        public Task<Result<GlucoseHistoryResult>> GetReadingsAsync(
            GlucoseHistoryRequest request,
            CancellationToken cancellationToken)
        {
            throw new IOException("History storage unavailable.");
        }
    }

    private sealed class FakeWidgetStatePublisher : IWidgetStatePublisher
    {
        public GlucoseReading? PublishedReading { get; private set; }

        public bool PublishedUnavailable { get; private set; }

        public CgmProviderKind UnavailableProviderKind { get; private set; }

        public Task<Result> PublishReadingAsync(
            GlucoseReading reading,
            CancellationToken cancellationToken)
        {
            PublishedReading = reading;
            return Task.FromResult(Result.Success());
        }

        public Task<Result> PublishLatestReadingAsync(
            IReadOnlyCollection<GlucoseReading> readings,
            CgmProviderKind providerKind,
            CancellationToken cancellationToken)
        {
            PublishedReading = readings
                .OrderByDescending(reading => reading.Timestamp)
                .FirstOrDefault();

            return Task.FromResult(Result.Success());
        }

        public Task<Result> PublishUnavailableAsync(
            CgmProviderKind providerKind,
            string? statusMessage,
            CancellationToken cancellationToken)
        {
            PublishedUnavailable = true;
            UnavailableProviderKind = providerKind;

            return Task.FromResult(Result.Success());
        }

        public Task<Result> ClearAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Result.Success());
        }
    }

    private sealed class FailingWidgetStatePublisher : IWidgetStatePublisher
    {
        public Task<Result> PublishReadingAsync(
            GlucoseReading reading,
            CancellationToken cancellationToken)
        {
            throw new IOException("Widget state unavailable.");
        }

        public Task<Result> PublishLatestReadingAsync(
            IReadOnlyCollection<GlucoseReading> readings,
            CgmProviderKind providerKind,
            CancellationToken cancellationToken)
        {
            throw new IOException("Widget state unavailable.");
        }

        public Task<Result> PublishUnavailableAsync(
            CgmProviderKind providerKind,
            string? statusMessage,
            CancellationToken cancellationToken)
        {
            throw new IOException("Widget state unavailable.");
        }

        public Task<Result> ClearAsync(CancellationToken cancellationToken)
        {
            throw new IOException("Widget state unavailable.");
        }
    }
}