using System.Runtime.CompilerServices;
using GlucoDesk.Application.Cgm.Backfill.Results;
using GlucoDesk.Application.Cgm.History.Continuity.Enums;
using GlucoDesk.Application.Cgm.History.Continuity.Requests;
using GlucoDesk.Application.Cgm.History.Continuity.Results;
using GlucoDesk.Application.Cgm.History.Continuity.Services.Abstractions;
using GlucoDesk.Application.Cgm.History.Results;
using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Core.Glucose.Readings;
using GlucoDesk.Core.Glucose.ValueObjects;
using GlucoDesk.Desktop.Cgm.History.Continuity.Enums;
using GlucoDesk.Desktop.Cgm.History.Continuity.Services;
using GlucoDesk.Desktop.Cgm.History.Continuity.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace GlucoDesk.Desktop.Tests.Cgm.History.Continuity.Services;

public sealed class DesktopHistoryContinuitySyncCoordinatorTests
{
    [Fact]
    public async Task RunStartupSyncAsync_ShouldExecuteStartupContinuitySync()
    {
    // Arrange
    var continuitySyncService = new FakeHistoryContinuitySyncService();
    
        var coordinator = CreateCoordinator(continuitySyncService);
    
        // Act
        var result = await coordinator.RunStartupSyncAsync(CancellationToken.None);
    
        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value.WasExecuted);
        Assert.False(result.Value.WasSkipped);
        Assert.Equal(CgmHistoryContinuitySyncTrigger.Startup, result.Value.Trigger);
    
