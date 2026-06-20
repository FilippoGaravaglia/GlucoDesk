using GlucoDesk.Application.Cgm.Backfill.Requests;
using GlucoDesk.Application.Cgm.Backfill.Results;
using GlucoDesk.Application.Common.Results;

namespace GlucoDesk.Application.Cgm.Backfill.Services.Abstractions;

/// <summary>
/// Creates a historical CGM backfill plan from detected local history gaps.
/// </summary>
public interface ICgmBackfillPlanService
{
    /// <summary>
    /// Creates a historical CGM backfill plan for the supplied detected gaps.
    /// </summary>
    /// <param name="request">The backfill plan request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The backfill plan result.</returns>
    Task<Result<CgmBackfillPlan>> CreatePlanAsync(
        CgmBackfillPlanRequest request,
        CancellationToken cancellationToken);
}