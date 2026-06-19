using GlucoDesk.Application.Cgm.WidgetState.Abstractions;
using GlucoDesk.Application.Cgm.WidgetState.Enums;
using GlucoDesk.Application.Cgm.WidgetState.Options;
using GlucoDesk.Application.Cgm.WidgetState.Results;
using GlucoDesk.Application.Cgm.WidgetState.Services;
using GlucoDesk.Application.Cgm.WidgetState.Snapshots;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Core.Glucose.Readings;
using GlucoDesk.Core.Glucose.ValueObjects;

namespace GlucoDesk.Application.Tests.Cgm.WidgetState.Services;

public sealed class GlucoseWidgetStatePublisherTests
{
    [Fact]
    public async Task PublishReadingAsync_ShouldSaveWidgetStateFromReading()
    {
        // Arrange
        var generatedAt = new DateTimeOffset(
            2026,
            6,
            19,
            10,
            1,
            0,
            TimeSpan.Zero);

        var store = new CapturingWidgetStateStore();
        var publisher = CreatePublisher(store, generatedAt);

        var reading = CreateReading(
            generatedAt.AddMinutes(-1),
            120m);

        // Act
        var result = await publisher.PublishReadingAsync(
            reading,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(store.SavedState);

        var state = store.SavedState!;

        Assert.Equal(1, state.SchemaVersion);
        Assert.Equal(generatedAt, state.GeneratedAt);
        Assert.Equal(reading.Timestamp, state.ReadingTimestamp);
        Assert.Equal(reading.Timestamp.AddMinutes(15), state.ExpiresAt);
        Assert.Equal(120m, state.GlucoseAmount);
        Assert.Equal(GlucoseUnit.MgDl, state.GlucoseUnit);
        Assert.Equal(TrendDirection.Flat, state.Trend);
        Assert.Equal(CgmProviderKind.DexcomShare, state.ProviderKind);
        Assert.Equal(GlucoseDataFreshness.NearRealTime, state.Freshness);
        Assert.Equal(WidgetGlucoseStatusLevel.InRange, state.StatusLevel);
        Assert.Equal("120", state.DisplayValue);
        Assert.Equal("mg/dL", state.UnitLabel);
        Assert.Equal("Flat", state.TrendLabel);
        Assert.Equal("Glucose in range", state.StatusMessage);
    }

    [Fact]
    public async Task PublishLatestReadingAsync_ShouldPublishMostRecentReading()
    {
        // Arrange
        var generatedAt = new DateTimeOffset(
            2026,
            6,
            19,
            10,
            10,
            0,
            TimeSpan.Zero);

        var store = new CapturingWidgetStateStore();
        var publisher = CreatePublisher(store, generatedAt);

        var olderReading = CreateReading(
            generatedAt.AddMinutes(-10),
            110m);

        var latestReading = CreateReading(
            generatedAt.AddMinutes(-2),
            130m);

        // Act
        var result = await publisher.PublishLatestReadingAsync(
            [olderReading, latestReading],
            CgmProviderKind.DexcomShare,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(store.SavedState);
        Assert.Equal(latestReading.Timestamp, store.SavedState!.ReadingTimestamp);
        Assert.Equal(130m, store.SavedState.GlucoseAmount);
    }

    [Fact]
    public async Task PublishLatestReadingAsync_ShouldPublishUnavailableState_WhenReadingsAreEmpty()
    {
        // Arrange
        var generatedAt = new DateTimeOffset(
            2026,
            6,
            19,
            10,
            10,
            0,
            TimeSpan.Zero);

        var store = new CapturingWidgetStateStore();
        var publisher = CreatePublisher(store, generatedAt);

        // Act
        var result = await publisher.PublishLatestReadingAsync(
            [],
            CgmProviderKind.DexcomShare,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(store.SavedState);

        var state = store.SavedState!;

        Assert.False(state.HasReading);
        Assert.Equal(CgmProviderKind.DexcomShare, state.ProviderKind);
        Assert.Equal(WidgetGlucoseStatusLevel.Unavailable, state.StatusLevel);
        Assert.Equal("--", state.DisplayValue);
        Assert.Equal("Glucose unavailable", state.StatusMessage);
    }

    [Fact]
    public async Task PublishUnavailableAsync_ShouldUseProvidedStatusMessage()
    {
        // Arrange
        var generatedAt = new DateTimeOffset(
            2026,
            6,
            19,
            10,
            10,
            0,
            TimeSpan.Zero);

        var store = new CapturingWidgetStateStore();
        var publisher = CreatePublisher(store, generatedAt);

        // Act
        var result = await publisher.PublishUnavailableAsync(
            CgmProviderKind.DexcomShare,
            "Provider not connected",
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(store.SavedState);
        Assert.Equal(WidgetGlucoseStatusLevel.Unavailable, store.SavedState!.StatusLevel);
        Assert.Equal("Provider not connected", store.SavedState.StatusMessage);
    }

    [Fact]
    public async Task PublishUnavailableAsync_ShouldUseDefaultStatusMessage_WhenStatusMessageIsEmpty()
    {
        // Arrange
        var generatedAt = new DateTimeOffset(
            2026,
            6,
            19,
            10,
            10,
            0,
            TimeSpan.Zero);

        var store = new CapturingWidgetStateStore();
        var publisher = CreatePublisher(store, generatedAt);

        // Act
        var result = await publisher.PublishUnavailableAsync(
            CgmProviderKind.DexcomShare,
            " ",
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(store.SavedState);
        Assert.Equal(WidgetGlucoseStatusLevel.Unavailable, store.SavedState!.StatusLevel);
        Assert.Equal("Glucose unavailable", store.SavedState.StatusMessage);
    }

    [Fact]
    public async Task ClearAsync_ShouldClearWidgetStateStore()
    {
        // Arrange
        var generatedAt = new DateTimeOffset(
            2026,
            6,
            19,
            10,
            10,
            0,
            TimeSpan.Zero);

        var store = new CapturingWidgetStateStore();
        var publisher = CreatePublisher(store, generatedAt);

        await publisher.PublishReadingAsync(
            CreateReading(generatedAt, 120m),
            CancellationToken.None);

        // Act
        var result = await publisher.ClearAsync(CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(store.WasCleared);
        Assert.Null(store.SavedState);
    }

    #region Helpers

    /// <summary>
    /// Creates a glucose widget state publisher for tests.
    /// </summary>
    /// <param name="store">The capturing widget state store.</param>
    /// <param name="generatedAt">The fixed generated timestamp.</param>
    /// <returns>The glucose widget state publisher.</returns>
    private static GlucoseWidgetStatePublisher CreatePublisher(
        CapturingWidgetStateStore store,
        DateTimeOffset generatedAt)
    {
        return new GlucoseWidgetStatePublisher(
            store,
            new FixedTimeProvider(generatedAt),
            WidgetStatePublisherOptions.Default());
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

    private sealed class CapturingWidgetStateStore : IWidgetStateStore
    {
        public GlucoseWidgetState? SavedState { get; private set; }

        public bool WasCleared { get; private set; }

        public Task<Result> SaveAsync(
            GlucoseWidgetState state,
            CancellationToken cancellationToken)
        {
            SavedState = state;

            return Task.FromResult(Result.Success());
        }

        public Task<Result<GlucoseWidgetStateReadResult>> ReadAsync(
            CancellationToken cancellationToken)
        {
            return Task.FromResult(
                Result<GlucoseWidgetStateReadResult>.Success(
                    SavedState is null
                        ? GlucoseWidgetStateReadResult.Empty()
                        : new GlucoseWidgetStateReadResult(SavedState)));
        }

        public Task<Result> ClearAsync(CancellationToken cancellationToken)
        {
            WasCleared = true;
            SavedState = null;

            return Task.FromResult(Result.Success());
        }
    }
}