using GlucoDesk.Application.Cgm.Backfill.Requests;
using GlucoDesk.Application.Cgm.Backfill.Results;
using GlucoDesk.Application.Cgm.Backfill.Services.Abstractions;
using GlucoDesk.Application.Cgm.History.Services.Abstractions;
using GlucoDesk.Application.Common.Results;

namespace GlucoDesk.Application.Cgm.Backfill.Services;

/// <summary>
/// Synchronizes historical CGM backfill readings into local glucose history.
/// </summary>
public sealed class CgmBackfillHistorySyncService : ICgmBackfillHistorySyncService
{
    private readonly ICgmBackfillExecutionService _backfillExecutionService;
    private readonly IGlucoseHistoryService _glucoseHistoryService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CgmBackfillHistorySyncService"/> class.
    /// </summary>
    /// <param name="backfillExecutionService">The backfill execution service.</param>
    /// <param name="glucoseHistoryService">The glucose history service.</param>
    public CgmBackfillHistorySyncService(
        ICgmBackfillExecutionService backfillExecutionService,
        IGlucoseHistoryService glucoseHistoryService)
    {
        ArgumentNullException.ThrowIfNull(backfillExecutionService);
        ArgumentNullException.ThrowIfNull(glucoseHistoryService);

        _backfillExecutionService = backfillExecutionService;
        _glucoseHistoryService = glucoseHistoryService;
    }

    /// <inheritdoc />
    public async Task<Result<CgmBackfillHistorySyncResult>> SyncAsync(
        CgmBackfillRunRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var backfillExecutionResult = await _backfillExecutionService
            .ExecuteAsync(request, cancellationToken)
            .ConfigureAwait(false);

        if (backfillExecutionResult.IsFailure)
        {
            return Result<CgmBackfillHistorySyncResult>.Failure(backfillExecutionResult.Error);
        }

        var backfillExecution = backfillExecutionResult.Value;

        if (!backfillExecution.HasFetchedReadings)
        {
            return Result<CgmBackfillHistorySyncResult>.Success(
                new CgmBackfillHistorySyncResult(
                    backfillExecution,
                    historySave: null,
                    "Backfill completed without readings to persist."));
        }

        var historySaveResult = await _glucoseHistoryService
            .SaveReadingsWithSummaryAsync(
                backfillExecution.FetchedReadings,
                cancellationToken)
            .ConfigureAwait(false);

        if (historySaveResult.IsFailure)
        {
            return Result<CgmBackfillHistorySyncResult>.Failure(historySaveResult.Error);
        }

        return Result<CgmBackfillHistorySyncResult>.Success(
            new CgmBackfillHistorySyncResult(
                backfillExecution,
                historySaveResult.Value,
                "Backfill readings synchronized into local history."));
    }
}