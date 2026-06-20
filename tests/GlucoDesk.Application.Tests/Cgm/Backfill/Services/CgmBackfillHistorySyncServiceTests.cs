using System.Runtime.CompilerServices;
using GlucoDesk.Application.Cgm.Backfill.Requests;
using GlucoDesk.Application.Cgm.Backfill.Results;
using GlucoDesk.Application.Cgm.Backfill.Services;
using GlucoDesk.Application.Cgm.Backfill.Services.Abstractions;
using GlucoDesk.Application.Cgm.History.Requests;
using GlucoDesk.Application.Cgm.History.Results;
using GlucoDesk.Application.Cgm.History.Services.Abstractions;
using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Core.Glucose.Readings;
using GlucoDesk.Core.Glucose.ValueObjects;

namespace GlucoDesk.Application.Tests.Cgm.Backfill.Services;

public sealed class CgmBackfillHistorySyncServiceTests
{
    [Fact]
    public async Task SyncAsync_ShouldReturnFailure_WhenBackfillExecutionFails()
    {
        // Arrange
        var expectedError = new Error(
            "Backfill.ExecutionFailed",
            "Backfill execution failed.");

        var backfillExecutionService = new FakeBackfillExecutionService
        {
            Result = Result<CgmBackfillExecutionResult>.Failure(expectedError)
        };

        var glucoseHistoryService = new FakeGlucoseHistoryService();

        var service = new CgmBackfillHistorySyncService(
            backfillExecutionService,
            glucoseHistoryService);

        // Act
        var result = await service.SyncAsync(
            CreateRequest(),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(expectedError.Code, result.Error.Code);
        Assert.Equal(1, backfillExecutionService.ExecuteCallCount);
        Assert.Equal(0, glucoseHistoryService.SaveReadingsWithSummaryCallCount);
    }

    [Fact]
    public async Task SyncAsync_ShouldNotPersistHistory_WhenBackfillReturnsNoReadings()
    {
        // Arrange
        var backfillExecution = CreateBackfillExecutionResult([]);

        var backfillExecutionService = new FakeBackfillExecutionService
        {
            Result = Result<CgmBackfillExecutionResult>.Success(backfillExecution)
        };

        var glucoseHistoryService = new FakeGlucoseHistoryService();

        var service = new CgmBackfillHistorySyncService(
            backfillExecutionService,
            glucoseHistoryService);

        // Act
        var result = await service.SyncAsync(
            CreateRequest(),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, backfillExecutionService.ExecuteCallCount);
        Assert.Equal(0, glucoseHistoryService.SaveReadingsWithSummaryCallCount);

        Assert.False(result.Value.WasPersisted);
        Assert.False(result.Value.HasNewReadings);
        Assert.Equal(0, result.Value.TotalFetchedReadings);
        Assert.Equal(0, result.Value.AddedReadingsCount);
        Assert.Equal(0, result.Value.DuplicateReadingsCount);
        Assert.Equal(0, result.Value.StoredReadingsCount);
        Assert.Null(result.Value.HistorySave);
    }

    [Fact]
    public async Task SyncAsync_ShouldPersistFetchedReadings_WhenBackfillReturnsReadings()
    {
        // Arrange
        var readings =
            new[]
            {
                CreateReading(new DateTimeOffset(2026, 6, 20, 8, 5, 0, TimeSpan.Zero)),
                CreateReading(new DateTimeOffset(2026, 6, 20, 8, 10, 0, TimeSpan.Zero))
            };

        var backfillExecution = CreateBackfillExecutionResult(readings);

        var historySave = new GlucoseHistorySaveResult(
            CgmProviderKind.Mock,
            incomingReadingsCount: 2,
            addedReadingsCount: 2,
            duplicateReadingsCount: 0,
            storedReadingsCount: 10);

        var backfillExecutionService = new FakeBackfillExecutionService
        {
            Result = Result<CgmBackfillExecutionResult>.Success(backfillExecution)
        };

        var glucoseHistoryService = new FakeGlucoseHistoryService
        {
            SaveResult = Result<GlucoseHistorySaveResult>.Success(historySave)
        };

        var service = new CgmBackfillHistorySyncService(
            backfillExecutionService,
            glucoseHistoryService);

        // Act
        var result = await service.SyncAsync(
            CreateRequest(),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, backfillExecutionService.ExecuteCallCount);
        Assert.Equal(1, glucoseHistoryService.SaveReadingsWithSummaryCallCount);

        Assert.Equal(2, glucoseHistoryService.LastSavedReadings.Count);
        Assert.Contains(readings[0], glucoseHistoryService.LastSavedReadings);
        Assert.Contains(readings[1], glucoseHistoryService.LastSavedReadings);

        Assert.True(result.Value.WasPersisted);
        Assert.True(result.Value.HasNewReadings);
        Assert.Equal(2, result.Value.TotalFetchedReadings);
        Assert.Equal(2, result.Value.AddedReadingsCount);
        Assert.Equal(0, result.Value.DuplicateReadingsCount);
        Assert.Equal(10, result.Value.StoredReadingsCount);
        Assert.Same(historySave, result.Value.HistorySave);
    }

    [Fact]
    public async Task SyncAsync_ShouldReturnFailure_WhenHistorySaveFails()
    {
        // Arrange
        var readings =
            new[]
            {
                CreateReading(new DateTimeOffset(2026, 6, 20, 8, 5, 0, TimeSpan.Zero))
            };

        var expectedError = new Error(
            "History.SaveFailed",
            "History save failed.");

        var backfillExecutionService = new FakeBackfillExecutionService
        {
            Result = Result<CgmBackfillExecutionResult>.Success(
                CreateBackfillExecutionResult(readings))
        };

        var glucoseHistoryService = new FakeGlucoseHistoryService
        {
            SaveResult = Result<GlucoseHistorySaveResult>.Failure(expectedError)
        };

        var service = new CgmBackfillHistorySyncService(
            backfillExecutionService,
            glucoseHistoryService);

        // Act
        var result = await service.SyncAsync(
            CreateRequest(),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(expectedError.Code, result.Error.Code);
        Assert.Equal(1, backfillExecutionService.ExecuteCallCount);
        Assert.Equal(1, glucoseHistoryService.SaveReadingsWithSummaryCallCount);
        Assert.Single(glucoseHistoryService.LastSavedReadings);
    }

    [Fact]
    public async Task SyncAsync_ShouldReturnSummary_WhenHistorySaveSucceedsWithDuplicates()
    {
        // Arrange
        var readings =
            new[]
            {
                CreateReading(new DateTimeOffset(2026, 6, 20, 8, 5, 0, TimeSpan.Zero)),
                CreateReading(new DateTimeOffset(2026, 6, 20, 8, 10, 0, TimeSpan.Zero)),
                CreateReading(new DateTimeOffset(2026, 6, 20, 8, 15, 0, TimeSpan.Zero))
            };

        var historySave = new GlucoseHistorySaveResult(
            CgmProviderKind.Mock,
            incomingReadingsCount: 3,
            addedReadingsCount: 1,
            duplicateReadingsCount: 2,
            storedReadingsCount: 42);

        var backfillExecutionService = new FakeBackfillExecutionService
        {
            Result = Result<CgmBackfillExecutionResult>.Success(
                CreateBackfillExecutionResult(readings))
        };

        var glucoseHistoryService = new FakeGlucoseHistoryService
        {
            SaveResult = Result<GlucoseHistorySaveResult>.Success(historySave)
        };

        var service = new CgmBackfillHistorySyncService(
            backfillExecutionService,
            glucoseHistoryService);

        // Act
        var result = await service.SyncAsync(
            CreateRequest(),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value.WasPersisted);
        Assert.True(result.Value.HasNewReadings);

        Assert.Equal(3, result.Value.TotalFetchedReadings);
        Assert.Equal(1, result.Value.AddedReadingsCount);
        Assert.Equal(2, result.Value.DuplicateReadingsCount);
        Assert.Equal(42, result.Value.StoredReadingsCount);
        Assert.Same(historySave, result.Value.HistorySave);
    }

    #region Helpers

    /// <summary>
    /// Creates a backfill sync request used by the tests.
    /// </summary>
    /// <returns>The backfill run request.</returns>
    private static CgmBackfillRunRequest CreateRequest()
    {
        var startsAt = new DateTimeOffset(2026, 6, 20, 8, 0, 0, TimeSpan.Zero);
        var endsAt = startsAt.AddHours(1);

        return new CgmBackfillRunRequest(
            startsAt,
            endsAt);
    }

    /// <summary>
    /// Creates a backfill execution result used by the sync service tests.
    /// </summary>
    /// <param name="readings">The fetched readings exposed by backfill execution.</param>
    /// <returns>The backfill execution result.</returns>
    private static CgmBackfillExecutionResult CreateBackfillExecutionResult(
        IReadOnlyCollection<GlucoseReading> readings)
    {
        var startsAt = new DateTimeOffset(2026, 6, 20, 8, 0, 0, TimeSpan.Zero);
        var endsAt = startsAt.AddHours(1);

        var gap = new CgmBackfillPlanGap(
            OriginalStartsAt: startsAt,
            OriginalEndsAt: endsAt,
            StartsAt: startsAt,
            EndsAt: endsAt,
            WasClampedByMaximumLookback: false);

        var fetchedGap = new CgmBackfillFetchedGapResult(
            gap,
            readings);

        return new CgmBackfillExecutionResult(
            status: default,
            run: CreateBackfillRunStub(),
            fetchedGaps: [fetchedGap],
            message: "Backfill execution completed.");
    }

    /// <summary>
    /// Creates a minimal backfill run stub because this test suite verifies sync orchestration,
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

    private sealed class FakeBackfillExecutionService : ICgmBackfillExecutionService
    {
        public Result<CgmBackfillExecutionResult> Result { get; init; } =
            Result<CgmBackfillExecutionResult>.Success(
                CreateBackfillExecutionResult([]));

        public int ExecuteCallCount { get; private set; }

        /// <inheritdoc />
        public Task<Result<CgmBackfillExecutionResult>> ExecuteAsync(
            CgmBackfillRunRequest request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            cancellationToken.ThrowIfCancellationRequested();

            ExecuteCallCount++;

            return Task.FromResult(Result);
        }
    }

    private sealed class FakeGlucoseHistoryService : IGlucoseHistoryService
    {
        public Result<GlucoseHistorySaveResult> SaveResult { get; init; } =
            Result<GlucoseHistorySaveResult>.Success(
                new GlucoseHistorySaveResult(
                    CgmProviderKind.Mock,
                    incomingReadingsCount: 0,
                    addedReadingsCount: 0,
                    duplicateReadingsCount: 0,
                    storedReadingsCount: 0));

        public int SaveReadingsWithSummaryCallCount { get; private set; }

        public IReadOnlyCollection<GlucoseReading> LastSavedReadings { get; private set; } = [];

        /// <inheritdoc />
        public Task<Result> SaveReadingsAsync(
            IReadOnlyCollection<GlucoseReading> readings,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException("Simple history save is not used by these tests.");
        }

        /// <inheritdoc />
        public Task<Result<GlucoseHistorySaveResult>> SaveReadingsWithSummaryAsync(
            IReadOnlyCollection<GlucoseReading> readings,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(readings);
            cancellationToken.ThrowIfCancellationRequested();

            SaveReadingsWithSummaryCallCount++;
            LastSavedReadings = readings;

            return Task.FromResult(SaveResult);
        }

        /// <inheritdoc />
        public Task<Result<GlucoseHistoryResult>> GetReadingsAsync(
            GlucoseHistoryRequest request,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException("History read is not used by these tests.");
        }
    }
}