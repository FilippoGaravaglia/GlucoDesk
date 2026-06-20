using GlucoDesk.Application.Common.Results;
using GlucoDesk.Desktop.Cgm.History.Continuity.Results;

namespace GlucoDesk.Desktop.Cgm.History.Continuity.Services.Abstractions;

/// <summary>
/// Coordinates desktop-triggered CGM history continuity synchronization runs.
/// </summary>
public interface IDesktopHistoryContinuitySyncCoordinator
{
    /// <summary>
    /// Runs a startup history continuity synchronization without blocking the desktop UI thread.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The desktop synchronization run result.</returns>
    Task<Result<DesktopHistoryContinuitySyncRunResult>> RunStartupSyncAsync(
        CancellationToken cancellationToken);

    /// <summary>
    /// Runs a resume history continuity synchronization without blocking the desktop UI thread.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The desktop synchronization run result.</returns>
    Task<Result<DesktopHistoryContinuitySyncRunResult>> RunResumeSyncAsync(
        CancellationToken cancellationToken);

    /// <summary>
    /// Runs a manual history continuity synchronization.
    /// </summary>
    /// <param name="lookback">The manual synchronization lookback window.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The desktop synchronization run result.</returns>
    Task<Result<DesktopHistoryContinuitySyncRunResult>> RunManualSyncAsync(
        TimeSpan lookback,
        CancellationToken cancellationToken);
}