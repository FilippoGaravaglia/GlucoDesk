using GlucoDesk.Application.Cgm.History.Continuity.Enums;
using GlucoDesk.Application.Cgm.History.Continuity.Results;

namespace GlucoDesk.Desktop.Cgm.History.Continuity.Results;

/// <summary>
/// Represents the result of a desktop-triggered CGM history continuity synchronization run.
/// </summary>
public sealed record DesktopHistoryContinuitySyncRunResult
{
    private DesktopHistoryContinuitySyncRunResult(
        CgmHistoryContinuitySyncTrigger trigger,
        bool wasExecuted,
        bool wasSkipped,
        CgmHistoryContinuitySyncResult? continuitySync,
        string message)
    {
        if (!Enum.IsDefined(trigger))
        {
            throw new ArgumentException(
                "History continuity synchronization trigger is not valid.",
                nameof(trigger));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        Trigger = trigger;
        WasExecuted = wasExecuted;
        WasSkipped = wasSkipped;
        ContinuitySync = continuitySync;
        Message = message;
    }

    /// <summary>
    /// Gets the synchronization trigger.
    /// </summary>
    public CgmHistoryContinuitySyncTrigger Trigger { get; }

    /// <summary>
    /// Gets a value indicating whether the synchronization was executed.
    /// </summary>
    public bool WasExecuted { get; }

    /// <summary>
    /// Gets a value indicating whether the synchronization was skipped.
    /// </summary>
    public bool WasSkipped { get; }

    /// <summary>
    /// Gets the underlying application continuity synchronization result, when executed.
    /// </summary>
    public CgmHistoryContinuitySyncResult? ContinuitySync { get; }

    /// <summary>
    /// Gets a diagnostic message describing the desktop synchronization run.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Creates a result for an executed synchronization.
    /// </summary>
    /// <param name="trigger">The synchronization trigger.</param>
    /// <param name="continuitySync">The application continuity synchronization result.</param>
    /// <returns>The executed synchronization result.</returns>
    public static DesktopHistoryContinuitySyncRunResult Executed(
        CgmHistoryContinuitySyncTrigger trigger,
        CgmHistoryContinuitySyncResult continuitySync)
    {
        ArgumentNullException.ThrowIfNull(continuitySync);

        return new DesktopHistoryContinuitySyncRunResult(
            trigger,
            wasExecuted: true,
            wasSkipped: false,
            continuitySync,
            $"Desktop history continuity synchronization completed for {trigger}.");
    }

    /// <summary>
    /// Creates a result for a skipped synchronization.
    /// </summary>
    /// <param name="trigger">The synchronization trigger.</param>
    /// <returns>The skipped synchronization result.</returns>
    public static DesktopHistoryContinuitySyncRunResult Skipped(
        CgmHistoryContinuitySyncTrigger trigger)
    {
        return new DesktopHistoryContinuitySyncRunResult(
            trigger,
            wasExecuted: false,
            wasSkipped: true,
            continuitySync: null,
            $"Desktop history continuity synchronization skipped for {trigger} because another synchronization is already running.");
    }
}