using GlucoDesk.Application.Cgm.Backfill.Requests;
using GlucoDesk.Application.Cgm.Backfill.Results;
using GlucoDesk.Application.Common.Results;

namespace GlucoDesk.Application.Cgm.Backfill.Services.Abstractions;

/// <summary>
/// Defines application-level operations for synchronizing historical backfill readings into local history.
/// </summary>
public interface ICgmBackfillHistorySyncService
{
    /// <summary>
    /// Executes historical backfill and persists fetched readings into local history.
    /// </summary>
    /// <param name="request">The backfill run request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The backfill-to-history synchronization result.</returns>
    Task<Result<CgmBackfillHistorySyncResult>> SyncAsync(
        CgmBackfillRunRequest request,
        CancellationToken cancellationToken);
}