using GlucoDesk.Application.Cgm.Backfill.Requests;
using GlucoDesk.Application.Cgm.Backfill.Results;
using GlucoDesk.Application.Common.Results;

namespace GlucoDesk.Application.Cgm.Backfill.Services.Abstractions;

/// <summary>
/// Executes historical CGM backfill runs using planned recoverable gaps.
/// </summary>
public interface ICgmBackfillExecutionService
{
    /// <summary>
    /// Executes a historical CGM backfill run for the supplied window.
    /// </summary>
    /// <param name="request">The backfill run request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The backfill execution result.</returns>
    Task<Result<CgmBackfillExecutionResult>> ExecuteAsync(
        CgmBackfillRunRequest request,
        CancellationToken cancellationToken);
}