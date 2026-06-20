using GlucoDesk.Application.Cgm.History.Continuity.Requests;
using GlucoDesk.Application.Cgm.History.Continuity.Services.Abstractions;
using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Desktop.Cgm.History.Continuity.Results;
using GlucoDesk.Desktop.Cgm.History.Continuity.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GlucoDesk.Desktop.Cgm.History.Continuity.Services;

/// <summary>
/// Coordinates desktop-triggered history continuity synchronization without blocking the UI.
/// </summary>
public sealed class DesktopHistoryContinuitySyncCoordinator : IDesktopHistoryContinuitySyncCoordinator
{
    private static readonly Error UnexpectedSyncError = new(
        "Desktop.HistoryContinuitySync.UnexpectedError",
        "An unexpected error occurred while synchronizing recent glucose history.");

    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<DesktopHistoryContinuitySyncCoordinator> _logger;
    private readonly IDesktopHistoryContinuitySyncStatusStore _statusStore;
    private readonly SemaphoreSlim _syncGate = new(1, 1);

    /// <summary>
    /// Initializes a new instance of the <see cref="DesktopHistoryContinuitySyncCoordinator"/> class.
    /// </summary>
    /// <param name="serviceScopeFactory">The service scope factory.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="statusStore">The history continuity synchronization status store.</param>
    /// <exception cref="ArgumentNullException">Thrown when a dependency is null.</exception>
    public DesktopHistoryContinuitySyncCoordinator(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<DesktopHistoryContinuitySyncCoordinator> logger,
        IDesktopHistoryContinuitySyncStatusStore statusStore)
    {
        ArgumentNullException.ThrowIfNull(serviceScopeFactory);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(statusStore);

        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _statusStore = statusStore;
    }

    /// <inheritdoc />
    public Task<Result<DesktopHistoryContinuitySyncRunResult>> RunStartupSyncAsync(
        CancellationToken cancellationToken)
    {
        return RunAsync(
            CgmHistoryContinuitySyncRequest.ForStartup(),
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result<DesktopHistoryContinuitySyncRunResult>> RunResumeSyncAsync(
        CancellationToken cancellationToken)
    {
        return RunAsync(
            CgmHistoryContinuitySyncRequest.ForResume(),
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result<DesktopHistoryContinuitySyncRunResult>> RunManualSyncAsync(
        TimeSpan lookback,
        CancellationToken cancellationToken)
    {
        return RunAsync(
            CgmHistoryContinuitySyncRequest.ForManual(lookback),
            cancellationToken);
    }

    #region Helpers

    /// <summary>
    /// Runs a history continuity synchronization request.
    /// </summary>
    /// <param name="request">The synchronization request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The desktop synchronization run result.</returns>
    private async Task<Result<DesktopHistoryContinuitySyncRunResult>> RunAsync(
        CgmHistoryContinuitySyncRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!await _syncGate.WaitAsync(0, cancellationToken).ConfigureAwait(false))
        {
            var skippedResult = DesktopHistoryContinuitySyncRunResult.Skipped(request.Trigger);

            _statusStore.MarkSkipped(
                request.Trigger,
                "History continuity synchronization skipped because another run is already in progress.");

            _logger.LogInformation(
                "History continuity synchronization skipped because another run is already in progress. Trigger: {Trigger}",
                request.Trigger);

            return Result<DesktopHistoryContinuitySyncRunResult>.Success(skippedResult);
        }

        try
        {
            _statusStore.MarkRunning(request.Trigger);

            await using var scope = _serviceScopeFactory.CreateAsyncScope();

            var continuitySyncService = scope.ServiceProvider
                .GetRequiredService<ICgmHistoryContinuitySyncService>();

            var syncResult = await continuitySyncService
                .SyncRecentHistoryAsync(request, cancellationToken)
                .ConfigureAwait(false);

            if (syncResult.IsFailure)
            {
                _statusStore.MarkFailed(request.Trigger, syncResult.Error);

                _logger.LogWarning(
                    "History continuity synchronization failed. Trigger: {Trigger}, ErrorCode: {ErrorCode}",
                    request.Trigger,
                    syncResult.Error.Code);

                return Result<DesktopHistoryContinuitySyncRunResult>.Failure(syncResult.Error);
            }

            var runResult = DesktopHistoryContinuitySyncRunResult.Executed(
                request.Trigger,
                syncResult.Value);

            _statusStore.MarkSucceeded(request.Trigger, runResult);

            _logger.LogInformation(
                "History continuity synchronization completed. Trigger: {Trigger}, AddedReadings: {AddedReadingsCount}, DuplicateReadings: {DuplicateReadingsCount}",
                request.Trigger,
                syncResult.Value.AddedReadingsCount,
                syncResult.Value.DuplicateReadingsCount);

            return Result<DesktopHistoryContinuitySyncRunResult>.Success(runResult);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _statusStore.MarkCanceled(request.Trigger);

            _logger.LogInformation(
                "History continuity synchronization was canceled. Trigger: {Trigger}",
                request.Trigger);

            throw;
        }
        catch (Exception exception)
        {
            _statusStore.MarkFailed(request.Trigger, UnexpectedSyncError);

            _logger.LogError(
                exception,
                "Unexpected error while running history continuity synchronization. Trigger: {Trigger}",
                request.Trigger);

            return Result<DesktopHistoryContinuitySyncRunResult>.Failure(UnexpectedSyncError);
        }
        finally
        {
            _syncGate.Release();
        }
    }

    #endregion
}