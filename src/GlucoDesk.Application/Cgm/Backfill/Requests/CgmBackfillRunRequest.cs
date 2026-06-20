namespace GlucoDesk.Application.Cgm.Backfill.Requests;

/// <summary>
/// Request used to orchestrate a historical CGM backfill run.
/// </summary>
/// <param name="StartsAt">The backfill orchestration window start timestamp.</param>
/// <param name="EndsAt">The backfill orchestration window end timestamp.</param>
public sealed record CgmBackfillRunRequest(
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt);