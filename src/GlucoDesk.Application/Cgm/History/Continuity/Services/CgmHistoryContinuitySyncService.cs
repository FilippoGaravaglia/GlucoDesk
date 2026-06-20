using GlucoDesk.Application.Cgm.Backfill.Requests;
using GlucoDesk.Application.Cgm.Backfill.Services.Abstractions;
using GlucoDesk.Application.Cgm.History.Continuity.Requests;
using GlucoDesk.Application.Cgm.History.Continuity.Results;
using GlucoDesk.Application.Cgm.History.Continuity.Services.Abstractions;
using GlucoDesk.Application.Common.Results;

namespace GlucoDesk.Application.Cgm.History.Continuity.Services;

/// <summary>
/// Synchronizes local CGM history continuity by executing recent backfill and merging fetched readings.
/// </summary>
public sealed class CgmHistoryContinuitySyncService : ICgmHistoryContinuitySyncService
{
    private readonly ICgmBackfillHistorySyncService _backfillHistorySyncService;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="CgmHistoryContinuitySyncService"/> class.
    /// </summary>
    /// <param name="backfillHistorySyncService">The backfill-to-history synchronization service.</param>
    /// <param name="timeProvider">The time provider.</param>
    public CgmHistoryContinuitySyncService(
        ICgmBackfillHistorySyncService backfillHistorySyncService,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(backfillHistorySyncService);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _backfillHistorySyncService = backfillHistorySyncService;
        _timeProvider = timeProvider;
    }

    /// <inheritdoc />
    public async Task<Result<CgmHistoryContinuitySyncResult>> SyncRecentHistoryAsync(
        CgmHistoryContinuitySyncRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var endsAt = _timeProvider.GetUtcNow();
        var startsAt = endsAt.Subtract(request.Lookback);

        var backfillSyncResult = await _backfillHistorySyncService
            .SyncAsync(
                new CgmBackfillRunRequest(
                    startsAt,
                    endsAt),
                cancellationToken)
            .ConfigureAwait(false);

        if (backfillSyncResult.IsFailure)
        {
            return Result<CgmHistoryContinuitySyncResult>.Failure(backfillSyncResult.Error);
        }

        return Result<CgmHistoryContinuitySyncResult>.Success(
            new CgmHistoryContinuitySyncResult(
                request,
                startsAt,
                endsAt,
                backfillSyncResult.Value,
                CreateMessage(request, backfillSyncResult.Value.HasNewReadings)));
    }

    #region Helpers

    /// <summary>
    /// Creates a diagnostic message for the continuity synchronization result.
    /// </summary>
    /// <param name="request">The original continuity synchronization request.</param>
    /// <param name="hasNewReadings">A value indicating whether new readings were added.</param>
    /// <returns>The diagnostic message.</returns>
    private static string CreateMessage(
        CgmHistoryContinuitySyncRequest request,
        bool hasNewReadings)
    {
        return hasNewReadings
            ? $"History continuity synchronization completed for {request.Trigger} and added new readings."
            : $"History continuity synchronization completed for {request.Trigger} without new readings.";
    }

    #endregion
}