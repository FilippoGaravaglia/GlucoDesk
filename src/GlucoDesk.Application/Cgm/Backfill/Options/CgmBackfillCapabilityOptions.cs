namespace GlucoDesk.Application.Cgm.Backfill.Options;

/// <summary>
/// Options used to describe how historical CGM backfill should be exposed.
/// </summary>
public sealed record CgmBackfillCapabilityOptions
{
    /// <summary>
    /// Gets the default backfill capability options.
    /// </summary>
    public static CgmBackfillCapabilityOptions Default { get; } = new();

    /// <summary>
    /// Gets a value indicating whether backfill is enabled at application level.
    /// </summary>
    public bool IsEnabled { get; init; } = true;

    /// <summary>
    /// Gets the maximum lookback window that GlucoDesk should try to recover automatically.
    /// </summary>
    public TimeSpan MaximumLookback { get; init; } = TimeSpan.FromHours(24);

    /// <summary>
    /// Gets the minimum gap duration that should be considered meaningful for backfill.
    /// </summary>
    public TimeSpan MinimumGapDuration { get; init; } = TimeSpan.FromMinutes(10);
}