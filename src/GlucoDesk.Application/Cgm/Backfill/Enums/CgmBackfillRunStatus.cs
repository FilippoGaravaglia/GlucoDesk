namespace GlucoDesk.Application.Cgm.Backfill.Enums;

/// <summary>
/// Defines the outcome status of a historical CGM backfill orchestration run.
/// </summary>
public enum CgmBackfillRunStatus
{
    Planned = 0,
    SkippedNoRecoverableGaps = 1
}