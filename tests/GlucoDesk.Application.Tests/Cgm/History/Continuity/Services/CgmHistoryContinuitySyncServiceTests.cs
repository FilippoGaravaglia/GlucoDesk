using System.Runtime.CompilerServices;
using GlucoDesk.Application.Cgm.Backfill.Requests;
using GlucoDesk.Application.Cgm.Backfill.Results;
using GlucoDesk.Application.Cgm.Backfill.Services.Abstractions;
using GlucoDesk.Application.Cgm.History.Continuity.Enums;
using GlucoDesk.Application.Cgm.History.Continuity.Requests;
using GlucoDesk.Application.Cgm.History.Continuity.Services;
using GlucoDesk.Application.Cgm.History.Results;
using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Core.Glucose.Readings;
using GlucoDesk.Core.Glucose.ValueObjects;

namespace GlucoDesk.Application.Tests.Cgm.History.Continuity.Services;

public sealed class CgmHistoryContinuitySyncServiceTests
{
    [Fact]
    public async Task SyncRecentHistoryAsync_ShouldCallBackfillSyncWithResolvedLookbackWindow()
    {
        // Arrange
        var utcNow = new DateTimeOffset(2026, 6, 20, 10, 0, 0, TimeSpan.Zero);
        var lookback = TimeSpan.FromHours(6);

        var backfillHistorySyncService = new FakeBackfillHistorySyncService
        {
            Result = Result<CgmBackfillHistorySyncResult>.Success(
                CreateBackfillHistorySyncResult(
                    fetchedReadingsCount: 1,
                    addedReadingsCount: 1,
                    duplicateReadingsCount: 0,
                    storedReadingsCount: 10))
        };

        var service = new CgmHistoryContinuitySyncService(
            backfillHistorySyncService,
            new FakeTimeProvider(utcNow));

        // Act
        var result = await service.SyncRecentHistoryAsync(
            new CgmHistoryContinuitySyncRequest(
                CgmHistoryContinuitySyncTrigger.Startup,
                lookback),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, backfillHistorySyncService.SyncCallCount);
        Assert.NotNull(backfillHistorySyncService.LastRequest);
        Assert.Equal(utcNow.Subtract(lookback), backfillHistorySyncService.LastRequest.StartsAt);
        Assert.Equal(utcNow, backfillHistorySyncService.LastRequest.EndsAt);

        Assert.Equal(utcNow.Subtract(lookback), result.Value.StartsAt);
        Assert.Equal(utcNow, result.Value.EndsAt);
    }

