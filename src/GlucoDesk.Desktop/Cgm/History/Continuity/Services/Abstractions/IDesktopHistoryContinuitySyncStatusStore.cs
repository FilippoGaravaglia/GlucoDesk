using GlucoDesk.Application.Cgm.History.Continuity.Enums;
using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Desktop.Cgm.History.Continuity.Results;

namespace GlucoDesk.Desktop.Cgm.History.Continuity.Services.Abstractions;

/// <summary>
/// Provides desktop-visible state tracking for history continuity synchronization.
/// </summary>
public interface IDesktopHistoryContinuitySyncStatusStore
{
    /// <summary>
    /// Raised whenever the history continuity synchronization status changes.
    /// </summary>
    event EventHandler<DesktopHistoryContinuitySyncStatusSnapshot>? StatusChanged;

    /// <summary>
    /// Gets the current synchronization status snapshot.
    /// </summary>
    DesktopHistoryContinuitySyncStatusSnapshot Current { get; }

    /// <summary>
    /// Marks the synchronization as running.
    /// </summary>
    /// <param name="trigger">The synchronization trigger.</param>
    void MarkRunning(CgmHistoryContinuitySyncTrigger trigger);

    /// <summary>
    /// Marks the synchronization as completed successfully.
    /// </summary>
    /// <param name="trigger">The synchronization trigger.</param>
    /// <param name="runResult">The desktop synchronization run result.</param>
    void MarkSucceeded(
        CgmHistoryContinuitySyncTrigger trigger,
        DesktopHistoryContinuitySyncRunResult runResult);

    /// <summary>
    /// Marks the synchronization as skipped.
    /// </summary>
    /// <param name="trigger">The synchronization trigger.</param>
    /// <param name="message">The skip message.</param>
    void MarkSkipped(
        CgmHistoryContinuitySyncTrigger trigger,
        string message);

    /// <summary>
    /// Marks the synchronization as failed.
    /// </summary>
    /// <param name="trigger">The synchronization trigger.</param>
    /// <param name="error">The failure error.</param>
    void MarkFailed(
        CgmHistoryContinuitySyncTrigger trigger,
        Error error);

    /// <summary>
    /// Marks the synchronization as canceled.
    /// </summary>
    /// <param name="trigger">The synchronization trigger.</param>
    void MarkCanceled(CgmHistoryContinuitySyncTrigger trigger);
}