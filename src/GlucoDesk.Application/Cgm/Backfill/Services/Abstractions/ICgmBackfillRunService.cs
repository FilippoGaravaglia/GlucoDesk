using GlucoDesk.Application.Cgm.Backfill.Requests;
using GlucoDesk.Application.Cgm.Backfill.Results;
using GlucoDesk.Application.Common.Results;

namespace GlucoDesk.Application.Cgm.Backfill.Services.Abstractions;

/// <summary>
/// Orchestrates historical CGM backfill runs from local history continuity data.
/// </summary>
public interface ICgmBackfillRunService
{
    /// <summary>
    /// Orchestrates a historical CGM backfill run for the supplied window.
    /// </summary>
    /// <param name="request">The backfill run request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The backfill run result.</returns>
    Task<Result<CgmBackfillRunResult>> RunAsync(
        CgmBackfillRunRequest request,
        CancellationToken cancellationToken);
}