        Assert.Equal(1, continuitySyncService.SyncCallCount);
        Assert.NotNull(continuitySyncService.LastRequest);
        Assert.Equal(CgmHistoryContinuitySyncTrigger.Startup, continuitySyncService.LastRequest.Trigger);
        Assert.Equal(CgmHistoryContinuitySyncRequest.DefaultStartupLookback, continuitySyncService.LastRequest.Lookback);
    }
    
    [Fact]
    public async Task RunResumeSyncAsync_ShouldExecuteResumeContinuitySync()
    {
        // Arrange
        var continuitySyncService = new FakeHistoryContinuitySyncService();
    
        var coordinator = CreateCoordinator(continuitySyncService);
    
        // Act
        var result = await coordinator.RunResumeSyncAsync(CancellationToken.None);
    
        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value.WasExecuted);
        Assert.False(result.Value.WasSkipped);
        Assert.Equal(CgmHistoryContinuitySyncTrigger.Resume, result.Value.Trigger);
    
        Assert.Equal(1, continuitySyncService.SyncCallCount);
        Assert.NotNull(continuitySyncService.LastRequest);
        Assert.Equal(CgmHistoryContinuitySyncTrigger.Resume, continuitySyncService.LastRequest.Trigger);
        Assert.Equal(CgmHistoryContinuitySyncRequest.DefaultResumeLookback, continuitySyncService.LastRequest.Lookback);
    }
    
    [Fact]
    public async Task RunManualSyncAsync_ShouldExecuteManualContinuitySyncWithProvidedLookback()
    {
        // Arrange
        var lookback = TimeSpan.FromHours(3);
        var continuitySyncService = new FakeHistoryContinuitySyncService();
    
        var coordinator = CreateCoordinator(continuitySyncService);
    
        // Act
        var result = await coordinator.RunManualSyncAsync(
            lookback,
            CancellationToken.None);
    
        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value.WasExecuted);
        Assert.False(result.Value.WasSkipped);
        Assert.Equal(CgmHistoryContinuitySyncTrigger.Manual, result.Value.Trigger);
    
        Assert.Equal(1, continuitySyncService.SyncCallCount);
        Assert.NotNull(continuitySyncService.LastRequest);
        Assert.Equal(CgmHistoryContinuitySyncTrigger.Manual, continuitySyncService.LastRequest.Trigger);
        Assert.Equal(lookback, continuitySyncService.LastRequest.Lookback);
    }
    
    [Fact]
    public async Task RunStartupSyncAsync_ShouldReturnFailure_WhenContinuitySyncFails()
    {
        // Arrange
        var expectedError = new Error(
            "HistoryContinuity.SyncFailed",
            "History continuity synchronization failed.");
    
        var continuitySyncService = new FakeHistoryContinuitySyncService
        {
            Result = Result<CgmHistoryContinuitySyncResult>.Failure(expectedError)
        };
    
        var coordinator = CreateCoordinator(continuitySyncService);
    
        // Act
        var result = await coordinator.RunStartupSyncAsync(CancellationToken.None);
    
        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(expectedError.Code, result.Error.Code);
        Assert.Equal(1, continuitySyncService.SyncCallCount);
    }
    
    [Fact]
    public async Task RunStartupSyncAsync_ShouldSkipSecondRun_WhenAnotherSyncIsAlreadyRunning()
    {
        // Arrange
        var continuitySyncService = new FakeHistoryContinuitySyncService
        {
            ShouldBlock = true
        };
    
        var coordinator = CreateCoordinator(continuitySyncService);
    
        var firstRunTask = coordinator.RunStartupSyncAsync(CancellationToken.None);
    
        await continuitySyncService.WaitUntilStartedAsync();
    
        // Act
        var secondRunResult = await coordinator.RunResumeSyncAsync(CancellationToken.None);
    
        continuitySyncService.CompleteBlockedSync();
    
        var firstRunResult = await firstRunTask;
    
        // Assert
        Assert.True(firstRunResult.IsSuccess);
        Assert.True(firstRunResult.Value.WasExecuted);
        Assert.False(firstRunResult.Value.WasSkipped);
        Assert.Equal(CgmHistoryContinuitySyncTrigger.Startup, firstRunResult.Value.Trigger);
    
        Assert.True(secondRunResult.IsSuccess);
        Assert.False(secondRunResult.Value.WasExecuted);
        Assert.True(secondRunResult.Value.WasSkipped);
        Assert.Equal(CgmHistoryContinuitySyncTrigger.Resume, secondRunResult.Value.Trigger);
    
        Assert.Equal(1, continuitySyncService.SyncCallCount);
    }
    
    [Fact]
    public async Task RunStartupSyncAsync_ShouldUpdateStatus_WhenSyncSucceeds()
    {
        // Arrange
        var continuitySyncService = new FakeHistoryContinuitySyncService();
        var statusStore = new DesktopHistoryContinuitySyncStatusStore(TimeProvider.System);
    
        var coordinator = CreateCoordinator(
            continuitySyncService,
            statusStore);
    
        // Act
        var result = await coordinator.RunStartupSyncAsync(CancellationToken.None);
    
        // Assert
        Assert.True(result.IsSuccess);
    
        var status = statusStore.Current;
    
        Assert.Equal(DesktopHistoryContinuitySyncRunState.Succeeded, status.State);
        Assert.Equal(CgmHistoryContinuitySyncTrigger.Startup, status.Trigger);
        Assert.NotNull(status.StartedAtUtc);
        Assert.NotNull(status.CompletedAtUtc);
        Assert.NotNull(status.LastSuccessfulSyncAtUtc);
        Assert.True(status.HasNewReadings);
        Assert.Equal(1, status.TotalFetchedReadings);
        Assert.Equal(1, status.AddedReadingsCount);
        Assert.Equal(0, status.DuplicateReadingsCount);
        Assert.Equal(1, status.StoredReadingsCount);
        Assert.Null(status.ErrorCode);
        Assert.Null(status.ErrorDescription);
    }
    
    [Fact]
    public async Task RunStartupSyncAsync_ShouldUpdateStatus_WhenSyncFails()
    {
        // Arrange
        var expectedError = new Error(
            "HistoryContinuity.SyncFailed",
            "History continuity synchronization failed.");
    
        var continuitySyncService = new FakeHistoryContinuitySyncService
        {
            Result = Result<CgmHistoryContinuitySyncResult>.Failure(expectedError)
        };
    
        var statusStore = new DesktopHistoryContinuitySyncStatusStore(TimeProvider.System);
    
        var coordinator = CreateCoordinator(
            continuitySyncService,
            statusStore);
    
        // Act
        var result = await coordinator.RunStartupSyncAsync(CancellationToken.None);
    
        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(expectedError.Code, result.Error.Code);
    
        var status = statusStore.Current;
    
        Assert.Equal(DesktopHistoryContinuitySyncRunState.Failed, status.State);
        Assert.Equal(CgmHistoryContinuitySyncTrigger.Startup, status.Trigger);
        Assert.NotNull(status.StartedAtUtc);
        Assert.NotNull(status.CompletedAtUtc);
        Assert.Null(status.LastSuccessfulSyncAtUtc);
        Assert.False(status.HasNewReadings);
        Assert.Equal(0, status.TotalFetchedReadings);
        Assert.Equal(expectedError.Code, status.ErrorCode);
        Assert.Equal(expectedError.Message, status.ErrorDescription);
    }
    
    #region Helpers
    
    /// <summary>
    /// Creates a desktop history continuity synchronization coordinator for tests.
    /// </summary>
    /// <param name="continuitySyncService">The fake continuity synchronization service.</param>
    /// <returns>The coordinator under test.</returns>
    private static IDesktopHistoryContinuitySyncCoordinator CreateCoordinator(
        ICgmHistoryContinuitySyncService continuitySyncService)
    {
        var statusStore = new DesktopHistoryContinuitySyncStatusStore(TimeProvider.System);
    
        return CreateCoordinator(
            continuitySyncService,
            statusStore);
    }
    
    /// <summary>
    /// Creates a desktop history continuity synchronization coordinator for tests.
    /// </summary>
    /// <param name="continuitySyncService">The fake continuity synchronization service.</param>
    /// <param name="statusStore">The status store used by the coordinator.</param>
    /// <returns>The coordinator under test.</returns>
    private static IDesktopHistoryContinuitySyncCoordinator CreateCoordinator(
        ICgmHistoryContinuitySyncService continuitySyncService,
        IDesktopHistoryContinuitySyncStatusStore statusStore)
    {
        var services = new ServiceCollection();
    
        services.AddSingleton(continuitySyncService);
    
        var serviceProvider = services.BuildServiceProvider(
            new ServiceProviderOptions
            {
                ValidateScopes = true,
                ValidateOnBuild = true
            });
    
        return new DesktopHistoryContinuitySyncCoordinator(
            serviceProvider.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<DesktopHistoryContinuitySyncCoordinator>.Instance,
            statusStore);
    }
    
    /// <summary>
    /// Creates a valid application continuity synchronization result used by desktop coordinator tests.
    /// </summary>
    /// <param name="request">The continuity synchronization request.</param>
    /// <returns>The continuity synchronization result.</returns>
    private static CgmHistoryContinuitySyncResult CreateContinuitySyncResult(
        CgmHistoryContinuitySyncRequest request)
    {
        var endsAt = new DateTimeOffset(2026, 6, 20, 10, 0, 0, TimeSpan.Zero);
        var startsAt = endsAt.Subtract(request.Lookback);
    
        var backfillSync = CreateBackfillHistorySyncResult(
            startsAt,
            endsAt);
    
        return new CgmHistoryContinuitySyncResult(
            request,
            startsAt,
            endsAt,
            backfillSync,
            "History continuity synchronization completed.");
    }
    
    /// <summary>
    /// Creates a valid backfill-to-history synchronization result used by desktop coordinator tests.
    /// </summary>
    /// <param name="startsAt">The synchronization start timestamp.</param>
    /// <param name="endsAt">The synchronization end timestamp.</param>
    /// <returns>The backfill-to-history synchronization result.</returns>
    private static CgmBackfillHistorySyncResult CreateBackfillHistorySyncResult(
        DateTimeOffset startsAt,
        DateTimeOffset endsAt)
    {
        var reading = CreateReading(startsAt.AddMinutes(5));
    
        var gap = new CgmBackfillPlanGap(
            OriginalStartsAt: startsAt,
            OriginalEndsAt: endsAt,
            StartsAt: startsAt,
            EndsAt: endsAt,
            WasClampedByMaximumLookback: false);
    
        var fetchedGap = new CgmBackfillFetchedGapResult(
            gap,
            [reading]);
    
        var execution = new CgmBackfillExecutionResult(
            status: default,
            run: CreateBackfillRunStub(),
            fetchedGaps: [fetchedGap],
            message: "Backfill execution completed.");
    
        var historySave = new GlucoseHistorySaveResult(
            CgmProviderKind.Mock,
            incomingReadingsCount: 1,
            addedReadingsCount: 1,
            duplicateReadingsCount: 0,
            storedReadingsCount: 1);
    
        return new CgmBackfillHistorySyncResult(
            execution,
            historySave,
            "Backfill readings synchronized into local history.");
    }
    
    /// <summary>
    /// Creates a minimal backfill run stub because this suite verifies desktop orchestration,
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
    
    private sealed class FakeHistoryContinuitySyncService : ICgmHistoryContinuitySyncService
    {
        private readonly TaskCompletionSource _syncStarted = new(
            TaskCreationOptions.RunContinuationsAsynchronously);
    
        private readonly TaskCompletionSource _syncCanComplete = new(
            TaskCreationOptions.RunContinuationsAsynchronously);
    
        public Result<CgmHistoryContinuitySyncResult>? Result { get; init; }
    
        public bool ShouldBlock { get; init; }
    
        public int SyncCallCount { get; private set; }
    
        public CgmHistoryContinuitySyncRequest? LastRequest { get; private set; }
    
        /// <inheritdoc />
        public async Task<Result<CgmHistoryContinuitySyncResult>> SyncRecentHistoryAsync(
            CgmHistoryContinuitySyncRequest request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            cancellationToken.ThrowIfCancellationRequested();
    
            SyncCallCount++;
            LastRequest = request;
    
            _syncStarted.TrySetResult();
    
            if (ShouldBlock)
            {
                await _syncCanComplete.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
            }
    
            return Result ?? Result<CgmHistoryContinuitySyncResult>.Success(
                CreateContinuitySyncResult(request));
        }
    
        /// <summary>
        /// Waits until the fake synchronization starts.
        /// </summary>
        /// <returns>A task representing the asynchronous wait operation.</returns>
        public Task WaitUntilStartedAsync()
        {
            return _syncStarted.Task;
        }
    
        /// <summary>
        /// Allows the blocked fake synchronization to complete.
        /// </summary>
        public void CompleteBlockedSync()
        {
            _syncCanComplete.TrySetResult();
        }
    }
}
