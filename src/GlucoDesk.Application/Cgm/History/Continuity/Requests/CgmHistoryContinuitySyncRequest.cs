using GlucoDesk.Application.Cgm.History.Continuity.Enums;

namespace GlucoDesk.Application.Cgm.History.Continuity.Requests;

/// <summary>
/// Request model for synchronizing recent local CGM history continuity.
/// </summary>
public sealed record CgmHistoryContinuitySyncRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CgmHistoryContinuitySyncRequest"/> class.
    /// </summary>
    /// <param name="trigger">The synchronization trigger.</param>
    /// <param name="lookback">The recent history lookback window to verify and backfill.</param>
    public CgmHistoryContinuitySyncRequest(
        CgmHistoryContinuitySyncTrigger trigger,
        TimeSpan lookback)
    {
        if (!Enum.IsDefined(trigger))
        {
            throw new ArgumentException(
                "History continuity synchronization trigger is not valid.",
                nameof(trigger));
        }

        if (lookback <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(lookback),
                lookback,
                "History continuity lookback must be greater than zero.");
        }

        Trigger = trigger;
        Lookback = lookback;
    }

    /// <summary>
    /// Gets the default startup lookback window.
    /// </summary>
    public static TimeSpan DefaultStartupLookback { get; } = TimeSpan.FromHours(24);

    /// <summary>
    /// Gets the default resume lookback window.
    /// </summary>
    public static TimeSpan DefaultResumeLookback { get; } = TimeSpan.FromHours(12);

    /// <summary>
    /// Gets the synchronization trigger.
    /// </summary>
    public CgmHistoryContinuitySyncTrigger Trigger { get; }

    /// <summary>
    /// Gets the recent history lookback window to verify and backfill.
    /// </summary>
    public TimeSpan Lookback { get; }

    /// <summary>
    /// Creates a startup continuity synchronization request.
    /// </summary>
    /// <param name="lookback">The optional startup lookback window.</param>
    /// <returns>The startup continuity synchronization request.</returns>
    public static CgmHistoryContinuitySyncRequest ForStartup(TimeSpan? lookback = null)
    {
        return new CgmHistoryContinuitySyncRequest(
            CgmHistoryContinuitySyncTrigger.Startup,
            lookback ?? DefaultStartupLookback);
    }

    /// <summary>
    /// Creates a resume continuity synchronization request.
    /// </summary>
    /// <param name="lookback">The optional resume lookback window.</param>
    /// <returns>The resume continuity synchronization request.</returns>
    public static CgmHistoryContinuitySyncRequest ForResume(TimeSpan? lookback = null)
    {
        return new CgmHistoryContinuitySyncRequest(
            CgmHistoryContinuitySyncTrigger.Resume,
            lookback ?? DefaultResumeLookback);
    }

    /// <summary>
    /// Creates a manual continuity synchronization request.
    /// </summary>
    /// <param name="lookback">The manual lookback window.</param>
    /// <returns>The manual continuity synchronization request.</returns>
    public static CgmHistoryContinuitySyncRequest ForManual(TimeSpan lookback)
    {
        return new CgmHistoryContinuitySyncRequest(
            CgmHistoryContinuitySyncTrigger.Manual,
            lookback);
    }
}