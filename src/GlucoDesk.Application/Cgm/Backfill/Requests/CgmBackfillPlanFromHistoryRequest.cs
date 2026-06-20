namespace GlucoDesk.Application.Cgm.Backfill.Requests;

/// <summary>
/// Request used to create a historical CGM backfill plan from the local history continuity analysis.
/// </summary>
/// <param name="StartsAt">The local history window start timestamp.</param>
/// <param name="EndsAt">The local history window end timestamp.</param>
public sealed record CgmBackfillPlanFromHistoryRequest(
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt);