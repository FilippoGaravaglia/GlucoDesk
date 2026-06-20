namespace GlucoDesk.Application.Cgm.Backfill.Requests;

/// <summary>
/// Request used to create a historical CGM backfill plan.
/// </summary>
/// <param name="StartsAt">The requested planning window start timestamp.</param>
/// <param name="EndsAt">The requested planning window end timestamp.</param>
/// <param name="DetectedGaps">The local history gaps detected in the requested window.</param>
public sealed record CgmBackfillPlanRequest(
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    IReadOnlyCollection<CgmBackfillDetectedGap> DetectedGaps);