    [Fact]
    public async Task SyncRecentHistoryAsync_ShouldReturnFailure_WhenBackfillSyncFails()
    {
        // Arrange
        var expectedError = new Error(
            "Backfill.SyncFailed",
            "Backfill synchronization failed.");

        var backfillHistorySyncService = new FakeBackfillHistorySyncService
        {
            Result = Result<CgmBackfillHistorySyncResult>.Failure(expectedError)
        };

        var service = new CgmHistoryContinuitySyncService(
            backfillHistorySyncService,
            new FakeTimeProvider(new DateTimeOffset(2026, 6, 20, 10, 0, 0, TimeSpan.Zero)));

        // Act
        var result = await service.SyncRecentHistoryAsync(
            CgmHistoryContinuitySyncRequest.ForStartup(),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(expectedError.Code, result.Error.Code);
        Assert.Equal(1, backfillHistorySyncService.SyncCallCount);
    }

    [Fact]
    public async Task SyncRecentHistoryAsync_ShouldReturnSummary_WhenBackfillSyncSucceedsWithNewReadings()
    {
        // Arrange
        var backfillHistorySync = CreateBackfillHistorySyncResult(
            fetchedReadingsCount: 3,
            addedReadingsCount: 2,
            duplicateReadingsCount: 1,
            storedReadingsCount: 42);

        var backfillHistorySyncService = new FakeBackfillHistorySyncService
        {
            Result = Result<CgmBackfillHistorySyncResult>.Success(backfillHistorySync)
        };

        var service = new CgmHistoryContinuitySyncService(
            backfillHistorySyncService,
            new FakeTimeProvider(new DateTimeOffset(2026, 6, 20, 10, 0, 0, TimeSpan.Zero)));

        // Act
        var result = await service.SyncRecentHistoryAsync(
            CgmHistoryContinuitySyncRequest.ForResume(TimeSpan.FromHours(4)),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(CgmHistoryContinuitySyncTrigger.Resume, result.Value.Request.Trigger);
        Assert.Same(backfillHistorySync, result.Value.BackfillSync);

        Assert.Equal(3, result.Value.TotalFetchedReadings);
        Assert.Equal(2, result.Value.AddedReadingsCount);
        Assert.Equal(1, result.Value.DuplicateReadingsCount);
        Assert.Equal(42, result.Value.StoredReadingsCount);
        Assert.True(result.Value.HasNewReadings);
    }

    [Fact]
    public async Task SyncRecentHistoryAsync_ShouldReturnSummary_WhenBackfillSyncSucceedsWithoutNewReadings()
    {
        // Arrange
        var backfillHistorySync = CreateBackfillHistorySyncResult(
            fetchedReadingsCount: 0,
            addedReadingsCount: 0,
            duplicateReadingsCount: 0,
            storedReadingsCount: 7);

        var backfillHistorySyncService = new FakeBackfillHistorySyncService
        {
            Result = Result<CgmBackfillHistorySyncResult>.Success(backfillHistorySync)
        };

        var service = new CgmHistoryContinuitySyncService(
            backfillHistorySyncService,
            new FakeTimeProvider(new DateTimeOffset(2026, 6, 20, 10, 0, 0, TimeSpan.Zero)));

        // Act
        var result = await service.SyncRecentHistoryAsync(
            CgmHistoryContinuitySyncRequest.ForManual(TimeSpan.FromHours(2)),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value.TotalFetchedReadings);
        Assert.Equal(0, result.Value.AddedReadingsCount);
        Assert.Equal(0, result.Value.DuplicateReadingsCount);
        Assert.Equal(7, result.Value.StoredReadingsCount);
        Assert.False(result.Value.HasNewReadings);
    }

    #region Helpers

    /// <summary>
    /// Creates a backfill-to-history synchronization result used by continuity tests.
    /// </summary>
    /// <param name="fetchedReadingsCount">The fetched readings count.</param>
    /// <param name="addedReadingsCount">The added readings count.</param>
    /// <param name="duplicateReadingsCount">The duplicate readings count.</param>
    /// <param name="storedReadingsCount">The stored readings count.</param>
    /// <returns>The backfill-to-history synchronization result.</returns>
    private static CgmBackfillHistorySyncResult CreateBackfillHistorySyncResult(
        int fetchedReadingsCount,
        int addedReadingsCount,
        int duplicateReadingsCount,
        int storedReadingsCount)
    {
        var startsAt = new DateTimeOffset(2026, 6, 20, 8, 0, 0, TimeSpan.Zero);
        var endsAt = startsAt.AddHours(1);

        var readings = Enumerable
            .Range(0, fetchedReadingsCount)
            .Select(index => CreateReading(startsAt.AddMinutes(index * 5)))
            .ToArray();

        var gap = new CgmBackfillPlanGap(
            OriginalStartsAt: startsAt,
            OriginalEndsAt: endsAt,
            StartsAt: startsAt,
            EndsAt: endsAt,
            WasClampedByMaximumLookback: false);

        var fetchedGap = new CgmBackfillFetchedGapResult(
            gap,
            readings);

        var execution = new CgmBackfillExecutionResult(
            status: default,
            run: CreateBackfillRunStub(),
            fetchedGaps: [fetchedGap],
            message: "Backfill execution completed.");

        var historySave = new GlucoseHistorySaveResult(
            CgmProviderKind.Mock,
            incomingReadingsCount: fetchedReadingsCount,
            addedReadingsCount: addedReadingsCount,
            duplicateReadingsCount: duplicateReadingsCount,
            storedReadingsCount: storedReadingsCount);

        return new CgmBackfillHistorySyncResult(
            execution,
            historySave,
            "Backfill readings synchronized into local history.");
    }

    /// <summary>
    /// Creates a minimal backfill run stub because this suite verifies continuity orchestration,
    /// not the internal content of the backfill run result.
    /// </summary>
    /// <returns>The backfill run stub.</returns>
    private static CgmBackfillRunResult CreateBackfillRunStub()
    {
        return (CgmBackfillRunResult)RuntimeHelpers.GetUninitializedObject(
            typeof(CgmBackfillRunResult));
    }

    /// <summary>
    /// Creates a glucose reading used by the tests.
    /// </summary>
    /// <param name="timestamp">The reading timestamp.</param>
    /// <returns>The glucose reading.</returns>
    private static GlucoseReading CreateReading(DateTimeOffset timestamp)
    {
        return new GlucoseReading(
            timestamp,
            new GlucoseValue(120, GlucoseUnit.MgDl),
            TrendDirection.Flat,
            CgmProviderKind.Mock,
            GlucoseDataFreshness.Historical);
    }

    #endregion

    private sealed class FakeBackfillHistorySyncService : ICgmBackfillHistorySyncService
    {
        public Result<CgmBackfillHistorySyncResult> Result { get; init; } =
            Result<CgmBackfillHistorySyncResult>.Success(
                CreateBackfillHistorySyncResult(
                    fetchedReadingsCount: 0,
                    addedReadingsCount: 0,
                    duplicateReadingsCount: 0,
                    storedReadingsCount: 0));

        public int SyncCallCount { get; private set; }

        public CgmBackfillRunRequest? LastRequest { get; private set; }

        /// <inheritdoc />
        public Task<Result<CgmBackfillHistorySyncResult>> SyncAsync(
            CgmBackfillRunRequest request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            cancellationToken.ThrowIfCancellationRequested();

            SyncCallCount++;
            LastRequest = request;

            return Task.FromResult(Result);
        }
    }

    private sealed class FakeTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        public FakeTimeProvider(DateTimeOffset utcNow)
        {
            _utcNow = utcNow;
        }

        public override DateTimeOffset GetUtcNow()
        {
            return _utcNow;
        }
    }
}