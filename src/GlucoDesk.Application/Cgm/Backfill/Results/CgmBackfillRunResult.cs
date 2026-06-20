using GlucoDesk.Application.Cgm.Backfill.Enums;

namespace GlucoDesk.Application.Cgm.Backfill.Results;

/// <summary>
/// Represents the result of a historical CGM backfill orchestration run.
/// </summary>
/// <param name="Status">The backfill run status.</param>
/// <param name="Plan">The generated backfill plan.</param>
/// <param name="RecoverableGapsCount">The number of recoverable gaps found by the run.</param>
/// <param name="Message">A user-facing or diagnostic message describing the run outcome.</param>
public sealed record CgmBackfillRunResult(
    CgmBackfillRunStatus Status,
    CgmBackfillPlan Plan,
    int RecoverableGapsCount,
    string Message)
{
    /// <summary>
    /// Gets a value indicating whether the run found at least one recoverable gap.
    /// </summary>
    public bool HasRecoverableGaps => RecoverableGapsCount > 0;
}