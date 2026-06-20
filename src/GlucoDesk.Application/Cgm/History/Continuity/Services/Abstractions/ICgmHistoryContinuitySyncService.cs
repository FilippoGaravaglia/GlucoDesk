using GlucoDesk.Application.Cgm.History.Continuity.Requests;
using GlucoDesk.Application.Cgm.History.Continuity.Results;
using GlucoDesk.Application.Common.Results;

namespace GlucoDesk.Application.Cgm.History.Continuity.Services.Abstractions;

/// <summary>
/// Defines application-level operations for synchronizing local CGM history continuity.
/// </summary>
public interface ICgmHistoryContinuitySyncService
{
    /// <summary>
    /// Synchronizes recent local CGM history continuity by executing backfill and merging fetched readings.
    /// </summary>
    /// <param name="request">The continuity synchronization request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The continuity synchronization result.</returns>
    Task<Result<CgmHistoryContinuitySyncResult>> SyncRecentHistoryAsync(
        CgmHistoryContinuitySyncRequest request,
        CancellationToken cancellationToken);
}