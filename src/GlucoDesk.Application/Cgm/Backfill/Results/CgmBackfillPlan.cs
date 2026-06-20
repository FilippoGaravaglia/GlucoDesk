namespace GlucoDesk.Application.Cgm.Backfill.Results;

/// <summary>
/// Describes the result of planning a historical CGM backfill operation.
/// </summary>
/// <param name="CanBackfill">A value indicating whether at least one gap can be backfilled.</param>
/// <param name="RequestedStartsAt">The original requested planning window start timestamp.</param>
/// <param name="RequestedEndsAt">The original requested planning window end timestamp.</param>
/// <param name="RecoverableFrom">The earliest timestamp that can be recovered according to the active capability.</param>
/// <param name="RecoverableTo">The latest timestamp that can be recovered according to the active capability.</param>
/// <param name="RecoverableGaps">The recoverable gaps included in the plan.</param>
/// <param name="IgnoredGapsCount">The number of detected gaps ignored by the planner.</param>
/// <param name="Message">A user-facing or diagnostic message describing the plan.</param>
public sealed record CgmBackfillPlan(
    bool CanBackfill,
    DateTimeOffset RequestedStartsAt,
    DateTimeOffset RequestedEndsAt,
    DateTimeOffset? RecoverableFrom,
    DateTimeOffset? RecoverableTo,
    IReadOnlyCollection<CgmBackfillPlanGap> RecoverableGaps,
    int IgnoredGapsCount,
    string Message);