using GlucoDesk.Application.Cgm.Backfill.Requests;
using GlucoDesk.Application.Cgm.Backfill.Results;
using GlucoDesk.Application.Common.Results;

namespace GlucoDesk.Application.Cgm.Backfill.Services.Abstractions;

/// <summary>
/// Creates historical CGM backfill plans from local history continuity data.
/// </summary>
public interface ICgmBackfillPlanQueryService
{
    /// <summary>
    /// Creates a historical CGM backfill plan from the local history continuity analysis.
    /// </summary>
    /// <param name="request">The backfill plan request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The backfill plan result.</returns>
    Task<Result<CgmBackfillPlan>> CreatePlanAsync(
        CgmBackfillPlanFromHistoryRequest request,
        CancellationToken cancellationToken);
}