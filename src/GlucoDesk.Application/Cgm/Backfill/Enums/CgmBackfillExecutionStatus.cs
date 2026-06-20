namespace GlucoDesk.Application.Cgm.Backfill.Enums;

/// <summary>
/// Defines the outcome status of a historical CGM backfill execution.
/// </summary>
public enum CgmBackfillExecutionStatus
{
    Completed = 0,
    SkippedNoRecoverableGaps = 1
}