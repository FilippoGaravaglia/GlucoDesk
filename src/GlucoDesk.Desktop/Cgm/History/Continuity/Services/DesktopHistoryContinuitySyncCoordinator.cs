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
/// Coordinates desktop-triggered CGM history continuity synchronization runs.
/// </summary>
public sealed class DesktopHistoryContinuitySyncCoordinator : IDesktopHistoryContinuitySyncCoordinator
{
    private static readonly Error UnexpectedSyncError = new(
        "Desktop.HistoryContinuitySync.UnexpectedError",
        "An unexpected error occurred while synchronizing local glucose history continuity.");

    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<DesktopHistoryContinuitySyncCoordinator> _logger;
    private readonly SemaphoreSlim _syncGate = new(1, 1);

    /// <summary>
    /// Initializes a new instance of the <see cref="DesktopHistoryContinuitySyncCoordinator"/> class.
    /// </summary>
    /// <param name="serviceScopeFactory">The service scope factory.</param>
    /// <param name="logger">The logger.</param>
    public DesktopHistoryContinuitySyncCoordinator(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<DesktopHistoryContinuitySyncCoordinator> logger)
    {
        ArgumentNullException.ThrowIfNull(serviceScopeFactory);
        ArgumentNullException.ThrowIfNull(logger);

        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
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
    /// Runs a desktop-triggered history continuity synchronization if no other synchronization is already running.
    /// </summary>
    /// <param name="request">The continuity synchronization request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The desktop synchronization run result.</returns>
    private async Task<Result<DesktopHistoryContinuitySyncRunResult>> RunAsync(
        CgmHistoryContinuitySyncRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!await _syncGate.WaitAsync(millisecondsTimeout: 0, cancellationToken).ConfigureAwait(false))
        {
            _logger.LogInformation(
                "Desktop history continuity sync skipped because another sync is already running. Trigger={Trigger}",
                request.Trigger);

            return Result<DesktopHistoryContinuitySyncRunResult>.Success(
                DesktopHistoryContinuitySyncRunResult.Skipped(request.Trigger));
        }

        try
        {
            _logger.LogInformation(
                "Desktop history continuity sync started. Trigger={Trigger}, Lookback={Lookback}",
                request.Trigger,
                request.Lookback);

            using var scope = _serviceScopeFactory.CreateScope();

            var continuitySyncService = scope.ServiceProvider
                .GetRequiredService<ICgmHistoryContinuitySyncService>();

            var syncResult = await continuitySyncService
                .SyncRecentHistoryAsync(request, cancellationToken)
                .ConfigureAwait(false);

            if (syncResult.IsFailure)
            {
                _logger.LogWarning(
                    "Desktop history continuity sync failed. Trigger={Trigger}, ErrorCode={ErrorCode}, ErrorMessage={ErrorMessage}",
                    request.Trigger,
                    syncResult.Error.Code,
                    syncResult.Error.Message);

                return Result<DesktopHistoryContinuitySyncRunResult>.Failure(syncResult.Error);
            }

            _logger.LogInformation(
                "Desktop history continuity sync completed. Trigger={Trigger}, HasNewReadings={HasNewReadings}, AddedReadings={AddedReadingsCount}, DuplicateReadings={DuplicateReadingsCount}, StoredReadings={StoredReadingsCount}",
                request.Trigger,
                syncResult.Value.HasNewReadings,
                syncResult.Value.AddedReadingsCount,
                syncResult.Value.DuplicateReadingsCount,
                syncResult.Value.StoredReadingsCount);

            return Result<DesktopHistoryContinuitySyncRunResult>.Success(
                DesktopHistoryContinuitySyncRunResult.Executed(
                    request.Trigger,
                    syncResult.Value));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation(
                "Desktop history continuity sync was cancelled. Trigger={Trigger}",
                request.Trigger);

            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Unexpected desktop history continuity sync error. Trigger={Trigger}",
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