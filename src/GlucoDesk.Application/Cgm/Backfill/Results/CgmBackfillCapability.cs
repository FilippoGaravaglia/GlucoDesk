using GlucoDesk.Application.Cgm.Backfill.Enums;

namespace GlucoDesk.Application.Cgm.Backfill.Results;

/// <summary>
/// Describes whether the active CGM provider can support historical backfill.
/// </summary>
/// <param name="IsSupported">A value indicating whether historical backfill is supported.</param>
/// <param name="Status">The detailed backfill support status.</param>
/// <param name="MaximumLookback">The maximum lookback window that may be recovered.</param>
/// <param name="MinimumGapDuration">The minimum meaningful gap duration for backfill.</param>
/// <param name="Message">A user-facing or diagnostic message describing the capability.</param>
public sealed record CgmBackfillCapability(
    bool IsSupported,
    CgmBackfillSupportStatus Status,
    TimeSpan? MaximumLookback,
    TimeSpan? MinimumGapDuration,
    string Message);